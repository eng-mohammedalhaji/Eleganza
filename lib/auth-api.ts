import { apiRequest } from "@/lib/api-client";

export type Credentials = {
  email: string;
  password: string;
};

export async function register(credentials: Credentials) {
  return apiRequest<void>("/api/auth/register", {
    method: "POST",
    body: JSON.stringify(credentials),
  });
}

export async function login(credentials: Credentials) {
  return apiRequest<void>("/api/auth/login?useCookies=true", {
    method: "POST",
    body: JSON.stringify(credentials),
  });
}

export async function logout() {
  return apiRequest<void>("/api/auth/logout", {
    method: "POST",
    body: JSON.stringify({}),
  });
}
