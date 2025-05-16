"use client";

interface LoginCredentials {
  email: string;
  password: string;
}

interface User {
  id: string;
  email: string;
  name: string;
  role: string;
}

interface AuthResponse {
  token: string;
  user: User;
}

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

const setToken = (token: string) => {
  if (typeof window !== 'undefined') {
    localStorage.setItem('auth_token', token);
  }
};

const getToken = (): string | null => {
  if (typeof window !== 'undefined') {
    return localStorage.getItem('auth_token');
  }
  return null;
};

const removeToken = () => {
  if (typeof window !== 'undefined') {
    localStorage.removeItem('auth_token');
  }
};

export async function login(credentials: LoginCredentials): Promise<User> {
  try {
    const response = await fetch(`${API_URL}/api/Auth/login`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(credentials),
    });

    if (!response.ok) {
      throw new Error("Login failed");
    }

    const data: AuthResponse = await response.json();

    setToken(data.token);

    return data.user;
  } catch (error) {
    console.error("Login error:", error);
    throw new Error("Login failed. Please try again later.");
  }
}

export async function logout(): Promise<void> {
  try {
    removeToken();
  } catch (error) {
    console.error("Logout error:", error);
    throw new Error("Logout failed. Please try again later.");
  }
}

export async function getCurrentUser(): Promise<User | null> {
  try {
    const token = getToken();
    if (!token) {
      return null;
    }

    const response = await fetch(`${API_URL}/api/Auth/user`, {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });

    if (!response.ok) {
      if (response.status === 401) {
        removeToken();
        return null;
      }
      throw new Error("Unable to retrieve user information");
    }

    return response.json();
  } catch (error) {
    console.error("Get current user error:", error);
    return null;
  }
}