import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Allow access to login page and NextAuth API routes
  if (pathname.startsWith("/login") || pathname.startsWith("/api/auth")) {
    return NextResponse.next();
  }

  // Get the token using next-auth/jwt which works in middleware
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET
  });

  // If there's no token (user is not authenticated), redirect to login
  if (!token) {
    const loginUrl = new URL("/login", request.url);
    // Optionally, add a callbackUrl to redirect back after login
    loginUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(loginUrl);
  }

  // If there is a token, allow the request to proceed
  return NextResponse.next();
}

export const config = {
  // Match all routes except for static files, _next internal routes, and favicon
  matcher: [
    "/((?!api/auth|_next/static|_next/image|favicon.ico|login).*)",
    "/dashboard/:path*",
    "/products/:path*",
    "/orders/:path*",
    "/customers/:path*",
    "/settings/:path*",
    "/banners/:path*",
  ],
};