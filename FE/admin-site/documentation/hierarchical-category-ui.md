# Hierarchical Category UI Components

## Overview

This document outlines the implementation of improved hierarchical category UI components for the DecorStore admin application. The new components provide clear visual representation of parent-child relationships between categories with multiple interaction patterns.

## Components Implemented

### 1. HierarchicalSelect (`components/ui/hierarchical-select.tsx`)

**Purpose**: Dropdown select with indented options showing hierarchical structure

**Features**:
- Visual indentation based on category level
- Full path display (e.g., "Furniture > Living Room > Sofas")
- Folder icons to distinguish parent/child categories
- Search functionality across category names and paths
- Clear selection option
- TypeScript support with proper CategoryDTO integration

**Usage**:
```tsx
<HierarchicalSelect
  categories={categories}
  value={selectedCategoryId}
  onValueChange={setSelectedCategoryId}
  placeholder="Select a category..."
  showPath={true}
  allowClear={true}
  className="w-full"
/>
```

**Props**:
- `categories`: CategoryDTO[] - Hierarchical category data
- `value`: number | string - Selected category ID
- `onValueChange`: Function to handle selection changes
- `placeholder`: String for placeholder text
- `showPath`: Boolean to show full category path
- `allowClear`: Boolean to allow clearing selection
- `leafOnly`: Boolean to only show categories without children
- `disabled`: Boolean to disable the component

### 2. CategoryTree (`components/ui/category-tree.tsx`)

**Purpose**: Expandable tree view for category navigation and selection

**Features**:
- Expandable/collapsible tree structure
- Single or multi-select modes
- Optional checkboxes for multi-selection
- Expand/Collapse all functionality
- Visual hierarchy with indentation
- Product count badges (when available)
- Click handlers for custom actions

**Usage**:
```tsx
// Single select mode
<CategoryTree
  categories={categories}
  selectedIds={selectedIds}
  onSelectionChange={setSelectedIds}
  multiSelect={false}
  showCheckboxes={false}
  expandedByDefault={false}
/>

// Multi-select mode with checkboxes
<CategoryTree
  categories={categories}
  selectedIds={selectedIds}
  onSelectionChange={setSelectedIds}
  multiSelect={true}
  showCheckboxes={true}
  expandedByDefault={true}
/>
```

### 3. CategoryBreadcrumbSelect (`components/ui/category-breadcrumb-select.tsx`)

**Purpose**: Step-by-step category selection with breadcrumb navigation

**Features**:
- Level-by-level category selection
- Breadcrumb-style navigation
- Clear individual levels or entire selection
- Visual hierarchy representation
- Home icon for root level indication

**Usage**:
```tsx
<CategoryBreadcrumbSelect
  categories={categories}
  value={selectedCategoryId}
  onValueChange={setSelectedCategoryId}
  placeholder="Choose category level by level..."
  allowClear={true}
/>
```

### 4. CategoryBreadcrumbDisplay (`components/ui/category-breadcrumb-select.tsx`)

**Purpose**: Display-only component showing category path

**Features**:
- Read-only breadcrumb display
- Customizable separator
- Home icon for root indication
- Responsive design

**Usage**:
```tsx
<CategoryBreadcrumbDisplay
  categories={categories}
  categoryId={selectedCategoryId}
  separator={<ChevronRight className="h-3 w-3" />}
  className="text-sm"
/>
```

## Utility Functions (`lib/category-utils.ts`)

### Core Functions
- `findCategoryById()` - Find category by ID in hierarchical structure
- `getCategoryPath()` - Get full path as array of categories
- `getCategoryPathString()` - Get path as formatted string
- `flattenCategoriesWithMetadata()` - Flatten hierarchy with metadata
- `searchCategories()` - Search with hierarchy preservation

### Helper Functions
- `getRootCategories()` - Get top-level categories
- `getSubcategories()` - Get children of specific parent
- `getAllCategoryIds()` - Extract all IDs from hierarchy
- `isAncestorOf()` - Check ancestor relationships
- `getCategoryDepth()` - Calculate category depth
- `canMoveCategory()` - Validate move operations

## Integration with Existing Components

### Updated Components

1. **CategoryForm.tsx**
   - Replaced basic Select with HierarchicalSelect
   - Shows full category path for parent selection
   - Prevents circular references (category can't be parent of itself)

2. **ProductForm.tsx**
   - Enhanced category selection with hierarchical display
   - Full path visibility for better context
   - Improved user experience for category assignment

3. **CategoryFilters.tsx**
   - Hierarchical parent category filtering
   - Path-based search and selection
   - Clear visual hierarchy in filter options

4. **ProductFilters.tsx**
   - Enhanced category filtering with hierarchy
   - Search across category paths
   - Better category context for filtering

## Design Patterns

### Visual Hierarchy Indicators
- **Indentation**: 16px per level for clear nesting
- **Icons**: Folder icons (open/closed) for parent categories
- **Typography**: Bold for root categories, normal for children
- **Colors**: Blue for parent folders, gray for leaf categories

### Interaction Patterns
- **Hover States**: Subtle background changes
- **Selection States**: Accent background for selected items
- **Loading States**: Skeleton components during data fetch
- **Error States**: Clear error messaging with retry options

### Responsive Design
- **Mobile**: Collapsible sections, touch-friendly targets
- **Desktop**: Full tree expansion, hover interactions
- **Tablet**: Hybrid approach with optimized spacing

## API Integration

### Endpoints Used
- `GET /api/Category/hierarchical` - Primary data source
- Existing CRUD endpoints for mutations
- Real-time updates through category store

### Data Structure
```typescript
interface CategoryDTO {
  id: number
  name: string
  slug: string
  description: string | null
  parentId: number | null
  subcategories?: CategoryDTO[]
  parentName?: string
  productCount?: number
  // ... other fields
}
```

## Performance Considerations

### Optimization Strategies
- **Memoization**: React.useMemo for expensive calculations
- **Virtual Scrolling**: For large category trees (future enhancement)
- **Lazy Loading**: Load subcategories on demand (future enhancement)
- **Search Debouncing**: Prevent excessive API calls

### Memory Management
- **Efficient Flattening**: Single-pass hierarchy flattening
- **Selective Rendering**: Only render visible tree nodes
- **Cleanup**: Proper component unmounting and state cleanup

## Accessibility Features

### ARIA Support
- Proper ARIA labels and descriptions
- Keyboard navigation support
- Screen reader compatibility
- Focus management

### Keyboard Navigation
- Arrow keys for tree navigation
- Enter/Space for selection
- Escape for closing dropdowns
- Tab order preservation

## Testing Strategy

### Unit Tests
- Component rendering with various props
- User interaction simulation
- Edge cases (empty data, deep nesting)
- Accessibility compliance

### Integration Tests
- Category store integration
- API data flow
- Real-time updates
- Cross-component communication

## Future Enhancements

### Planned Features
1. **Drag & Drop**: Reorder categories in tree view
2. **Bulk Operations**: Multi-select actions
3. **Advanced Search**: Fuzzy search with highlighting
4. **Virtual Scrolling**: Handle thousands of categories
5. **Lazy Loading**: Load subcategories on demand
6. **Export/Import**: Category hierarchy management

### Performance Improvements
1. **Virtualization**: For large datasets
2. **Caching**: Client-side category cache
3. **Prefetching**: Anticipatory data loading
4. **Compression**: Optimized data transfer

## Demo Page

A comprehensive demo page is available at `/categories/demo` showcasing all hierarchical components with interactive examples and documentation.

## Conclusion

The hierarchical category UI improvements provide a significant enhancement to the user experience when working with category data. The components offer multiple interaction patterns to suit different use cases while maintaining consistency with the existing design system.
