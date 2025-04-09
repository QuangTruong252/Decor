'use client';

import React from 'react';
import Image, { ImageProps } from 'next/image';

type PlaceholderImageProps = Omit<ImageProps, 'src'> & {
  src?: string;
};

const PlaceholderImage = ({ src, alt, ...props }: PlaceholderImageProps) => {
  return (
    <Image
      src={src || '/assets/placeholder.png'}
      alt={alt || 'Placeholder image'}
      {...props}
    />
  );
};

export default PlaceholderImage;
