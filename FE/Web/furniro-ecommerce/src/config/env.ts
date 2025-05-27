import { z } from 'zod';

const envSchema = z.object({
  NEXT_PUBLIC_API_BASE_URL: z.string().url('Invalid API base URL'),
  NEXT_PUBLIC_API_IMAGE_URL: z.string().url('Invalid API image URL'),
  NEXT_PUBLIC_APP_URL: z.string().url('Invalid app URL'),
  NEXT_PUBLIC_JWT_SECRET: z.string().min(1, 'JWT secret is required'),
  NEXT_PUBLIC_APP_NAME: z.string().min(1, 'App name is required'),
  NEXT_PUBLIC_APP_VERSION: z.string().min(1, 'App version is required'),
  NODE_ENV: z.enum(['development', 'production', 'test']).default('development'),
});

export type Env = z.infer<typeof envSchema>;

function validateEnv(): Env {
  try {
    return envSchema.parse({
      NEXT_PUBLIC_API_BASE_URL: process.env.NEXT_PUBLIC_API_BASE_URL,
      NEXT_PUBLIC_API_IMAGE_URL: process.env.NEXT_PUBLIC_API_IMAGE_URL,
      NEXT_PUBLIC_APP_URL: process.env.NEXT_PUBLIC_APP_URL,
      NEXT_PUBLIC_JWT_SECRET: process.env.NEXT_PUBLIC_JWT_SECRET,
      NEXT_PUBLIC_APP_NAME: process.env.NEXT_PUBLIC_APP_NAME,
      NEXT_PUBLIC_APP_VERSION: process.env.NEXT_PUBLIC_APP_VERSION,
      NODE_ENV: process.env.NODE_ENV,
    });
  } catch (error) {
    console.error('❌ Invalid environment variables:', error);
    throw new Error('Invalid environment variables');
  }
}

export const env = validateEnv();

// Export individual environment variables for convenience
export const {
  NEXT_PUBLIC_API_BASE_URL,
  NEXT_PUBLIC_API_IMAGE_URL,
  NEXT_PUBLIC_APP_URL,
  NEXT_PUBLIC_JWT_SECRET,
  NEXT_PUBLIC_APP_NAME,
  NEXT_PUBLIC_APP_VERSION,
  NODE_ENV,
} = env;
