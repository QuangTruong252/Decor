'use client';

import React, { useState } from 'react';
import MainLayout from '@/components/layout/MainLayout';
import Link from 'next/link';

export default function ContactPage() {
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    subject: '',
    message: '',
  });

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // In a real app, this would submit the form to an API
    console.log('Form submitted:', formData);
    // Reset form
    setFormData({
      name: '',
      email: '',
      subject: '',
      message: '',
    });
    // Show success message
    alert('Your message has been sent. We will get back to you soon!');
  };

  return (
    <MainLayout>
      {/* Contact Banner */}
      <div className="bg-secondary py-16">
        <div className="container-custom">
          <h1 className="text-4xl font-bold text-dark text-center">Contact</h1>
          <div className="flex items-center justify-center mt-4">
            <Link href="/" className="text-dark hover:text-primary transition-colors">
              Home
            </Link>
            <span className="mx-2">{'>'}</span>
            <span className="text-text-secondary">Contact</span>
          </div>
        </div>
      </div>

      {/* Contact Content */}
      <section className="py-16">
        <div className="container-custom">
          <div className="max-w-3xl mx-auto text-center mb-16">
            <h2 className="text-3xl font-bold text-dark mb-4">Get In Touch With Us</h2>
            <p className="text-text-secondary">
              For More Information About Our Product & Services. Please Feel Free To Drop Us An Email. Our Staff Always Be There To Help You Out. Do Not Hesitate!
            </p>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-12">
            {/* Contact Information */}
            <div>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
                <div>
                  <h3 className="text-xl font-medium mb-4">Address</h3>
                  <p className="text-text-secondary">
                    236 5th SE Avenue, New York NY10000, United States
                  </p>
                </div>
                
                <div>
                  <h3 className="text-xl font-medium mb-4">Phone</h3>
                  <p className="text-text-secondary">
                    Mobile: +(84) 546-6789<br />
                    Hotline: +(84) 456-6789
                  </p>
                </div>
                
                <div>
                  <h3 className="text-xl font-medium mb-4">Working Time</h3>
                  <p className="text-text-secondary">
                    Monday-Friday: 9:00 - 22:00<br />
                    Saturday-Sunday: 9:00 - 21:00
                  </p>
                </div>
                
                <div>
                  <h3 className="text-xl font-medium mb-4">Email</h3>
                  <p className="text-text-secondary">
                    info@furniro.com<br />
                    support@furniro.com
                  </p>
                </div>
              </div>
            </div>
            
            {/* Contact Form */}
            <div>
              <form onSubmit={handleSubmit} className="space-y-6">
                <div>
                  <label htmlFor="name" className="block text-dark mb-2">Your name</label>
                  <input
                    type="text"
                    id="name"
                    name="name"
                    required
                    className="w-full border border-border-color p-3 rounded-lg focus:outline-none focus:border-primary"
                    value={formData.name}
                    onChange={handleInputChange}
                    placeholder="Abc"
                  />
                </div>
                
                <div>
                  <label htmlFor="email" className="block text-dark mb-2">Email address</label>
                  <input
                    type="email"
                    id="email"
                    name="email"
                    required
                    className="w-full border border-border-color p-3 rounded-lg focus:outline-none focus:border-primary"
                    value={formData.email}
                    onChange={handleInputChange}
                    placeholder="Abc@def.com"
                  />
                </div>
                
                <div>
                  <label htmlFor="subject" className="block text-dark mb-2">Subject</label>
                  <input
                    type="text"
                    id="subject"
                    name="subject"
                    className="w-full border border-border-color p-3 rounded-lg focus:outline-none focus:border-primary"
                    value={formData.subject}
                    onChange={handleInputChange}
                    placeholder="This is an optional"
                  />
                </div>
                
                <div>
                  <label htmlFor="message" className="block text-dark mb-2">Message</label>
                  <textarea
                    id="message"
                    name="message"
                    required
                    rows={5}
                    className="w-full border border-border-color p-3 rounded-lg focus:outline-none focus:border-primary"
                    value={formData.message}
                    onChange={handleInputChange}
                    placeholder="Hi! I'd like to ask about"
                  ></textarea>
                </div>
                
                <div>
                  <button type="submit" className="bg-primary text-white py-3 px-8 rounded-lg hover:bg-opacity-90 transition-all">
                    Submit
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      </section>

      {/* Map Section */}
      <section className="py-16 bg-light">
        <div className="container-custom">
          <div className="aspect-[16/9] w-full">
            <iframe
              src="https://www.google.com/maps/embed?pb=!1m18!1m12!1m3!1d3022.9663095343008!2d-74.0059418846902!3d40.74076737932881!2m3!1f0!2f0!3f0!3m2!1i1024!2i768!4f13.1!3m3!1m2!1s0x89c259bf5c1654f3%3A0xc80f9cfce5383d5d!2sGoogle!5e0!3m2!1sen!2sus!4v1586539283960!5m2!1sen!2sus"
              width="100%"
              height="100%"
              style={{ border: 0 }}
              allowFullScreen
              loading="lazy"
              referrerPolicy="no-referrer-when-downgrade"
              title="Google Maps"
            ></iframe>
          </div>
        </div>
      </section>
    </MainLayout>
  );
}
