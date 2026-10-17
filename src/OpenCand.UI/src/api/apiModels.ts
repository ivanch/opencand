// Type definitions based on the API specs
export interface CandidateSearchResult {
  candidatos: Candidate[];
}

export interface RandomCandidate {
  idCandidato: string;
}

export interface Candidate {
  idCandidato: string;
  nome: string;
  cpf: string;
  dataNascimento: string;
  sexo: string;
  apelido: string;
  fotoUrl: string;
  ultimoano: number;
  localidade: string;
}

export interface CandidateDetails extends Candidate {
  eleicoes: Election[];
  candidatoExt: CandidateExt[];
}

export interface Election {
  sqCandidato: string;
  tipoEleicao: string;
  cargo: string;
  ano: number;
  siglaUF: string;
  nomeUE: string;
  nrCandidato: string;
  nomeCandidato: string;
  resultado: string;
  partido: Partido;
}

export interface CandidateExt {
  ano: number;
  apelido: string;
  email: string;
  estadoCivil: string;
  escolaridade: string;
  ocupacao: string;
}

export interface CandidateAssets {
  bens: Asset[];
}

export interface Asset {
  idCandidato: string;
  ano: number;
  tipoBem: string;
  descricao: string;
  valor: number;
}

export interface CandidateRedesSociais {
  redesSociais: RedeSocial[];
}

export interface RedeSocial {
  idCandidato: string;
  rede: "Facebook" | "Instagram" | "X/Twitter" | "TikTok" | "YouTube" | "LinkedIn" | "WhatsApp" | "Threads" | "Telegram" | "Kwai" | "Spotify" | "Outros";
  siglaUF: string;
  link: string;
  ano: number;
}

export interface PlatformStats {
  totalCandidatos: number;
  totalBemCandidatos: number;
  totalValorBemCandidatos: number;
  totalRedesSociais: number;
  totalEleicoes: number;
}

export interface CpfRevealResult {
    cpf: string;
}

export interface Partido {
  nome: string;
  sigla: string;
  numero: number;
}

export interface CandidateExpenses {
  despesas: Expense[];
}

export interface Expense {
  idDespesa: string;
  idCandidato: string;
  ano: number;
  turno: number;
  sqCandidato: string;
  sgPartido: string;
  tipoFornecedor: string;
  cpfFornecedor: string;
  cnpjFornecedor: string;
  nomeFornecedor: string;
  nomeFornecedorRFB: string;
  municipioFornecedor: string;
  tipoDocumento: string;
  dataDespesa: string; // ISO date
  descricao: string;
  origemDespesa: string;
  valor: number;
}

export interface CandidateIncome {
  receitas: Income[];
}

export interface Income {
  idReceita: string;
  idCandidato: string;
  ano: number;
  turno: number;
  sqCandidato: string;
  sgPartido: string;
  fonteReceita: string;
  origemReceita: string;
  naturezaReceita: string;
  especieReceita: string;
  cpfDoador: string;
  cnpjDoador: string;
  nomeDoador: string;
  nomeDoadorRFB: string;
  municipioDoador: string;
  dataReceita: string; // ISO date
  descricao: string;
  valor: number;
}

export interface OpenCandDataAvailabilityStats {
  candidatos: number[];
  bemCandidatos: number[];
  despesaCandidatos: number[];
  receitaCandidatos: number[];
  redeSocialCandidatos: number[];
  fotosCandidatos: number[];
}

export interface OpenCandDatabaseStats {
  tables: {
    name: string;
    totalSize: number; // in bytes
    entries: number; // number of rows
  }[];
  materializedViews: {
    name: string;
    totalSize: number; // in bytes
    entries: number; // number of rows
  }[];
  indexes: {
    amount: number; // number of indexes
    size: number; // total size of indexes in bytes
  };
  totalSize: number; // total size of the database in bytes
  totalEntries: number; // total number of entries across all tables
}