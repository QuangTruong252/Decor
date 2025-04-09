import type { Config } from 'tailwindcss'

const config: Config = {
  darkMode: ["class"],
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  prefix: "",
  theme: {
    container: {
      center: true,
      padding: "1rem",
      screens: {
        sm: "640px",
        md: "768px",
        lg: "1024px",
        xl: "1280px",
        "2xl": "1400px",
      },
    },
    extend: {
      fontFamily: {
        sans: ['Inter', 'sans-serif'],
      },
      colors: {
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "#0065F7", // ThinkPro primary blue
          foreground: "#FFFFFF",
          50: "#E6F0FF",
          100: "#CCE0FF",
          200: "#99C2FF",
          300: "#66A3FF",
          400: "#3385FF",
          500: "#0065F7", // Main primary
          600: "#0052CC",
          700: "#003D99",
          800: "#002966",
          900: "#001433",
        },
        secondary: {
          DEFAULT: "#242424", // Dark gray used by ThinkPro for secondary elements
          foreground: "#FFFFFF",
        },
        accent: { 
          DEFAULT: "#FF424E", // ThinkPro accent red
          foreground: "#FFFFFF",
        },
        success: {
          DEFAULT: "#00C271", // Green for success states
          foreground: "#FFFFFF",
        },
        warning: {
          DEFAULT: "#FFB800", // Warning yellow
          foreground: "#FFFFFF",
        },
        destructive: {
          DEFAULT: "#FF424E", // Same as accent for destructive actions
          foreground: "#FFFFFF",
        },
        muted: {
          DEFAULT: "#F5F5F5", // Light gray for muted backgrounds
          foreground: "#71717A",
        },
        popover: {
          DEFAULT: "#FFFFFF",
          foreground: "#242424",
        },
        card: {
          DEFAULT: "#FFFFFF",
          foreground: "#242424",
        },
        // ThinkPro specific background colors for sections
        green: {
          light: "#E8F7FA", // Light green for card backgrounds
          DEFAULT: "#00C271",
        },
        pink: {
          light: "#FFF1F5", // Light pink for card backgrounds
          DEFAULT: "#FF80AB",
        },
        blue: {
          dark: "#03204C", // Dark blue for WhyThinkPro section
          light: "#E6F0FF", // Light blue for backgrounds
          DEFAULT: "#0065F7",
        },
      },
      borderRadius: {
        lg: "0.75rem",
        md: "0.5rem",
        sm: "0.25rem",
      },
      keyframes: {
        "accordion-down": {
          from: { height: "0" },
          to: { height: "var(--radix-accordion-content-height)" },
        },
        "accordion-up": {
          from: { height: "var(--radix-accordion-content-height)" },
          to: { height: "0" },
        },
      },
      animation: {
        "accordion-down": "accordion-down 0.2s ease-out",
        "accordion-up": "accordion-up 0.2s ease-out",
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
}

export default config;
