import { NextResponse } from "next/server";
import type { NextRequest } from "next/server";
import { getToken } from "next-auth/jwt";

export async function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  console.log(`[Middleware] Checking path: ${pathname}`);

  // Allow access to login page, root page, and NextAuth API routes
  if (pathname.startsWith("/login") || pathname.startsWith("/api/auth") || pathname === "/") {
    console.log(`[Middleware] Allowing public path: ${pathname}`);
    return NextResponse.next();
  }

  // Get the token using next-auth/jwt which works in middleware
  const token = await getToken({
    req: request,
    secret: process.env.NEXTAUTH_SECRET
  });

  console.log(`[Middleware] Token status:`, { 
    hasToken: !!token, 
    tokenId: token?.id,
    tokenEmail: token?.email 
  });

  // If there's no token (user is not authenticated), redirect to login
  if (!token) {
    console.log(`[Middleware] No token found, redirecting to login`);
    const loginUrl = new URL("/login", request.url);
    // Optionally, add a callbackUrl to redirect back after login
    loginUrl.searchParams.set("callbackUrl", pathname);
    return NextResponse.redirect(loginUrl);
  }

  console.log(`[Middleware] Token found, allowing access to: ${pathname}`);
  // If there is a token, allow the request to proceed
  return NextResponse.next();
}

export const config = {
  // Match all routes except for static files, _next internal routes, and favicon
  matcher: [
    "/((?!api/auth|_next/static|_next/image|favicon.ico|login|$).*)",
    "/dashboard/:path*",
    "/products/:path*",
    "/orders/:path*",
    "/customers/:path*",
    "/settings/:path*",
    "/banners/:path*",
  ],
};
