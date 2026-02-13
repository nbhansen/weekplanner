import { coreAxiosInstance } from "./coreAxiosConfig";

type CreateUserRequestProps = {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
};

type CoreRegisterResponse = {
  id: number;
  username: string;
  email: string;
  first_name: string;
  last_name: string;
  display_name: string;
  is_active: boolean;
};

/**
 * Register a new user via GIRAF Core API.
 * Core uses `username` as the login identifier — we map `email` to that.
 */
export const createUserRequest = (userData: CreateUserRequestProps): Promise<{ id: string }> => {
  return coreAxiosInstance
    .post<CoreRegisterResponse>(`/auth/register`, {
      username: userData.email,
      password: userData.password,
      email: userData.email,
      first_name: userData.firstName,
      last_name: userData.lastName,
    })
    .then((res) => ({ id: String(res.data.id) }))
    .catch((error) => {
      const detail = error.response?.data?.detail;
      const message = Array.isArray(detail) ? detail.join(", ") : detail || "Fejl: Kunne ikke oprette bruger";
      throw new Error(message);
    });
};
