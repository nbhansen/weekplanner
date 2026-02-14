import { act, renderHook, waitFor } from "@testing-library/react-native";
import { tryLogin } from "../apis/authorizationAPI";
import { useToast } from "../providers/ToastProvider";
import AuthenticationProvider, { useAuthentication } from "../providers/AuthenticationProvider";
import { router } from "expo-router";
import { setBearer } from "../apis/axiosConfig";
import { setCoreBearer } from "../apis/coreAxiosConfig";

jest.mock("../apis/registerAPI");
jest.mock("../apis/authorizationAPI");
jest.mock("../apis/axiosConfig", () => ({
  setBearer: jest.fn(),
}));
jest.mock("../apis/coreAxiosConfig", () => ({
  setCoreBearer: jest.fn(),
}));
jest.mock("expo-router", () => ({
  router: {
    replace: jest.fn(),
  },
}));

jest.mock("../providers/ToastProvider", () => ({
  useToast: jest.fn(),
}));

jest.mock("@react-native-async-storage/async-storage", () =>
  require("@react-native-async-storage/async-storage/jest/async-storage-mock")
);

jest.mock("../utils/jwtDecode", () => ({
  getUserIdFromToken: jest.fn((token: string) => {
    try {
      const payload = JSON.parse(atob(token.split(".")[1]));
      return payload.sub;
    } catch {
      return null;
    }
  }),
  isTokenExpired: jest.fn(() => false),
}));

describe("AuthenticationProvider and useAuthentication", () => {
  const addToast = jest.fn();

  beforeEach(() => {
    (useToast as jest.Mock).mockReturnValue({ addToast });
    jest.clearAllMocks();
  });

  it("should login and store a jwt token", async () => {
    const mockToken = "mockToken";
    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: mockToken,
      refresh: "mockRefreshToken",
      org_roles: { "1": "owner" }
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(result.current.jwt).toBe(mockToken);
    });
  });

  it("should add a toast if login fails", async () => {
    const error = new Error("Login failed");
    (tryLogin as jest.Mock).mockRejectedValueOnce(error);

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(addToast).toHaveBeenCalledWith({
        message: error.message,
        type: "error",
      });
    });

    expect(router.replace).not.toHaveBeenCalled();
  });

  it("should add a toast if no token is returned on login", async () => {
    (tryLogin as jest.Mock).mockResolvedValueOnce({ access: null }); // No access token

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(addToast).toHaveBeenCalledWith({
        message: "Token er ikke blevet modtaget",
        type: "error",
      });
    });

    expect(router.replace).not.toHaveBeenCalled();
  });

  it("should add a toast if register fails", async () => {
    const error = new Error("Register failed");
    jest.spyOn(require("../apis/registerAPI"), "createUserRequest").mockRejectedValueOnce(error);

    const mockAddToast = jest.fn();
    jest.spyOn(require("../providers/ToastProvider"), "useToast").mockReturnValue({
      addToast: mockAddToast,
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    await act(async () => {
      await result.current.register({
        email: "Test@gmail.com",
        password: "TestTest1",
        firstName: "Test",
        lastName: "Test",
        confirmPassword: "TestTest1",
      });
    });

    await waitFor(() => {
      expect(mockAddToast).toHaveBeenCalledWith({
        message: error.message,
        type: "error",
      });
    });

    expect(router.replace).not.toHaveBeenCalled();
  });

  it("should return false for isAuthenticated if jwt is not set", async () => {
    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    expect(result.current.isAuthenticated()).toBe(false);
  });

  it("should return false for isAuthenticated if jwt is expired", async () => {
    jest.spyOn(require("../utils/jwtDecode"), "isTokenExpired").mockReturnValueOnce(true);

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    await act(async () => {
      result.current.login("test@test.dk", "testTest1");
    });

    expect(result.current.isAuthenticated()).toBe(false);
  });

  it("should have initial jwt as null and isAuthenticated should return false", async () => {
    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    expect(result.current.jwt).toBeNull();
    expect(result.current.isAuthenticated()).toBe(false);
  });

  it("should throw error if used outside of provider", async () => {
    const consoleErrorMock = jest.spyOn(console, "error").mockImplementation(() => {});
    try {
      renderHook(() => useAuthentication());
    } catch (error) {
      expect(error).toEqual(new Error("useAuthentication skal bruges i en AuthenticationProvider"));
    }
    consoleErrorMock.mockRestore();
  });

  // Core Auth Format Tests
  it("should store org_roles from Core login response", async () => {
    const mockOrgRoles = { "1": "owner", "5": "member", "10": "admin" };
    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: "mockAccessToken",
      refresh: "mockRefreshToken",
      org_roles: mockOrgRoles,
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(result.current.orgRoles).toEqual(mockOrgRoles);
    });
  });

  it("should set bearer on both weekplanner and Core axios instances", async () => {
    const mockAccessToken = "mockAccessToken";
    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: mockAccessToken,
      refresh: "mockRefreshToken",
      org_roles: { "1": "owner" },
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(setBearer).toHaveBeenCalledWith(mockAccessToken);
    });
    expect(setCoreBearer).toHaveBeenCalledWith(mockAccessToken);
  });

  it("should extract userId from access token", async () => {
    // Create a mock JWT with 'sub' claim (Core uses 'sub' for user ID)
    const mockUserId = "user-123";
    const mockPayload = { sub: mockUserId, exp: Date.now() / 1000 + 3600 };
    const mockAccessToken = "header." + btoa(JSON.stringify(mockPayload)) + ".signature";

    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: mockAccessToken,
      refresh: "mockRefreshToken",
      org_roles: { "1": "owner" },
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(result.current.userId).toBe(mockUserId);
    });
  });

  it("should handle Core login with all fields (access, refresh, org_roles)", async () => {
    const mockUserId = "user-456";
    const mockPayload = { sub: mockUserId, exp: Date.now() / 1000 + 3600 };
    const mockAccessToken = "header." + btoa(JSON.stringify(mockPayload)) + ".signature";
    const mockRefreshToken = "refresh.token.here";
    const mockOrgRoles = { "2": "admin", "7": "member" };

    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: mockAccessToken,
      refresh: mockRefreshToken,
      org_roles: mockOrgRoles,
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(result.current.jwt).toBe(mockAccessToken);
    });
    await waitFor(() => {
      expect(result.current.isAuthenticated()).toBe(true);
    });
    expect(result.current.userId).toBe(mockUserId);
    expect(result.current.orgRoles).toEqual(mockOrgRoles);
    expect(setBearer).toHaveBeenCalledWith(mockAccessToken);
    expect(setCoreBearer).toHaveBeenCalledWith(mockAccessToken);
    expect(router.replace).toHaveBeenCalledWith("/auth/profile/profilepage");
  });

  it("should handle empty org_roles gracefully", async () => {
    (tryLogin as jest.Mock).mockResolvedValueOnce({
      access: "mockAccessToken",
      refresh: "mockRefreshToken",
      org_roles: undefined, // User has no org memberships yet
    });

    const { result } = renderHook(() => useAuthentication(), {
      wrapper: AuthenticationProvider,
    });

    act(() => {
      result.current.login("test@test.dk", "testTest1");
    });

    await waitFor(() => {
      expect(result.current.orgRoles).toEqual({});
    });
    expect(result.current.isAuthenticated()).toBe(true);
  });
});
