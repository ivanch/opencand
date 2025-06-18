-- This script creates materialized views for bem_candidato, despesas_candidato, and receitas_candidato
-- Drop existing materialized views if they exist
DROP MATERIALIZED VIEW IF EXISTS mv_bem_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_despesas_candidato;
DROP MATERIALIZED VIEW IF EXISTS mv_receitas_candidato;

-- Creation stage
---
--- MV for bem_candidato
CREATE MATERIALIZED VIEW mv_bem_candidato AS
SELECT
    bem_candidato.ano,
    bem_candidato.idcandidato,
    candidato_mapping.siglauf as siglauf,
    candidato_mapping.sgpartido,
    cargo,
    SUM(valor) AS valor
FROM
    bem_candidato
JOIN candidato_mapping ON bem_candidato.idcandidato = candidato_mapping.idcandidato AND bem_candidato.ano = candidato_mapping.ano AND candidato_mapping.turno = '1'
GROUP BY
    bem_candidato.ano,
    bem_candidato.idcandidato,
    candidato_mapping.sgpartido,
    siglauf,
    cargo;
CREATE INDEX idx_mv_bem_candidato ON mv_bem_candidato (ano, idcandidato, siglauf, sgpartido, cargo);

---
--- MV for despesas_candidato
CREATE MATERIALIZED VIEW mv_despesas_candidato AS
SELECT
    despesas_candidato.ano,
    despesas_candidato.idcandidato,
    candidato_mapping.siglauf as siglauf,
    despesas_candidato.sgpartido,
    cargo,
    SUM(valor) AS valor
FROM
    despesas_candidato
JOIN candidato_mapping ON despesas_candidato.idcandidato = candidato_mapping.idcandidato AND despesas_candidato.ano = candidato_mapping.ano AND candidato_mapping.turno = '1'
GROUP BY
    despesas_candidato.ano,
    despesas_candidato.idcandidato,
    despesas_candidato.sgpartido,
    siglauf,
    cargo;
CREATE INDEX idx_mv_despesas_candidato ON mv_despesas_candidato (ano, idcandidato, siglauf, sgpartido, cargo);

---
--- MV for receitas_candidato
CREATE MATERIALIZED VIEW mv_receitas_candidato AS
SELECT
    receitas_candidato.ano,
    receitas_candidato.idcandidato,
    candidato_mapping.siglauf as siglauf,
    receitas_candidato.sgpartido,
    cargo,
    SUM(valor) AS valor
FROM
    receitas_candidato
JOIN candidato_mapping ON receitas_candidato.idcandidato = candidato_mapping.idcandidato AND receitas_candidato.ano = candidato_mapping.ano AND candidato_mapping.turno = '1'
GROUP BY
    receitas_candidato.ano,
    receitas_candidato.idcandidato,
    receitas_candidato.sgpartido,
    siglauf,
    cargo;
CREATE INDEX idx_mv_receitas_candidato ON mv_receitas_candidato (ano, idcandidato, siglauf, sgpartido, cargo);
