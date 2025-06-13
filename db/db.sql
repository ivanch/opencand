DROP TABLE IF EXISTS bem_candidato CASCADE;
DROP TABLE IF EXISTS candidato_mapping CASCADE;
DROP TABLE IF EXISTS candidato_ext CASCADE;
DROP TABLE IF EXISTS rede_social CASCADE;
DROP TABLE IF EXISTS candidato CASCADE;
DROP TABLE IF EXISTS partido CASCADE;
DROP TABLE IF EXISTS despesas_candidato CASCADE;
DROP TABLE IF EXISTS receitas_candidato CASCADE;

CREATE TABLE candidato (
    idcandidato UUID NOT NULL PRIMARY KEY,
    cpf VARCHAR(11),
    nome VARCHAR(255) NOT NULL,
    datanascimento TIMESTAMPTZ,
    sexo CHAR(15),
    apelido VARCHAR(255),
    localidade VARCHAR(100),
    ultimoano INT,
    popularidade BIGINT DEFAULT 0
);
CREATE INDEX idx_candidato_nome ON candidato (nome);
CREATE INDEX idx_candidato_apelido ON candidato (apelido);
CREATE INDEX idx_candidato_datanascimento ON candidato (datanascimento);
CREATE INDEX idx_candidato_nomenascimento ON candidato (nome, datanascimento);
CREATE INDEX idx_candidato_cpf ON candidato (cpf);

-- Each candidato (idcandidato, cpf, nome) will be mapped to a (sqcandidato, ano, tipo_eleicao, sg_uf, cargo, resultado) 
CREATE TABLE candidato_mapping (
    idcandidato UUID NOT NULL,
    cpf VARCHAR(11),
    nome VARCHAR(255) NOT NULL,
    sqcandidato VARCHAR(50) NOT NULL,
    turno VARCHAR(2) NOT NULL,
    ano INT NOT NULL,
    tipoeleicao VARCHAR(50),
    siglauf VARCHAR(2),
    nomeue VARCHAR(100),
    cargo VARCHAR(50),
    sgpartido VARCHAR(50),
    nrcandidato VARCHAR(20),
    resultado VARCHAR(50),
    CONSTRAINT pk_candidato_mapping PRIMARY KEY (idcandidato, ano, siglauf, nomeue, cargo, nrcandidato, resultado),
    CONSTRAINT fk_candidato_mapping_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX idx_candidato_mapping_idcandidato ON candidato_mapping (idcandidato);
CREATE INDEX idx_candidato_mapping_cpf ON candidato_mapping (cpf);
CREATE INDEX idx_candidato_mapping_nome ON candidato_mapping (nome);
CREATE INDEX idx_candidato_mapping_ano ON candidato_mapping (ano);
CREATE INDEX idx_candidato_mapping_sqcandidato ON candidato_mapping (sqcandidato);

CREATE TABLE candidato_ext (
    idcandidato UUID NOT NULL,
    ano INT NOT NULL,
    apelido VARCHAR(255),
    email TEXT,
    estadocivil VARCHAR(50),
    escolaridade VARCHAR(50),
    ocupacao TEXT,
    CONSTRAINT pk_candidato_ext PRIMARY KEY (idcandidato, ano)
);
CREATE INDEX idx_candidato_ext_idcandidato ON candidato_ext (idcandidato);
CREATE INDEX idx_candidato_ext_ano ON candidato_ext (ano);

---- Table for storing assets of candidates
CREATE TABLE bem_candidato (
    idcandidato UUID NOT NULL,
    ano INT NOT NULL,
    ordembem INT,
    tipobem VARCHAR(150),
    descricao VARCHAR(500),
    valor NUMERIC(20, 2),
    CONSTRAINT fk_bem_candidato_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
ALTER TABLE bem_candidato ADD CONSTRAINT pk_bem_candidato PRIMARY KEY (idcandidato, ano, ordembem);
CREATE INDEX idx_bem_candidato_idcandidato ON bem_candidato (idcandidato);
CREATE INDEX idx_bem_candidato_valor ON bem_candidato (valor);

---- Table for storing social media links of candidates
CREATE TABLE rede_social (
    idcandidato UUID NOT NULL,
    rede VARCHAR(50) NOT NULL,
    siglauf VARCHAR(2),
    ano INT NOT NULL,
    link TEXT NOT NULL,
    CONSTRAINT pk_rede_social PRIMARY KEY (idcandidato, rede, siglauf, ano),
    CONSTRAINT fk_rede_social_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX idx_rede_social_idcandidato ON rede_social (idcandidato);

---- Table for storing party information
CREATE TABLE partido (
    sigla VARCHAR(50) NOT NULL PRIMARY KEY,
    nome VARCHAR(255) NOT NULL,
    numero INT NOT NULL
);
CREATE INDEX idx_partido_nome ON partido (nome);
CREATE INDEX idx_partido_numero ON partido (numero);

---- Tables for storing despesas e receitas of candidacies
CREATE TABLE despesas_candidato (
    iddespesa UUID NOT NULL DEFAULT gen_random_uuid(),
    idcandidato UUID NOT NULL,
    ano INT NOT NULL,
    turno VARCHAR(2) NOT NULL,
    sqcandidato VARCHAR(50) NOT NULL,
    sgpartido VARCHAR(50) NOT NULL,
    tipofornecedor VARCHAR(150),
    cnpjfornecedor VARCHAR(14),
    cpffornecedor VARCHAR(11),
    nomefornecedor VARCHAR(255),
    nomefornecedorrfb VARCHAR(255),
    municipiofornecedor VARCHAR(100),
    tipodocumento VARCHAR(50),
    datadespesa TIMESTAMPTZ,
    descricao TEXT,
    origemdespesa TEXT,
    valor NUMERIC(20, 2),
    CONSTRAINT pk_despesas_candidato PRIMARY KEY (iddespesa),
    CONSTRAINT fk_despesas_candidato_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX idx_despesas_candidato_idcandidato ON despesas_candidato (idcandidato);
CREATE INDEX idx_despesas_candidato_ano ON despesas_candidato (ano);
CREATE INDEX idx_despesas_candidato_sqcandidato ON despesas_candidato (sqcandidato);
CREATE INDEX idx_despesas_candidato_sgpartido ON despesas_candidato (sgpartido);

CREATE TABLE receitas_candidato (
    idreceita UUID NOT NULL DEFAULT gen_random_uuid(),
    idcandidato UUID NOT NULL,
    ano INT NOT NULL,
    turno VARCHAR(2) NOT NULL,
    sqcandidato VARCHAR(50) NOT NULL,
    sgpartido VARCHAR(50) NOT NULL,
    fontereceita VARCHAR(150),
    origemreceita VARCHAR(250),
    naturezareceita VARCHAR(250),
    especiereceita VARCHAR(250),
    cnpjdoador VARCHAR(14),
    cpfdoador VARCHAR(11),
    nomedoador VARCHAR(255),
    nomedoadorrfb VARCHAR(255),
    municipiodoador VARCHAR(100),
    datareceita TIMESTAMPTZ,
    descricao TEXT,
    valor NUMERIC(20, 2),
    CONSTRAINT pk_receitas_candidato PRIMARY KEY (idreceita),
    CONSTRAINT fk_receitas_candidato_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX idx_receitas_candidato_idcandidato ON receitas_candidato (idcandidato);
CREATE INDEX idx_receitas_candidato_ano ON receitas_candidato (ano);
CREATE INDEX idx_receitas_candidato_sqcandidato ON receitas_candidato (sqcandidato);
CREATE INDEX idx_receitas_candidato_sgpartido ON receitas_candidato (sgpartido);



-- Search function
CREATE EXTENSION IF NOT EXISTS pg_trgm;
CREATE INDEX idx_candidato_nome_trgm    ON candidato USING GIN (nome    gin_trgm_ops);
CREATE INDEX idx_candidato_apelido_trgm ON candidato USING GIN (apelido gin_trgm_ops);
CREATE INDEX idx_candidato_cpf_trgm     ON candidato USING GIN (cpf     gin_trgm_ops);