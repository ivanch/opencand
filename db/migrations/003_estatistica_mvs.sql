-- Migration 003: materialized views for /v1/estatistica.
--
-- No schema change here: db/mv.sql now also creates the rollup views (mv_bem_resumo, mv_despesas_resumo,
-- mv_receitas_resumo), mv_enriquecimento_candidato and filter indexes on the per-candidate views. DbMigrator re-runs
-- mv.sql only after applying a migration, so this script exists to rebuild the views on databases that already have them.

SELECT 1;
