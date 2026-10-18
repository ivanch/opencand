-- Migration 001: ETL support and schema fixes.
--
--  * candidato.datanascimento TIMESTAMPTZ -> DATE, candidato.sexo CHAR(15) -> VARCHAR(20)
--  * '' cpf / email -> NULL
--  * merges candidatos that share a cpf (keeps the canonical id, records the others in candidato_alias)
--  * UNIQUE(cpf), rede_social PK (idcandidato, ano, link), candidato_ext FK, index changes
--  * etl_run table, tsm_system_rows extension
--
-- Idempotent: safe to run on an already-migrated database (and on a fresh db.sql install, where it is a no-op).
-- Applied automatically on API/ETL startup by DbMigrator (src/OpenCand.Core/Database), which wraps it in a transaction
-- and re-runs db/mv.sql afterwards (the materialized views still reference merged candidate ids until then).
-- Take a pg_dump before deploying it to an existing database.

CREATE EXTENSION IF NOT EXISTS tsm_system_rows;

-- Table order matters for the locks below; take them all up front to avoid deadlocks with a running API.
LOCK TABLE candidato, candidato_mapping, candidato_ext, bem_candidato, rede_social, despesas_candidato, receitas_candidato
    IN SHARE ROW EXCLUSIVE MODE;

---------------------------------------------------------------------------------------------------
-- 1. Column types
---------------------------------------------------------------------------------------------------
DO $$
BEGIN
    IF (SELECT data_type FROM information_schema.columns
        WHERE table_schema = current_schema() AND table_name = 'candidato' AND column_name = 'datanascimento')
       <> 'date' THEN
        EXECUTE $q$ALTER TABLE candidato ALTER COLUMN datanascimento TYPE DATE
                    USING (datanascimento AT TIME ZONE 'UTC')::date$q$;
    END IF;

    IF (SELECT data_type || ':' || coalesce(character_maximum_length, 0)
        FROM information_schema.columns
        WHERE table_schema = current_schema() AND table_name = 'candidato' AND column_name = 'sexo')
       <> 'character varying:20' THEN
        EXECUTE $q$ALTER TABLE candidato ALTER COLUMN sexo TYPE VARCHAR(20)
                    USING NULLIF(btrim(sexo::text), '')$q$;
    END IF;
END $$;

---------------------------------------------------------------------------------------------------
-- 2. Empty strings -> NULL
---------------------------------------------------------------------------------------------------
UPDATE candidato         SET cpf   = NULL WHERE cpf   IS NOT NULL AND btrim(cpf)   = '';
UPDATE candidato_mapping SET cpf   = NULL WHERE cpf   IS NOT NULL AND btrim(cpf)   = '';
UPDATE candidato_ext     SET email = NULL WHERE email IS NOT NULL AND btrim(email) = '';

---------------------------------------------------------------------------------------------------
-- 3. Alias table (old candidate id -> canonical id), so previously published links keep working
---------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS candidato_alias (
    old_id UUID NOT NULL PRIMARY KEY,
    new_id UUID NOT NULL,
    CONSTRAINT ck_candidato_alias_distinct CHECK (old_id <> new_id)
);

---------------------------------------------------------------------------------------------------
-- 4. rede_social PK: (idcandidato, rede, siglauf, ano) -> (idcandidato, ano, link)
--    Done before the merge so that merge conflicts are resolved against the final key.
---------------------------------------------------------------------------------------------------
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint
                   WHERE conrelid = 'rede_social'::regclass AND conname = 'pk_rede_social'
                     AND pg_get_constraintdef(oid) = 'PRIMARY KEY (idcandidato, ano, link)') THEN
        -- Keep a single row per (idcandidato, ano, link) before the new key is created.
        DELETE FROM rede_social a
        USING rede_social b
        WHERE a.idcandidato = b.idcandidato AND a.ano = b.ano AND a.link = b.link AND a.ctid > b.ctid;

        ALTER TABLE rede_social DROP CONSTRAINT IF EXISTS pk_rede_social;
        ALTER TABLE rede_social ADD CONSTRAINT pk_rede_social PRIMARY KEY (idcandidato, ano, link);
    END IF;
END $$;
-- siglauf was NOT NULL only because it was part of the old PK
ALTER TABLE rede_social ALTER COLUMN siglauf DROP NOT NULL;

---------------------------------------------------------------------------------------------------
-- 5. Merge candidatos that share a non-null cpf
--    canonical = most candidato_mapping rows, then highest popularidade (ties: latest ultimoano, then smallest id)
---------------------------------------------------------------------------------------------------
DO $$
DECLARE
    bad_cpfs TEXT;
BEGIN
    -- Refuse to merge on values that are clearly placeholders instead of CPFs: that would fuse different people.
    SELECT string_agg(DISTINCT cpf, ', ') INTO bad_cpfs
    FROM (SELECT cpf FROM candidato WHERE cpf IS NOT NULL GROUP BY cpf HAVING count(*) > 1) d
    WHERE cpf !~ '^[0-9]{11}$' OR cpf ~ '^(.)\1{10}$';
    IF bad_cpfs IS NOT NULL THEN
        RAISE EXCEPTION 'Duplicate candidato.cpf values that are not valid CPFs (%); clean them up before running this migration', bad_cpfs;
    END IF;
END $$;

CREATE TEMP TABLE _cand_merge (old_id UUID PRIMARY KEY, new_id UUID NOT NULL) ON COMMIT DROP;

INSERT INTO _cand_merge (old_id, new_id)
WITH dup AS (
    SELECT c.idcandidato, c.cpf, c.popularidade, c.ultimoano,
           (SELECT count(*) FROM candidato_mapping m WHERE m.idcandidato = c.idcandidato) AS nmap
    FROM candidato c
    WHERE c.cpf IN (SELECT cpf FROM candidato WHERE cpf IS NOT NULL GROUP BY cpf HAVING count(*) > 1)
),
ranked AS (
    SELECT idcandidato, cpf,
           row_number() OVER (PARTITION BY cpf
                              ORDER BY nmap DESC, popularidade DESC NULLS LAST, ultimoano DESC NULLS LAST, idcandidato) AS rn
    FROM dup
)
SELECT r.idcandidato, k.idcandidato
FROM ranked r
JOIN ranked k ON k.cpf = r.cpf AND k.rn = 1
WHERE r.rn > 1;

-- Fold attributes of the merged rows into the canonical row (only fill gaps, never overwrite).
UPDATE candidato c
SET datanascimento = coalesce(c.datanascimento, a.datanascimento),
    sexo           = coalesce(c.sexo, a.sexo),
    apelido        = coalesce(c.apelido, a.apelido),
    localidade     = coalesce(c.localidade, a.localidade),
    ultimoano      = greatest(c.ultimoano, a.ultimoano),
    popularidade   = coalesce(c.popularidade, 0) + a.popularidade
FROM (
    SELECT m.new_id,
           (array_agg(o.datanascimento ORDER BY o.ultimoano DESC NULLS LAST) FILTER (WHERE o.datanascimento IS NOT NULL))[1] AS datanascimento,
           (array_agg(o.sexo           ORDER BY o.ultimoano DESC NULLS LAST) FILTER (WHERE o.sexo           IS NOT NULL))[1] AS sexo,
           (array_agg(o.apelido        ORDER BY o.ultimoano DESC NULLS LAST) FILTER (WHERE o.apelido        IS NOT NULL))[1] AS apelido,
           (array_agg(o.localidade     ORDER BY o.ultimoano DESC NULLS LAST) FILTER (WHERE o.localidade     IS NOT NULL))[1] AS localidade,
           max(o.ultimoano) AS ultimoano,
           sum(coalesce(o.popularidade, 0)) AS popularidade
    FROM _cand_merge m
    JOIN candidato o ON o.idcandidato = m.old_id
    GROUP BY m.new_id
) a
WHERE c.idcandidato = a.new_id;

-- Aliases: record merged ids, and collapse chains left over from earlier merges.
INSERT INTO candidato_alias (old_id, new_id)
SELECT old_id, new_id FROM _cand_merge
ON CONFLICT (old_id) DO UPDATE SET new_id = EXCLUDED.new_id;

UPDATE candidato_alias al SET new_id = m.new_id
FROM _cand_merge m
WHERE al.new_id = m.old_id;

-- Re-point child rows. Tables with a business key that can collide: insert-or-skip, then delete the leftovers.
INSERT INTO candidato_mapping (idcandidato, cpf, nome, sqcandidato, turno, ano, tipoeleicao, siglauf, nomeue,
                               cargo, sgpartido, nrcandidato, resultado)
SELECT m.new_id, x.cpf, x.nome, x.sqcandidato, x.turno, x.ano, x.tipoeleicao, x.siglauf, x.nomeue,
       x.cargo, x.sgpartido, x.nrcandidato, x.resultado
FROM candidato_mapping x
JOIN _cand_merge m ON m.old_id = x.idcandidato
ON CONFLICT DO NOTHING;
DELETE FROM candidato_mapping x USING _cand_merge m WHERE x.idcandidato = m.old_id;

INSERT INTO candidato_ext (idcandidato, ano, apelido, email, estadocivil, escolaridade, ocupacao)
SELECT m.new_id, x.ano, x.apelido, x.email, x.estadocivil, x.escolaridade, x.ocupacao
FROM candidato_ext x
JOIN _cand_merge m ON m.old_id = x.idcandidato
ON CONFLICT DO NOTHING;
DELETE FROM candidato_ext x USING _cand_merge m WHERE x.idcandidato = m.old_id;

INSERT INTO bem_candidato (idcandidato, ano, ordembem, tipobem, descricao, valor)
SELECT m.new_id, x.ano, x.ordembem, x.tipobem, x.descricao, x.valor
FROM bem_candidato x
JOIN _cand_merge m ON m.old_id = x.idcandidato
ON CONFLICT DO NOTHING;
DELETE FROM bem_candidato x USING _cand_merge m WHERE x.idcandidato = m.old_id;

INSERT INTO rede_social (idcandidato, rede, siglauf, ano, link)
SELECT m.new_id, x.rede, x.siglauf, x.ano, x.link
FROM rede_social x
JOIN _cand_merge m ON m.old_id = x.idcandidato
ON CONFLICT DO NOTHING;
DELETE FROM rede_social x USING _cand_merge m WHERE x.idcandidato = m.old_id;

-- Surrogate (random uuid) primary keys: nothing can collide, a plain re-point is enough.
UPDATE despesas_candidato x SET idcandidato = m.new_id FROM _cand_merge m WHERE x.idcandidato = m.old_id;
UPDATE receitas_candidato x SET idcandidato = m.new_id FROM _cand_merge m WHERE x.idcandidato = m.old_id;

DELETE FROM candidato c USING _cand_merge m WHERE c.idcandidato = m.old_id;

---------------------------------------------------------------------------------------------------
-- 6. Constraints and indexes
---------------------------------------------------------------------------------------------------
CREATE UNIQUE INDEX IF NOT EXISTS ux_candidato_cpf ON candidato (cpf) WHERE cpf IS NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint
                   WHERE conrelid = 'candidato_ext'::regclass AND conname = 'fk_candidato_ext_candidato') THEN
        DELETE FROM candidato_ext e WHERE NOT EXISTS (SELECT 1 FROM candidato c WHERE c.idcandidato = e.idcandidato);
        ALTER TABLE candidato_ext
            ADD CONSTRAINT fk_candidato_ext_candidato FOREIGN KEY (idcandidato)
            REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_candidato_mapping_ano_sqcandidato ON candidato_mapping (ano, sqcandidato);

DROP INDEX IF EXISTS idx_candidato_nome;                    -- covered by idx_candidato_nomenascimento
DROP INDEX IF EXISTS idx_candidato_mapping_idcandidato;     -- covered by pk_candidato_mapping
DROP INDEX IF EXISTS idx_candidato_ext_idcandidato;         -- covered by pk_candidato_ext
DROP INDEX IF EXISTS idx_bem_candidato_idcandidato;         -- covered by pk_bem_candidato
DROP INDEX IF EXISTS idx_rede_social_idcandidato;           -- covered by pk_rede_social

---------------------------------------------------------------------------------------------------
-- 7. ETL run log (used by the API to invalidate caches after a load)
---------------------------------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS etl_run (
    id SERIAL PRIMARY KEY,
    started_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    finished_at TIMESTAMPTZ,
    status TEXT NOT NULL DEFAULT 'running'
);
