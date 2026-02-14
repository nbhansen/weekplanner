import axios from "axios";
import { CORE_BASE_URL } from "../utils/globals";

export const coreAxiosInstance = axios.create({
  baseURL: CORE_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

export const setCoreBearer = (token: string) => {
  coreAxiosInstance.defaults.headers.common["Authorization"] = `Bearer ${token}`;
};
