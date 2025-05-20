import NextAuth from "next-auth";
import { authOptions } from "@/lib/auth";

// Create the handler with the authOptions
const handler = NextAuth(authOptions);

// Export the handler for GET and POST requests
export { handler as GET, handler as POST };