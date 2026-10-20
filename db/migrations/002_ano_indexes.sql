-- Indexes on ano for bem_candidato and rede_social, so the API can list the years available in each table with an
-- index skip scan (see OpenCandRepository.GetDataAvailabilityAsync) instead of reading the whole table.
CREATE INDEX IF NOT EXISTS idx_bem_candidato_ano ON bem_candidato (ano);
CREATE INDEX IF NOT EXISTS idx_rede_social_ano ON rede_social (ano);
