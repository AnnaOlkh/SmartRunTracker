import {
  clearTokens,
  getAccessToken,
  setAccessToken,
} from "../auth/tokenStorage";

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7001/api";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

interface RequestOptions {
  method?: HttpMethod;
  body?: unknown;
  skipAuth?: boolean;
  skipRefresh?: boolean;
}

interface RefreshResponse {
  accessToken: string;
}

export class ApiError extends Error {
  public readonly status: number;
  public readonly payload: unknown;

  public constructor(message: string, status: number, payload: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

export async function apiRequest<T>(
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const response = await sendRequest(path, options);

  if (response.status === 401 && !options.skipRefresh) {
    const refreshed = await tryRefreshToken();

    if (refreshed) {
      const retryResponse = await sendRequest(path, {
        ...options,
        skipRefresh: true,
      });

      return handleResponse<T>(retryResponse);
    }

    clearTokens();
  }

  return handleResponse<T>(response);
}

async function sendRequest(
  path: string,
  options: RequestOptions,
): Promise<Response> {
  const body = options.body;

  let requestBody: BodyInit | null | undefined;
  let headers: HeadersInit = {};

  if (body === undefined || body === null) {
    requestBody = undefined;
  } else if (body instanceof FormData) {
    requestBody = body;
  } else {
    requestBody = JSON.stringify(body);
    headers = {
      ...headers,
      "Content-Type": "application/json",
    };
  }

  if (!options.skipAuth) {
    const accessToken = getAccessToken();

    if (accessToken) {
      headers = {
        ...headers,
        Authorization: `Bearer ${accessToken}`,
      };
    }
  }

  return fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: requestBody,
    credentials: "include",
  });
}

async function handleResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const payload = await readJsonSafely(response);
    const message = extractErrorMessage(payload, response.statusText);

    throw new ApiError(message, response.status, payload);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

async function tryRefreshToken(): Promise<boolean> {
  const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
    method: "POST",
    credentials: "include",
  });

  if (!response.ok) {
    return false;
  }

  const data = (await response.json()) as RefreshResponse;

  setAccessToken(data.accessToken);

  return true;
}

async function readJsonSafely(response: Response): Promise<unknown> {
  try {
    return await response.json();
  } catch {
    return null;
  }
}

function extractErrorMessage(payload: unknown, fallback: string): string {
  if (
    payload !== null &&
    typeof payload === "object" &&
    "message" in payload &&
    typeof payload.message === "string"
  ) {
    return payload.message;
  }

  return fallback || "Request failed.";
}