import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

// Routes that require authentication
const protectedPaths = ['/account'];

// Auth routes (redirect to account if already authenticated)
const authPaths = ['/login', '/register', '/forgot-password', '/reset-password'];

export function middleware(request: NextRequest) {
  const { pathname } = request.nextUrl;

  // Check for auth token in cookies
  // Note: We store tokens in localStorage, but we can also set a cookie for middleware check
  const token = request.cookies.get('accessToken')?.value;

  // Redirect authenticated users away from auth pages
  if (authPaths.some((path) => pathname.startsWith(path)) && token) {
    return NextResponse.redirect(new URL('/account', request.url));
  }

  // Protect account pages
  if (protectedPaths.some((path) => pathname.startsWith(path)) && !token) {
    const loginUrl = new URL('/login', request.url);
    loginUrl.searchParams.set('redirect', pathname);
    return NextResponse.redirect(loginUrl);
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/account/:path*', '/login', '/register', '/forgot-password', '/reset-password'],
};
