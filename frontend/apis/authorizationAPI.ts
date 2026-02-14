import { coreAxiosInstance } from "./coreAxiosConfig";

export type LoginResponse = {
  access: string;
  refresh: string;
  org_roles: Record<string, string>;
};

/**
 * Authenticate with GIRAF Core API.
 * @param username - The username of the user.
 * @param password - The password of the user.
 */
export async function tryLogin(username: string, password: string): Promise<LoginResponse> {
  return coreAxiosInstance
    .post<LoginResponse>(`/token/pair`, { username, password })
    .then((res) => res.data)
    .catch(() => {
      throw new Error("Fejl: Der opstod et problem med login");
    });
}
