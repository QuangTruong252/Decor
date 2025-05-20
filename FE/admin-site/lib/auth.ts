import { NextAuthOptions } from "next-auth";
import CredentialsProvider from "next-auth/providers/credentials";
import { JWT } from "next-auth/jwt";
import { getServerSession } from "next-auth/next";

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

interface BackendUser {
  id: string;
  email: string;
  name: string;
  role: string;
}

interface AuthResponse {
  token: string;
  user: BackendUser;
}

export const authOptions: NextAuthOptions = {
  providers: [
    CredentialsProvider({
      name: "Credentials",
      credentials: {
        email: { label: "Email", type: "email" },
        password: { label: "Password", type: "password" },
      },
      async authorize(credentials, req) {
        if (!credentials?.email || !credentials?.password) {
          return null;
        }

        try {
          const response = await fetch(`${API_URL}/api/Auth/login`, {
            method: "POST",
            headers: {
              "Content-Type": "application/json",
            },
            body: JSON.stringify({
              email: credentials.email,
              password: credentials.password,
            }),
          });

          if (!response.ok) {
            const errorData = await response.json();
            throw new Error(errorData.message || "Login failed");
          }

          const data: AuthResponse = await response.json();

          if (data.token && data.user) {
            return {
              id: data.user.id,
              email: data.user.email,
              name: data.user.name,
              role: data.user.role,
              accessToken: data.token,
            };
          }
          return null;
        } catch (error) {
          console.error("Authorize error:", error);
          // Return null if authentication fails or throw an error
          // For a better UX, you might want to throw a specific error that can be caught on the client-side
          throw new Error(error instanceof Error ? error.message : "Invalid credentials");
        }
      },
    }),
  ],
  session: {
    strategy: "jwt",
    maxAge: 30 * 24 * 60 * 60, // 30 days
  },
  callbacks: {
    async jwt({ token, user, account }: { token: JWT; user?: any; account?: any }): Promise<JWT> {
      if (account && user) {
        // This is the first login
        token.accessToken = user.accessToken;
        token.id = user.id;
        token.role = user.role; // Add role to the token
        token.email = user.email;
        token.name = user.name;
      }
      return token;
    },
    async session({ session, token }: { session: any; token: JWT }): Promise<any> {
      if (session.user) {
        session.user.accessToken = token.accessToken as string;
        session.user.id = token.id as string;
        session.user.role = token.role as string; // Add role to the session
        // The default session already includes user.name, user.email, user.image
        // Ensure these are populated if they come from the token
        if (token.name) session.user.name = token.name as string;
        if (token.email) session.user.email = token.email as string;
      }
      return session;
    },
  },
  pages: {
    signIn: "/login",
    // error: '/auth/error', // Custom error page (optional)
  },
  secret: process.env.NEXTAUTH_SECRET, // A secret is required for JWT
  debug: process.env.NODE_ENV === 'development',
};

// Helper function to get the session on the server side
export const getServerAuthSession = () => getServerSession(authOptions);

// This is a separate function that can be used in middleware
export const auth = authOptions;