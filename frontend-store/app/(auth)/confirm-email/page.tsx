'use client';

import { Suspense, useEffect, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useConfirmEmail, useResendConfirmation } from '@/features/auth/hooks/useAuth';

function ConfirmEmailContent() {
  const searchParams = useSearchParams();
  const userId = searchParams.get('userId');
  const token = searchParams.get('token');

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const confirmEmail = useConfirmEmail();
  const resendConfirmation = useResendConfirmation();
  const [email, setEmail] = useState('');

  useEffect(() => {
    if (userId && token) {
      confirmEmail.mutate(
        { userId, token },
        {
          onSuccess: () => setStatus('success'),
          onError: () => setStatus('error'),
        }
      );
    } else {
      setStatus('error');
    }
  }, [userId, token]); // eslint-disable-line react-hooks/exhaustive-deps

  const handleResend = async () => {
    if (!email) return;
    try {
      await resendConfirmation.mutateAsync(email);
    } catch {
      // Error handled by mutation
    }
  };

  if (status === 'loading') {
    return (
      <div className="rounded-lg border bg-white p-8 shadow-sm">
        <div className="text-center">
          <Loader2 className="mx-auto h-12 w-12 animate-spin text-slate-400" />
          <h1 className="mt-4 text-xl font-semibold">Confirming your email...</h1>
          <p className="mt-2 text-slate-600">Please wait while we verify your email address.</p>
        </div>
      </div>
    );
  }

  if (status === 'success') {
    return (
      <div className="rounded-lg border bg-white p-8 shadow-sm">
        <div className="text-center">
          <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
            <CheckCircle className="h-8 w-8 text-green-600" />
          </div>
          <h1 className="text-2xl font-bold">Email Confirmed!</h1>
          <p className="mt-2 text-slate-600">
            Your email has been successfully verified. You can now sign in to your account.
          </p>
          <Button className="mt-6 w-full" asChild>
            <Link href="/login">Sign In</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="rounded-lg border bg-white p-8 shadow-sm">
      <div className="text-center">
        <div className="mx-auto mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-red-100">
          <XCircle className="h-8 w-8 text-red-600" />
        </div>
        <h1 className="text-2xl font-bold">Verification Failed</h1>
        <p className="mt-2 text-slate-600">
          The verification link is invalid or has expired.
        </p>
      </div>

      <div className="mt-6 space-y-4">
        <div>
          <label htmlFor="email" className="block text-sm font-medium text-slate-700">
            Enter your email to resend verification
          </label>
          <input
            type="email"
            id="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            className="mt-1 w-full rounded-md border px-3 py-2 focus:border-slate-900 focus:outline-none focus:ring-1 focus:ring-slate-900"
            placeholder="you@example.com"
          />
        </div>

        <Button
          className="w-full"
          onClick={handleResend}
          disabled={!email || resendConfirmation.isPending}
        >
          {resendConfirmation.isPending && (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          )}
          Resend Verification Email
        </Button>

        <div className="text-center">
          <Link href="/login" className="text-sm text-slate-600 hover:text-slate-900">
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}

export default function ConfirmEmailPage() {
  return (
    <Suspense
      fallback={
        <div className="rounded-lg border bg-white p-8 shadow-sm">
          <div className="text-center">
            <Loader2 className="mx-auto h-12 w-12 animate-spin text-slate-400" />
            <h1 className="mt-4 text-xl font-semibold">Loading...</h1>
          </div>
        </div>
      }
    >
      <ConfirmEmailContent />
    </Suspense>
  );
}
