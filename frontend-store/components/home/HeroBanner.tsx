import Link from 'next/link';
import Image from 'next/image';
import { Button } from '@/components/ui/button';

export function HeroBanner() {
  return (
    <section className="relative h-[500px] w-full overflow-hidden bg-slate-900 md:h-[600px]">
      {/* Background image placeholder - replace with actual image */}
      <div className="absolute inset-0 bg-gradient-to-r from-slate-900 via-slate-900/80 to-transparent" />
      <div
        className="absolute inset-0 bg-cover bg-center bg-no-repeat opacity-50"
        style={{
          backgroundImage: "url('/images/hero-bg.jpg')",
        }}
      />

      {/* Content */}
      <div className="container-custom relative z-10 flex h-full items-center">
        <div className="max-w-xl text-white">
          <span className="mb-4 inline-block rounded-full bg-white/10 px-4 py-1 text-sm backdrop-blur-sm">
            New Collection 2024
          </span>
          <h1 className="mb-6 text-4xl font-bold leading-tight md:text-5xl lg:text-6xl">
            Discover Your Perfect Style
          </h1>
          <p className="mb-8 text-lg text-slate-300">
            Explore our latest collection of fashion essentials. Quality meets style in every piece.
          </p>
          <div className="flex flex-wrap gap-4">
            <Button size="lg" asChild>
              <Link href="/products">Shop Now</Link>
            </Button>
            <Button size="lg" variant="outline" className="border-white text-white hover:bg-white hover:text-slate-900" asChild>
              <Link href="/categories">Browse Categories</Link>
            </Button>
          </div>
        </div>
      </div>

      {/* Decorative elements */}
      <div className="absolute bottom-0 left-0 right-0 h-32 bg-gradient-to-t from-white to-transparent" />
    </section>
  );
}
