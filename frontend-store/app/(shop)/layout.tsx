import { Header, Footer, MobileMenu, CartDrawer } from '@/components/layout';

export default function ShopLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <main className="flex-1">{children}</main>
      <Footer />
      <MobileMenu />
      <CartDrawer />
    </div>
  );
}
