const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7001/api";

type HttpMethod = "GET" | "POST" | "PUT" | "PATCH" | "DELETE";

interface RequestOptions {
  method?: HttpMethod;
  body?: unknown;
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
  const body = options.body;

  let requestBody: BodyInit | null | undefined;
  let headers: HeadersInit | undefined;

  if (body === undefined || body === null) {
    requestBody = undefined;
    headers = undefined;
  } else if (body instanceof FormData) {
    requestBody = body;
    headers = undefined;
  } else {
    requestBody = JSON.stringify(body);
    headers = {
      "Content-Type": "application/json",
    };
  }

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: requestBody,
  });

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