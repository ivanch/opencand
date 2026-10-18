
import { openCandApi } from '../../api/openCandApi';
import { isRequestCancelled } from '../../api/base';
import type { EnrichmentResponse, ValueSumRequest, ValueSumResponse, CargoFilter } from '../../api/apiStatisticsModels';

// Statistics data interfaces
export interface StatisticsData {
  // First Row
  candidatesWithMostAssets: ValueSumResponse[];
  enrichmentData: EnrichmentResponse[] | null;
  
  // Second Row
  candidatesWithMostRevenue: ValueSumResponse[];
  candidatesWithMostExpenses: ValueSumResponse[];
  
  // Third Row
  partiesWithMostAssets: ValueSumResponse[];
  partiesWithMostExpenses: ValueSumResponse[];
  partiesWithMostRevenue: ValueSumResponse[];
  
  // Fourth Row
  statesWithMostAssets: ValueSumResponse[];
  statesWithMostExpenses: ValueSumResponse[];
  statesWithMostRevenue: ValueSumResponse[];
}

export interface StatisticsRequestOptions {
  filters?: StatisticsRequestFilters;
  /** Cancels every request of the batch when aborted. */
  signal?: AbortSignal;
}

export interface StatisticsRequestFilters {
  partido?: string | null;
  uf?: string | null;
  ano?: number | null;
  cargo?: CargoFilter;
}

// First Row Requests
export async function getCandidatesWithMostAssets(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "bem",
    groupBy: "candidato",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getEnrichmentData(filters?: StatisticsRequestFilters, signal?: AbortSignal): Promise<EnrichmentResponse[] | null> {
  try {
    return await openCandApi.getStatisticsEnrichment(filters, signal);
  } catch (error) {
    if (isRequestCancelled(error)) throw error;
    console.error('Error fetching enrichment data:', error);
    return null;
  }
}

// Second Row Requests
export async function getCandidatesWithMostRevenue(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "receita",
    groupBy: "candidato",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getCandidatesWithMostExpenses(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "despesa",
    groupBy: "candidato",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

// Third Row Requests
export async function getPartiesWithMostAssets(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "bem",
    groupBy: "partido",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getPartiesWithMostExpenses(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "despesa",
    groupBy: "partido",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getPartiesWithMostRevenue(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "receita",
    groupBy: "partido",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

// Fourth Row Requests
export async function getStatesWithMostAssets(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "bem",
    groupBy: "uf",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getStatesWithMostExpenses(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "despesa",
    groupBy: "uf",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

export async function getStatesWithMostRevenue(options?: StatisticsRequestOptions): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type: "receita",
    groupBy: "uf",
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}

// Main function to fetch all statistics data
export async function fetchAllStatisticsData(options?: StatisticsRequestOptions): Promise<StatisticsData> {
  try {
    const [
      // First Row
      candidatesWithMostAssets,
      enrichmentData,
      
      // Second Row
      candidatesWithMostRevenue,
      candidatesWithMostExpenses,
      
      // Third Row
      partiesWithMostAssets,
      partiesWithMostExpenses,
      partiesWithMostRevenue,
      
      // Fourth Row
      statesWithMostAssets,
      statesWithMostExpenses,
      statesWithMostRevenue
    ] = await Promise.all([
      getCandidatesWithMostAssets(options),
      getEnrichmentData(options?.filters, options?.signal),
      getCandidatesWithMostRevenue(options),
      getCandidatesWithMostExpenses(options),
      getPartiesWithMostAssets(options),
      getPartiesWithMostExpenses(options),
      getPartiesWithMostRevenue(options),
      getStatesWithMostAssets(options),
      getStatesWithMostExpenses(options),
      getStatesWithMostRevenue(options)
    ]);

    return {
      candidatesWithMostAssets,
      enrichmentData,
      candidatesWithMostRevenue,
      candidatesWithMostExpenses,
      partiesWithMostAssets,
      partiesWithMostExpenses,
      partiesWithMostRevenue,
      statesWithMostAssets,
      statesWithMostExpenses,
      statesWithMostRevenue
    };
  } catch (error) {
    if (!isRequestCancelled(error)) {
      console.error('Error fetching statistics data:', error);
    }
    throw error;
  }
}

// Helper function to fetch data for specific category
export async function fetchStatisticsByCategory(
  type: 'bem' | 'despesa' | 'receita',
  groupBy: 'candidato' | 'partido' | 'uf' | 'cargo',
  options?: StatisticsRequestOptions
): Promise<ValueSumResponse[]> {
  const request: ValueSumRequest = {
    type,
    groupBy,
    filter: options?.filters
  };
  
  const response = await openCandApi.getStatisticsValueSum(request, options?.signal);
  return Array.isArray(response) ? response : [response];
}