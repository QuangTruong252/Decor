# ThinkPro E-commerce Store Clone

This project is a frontend implementation of the ThinkPro VN website using Next.js. The goal is to create a clean, extensible, and manageable frontend codebase that closely resembles the original ThinkPro website's UI.

## Tech Stack

- **Framework**: [Next.js](https://nextjs.org)
- **UI Components**: [Shadcn UI](https://ui.shadcn.com) + [Radix UI](https://radix-ui.com)
- **State Management**: Zustand with React Context for SSR
- **Styling**: Tailwind CSS and CSS Modules
- **Data Fetching**: React Query
- **Form Handling**: React Hook Form
- **Utilities**: date-fns, Lodash
- **Icons**: Lucide React
- **Linting & Formatting**: ESLint, Prettier

## Installation

```bash
# Clone the repository
git clone <repository-url>
cd <repository-folder>

# Install dependencies
npm install
```

## Development

Start the development server:

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Build and Start

To build and start the production version:

```bash
# Build the application
npm run build

# Start the production server
npm start
```

## Project Structure

```
/src
  /app                   # Next.js App Router
    /components          # React components for each section
      /ui                # UI primitives and reusable components
    /page.tsx            # Main homepage
  /components            # Legacy components (Pages Router)
  /lib                   # Utility functions, hooks, and shared logic
  /pages                 # Next.js Pages Router
/public
  /assets               # Static assets (images, icons, etc.)
    /categories         # Category images
    /laptops            # Laptop images
    /products           # Product images
    /news               # News images
    /payment            # Payment method icons
```

## Features

- Responsive design that works on mobile, tablet, and desktop
- Performance optimized with lazy-loaded images via next/image
- Accessible UI components with proper semantic HTML and ARIA attributes
- Component-based architecture for maintainability and reusability

## UI Sections

- Header with logo, search, and nav icons
- Hero Banner with CTA
- Categories with horizontal scrollable list
- Featured Laptops with colored cards and carousels
- FlashSale with countdown timer
- Preorder section with product badges
- WhyThinkPro benefits section
- Recommendations grid
- TechNews with featured news and list
- Tags for easy navigation
- Footer with links and company info
