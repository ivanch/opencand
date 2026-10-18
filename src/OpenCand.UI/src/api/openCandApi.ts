import { BaseApiClient } from './base';
import { API_CONFIG } from '../config/api';
import type { CandidateAssets, CandidateDetails, CandidateExpenses, CandidateIncome, FinanceSummary, CandidateRedesSociais, CandidateSearchResult, CpfRevealResult, OpenCandDataAvailabilityStats, OpenCandDatabaseStats, PlatformStats, RandomCandidate } from './apiModels';
import type { EnrichmentResponse, StatisticsConfig, ValueSumRequest, ValueSumResponse } from './apiStatisticsModels';
import type { StatisticsRequestFilters } from '../components/StatisticsPage/statisticsRequests';

/**
 * OpenCand API client for interacting with the OpenCand platform
 */
export class OpenCandApi extends BaseApiClient {
  constructor(baseUrl: string = '') {
    super(baseUrl);
  }

  /**
   * Get platform statistics
   * GET /v1/stats
   */
  async getStats(): Promise<PlatformStats> {
    return this.get<PlatformStats>('/v1/stats');
  }

  /**
   * Get platform data stats
   * GET /v1/stats
   */
  async getDataAvailabilityStats(): Promise<OpenCandDataAvailabilityStats> {
    return this.get<OpenCandDataAvailabilityStats>('/v1/stats/data-availability');
  }

  /**
   * Get the database tech stats
   */
  async getDatabaseTechStats(): Promise<OpenCandDatabaseStats> {
    return this.get<OpenCandDatabaseStats>(`/v1/stats/tech`);
  }

  /**
   * Search for candidates by name or other attributes
   */
  async searchCandidates(query: string, signal?: AbortSignal): Promise<CandidateSearchResult> {
    const encodedQuery = encodeURIComponent(query);
    return this.get<CandidateSearchResult>(`/v1/candidato/search?q=${encodedQuery}`, { signal });
  }

  /**
   * Get a random candidate from the database
   */
  async getRandomCandidate(): Promise<RandomCandidate> {
    return this.get<RandomCandidate>(`/v1/candidato/random`);
  }

  /**
   * Get detailed information about a specific candidate by ID
   */
  async getCandidateById(id: string, signal?: AbortSignal): Promise<CandidateDetails> {
    return this.get<CandidateDetails>(`/v1/candidato/${id}`, { signal });
  }

  /**
   * Get the assets of a specific candidate by ID
   */
  async getCandidateAssets(id: string, signal?: AbortSignal): Promise<CandidateAssets> {
    return this.get<CandidateAssets>(`/v1/candidato/${id}/bens`, { signal });
  }

  /**
   * Get the social networks of a specific candidate by ID
   */
  async getCandidateRedesSociais(id: string, signal?: AbortSignal): Promise<CandidateRedesSociais> {
    return this.get<CandidateRedesSociais>(`/v1/candidato/${id}/rede-social`, { signal });
  }

  /**
   * Get the social networks of a specific candidate by ID
   */
  async getCandidateCpf(id: string): Promise<CpfRevealResult> {
    return this.get<CpfRevealResult>(`/v1/candidato/${id}/reveal-cpf`);
  }

  /**
   * Get the expenses for a specific candidate by ID and year
   */
  async getCandidateDepesas(id: string, signal?: AbortSignal): Promise<CandidateExpenses> {
    return this.get<CandidateExpenses>(`/v1/candidato/${id}/despesas`, { signal });
  }

  /**
   * Get the expenses for a specific candidate by ID and year
   */
  async getCandidateReceitas(id: string, signal?: AbortSignal): Promise<CandidateIncome> {
    return this.get<CandidateIncome>(`/v1/candidato/${id}/receitas`, { signal });
  }

  /**
   * Get the totals of the expenses of a candidate, per election year
   */
  async getCandidateDespesasResumo(id: string, signal?: AbortSignal): Promise<FinanceSummary> {
    return this.get<FinanceSummary>(`/v1/candidato/${id}/despesas/resumo`, { signal });
  }

  /**
   * Get the totals of the income of a candidate, per election year
   */
  async getCandidateReceitasResumo(id: string, signal?: AbortSignal): Promise<FinanceSummary> {
    return this.get<FinanceSummary>(`/v1/candidato/${id}/receitas/resumo`, { signal });
  }

  /**
   * Get one page of the expenses of a candidate in an election year
   */
  async getCandidateDespesasPage(id: string, ano: number, page: number, pageSize: number, signal?: AbortSignal): Promise<CandidateExpenses> {
    return this.get<CandidateExpenses>(`/v1/candidato/${id}/despesas?ano=${ano}&page=${page}&pageSize=${pageSize}`, { signal });
  }

  /**
   * Get one page of the income of a candidate in an election year
   */
  async getCandidateReceitasPage(id: string, ano: number, page: number, pageSize: number, signal?: AbortSignal): Promise<CandidateIncome> {
    return this.get<CandidateIncome>(`/v1/candidato/${id}/receitas?ano=${ano}&page=${page}&pageSize=${pageSize}`, { signal });
  }

  /**
   * Get the configuration for statistics filters
   */
  async getStatisticsConfig(): Promise<StatisticsConfig> {
    return this.get<StatisticsConfig>(`/v1/estatistica/configuration`);
  }

  /**
   * Get the enrichment statistics for candidates
   */
  async getStatisticsEnrichment(filters?: StatisticsRequestFilters, signal?: AbortSignal): Promise<EnrichmentResponse[]> {
    let url = `/v1/estatistica/enriquecimento`;
    if (filters) {
      const params = new URLSearchParams();
      if (filters.partido !== null && filters.partido !== undefined) {
        params.append('partido', filters.partido);
      }
      if (filters.uf !== null && filters.uf !== undefined) {
        params.append('uf', filters.uf);
      }
      if (filters.ano !== null && filters.ano !== undefined) {
        params.append('ano', String(filters.ano));
      }
      if (filters.cargo !== null && filters.cargo !== undefined) {
        params.append('cargo', filters.cargo);
      }
      const queryString = params.toString();
      if (queryString) {
        url += `?${queryString}`;
      }
    }
    return this.get<EnrichmentResponse[]>(url, { timeout: 90000, signal });
  }

  /**
   * Get the sum of values for a specific type and grouping
   */
  async getStatisticsValueSum(request: ValueSumRequest, signal?: AbortSignal): Promise<ValueSumResponse> {
    return this.post<ValueSumResponse>(`/v1/estatistica/values-sum`, request, { timeout: 90000, signal });
  }
}

// Create a default instance for easy usage with proper configuration
export const openCandApi = new OpenCandApi(API_CONFIG.baseUrl);

// Export the API error for error handling
export { ApiError, isRequestCancelled } from './base';
