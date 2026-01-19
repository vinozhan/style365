import { useState } from 'react';
import { ChevronRight, ChevronDown, FolderTree, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';
import type { Category } from '@/types';

interface CategoryTreeProps {
  categories: Category[];
  onEdit: (category: Category) => void;
  onDelete: (category: Category) => void;
}

export function CategoryTree({ categories, onEdit, onDelete }: CategoryTreeProps) {
  return (
    <div className="rounded-md border">
      {categories.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <FolderTree className="h-8 w-8 text-slate-400" />
          <p className="mt-4 text-lg font-semibold text-slate-900">No categories</p>
          <p className="mt-2 text-sm text-slate-500">Get started by creating your first category</p>
        </div>
      ) : (
        <div className="divide-y">
          {categories.map((category) => (
            <CategoryTreeItem
              key={category.id}
              category={category}
              level={0}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface CategoryTreeItemProps {
  category: Category;
  level: number;
  onEdit: (category: Category) => void;
  onDelete: (category: Category) => void;
}

function CategoryTreeItem({ category, level, onEdit, onDelete }: CategoryTreeItemProps) {
  const [isExpanded, setIsExpanded] = useState(true);
  const hasChildren = category.children && category.children.length > 0;

  return (
    <div>
      <div
        className={cn(
          'flex items-center justify-between px-4 py-3 hover:bg-slate-50',
          level > 0 && 'border-l-2 border-slate-200'
        )}
        style={{ paddingLeft: `${16 + level * 24}px` }}
      >
        <div className="flex items-center gap-2">
          {hasChildren ? (
            <button
              onClick={() => setIsExpanded(!isExpanded)}
              className="rounded p-0.5 hover:bg-slate-200"
            >
              {isExpanded ? (
                <ChevronDown className="h-4 w-4 text-slate-500" />
              ) : (
                <ChevronRight className="h-4 w-4 text-slate-500" />
              )}
            </button>
          ) : (
            <span className="w-5" />
          )}

          <span className="font-medium">{category.name}</span>

          {!category.isActive && (
            <Badge variant="secondary" className="text-xs">
              Inactive
            </Badge>
          )}

          <span className="text-sm text-slate-500">
            ({category.productsCount || 0} products)
          </span>
        </div>

        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" className="h-8 w-8" onClick={() => onEdit(category)}>
            <Pencil className="h-4 w-4" />
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="h-8 w-8 text-red-600 hover:text-red-700"
            onClick={() => onDelete(category)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      </div>

      {hasChildren && isExpanded && (
        <div>
          {category.children!.map((child) => (
            <CategoryTreeItem
              key={child.id}
              category={child}
              level={level + 1}
              onEdit={onEdit}
              onDelete={onDelete}
            />
          ))}
        </div>
      )}
    </div>
  );
}
