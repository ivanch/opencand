-- Materialized views. Idempotent: drops and recreates all of them.
--
-- Every view has a UNIQUE index, so `REFRESH MATERIALIZED VIEW CONCURRENTLY <name>` works after the first populate.
-- Plain unique indexes (no NULLS NOT DISTINCT, which needs PostgreSQL 15+): the GROUP BY output is unique anyway, and a
-- concurrent refresh just rewrites rows whose key has a NULL instead of matching them.
--
-- Each fact row is attached to exactly one turno='1' candidato_mapping row per (idcandidato, ano), picked
-- deterministically with DISTINCT ON, so a candidate with several mappings in a year never multiplies the sums.
--
-- Views are created WITH DATA, so no extra REFRESH is needed here.

-- Views built on top of other views are dropped first.
DROP MATERIALIZED VIEW IF EXISTS mv_enriquecimento_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_bem_resumo;
DROP MATERIALIZED VIEW IF EXISTS mv_despesas_resumo;
DROP MATERIALIZED VIEW IF EXISTS mv_receitas_resumo;
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
CREATE UNIQUE INDEX ux_mv_bem_candidato ON mv_bem_candidato (ano, idcandidato, siglauf, sgpartido, cargo);
-- Filters of /v1/estatistica/values-sum (groupBy=candidato) without ano
CREATE INDEX idx_mv_bem_candidato_sgpartido ON mv_bem_candidato (sgpartido, ano);
CREATE INDEX idx_mv_bem_candidato_siglauf ON mv_bem_candidato (siglauf, ano);
CREATE INDEX idx_mv_bem_candidato_cargo ON mv_bem_candidato (cargo, ano);

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
CREATE UNIQUE INDEX ux_mv_despesas_candidato ON mv_despesas_candidato (ano, idcandidato, siglauf, sgpartido, cargo);
-- Filters of /v1/estatistica/values-sum (groupBy=candidato) without ano
CREATE INDEX idx_mv_despesas_candidato_sgpartido ON mv_despesas_candidato (sgpartido, ano);
CREATE INDEX idx_mv_despesas_candidato_siglauf ON mv_despesas_candidato (siglauf, ano);
CREATE INDEX idx_mv_despesas_candidato_cargo ON mv_despesas_candidato (cargo, ano);

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
CREATE UNIQUE INDEX ux_mv_receitas_candidato ON mv_receitas_candidato (ano, idcandidato, siglauf, sgpartido, cargo);
-- Filters of /v1/estatistica/values-sum (groupBy=candidato) without ano
CREATE INDEX idx_mv_receitas_candidato_sgpartido ON mv_receitas_candidato (sgpartido, ano);
CREATE INDEX idx_mv_receitas_candidato_siglauf ON mv_receitas_candidato (siglauf, ano);
CREATE INDEX idx_mv_receitas_candidato_cargo ON mv_receitas_candidato (cargo, ano);

---
--- mv_bem_candidato rolled up by (ano, siglauf, sgpartido, cargo): /v1/estatistica/values-sum grouped by partido/uf/cargo
CREATE MATERIALIZED VIEW mv_bem_resumo AS
SELECT ano, siglauf, sgpartido, cargo, SUM(valor) AS valor
FROM mv_bem_candidato
GROUP BY ano, siglauf, sgpartido, cargo;
CREATE UNIQUE INDEX ux_mv_bem_resumo ON mv_bem_resumo (ano, siglauf, sgpartido, cargo);

---
--- mv_despesas_candidato rolled up by (ano, siglauf, sgpartido, cargo): /v1/estatistica/values-sum grouped by partido/uf/cargo
CREATE MATERIALIZED VIEW mv_despesas_resumo AS
SELECT ano, siglauf, sgpartido, cargo, SUM(valor) AS valor
FROM mv_despesas_candidato
GROUP BY ano, siglauf, sgpartido, cargo;
CREATE UNIQUE INDEX ux_mv_despesas_resumo ON mv_despesas_resumo (ano, siglauf, sgpartido, cargo);

---
--- mv_receitas_candidato rolled up by (ano, siglauf, sgpartido, cargo): /v1/estatistica/values-sum grouped by partido/uf/cargo
CREATE MATERIALIZED VIEW mv_receitas_resumo AS
SELECT ano, siglauf, sgpartido, cargo, SUM(valor) AS valor
FROM mv_receitas_candidato
GROUP BY ano, siglauf, sgpartido, cargo;
CREATE UNIQUE INDEX ux_mv_receitas_resumo ON mv_receitas_resumo (ano, siglauf, sgpartido, cargo);

---
--- first and last declared assets per candidate (at least two years): the "enriquecimento" ranking
CREATE MATERIALIZED VIEW mv_enriquecimento_candidato AS
WITH extremos AS (
    SELECT idcandidato, MIN(ano) AS anoinicial, MAX(ano) AS anofinal
    FROM mv_bem_candidato_ano
    GROUP BY idcandidato
    HAVING MIN(ano) < MAX(ano)
)
SELECT
    e.idcandidato,
    e.anoinicial,
    p_ini.valor AS patrimonioinicial,
    e.anofinal,
    p_fim.valor AS patrimoniofinal,
    p_fim.valor - p_ini.valor AS enriquecimento
FROM extremos e
JOIN mv_bem_candidato_ano p_ini ON p_ini.idcandidato = e.idcandidato AND p_ini.ano = e.anoinicial
JOIN mv_bem_candidato_ano p_fim ON p_fim.idcandidato = e.idcandidato AND p_fim.ano = e.anofinal;
CREATE UNIQUE INDEX ux_mv_enriquecimento_candidato ON mv_enriquecimento_candidato (idcandidato);
CREATE INDEX idx_mv_enriquecimento_candidato_enriquecimento ON mv_enriquecimento_candidato (enriquecimento DESC);

---
--- candidate mapping summary (first round only)
CREATE MATERIALIZED VIEW mv_candidato_mapping_analytics AS
SELECT DISTINCT cm.idcandidato, c.nome, cm.ano, cm.turno, cm.sgpartido, cm.siglauf, cm.cargo
FROM candidato_mapping cm
JOIN candidato c ON cm.idcandidato = c.idcandidato
WHERE cm.turno = '1';
-- nome is determined by idcandidato and turno is constant, so this key is unique
CREATE UNIQUE INDEX ux_mv_candidato_mapping_analytics ON mv_candidato_mapping_analytics (idcandidato, ano, siglauf, sgpartido, cargo);

ANALYZE mv_bem_candidato;
ANALYZE mv_bem_candidato_ano;
ANALYZE mv_despesas_candidato;
ANALYZE mv_receitas_candidato;
ANALYZE mv_bem_resumo;
ANALYZE mv_despesas_resumo;
ANALYZE mv_receitas_resumo;
ANALYZE mv_enriquecimento_candidato;
ANALYZE mv_candidato_mapping_analytics;
