# Furniro - Furniture E-commerce Website

A modern furniture e-commerce website built with Next.js, TypeScript, and Tailwind CSS.

## Features

- Responsive design for all screen sizes
- Product catalog with filtering and sorting
- Product detail pages with image gallery
- Shopping cart functionality
- Checkout process
- User authentication (login/register)
- Modern UI/UX based on Figma design

## Tech Stack

- **Framework**: Next.js 15 with App Router
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **State Management**: React Context API
- **Deployment**: Vercel (recommended)

## Getting Started

### Prerequisites

- Node.js 18.17.0 or later
- npm, yarn, or pnpm

### Installation

1. Clone the repository:

```bash
git clone https://github.com/yourusername/furniro-ecommerce.git
cd furniro-ecommerce
```

2. Install dependencies:

```bash
npm install
# or
yarn install
# or
pnpm install
```

3. Run the development server:

```bash
npm run dev
# or
yarn dev
# or
pnpm dev
```

4. Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Project Structure

```
furniro-ecommerce/
├── public/             # Static assets
│   └── images/         # Product and other images
├── src/
│   ├── app/            # App router pages
│   ├── components/     # Reusable components
│   │   ├── layout/     # Layout components (Header, Footer)
│   │   ├── products/   # Product-related components
│   │   ├── sections/   # Page sections
│   │   └── ui/         # UI components (Button, Input, etc.)
│   ├── context/        # React Context for state management
│   └── lib/            # Utility functions
├── tailwind.config.js  # Tailwind CSS configuration
└── package.json        # Project dependencies and scripts
```

## Pages

- **Home**: Landing page with featured products and categories
- **Shop**: Product listing with filters and sorting
- **Product Detail**: Detailed product information and add to cart
- **Cart**: Shopping cart with product list and totals
- **Checkout**: Multi-step checkout process
- **About**: Company information
- **Contact**: Contact form and information

## Future Improvements

- Integration with a real backend API
- User authentication and profile management
- Wishlist functionality
- Product reviews and ratings
- Search functionality
- Payment gateway integration

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Deploy on Vercel

The easiest way to deploy your Next.js app is to use the [Vercel Platform](https://vercel.com/new?utm_medium=default-template&filter=next.js&utm_source=create-next-app&utm_campaign=create-next-app-readme) from the creators of Next.js.

Check out our [Next.js deployment documentation](https://nextjs.org/docs/app/building-your-application/deploying) for more details.
