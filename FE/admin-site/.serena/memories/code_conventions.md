# Code Conventions

## Project Structure
- `app/`: Next.js App Router pages and layouts
- `components/`: Reusable React components grouped by feature
- `contexts/`: React context providers
- `hooks/`: Custom React hooks
- `services/`: API service functions
- `types/`: TypeScript type definitions
- `lib/`: Utility functions and helpers

## TypeScript Conventions
- Use TypeScript for all files
- Define interfaces/types in separate files under `types/`
- Use explicit type annotations for function parameters and returns

## Component Conventions
- Use functional components with hooks
- Group related components in feature-specific folders
- Keep components focused and single-responsibility
- Use shared UI components from `components/ui`

## State Management
- Use Zustand for global state
- Use React Query for server state
- Use React Context for shared component state

## API Integration
- API services are organized by domain in `services/`
- Use React Query hooks for data fetching
- Handle authentication with NextAuth.js

## Styling
- Use TailwindCSS for styling
- Follow utility-first CSS approach
- Use class-variance-authority for component variants

## Error Handling
- Use toast notifications for user feedback
- Implement proper error boundaries
- Handle API errors consistently

## Code Quality
- Follow ESLint rules
- Use TypeScript strict mode
- Write self-documenting code with clear naming