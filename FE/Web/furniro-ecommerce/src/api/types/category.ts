import { BaseEntity } from './common';
import { PagedResult } from './api';

// Category related types
export interface CategoryDTO extends BaseEntity {
  name: string;
  slug: string;
  description: string;
  parentId: number | null;
  parentName: string | null;
  imageUrl: string | null;
  subcategories: CategoryDTO[] | null;
}

// Paged result for categories
export interface CategoryDTOPagedResult extends PagedResult<CategoryDTO> {}

export interface CreateCategoryDTO {
  name: string;
  slug: string;
  description?: string;
  parentId?: number;
  imageFile?: File;
}

export interface UpdateCategoryDTO {
  name?: string;
  slug?: string;
  description?: string;
  parentId?: number;
  imageFile?: File;
}

export interface CategoryTreeNode extends CategoryDTO {
  children: CategoryTreeNode[];
  level: number;
  hasChildren: boolean;
  isExpanded?: boolean;
}

export interface CategoryBreadcrumb {
  id: number;
  name: string;
  slug: string;
}

export interface CategoryStats {
  totalProducts: number;
  totalSubcategories: number;
  isActive: boolean;
}

export interface CategoryWithStats extends CategoryDTO {
  stats: CategoryStats;
}

// Category navigation types
export interface CategoryNavItem {
  id: number;
  name: string;
  slug: string;
  imageUrl?: string;
  children?: CategoryNavItem[];
}

export interface CategoryFilter {
  id: number;
  name: string;
  count: number;
  isSelected: boolean;
}

// Category form types
export interface CategoryFormData extends Omit<CreateCategoryDTO, 'imageFile'> {
  imageFile?: File;
  existingImageUrl?: string;
}

// Hierarchical category display
export interface HierarchicalCategory extends CategoryDTO {
  subcategories: HierarchicalCategory[];
  productCount: number;
  depth: number;
}

// Category search and filter
export interface CategorySearchParams {
  query?: string;
  parentId?: number;
  hasProducts?: boolean;
  sortBy?: 'name' | 'createdAt' | 'productCount';
  sortOrder?: 'asc' | 'desc';
}

// Category management
export interface CategoryManagement {
  category: CategoryDTO;
  canEdit: boolean;
  canDelete: boolean;
  hasProducts: boolean;
  hasSubcategories: boolean;
}

// Hooks return types
export interface UseCategoriesReturn {
  categories: CategoryDTO[];
  hierarchicalCategories: HierarchicalCategory[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export interface UseCategoryReturn {
  category: CategoryWithStats | null;
  breadcrumbs: CategoryBreadcrumb[];
  isLoading: boolean;
  error: string | null;
  refetch: () => void;
}

export interface UseCategoryTreeReturn {
  tree: CategoryTreeNode[];
  expandedNodes: Set<number>;
  toggleNode: (nodeId: number) => void;
  expandAll: () => void;
  collapseAll: () => void;
  isLoading: boolean;
  error: string | null;
}
