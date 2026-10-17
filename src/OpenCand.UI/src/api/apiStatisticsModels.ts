// Type definitions based on the API specs for statistics endpoints

export interface StatisticsConfig {
  partidos: string[];
  siglasUF: string[];
  anos: number[];
  cargos: string[];
}


export interface EnrichmentResponse {
  idCandidato: string;
  nome: string;
  patrimonioInicial: number;
  anoInicial: number;
  patrimonioFinal: number;
  anoFinal: number;
  enriquecimento: number;
}

export interface ValueSumRequest {
  type: 'bem' | 'despesa' | 'receita';
  groupBy: 'candidato' | 'partido' | 'uf' | 'cargo';
  filter?: {
    partido?: string | null; // Optional, can be null
    uf?: string | null; // Optional, can be null
    ano?: number | null; // Optional, can be null
    cargo?: CargoFilter; // Optional, can be null
  }
}

export type CargoFilter =
  | '1º SUPLENTE SENADOR'
  | 'VICE-GOVERNADOR'
  | '2º SUPLENTE'
  | 'PRESIDENTE'
  | 'DEPUTADO DISTRITAL'
  | 'PREFEITO'
  | 'VICE-PRESIDENTE'
  | '2º SUPLENTE SENADOR'
  | 'SENADOR'
  | 'DEPUTADO ESTADUAL'
  | '1º SUPLENTE'
  | 'GOVERNADOR'
  | 'VICE-PREFEITO'
  | 'VEREADOR'
  | 'DEPUTADO FEDERAL'
  | null;

export interface ValueSumResponse {
  idCandidato?: string;
  sgpartido?: string;
  siglaUf?: string;
  cargo?: string;
  nome?: string;
  ano: number;
  valor: number;
}