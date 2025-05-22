import React from 'react';
import Link from 'next/link';
import Image from 'next/image';

// Sample data - in a real app, this would come from an API
const categories = [
  {
    id: 1,
    name: 'Dining',
    image: '/images/dining-category.png',
    slug: 'dining',
  },
  {
    id: 2,
    name: 'Living',
    image: '/images/living-category.png',
    slug: 'living',
  },
  {
    id: 3,
    name: 'Bedroom',
    image: '/images/bedroom-category.png',
    slug: 'bedroom',
  },
];

const Categories = () => {
  return (
    <section className="py-16 bg-light">
      <div className="container-custom">
        <div className="text-center mb-12">
          <h2 className="text-3xl font-bold text-dark mb-2">Browse The Range</h2>
          <p className="text-text-secondary">Find furniture for every room in your home</p>
        </div>
        
        <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
          {categories.map((category) => (
            <Link 
              key={category.id} 
              href={`/category/${category.slug}`}
              className="group"
            >
              <div className="relative overflow-hidden rounded-lg aspect-square">
                <Image
                  src={category.image}
                  alt={category.name}
                  fill
                  className="object-cover object-center transition-transform duration-300 group-hover:scale-105"
                />
              </div>
              <h3 className="mt-4 text-xl font-medium text-center text-dark group-hover:text-primary transition-colors">
                {category.name}
              </h3>
            </Link>
          ))}
        </div>
      </div>
    </section>
  );
};

export default Categories;
