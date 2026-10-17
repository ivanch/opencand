// Export the main API client and types
export { OpenCandApi, openCandApi } from './openCandApi';
export type {
  CandidateSearchResult,
  Candidate,
  CandidateDetails,
  Election,
  CandidateAssets,
  Asset,
  CandidateRedesSociais,
  RedeSocial,
  PlatformStats,
  CpfRevealResult,
  Partido,
  CandidateExpenses,
  Expense,
  CandidateIncome,
  Income,
} from './apiModels';

// Export base API classes for custom implementations
export { BaseApiClient, ApiError } from './base';
export type { ApiRequestConfig } from './base';
