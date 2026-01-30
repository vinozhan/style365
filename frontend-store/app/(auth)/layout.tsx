import Link from 'next/link';

export default function AuthLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="flex min-h-screen flex-col">
      {/* Simple header */}
      <header className="border-b">
        <div className="container-custom flex h-16 items-center">
          <Link href="/" className="text-2xl font-bold">
            Style365
          </Link>
        </div>
      </header>

      {/* Main content */}
      <main className="flex flex-1 items-center justify-center bg-slate-50 py-12">
        <div className="w-full max-w-md px-4">
          {children}
        </div>
      </main>

      {/* Simple footer */}
      <footer className="border-t bg-white py-4">
        <div className="container-custom text-center text-sm text-slate-500">
          &copy; {new Date().getFullYear()} Style365. All rights reserved.
        </div>
      </footer>
    </div>
  );
}
