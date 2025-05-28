import { CategoryDTO } from "@/services/categories"

/**
 * Utility functions for working with hierarchical categories
 */

/**
 * Finds a category by ID in hierarchical structure
 */
export function findCategoryById(categories: CategoryDTO[], id: number | string): CategoryDTO | undefined {
  const targetId = typeof id === 'string' ? parseInt(id) : id
  
  for (const category of categories) {
    if (category.id === targetId) {
      return category
    }
    if (category.subcategories) {
      const found = findCategoryById(category.subcategories, targetId)
      if (found) return found
    }
  }
  return undefined
}

/**
 * Gets the full path for a category as an array of categories
 */
export function getCategoryPath(categories: CategoryDTO[], categoryId: number | string): CategoryDTO[] {
  const targetId = typeof categoryId === 'string' ? parseInt(categoryId) : categoryId
  const path: CategoryDTO[] = []
  
  function findPath(cats: CategoryDTO[], targetId: number): boolean {
    for (const cat of cats) {
      path.push(cat)
      
      if (cat.id === targetId) {
        return true
      }
      
      if (cat.subcategories && findPath(cat.subcategories, targetId)) {
        return true
      }
      
      path.pop()
    }
    return false
  }
  
  findPath(categories, targetId)
  return path
}

/**
 * Gets the full path for a category as a string
 */
export function getCategoryPathString(
  categories: CategoryDTO[], 
  categoryId: number | string, 
  separator: string = " > "
): string {
  const path = getCategoryPath(categories, categoryId)
  return path.map(cat => cat.name).join(separator)
}

/**
 * Flattens hierarchical categories into a flat list with level and path information
 */
export interface FlatCategoryOption {
  category: CategoryDTO
  level: number
  path: CategoryDTO[]
  pathString: string
  hasChildren: boolean
  parentId?: number
}

export function flattenCategoriesWithMetadata(
  categories: CategoryDTO[], 
  level = 0, 
  parentPath: CategoryDTO[] = []
): FlatCategoryOption[] {
  const result: FlatCategoryOption[] = []
  
  categories.forEach(category => {
    const currentPath = [...parentPath, category]
    const hasChildren = category.subcategories && category.subcategories.length > 0
    
    result.push({
      category,
      level,
      path: currentPath,
      pathString: currentPath.map(cat => cat.name).join(" > "),
      hasChildren: hasChildren || false,
      parentId: category.parentId || undefined
    })
    
    if (hasChildren) {
      result.push(...flattenCategoriesWithMetadata(category.subcategories!, level + 1, currentPath))
    }
  })
  
  return result
}

/**
 * Gets all root categories (categories without parents)
 */
export function getRootCategories(categories: CategoryDTO[]): CategoryDTO[] {
  return categories.filter(cat => !cat.parentId)
}

/**
 * Gets all subcategories of a specific parent
 */
export function getSubcategories(categories: CategoryDTO[], parentId: number): CategoryDTO[] {
  const parent = findCategoryById(categories, parentId)
  return parent?.subcategories || []
}

/**
 * Gets all category IDs from hierarchical structure
 */
export function getAllCategoryIds(categories: CategoryDTO[]): number[] {
  const ids: number[] = []
  
  function collectIds(cats: CategoryDTO[]) {
    cats.forEach(cat => {
      ids.push(cat.id)
      if (cat.subcategories) {
        collectIds(cat.subcategories)
      }
    })
  }
  
  collectIds(categories)
  return ids
}

/**
 * Checks if a category is an ancestor of another category
 */
export function isAncestorOf(categories: CategoryDTO[], ancestorId: number, descendantId: number): boolean {
  const descendantPath = getCategoryPath(categories, descendantId)
  return descendantPath.some(cat => cat.id === ancestorId)
}

/**
 * Gets the depth of a category in the hierarchy
 */
export function getCategoryDepth(categories: CategoryDTO[], categoryId: number): number {
  const path = getCategoryPath(categories, categoryId)
  return path.length
}

/**
 * Gets the maximum depth of the category tree
 */
export function getMaxCategoryDepth(categories: CategoryDTO[]): number {
  let maxDepth = 0
  
  function findMaxDepth(cats: CategoryDTO[], currentDepth: number) {
    maxDepth = Math.max(maxDepth, currentDepth)
    cats.forEach(cat => {
      if (cat.subcategories) {
        findMaxDepth(cat.subcategories, currentDepth + 1)
      }
    })
  }
  
  findMaxDepth(categories, 1)
  return maxDepth
}

/**
 * Filters categories by search term (searches in name and path)
 */
export function searchCategories(
  categories: CategoryDTO[], 
  searchTerm: string, 
  includeAncestors: boolean = true
): CategoryDTO[] {
  if (!searchTerm.trim()) return categories
  
  const flatOptions = flattenCategoriesWithMetadata(categories)
  const matchingOptions = flatOptions.filter(option =>
    option.category.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
    option.pathString.toLowerCase().includes(searchTerm.toLowerCase())
  )
  
  if (!includeAncestors) {
    return matchingOptions.map(opt => opt.category)
  }
  
  // Include ancestors to maintain hierarchy
  const matchingIds = new Set(matchingOptions.map(opt => opt.category.id))
  
  // Add ancestors of matching categories
  matchingOptions.forEach(option => {
    option.path.forEach(ancestor => {
      matchingIds.add(ancestor.id)
    })
  })
  
  return flatOptions
    .filter(opt => matchingIds.has(opt.category.id))
    .map(opt => opt.category)
}

/**
 * Sorts categories by name while maintaining hierarchy
 */
export function sortCategoriesHierarchically(categories: CategoryDTO[]): CategoryDTO[] {
  return categories
    .map(category => ({
      ...category,
      subcategories: category.subcategories 
        ? sortCategoriesHierarchically(category.subcategories)
        : undefined
    }))
    .sort((a, b) => a.name.localeCompare(b.name))
}

/**
 * Validates that a category can be moved to a new parent (prevents circular references)
 */
export function canMoveCategory(
  categories: CategoryDTO[], 
  categoryId: number, 
  newParentId: number | null
): boolean {
  if (newParentId === null) return true // Moving to root is always valid
  if (categoryId === newParentId) return false // Can't be parent of itself
  
  // Check if the new parent is a descendant of the category being moved
  return !isAncestorOf(categories, categoryId, newParentId)
}

/**
 * Gets category statistics
 */
export interface CategoryStats {
  totalCategories: number
  rootCategories: number
  maxDepth: number
  averageDepth: number
  categoriesWithChildren: number
  leafCategories: number
}

export function getCategoryStats(categories: CategoryDTO[]): CategoryStats {
  const flatOptions = flattenCategoriesWithMetadata(categories)
  const depths = flatOptions.map(opt => opt.level + 1)
  
  return {
    totalCategories: flatOptions.length,
    rootCategories: categories.length,
    maxDepth: Math.max(...depths, 0),
    averageDepth: depths.length > 0 ? depths.reduce((a, b) => a + b, 0) / depths.length : 0,
    categoriesWithChildren: flatOptions.filter(opt => opt.hasChildren).length,
    leafCategories: flatOptions.filter(opt => !opt.hasChildren).length
  }
}
