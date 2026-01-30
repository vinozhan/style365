'use client';

import { useState, useEffect, useRef } from 'react';
import { useRouter } from 'next/navigation';
import { Search, X, Loader2 } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useUIStore } from '@/stores/uiStore';
import { useSearchSuggestions } from '@/features/products/hooks/useProducts';
import { cn } from '@/lib/utils';

export function SearchBar() {
  const router = useRouter();
  const { closeSearch, searchQuery, setSearchQuery } = useUIStore();
  const inputRef = useRef<HTMLInputElement>(null);
  const [localQuery, setLocalQuery] = useState(searchQuery);

  const { data: suggestions, isLoading } = useSearchSuggestions(localQuery);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  useEffect(() => {
    const timer = setTimeout(() => {
      setSearchQuery(localQuery);
    }, 300);
    return () => clearTimeout(timer);
  }, [localQuery, setSearchQuery]);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (localQuery.trim()) {
      router.push(`/search?q=${encodeURIComponent(localQuery.trim())}`);
      closeSearch();
    }
  };

  const handleSuggestionClick = (suggestion: string) => {
    router.push(`/search?q=${encodeURIComponent(suggestion)}`);
    closeSearch();
  };

  return (
    <div className="absolute inset-x-0 top-full z-50 border-b bg-white shadow-lg">
      <div className="container-custom py-4">
        <form onSubmit={handleSearch} className="relative">
          <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-slate-400" />
          <Input
            ref={inputRef}
            type="search"
            placeholder="Search for products..."
            value={localQuery}
            onChange={(e) => setLocalQuery(e.target.value)}
            className="h-12 pl-10 pr-20 text-base"
          />
          <div className="absolute right-2 top-1/2 flex -translate-y-1/2 items-center gap-2">
            {isLoading && <Loader2 className="h-4 w-4 animate-spin text-slate-400" />}
            <Button type="button" variant="ghost" size="icon" onClick={closeSearch}>
              <X className="h-5 w-5" />
            </Button>
          </div>
        </form>

        {/* Suggestions */}
        {suggestions && suggestions.length > 0 && (
          <div className="mt-2 rounded-md border bg-white">
            {suggestions.map((suggestion, index) => (
              <button
                key={index}
                onClick={() => handleSuggestionClick(suggestion)}
                className="flex w-full items-center gap-3 px-4 py-3 text-left text-sm hover:bg-slate-50"
              >
                <Search className="h-4 w-4 text-slate-400" />
                {suggestion}
              </button>
            ))}
          </div>
        )}

        {/* Quick links */}
        <div className="mt-4 flex flex-wrap gap-2">
          <span className="text-sm text-slate-500">Popular:</span>
          {['T-Shirts', 'Jeans', 'Dresses', 'Jackets'].map((term) => (
            <button
              key={term}
              onClick={() => handleSuggestionClick(term)}
              className="rounded-full bg-slate-100 px-3 py-1 text-sm hover:bg-slate-200"
            >
              {term}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
