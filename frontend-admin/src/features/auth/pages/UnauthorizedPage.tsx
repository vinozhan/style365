import { ShieldX } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';

export function UnauthorizedPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-slate-50 px-4">
      <ShieldX className="h-16 w-16 text-red-500" />
      <h1 className="mt-4 text-2xl font-bold text-slate-900">Access Denied</h1>
      <p className="mt-2 text-slate-500">You don't have permission to access this page.</p>
      <p className="text-slate-500">Admin privileges are required.</p>
      <Button asChild className="mt-6">
        <Link to="/login">Back to Login</Link>
      </Button>
    </div>
  );
}
