import { BaseApiClient } from './base';
import { API_CONFIG } from '../config/api';
import type { CandidateAssets, CandidateDetails, CandidateExpenses, CandidateIncome, CandidateRedesSociais, CandidateSearchResult, CpfRevealResult, OpenCandDataAvailabilityStats, OpenCandDatabaseStats, PlatformStats, RandomCandidate } from './apiModels';
import type { EnrichmentResponse, StatisticsConfig, ValueSumRequest, ValueSumResponse } from './apiStatisticsModels';
import type { StatisticsRequestFilters, StatisticsRequestOptions } from '../components/StatisticsPage/statisticsRequests';

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
  async searchCandidates(query: string): Promise<CandidateSearchResult> {
    const encodedQuery = encodeURIComponent(query);
    return this.get<CandidateSearchResult>(`/v1/candidato/search?q=${encodedQuery}`);
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
  async getCandidateById(id: string): Promise<CandidateDetails> {
    return this.get<CandidateDetails>(`/v1/candidato/${id}`);
  }

  /**
   * Get the assets of a specific candidate by ID
   */
  async getCandidateAssets(id: string): Promise<CandidateAssets> {
    return this.get<CandidateAssets>(`/v1/candidato/${id}/bens`);
  }

  /**
   * Get the social networks of a specific candidate by ID
   */
  async getCandidateRedesSociais(id: string): Promise<CandidateRedesSociais> {
    return this.get<CandidateRedesSociais>(`/v1/candidato/${id}/rede-social`);
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
  async getCandidateDepesas(id: string): Promise<CandidateExpenses> {
    return this.get<CandidateExpenses>(`/v1/candidato/${id}/despesas`);
  }

  /**
   * Get the expenses for a specific candidate by ID and year
   */
  async getCandidateReceitas(id: string): Promise<CandidateIncome> {
    return this.get<CandidateIncome>(`/v1/candidato/${id}/receitas`);
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
  async getStatisticsEnrichment(filters?: StatisticsRequestFilters): Promise<EnrichmentResponse[]> {
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
    return this.get<EnrichmentResponse[]>(url, { timeout: 90000 });
  }

  /**
   * Get the sum of values for a specific type and grouping
   */
  async getStatisticsValueSum(request: ValueSumRequest): Promise<ValueSumResponse> {
    return this.post<ValueSumResponse>(`/v1/estatistica/values-sum`, request, { timeout: 90000 });
  }
}

// Create a default instance for easy usage with proper configuration
export const openCandApi = new OpenCandApi(API_CONFIG.baseUrl);

// Export the API error for error handling
export { ApiError } from './base';
