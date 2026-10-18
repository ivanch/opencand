-- Materialized views. Idempotent: drops and recreates all of them.
--
-- Every view has a UNIQUE index, so `REFRESH MATERIALIZED VIEW CONCURRENTLY <name>` works after the first populate.
-- (NULLS NOT DISTINCT: siglauf / cargo / sgpartido can be NULL and must still count as one key.)
--
-- Each fact row is attached to exactly one turno='1' candidato_mapping row per (idcandidato, ano), picked
-- deterministically with DISTINCT ON, so a candidate with several mappings in a year never multiplies the sums.
--
-- Views are created WITH DATA, so no extra REFRESH is needed here.

DROP MATERIALIZED VIEW IF EXISTS mv_bem_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_bem_candidato_ano;
DROP MATERIALIZED VIEW IF EXISTS mv_despesas_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_receitas_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_candidato_mapping_analytics;

---
--- bem_candidato by (ano, idcandidato, siglauf, sgpartido, cargo)
CREATE MATERIALIZED VIEW mv_bem_candidato AS
WITH cm AS (
    SELECT DISTINCT ON (idcandidato, ano)
        idcandidato, ano, siglauf, sgpartido, cargo
    FROM candidato_mapping
    WHERE turno = '1'
    ORDER BY idcandidato, ano, cargo, siglauf, nomeue, nrcandidato, resultado
)
SELECT
    b.ano,
    b.idcandidato,
    cm.siglauf,
    cm.sgpartido,
    cm.cargo,
    SUM(b.valor) AS valor
FROM bem_candidato b
JOIN cm ON cm.idcandidato = b.idcandidato AND cm.ano = b.ano
GROUP BY b.ano, b.idcandidato, cm.siglauf, cm.sgpartido, cm.cargo;
CREATE UNIQUE INDEX ux_mv_bem_candidato ON mv_bem_candidato (ano, idcandidato, siglauf, sgpartido, cargo) NULLS NOT DISTINCT;

---
--- bem_candidato by (idcandidato, ano), no mapping join: used by the "enriquecimento" ranking
CREATE MATERIALIZED VIEW mv_bem_candidato_ano AS
SELECT
    idcandidato,
    ano,
    SUM(valor) AS valor
FROM bem_candidato
GROUP BY idcandidato, ano;
CREATE UNIQUE INDEX ux_mv_bem_candidato_ano ON mv_bem_candidato_ano (idcandidato, ano);

---
--- despesas_candidato by (ano, idcandidato, siglauf, sgpartido, cargo)
CREATE MATERIALIZED VIEW mv_despesas_candidato AS
WITH cm AS (
    SELECT DISTINCT ON (idcandidato, ano)
        idcandidato, ano, siglauf, cargo
    FROM candidato_mapping
    WHERE turno = '1'
    ORDER BY idcandidato, ano, cargo, siglauf, nomeue, nrcandidato, resultado
)
SELECT
    d.ano,
    d.idcandidato,
    cm.siglauf,
    d.sgpartido,
    cm.cargo,
    SUM(d.valor) AS valor
FROM despesas_candidato d
JOIN cm ON cm.idcandidato = d.idcandidato AND cm.ano = d.ano
GROUP BY d.ano, d.idcandidato, cm.siglauf, d.sgpartido, cm.cargo;
CREATE UNIQUE INDEX ux_mv_despesas_candidato ON mv_despesas_candidato (ano, idcandidato, siglauf, sgpartido, cargo) NULLS NOT DISTINCT;

---
--- receitas_candidato by (ano, idcandidato, siglauf, sgpartido, cargo)
CREATE MATERIALIZED VIEW mv_receitas_candidato AS
WITH cm AS (
    SELECT DISTINCT ON (idcandidato, ano)
        idcandidato, ano, siglauf, cargo
    FROM candidato_mapping
    WHERE turno = '1'
    ORDER BY idcandidato, ano, cargo, siglauf, nomeue, nrcandidato, resultado
)
SELECT
    r.ano,
    r.idcandidato,
    cm.siglauf,
    r.sgpartido,
    cm.cargo,
    SUM(r.valor) AS valor
FROM receitas_candidato r
JOIN cm ON cm.idcandidato = r.idcandidato AND cm.ano = r.ano
GROUP BY r.ano, r.idcandidato, cm.siglauf, r.sgpartido, cm.cargo;
CREATE UNIQUE INDEX ux_mv_receitas_candidato ON mv_receitas_candidato (ano, idcandidato, siglauf, sgpartido, cargo) NULLS NOT DISTINCT;

---
--- candidate mapping summary (first round only)
CREATE MATERIALIZED VIEW mv_candidato_mapping_analytics AS
SELECT DISTINCT cm.idcandidato, c.nome, cm.ano, cm.turno, cm.sgpartido, cm.siglauf, cm.cargo
FROM candidato_mapping cm
JOIN candidato c ON cm.idcandidato = c.idcandidato
WHERE cm.turno = '1';
-- nome is determined by idcandidato and turno is constant, so this key is unique
CREATE UNIQUE INDEX ux_mv_candidato_mapping_analytics ON mv_candidato_mapping_analytics (idcandidato, ano, siglauf, sgpartido, cargo) NULLS NOT DISTINCT;

ANALYZE mv_bem_candidato;
ANALYZE mv_bem_candidato_ano;
ANALYZE mv_despesas_candidato;
ANALYZE mv_receitas_candidato;
ANALYZE mv_candidato_mapping_analytics;
