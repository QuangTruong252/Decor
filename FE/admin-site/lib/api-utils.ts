"use client";

import { getSession, signOut } from "next-auth/react";

export const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

/**
 * Lấy token xác thực từ NextAuth session
 * @returns Token xác thực hoặc undefined nếu chưa đăng nhập
 */
export async function getAuthToken(): Promise<string | undefined> {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const session:any = await getSession();
  return session?.user?.accessToken;
}

/**
 * Lấy header xác thực cho các request API
 * @returns Object chứa header Authorization
 */
export async function getAuthHeader(): Promise<HeadersInit> {
  const token = await getAuthToken();
  
  return {
    "Content-Type": "application/json",
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

/**
 * Wrapper cho fetch API với xác thực tự động
 * @param url URL của API endpoint
 * @param options Các options cho fetch API
 * @returns Response từ fetch API
 */
export async function fetchWithAuth(
  url: string, 
  options: RequestInit = {}
): Promise<Response> {
  const headers = await getAuthHeader();
  
  const response = await fetch(url, {
    ...options,
    headers: {
      ...headers,
      ...options.headers,
    },
  });
  
  // Xử lý lỗi xác thực
  if (response.status === 401) {
    // Đăng xuất người dùng khi token không hợp lệ hoặc hết hạn
    await signOut({ redirect: true, callbackUrl: "/login" });
    throw new Error("Session expired. Please login again.");
  }
  
  return response;
}

/**
 * Wrapper cho fetchWithAuth với FormData
 * @param url URL của API endpoint
 * @param formData FormData để gửi
 * @param method HTTP method (mặc định là POST)
 * @returns Response từ fetch API
 */
export async function fetchWithAuthFormData(
  url: string,
  formData: FormData,
  method: string = "POST"
): Promise<Response> {
  const token = await getAuthToken();
  
  const headers: HeadersInit = {};
  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }
  
  const response = await fetch(url, {
    method,
    headers,
    body: formData,
  });
  
  // Xử lý lỗi xác thực
  if (response.status === 401) {
    await signOut({ redirect: true, callbackUrl: "/login" });
    throw new Error("Session expired. Please login again.");
  }
  
  return response;
}

/**
 * Lấy thông tin người dùng hiện tại từ session
 * @returns Thông tin người dùng hoặc null nếu chưa đăng nhập
 */
export async function getCurrentUser() {
  const session = await getSession();
  return session?.user || null;
}
