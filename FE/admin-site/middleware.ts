import { NextRequest, NextResponse } from "next/server";

const PUBLIC_PATHS = ["/login", "/_next", "/favicon.ico", "/api"];

function isPublicPath(path: string) {
  return PUBLIC_PATHS.some((publicPath) => path.startsWith(publicPath));
}

interface JwtPayload {
  exp?: number;
}

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;
  if (isPublicPath(pathname)) {
    return NextResponse.next();
  }

  const token = request.cookies.get("auth_token")?.value || request.headers.get("authorization")?.replace("Bearer ", "");

  if (!token) {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  try {
    const { jwtDecode } = await import("jwt-decode");
    const decoded = jwtDecode<JwtPayload>(token);
    if (decoded.exp && Date.now() >= decoded.exp * 1000) {
      // Token expired
      return NextResponse.redirect(new URL("/login", request.url));
    }
  } catch {
    return NextResponse.redirect(new URL("/login", request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ["/(admin)/(.*)", "/dashboard", "/products", "/orders", "/customers", "/settings"],
};