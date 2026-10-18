/**
 * Base API client class for handling HTTP requests and common functionality
 */
export class ApiError extends Error {
  public status: number;
  public response?: Response;

  constructor(
    message: string,
    status: number,
    response?: Response
  ) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.response = response;
  }
}

/**
 * Thrown when a request is cancelled by the caller through its AbortSignal.
 * Callers should swallow it (see isRequestCancelled): it is not a failure to show to the user.
 */
export class RequestCancelledError extends Error {
  constructor() {
    super('Request cancelled');
    this.name = 'RequestCancelledError';
  }
}

export const isRequestCancelled = (error: unknown): boolean =>
  error instanceof RequestCancelledError ||
  (error instanceof Error && error.name === 'AbortError');

export interface ApiRequestConfig {
  headers?: Record<string, string>;
  timeout?: number;
  /** Aborts the request when triggered; the promise rejects with RequestCancelledError. */
  signal?: AbortSignal;
}

export class BaseApiClient {
  private baseUrl: string;
  private defaultHeaders: Record<string, string>;
  private timeout: number;

  constructor(baseUrl: string, defaultHeaders: Record<string, string> = {}) {
    this.baseUrl = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
    // Content-Type is only sent on requests with a body: setting it on GETs forces a CORS preflight.
    this.defaultHeaders = { ...defaultHeaders };
    this.timeout = 30000; // 30 seconds default timeout
  }

  /**
   * Set the base URL for API requests
   */
  setBaseUrl(url: string): void {
    this.baseUrl = url.endsWith('/') ? url.slice(0, -1) : url;
  }

  /**
   * Set default headers for all requests
   */
  setDefaultHeaders(headers: Record<string, string>): void {
    this.defaultHeaders = { ...this.defaultHeaders, ...headers };
  }

  /**
   * Set request timeout
   */
  setTimeout(timeout: number): void {
    this.timeout = timeout;
  }

  /**
   * Build complete URL with base URL and endpoint
   */
  private buildUrl(endpoint: string): string {
    const cleanEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
    return `${this.baseUrl}${cleanEndpoint}`;
  }

  /**
   * Handle API errors and create appropriate error objects
   */
  private async handleApiError(response: Response): Promise<never> {
    let errorMessage = `HTTP ${response.status}: ${response.statusText}`;
    
    try {
      const errorData = await response.json();
      if (errorData.message) {
        errorMessage = errorData.message;
      } else if (errorData.detail || errorData.title) {
        // ASP.NET ProblemDetails
        errorMessage = errorData.detail || errorData.title;
      } else if (errorData.error) {
        errorMessage = errorData.error;
      }
    } catch {
      // If we can't parse the error response, use the default message
    }

    throw new ApiError(errorMessage, response.status, response);
  }

  /**
   * Make a GET request
   */
  async get<T>(endpoint: string, config: ApiRequestConfig = {}): Promise<T> {
    return this.request<T>('GET', endpoint, undefined, config);
  }

  /**
   * Make a POST request
   */
  async post<T>(
    endpoint: string,
    data?: unknown,
    config: ApiRequestConfig = {}
  ): Promise<T> {
    return this.request<T>('POST', endpoint, data, config);
  }

  /**
   * Make a PUT request
   */
  async put<T>(
    endpoint: string,
    data?: unknown,
    config: ApiRequestConfig = {}
  ): Promise<T> {
    return this.request<T>('PUT', endpoint, data, config);
  }

  /**
   * Make a DELETE request
   */
  async delete<T>(endpoint: string, config: ApiRequestConfig = {}): Promise<T> {
    return this.request<T>('DELETE', endpoint, undefined, config);
  }

  /**
   * Make a generic HTTP request
   */
  private async request<T>(
    method: string,
    endpoint: string,
    data?: unknown,
    config: ApiRequestConfig = {}
  ): Promise<T> {
    const url = this.buildUrl(endpoint);
    const headers: Record<string, string> = {
      ...this.defaultHeaders,
      ...(data !== undefined ? { 'Content-Type': 'application/json' } : {}),
      ...config.headers,
    };
    const timeout = config.timeout || this.timeout;

    const requestInit: RequestInit = {
      method,
      headers,
    };

    if (data !== undefined) {
      requestInit.body = JSON.stringify(data);
    }

    if (config.signal?.aborted) {
      throw new RequestCancelledError();
    }

    // The controller is aborted either by the timeout or by the caller's signal
    const controller = new AbortController();
    const timeoutId = window.setTimeout(() => controller.abort(), timeout);
    const onCallerAbort = () => controller.abort();
    config.signal?.addEventListener('abort', onCallerAbort, { once: true });

    try {
      const response = await fetch(url, {
        ...requestInit,
        signal: controller.signal,
      });

      clearTimeout(timeoutId);

      if (!response.ok) {
        await this.handleApiError(response);
      }

      // Handle empty responses (like 204 No Content)
      if (response.status === 204 || response.headers.get('content-length') === '0') {
        return {} as T;
      }

      const responseData = await response.json();
      return responseData as T;
    } catch (error) {
      clearTimeout(timeoutId);

      if (error instanceof ApiError) {
        throw error;
      }

      if (config.signal?.aborted) {
        throw new RequestCancelledError();
      }

      if (error instanceof Error && error.name === 'AbortError') {
        throw new ApiError('Request timeout', 408);
      }

      if (error instanceof Error) {
        throw new ApiError(`Network error: ${error.message}`, 0);
      }

      throw new ApiError('Unknown error occurred', 0);
    } finally {
      config.signal?.removeEventListener('abort', onCallerAbort);
    }
  }
}
