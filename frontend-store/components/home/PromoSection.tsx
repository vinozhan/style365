import Link from 'next/link';
import { Button } from '@/components/ui/button';
import { Truck, RefreshCcw, Shield, Headphones } from 'lucide-react';

const features = [
  {
    icon: Truck,
    title: 'Free Shipping',
    description: 'On orders over Rs. 5,000',
  },
  {
    icon: RefreshCcw,
    title: 'Easy Returns',
    description: '30-day return policy',
  },
  {
    icon: Shield,
    title: 'Secure Payment',
    description: '100% secure checkout',
  },
  {
    icon: Headphones,
    title: '24/7 Support',
    description: 'Dedicated support team',
  },
];

export function PromoSection() {
  return (
    <>
      {/* Features bar */}
      <section className="border-y bg-white py-8">
        <div className="container-custom">
          <div className="grid grid-cols-2 gap-6 md:grid-cols-4">
            {features.map((feature) => (
              <div key={feature.title} className="flex items-center gap-3">
                <div className="flex h-12 w-12 items-center justify-center rounded-full bg-slate-100">
                  <feature.icon className="h-6 w-6 text-slate-700" />
                </div>
                <div>
                  <h3 className="font-semibold">{feature.title}</h3>
                  <p className="text-sm text-slate-500">{feature.description}</p>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Promotional banner */}
      <section className="bg-slate-900 py-16 text-white">
        <div className="container-custom">
          <div className="flex flex-col items-center justify-between gap-6 md:flex-row">
            <div>
              <h2 className="text-2xl font-bold md:text-3xl">Get 20% Off Your First Order</h2>
              <p className="mt-2 text-slate-300">
                Sign up for our newsletter and receive an exclusive discount code.
              </p>
            </div>
            <Button size="lg" variant="secondary" asChild>
              <Link href="/register">Sign Up Now</Link>
            </Button>
          </div>
        </div>
      </section>
    </>
  );
}
