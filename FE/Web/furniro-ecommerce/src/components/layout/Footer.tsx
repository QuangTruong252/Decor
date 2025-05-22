import React from 'react';
import Link from 'next/link';

const Footer = () => {
  return (
    <footer className="bg-white border-t border-border-color">
      <div className="container-custom py-16">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
          {/* Column 1 */}
          <div>
            <h3 className="text-xl font-bold mb-6">Furniro.</h3>
            <p className="text-text-secondary mb-6">
              400 University Drive Suite 200 Coral Gables,<br />
              FL 33134 USA
            </p>
          </div>

          {/* Column 2 */}
          <div>
            <h3 className="text-lg font-medium text-text-secondary mb-6">Links</h3>
            <ul className="space-y-4">
              <li>
                <Link href="/" className="text-dark hover:text-primary transition-colors">
                  Home
                </Link>
              </li>
              <li>
                <Link href="/shop" className="text-dark hover:text-primary transition-colors">
                  Shop
                </Link>
              </li>
              <li>
                <Link href="/about" className="text-dark hover:text-primary transition-colors">
                  About
                </Link>
              </li>
              <li>
                <Link href="/contact" className="text-dark hover:text-primary transition-colors">
                  Contact
                </Link>
              </li>
            </ul>
          </div>

          {/* Column 3 */}
          <div>
            <h3 className="text-lg font-medium text-text-secondary mb-6">Help</h3>
            <ul className="space-y-4">
              <li>
                <Link href="/payment-options" className="text-dark hover:text-primary transition-colors">
                  Payment Options
                </Link>
              </li>
              <li>
                <Link href="/returns" className="text-dark hover:text-primary transition-colors">
                  Returns
                </Link>
              </li>
              <li>
                <Link href="/privacy-policy" className="text-dark hover:text-primary transition-colors">
                  Privacy Policies
                </Link>
              </li>
            </ul>
          </div>

          {/* Column 4 */}
          <div>
            <h3 className="text-lg font-medium text-text-secondary mb-6">Newsletter</h3>
            <form className="mt-4">
              <div className="flex flex-col space-y-4">
                <input
                  type="email"
                  placeholder="Enter Your Email Address"
                  className="border-b border-border-color pb-2 focus:outline-none focus:border-primary"
                />
                <button
                  type="submit"
                  className="text-dark font-medium hover:text-primary transition-colors text-left"
                >
                  SUBSCRIBE
                </button>
              </div>
            </form>
          </div>
        </div>

        {/* Copyright */}
        <div className="mt-16 pt-8 border-t border-border-color">
          <p className="text-center text-dark">
            2023 furniro. All rights reserved
          </p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
