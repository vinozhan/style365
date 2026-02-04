import { HeroBanner, FeaturedProducts, CategoryShowcase, PromoSection } from '@/components/home';

export default function HomePage() {
  return (
    <>
      <HeroBanner />
      <CategoryShowcase />
      <FeaturedProducts />
      <PromoSection />
    </>
  );
}
