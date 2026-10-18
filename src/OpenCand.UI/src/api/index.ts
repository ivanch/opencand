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
  YearSummary,
  FinanceSummary,
} from './apiModels';

// Export base API classes for custom implementations
export { BaseApiClient, ApiError, RequestCancelledError, isRequestCancelled } from './base';
export type { ApiRequestConfig } from './base';
