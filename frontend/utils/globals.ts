export const PORT = process.env.EXPO_PUBLIC_API_PORT;
export const URL = process.env.EXPO_PUBLIC_API_URL;

export const BASE_URL = `${URL}:${PORT}`;

export const CORE_PORT = process.env.EXPO_PUBLIC_CORE_API_PORT;
export const CORE_URL = process.env.EXPO_PUBLIC_CORE_API_URL;

export const CORE_BASE_URL = `${CORE_URL}:${CORE_PORT}/api/v1`;
export const CORE_HOST = `${CORE_URL}:${CORE_PORT}`;

export function pictogramImageUrl(url: string | undefined): string {
  if (!url) return "";
  if (url.startsWith("http")) return url;
  return `${CORE_HOST}/${url}`;
}
