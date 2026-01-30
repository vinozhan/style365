import type { Metadata } from 'next';
import { Geist, Geist_Mono } from 'next/font/google';
import { Toaster } from 'sonner';
import { QueryProvider } from '@/providers';
import './globals.css';

const geistSans = Geist({
  variable: '--font-geist-sans',
  subsets: ['latin'],
});

const geistMono = Geist_Mono({
  variable: '--font-geist-mono',
  subsets: ['latin'],
});

export const metadata: Metadata = {
  title: {
    default: 'Style365 - Fashion & Clothing Store',
    template: '%s | Style365',
  },
  description: 'Discover the latest fashion trends at Style365. Shop clothing, accessories, and more with free shipping on orders over Rs. 5000.',
  keywords: ['fashion', 'clothing', 'online store', 'Sri Lanka', 'style365'],
  authors: [{ name: 'Style365' }],
  creator: 'Style365',
  metadataBase: new URL(process.env.NEXT_PUBLIC_APP_URL || 'https://www.style365.com'),
  openGraph: {
    type: 'website',
    locale: 'en_US',
    url: '/',
    siteName: 'Style365',
    title: 'Style365 - Fashion & Clothing Store',
    description: 'Discover the latest fashion trends at Style365.',
  },
  twitter: {
    card: 'summary_large_image',
    title: 'Style365 - Fashion & Clothing Store',
    description: 'Discover the latest fashion trends at Style365.',
  },
  robots: {
    index: true,
    follow: true,
  },
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${geistSans.variable} ${geistMono.variable} antialiased min-h-screen`}>
        <QueryProvider>
          {children}
          <Toaster position="top-right" richColors closeButton />
        </QueryProvider>
      </body>
    </html>
  );
}
