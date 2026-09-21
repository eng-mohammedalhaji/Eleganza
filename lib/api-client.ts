export class ApiClientError extends Error {
  constructor(
    message: string,
    public readonly status: number,
    public readonly code?: string,
    public readonly traceId?: string,
  ) {
    super(message);
  }
}

export function getApiBaseUrl() {
  return process.env.NEXT_PUBLIC_API_URL?.replace(/\/$/, "") || null;
}

export async function apiRequest<T>(path: string, init: RequestInit = {}): Promise<T> {
  const baseUrl = getApiBaseUrl();
  if (!baseUrl) {
    throw new ApiClientError("API URL is not configured.", 0);
  }

  const response = await fetch(`${baseUrl}${path}`, {
    ...init,
    credentials: "include",
    headers: {
      ...(init.body ? { "Content-Type": "application/json" } : {}),
      ...init.headers,
    },
  });

  if (!response.ok) {
    const body = (await response.json().catch(() => null)) as
      | { detail?: string; title?: string; code?: string; traceId?: string; errors?: Record<string, string[]> }
      | null;
    const validationMessage = body?.errors
      ? Object.values(body.errors).flat().join(" ")
      : undefined;
    throw new ApiClientError(
      validationMessage || body?.detail || body?.title || "تعذر تنفيذ العملية.",
      response.status,
      body?.code,
      body?.traceId,
    );
  }

  if (response.status === 204) return undefined as T;
  return (await response.json()) as T;
}
