DROP TABLE IF EXISTS bem_candidato CASCADE;
DROP TABLE IF EXISTS candidato_mapping CASCADE;
DROP TABLE IF EXISTS rede_social CASCADE;
DROP TABLE IF EXISTS candidato CASCADE;

CREATE TABLE candidato (
    idcandidato UUID NOT NULL PRIMARY KEY,
    cpf VARCHAR(11),
    nome VARCHAR(255) NOT NULL,
    datanascimento TIMESTAMPTZ,
    email TEXT,
    sexo CHAR(15),
    estadocivil VARCHAR(50),
    escolaridade VARCHAR(50),
    ocupacao VARCHAR(150)
);
CREATE INDEX idx_candidato_nome ON candidato (nome);

-- Each candidato (idcandidato, cpf, nome) will be mapped to a (sqcandidato, ano, tipo_eleicao, sg_uf, cargo, resultado) 
CREATE TABLE candidato_mapping (
    idcandidato UUID NOT NULL,
    cpf VARCHAR(11),
    nome VARCHAR(255) NOT NULL,
    sqcandidato TEXT,
    ano INT NOT NULL,
    tipoeleicao VARCHAR(50),
    siglauf VARCHAR(2),
    nomeue VARCHAR(100),
    cargo VARCHAR(50),
    nrcandidato VARCHAR(20),
    resultado VARCHAR(50),
    CONSTRAINT pk_candidato_mapping PRIMARY KEY (idcandidato, ano, siglauf, nomeue, cargo, nrcandidato, resultado),
    CONSTRAINT fk_candidato_mapping_candidato FOREIGN KEY (idcandidato) REFERENCES candidato(idcandidato) ON DELETE CASCADE ON UPDATE CASCADE
);
CREATE INDEX idx_candidato_mapping_cpf ON candidato_mapping (cpf);
CREATE INDEX idx_candidato_mapping_nome ON candidato_mapping (nome);
CREATE INDEX idx_candidato_mapping_ano ON candidato_mapping (ano);
CREATE INDEX idx_candidato_mapping_sqcandidato ON candidato_mapping (sqcandidato);

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