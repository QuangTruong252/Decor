"use client"

import * as React from "react"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card"
import { HierarchicalSelect } from "@/components/ui/hierarchical-select"
import { CategoryTree } from "@/components/ui/category-tree"
import { CategoryBreadcrumbSelect, CategoryBreadcrumbDisplay } from "@/components/ui/category-breadcrumb-select"
import { useHierarchicalCategories } from "@/hooks/useCategoryStore"
import { Button } from "@/components/ui/button"
import { Separator } from "@/components/ui/separator"

export default function CategoryHierarchyDemo() {
  const { data: categories, isLoading } = useHierarchicalCategories()

  // State for different components
  const [hierarchicalSelectValue, setHierarchicalSelectValue] = React.useState<number | undefined>()
  const [breadcrumbSelectValue, setBreadcrumbSelectValue] = React.useState<number | undefined>()
  const [treeSelectedIds, setTreeSelectedIds] = React.useState<number[]>([])
  const [multiTreeSelectedIds, setMultiTreeSelectedIds] = React.useState<number[]>([])

  if (isLoading) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">Loading categories...</div>
      </div>
    )
  }

  if (!categories || categories.length === 0) {
    return (
      <div className="container mx-auto p-6">
        <div className="text-center">No categories available</div>
      </div>
    )
  }

  return (
    <div className="container mx-auto p-6 space-y-8">
      <div className="space-y-2">
        <h1 className="text-3xl font-bold">Category Hierarchy Components Demo</h1>
        <p className="text-muted-foreground">
          Demonstration of various hierarchical category selection components
        </p>
      </div>

      <Separator />

      {/* Hierarchical Select */}
      <Card>
        <CardHeader>
          <CardTitle>Hierarchical Select</CardTitle>
          <CardDescription>
            Dropdown with indented options showing parent-child relationships and full paths
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div className="space-y-2">
              <label className="text-sm font-medium">All Categories (With Path)</label>
              <HierarchicalSelect
                categories={categories}
                value={hierarchicalSelectValue}
                onValueChange={(value) => setHierarchicalSelectValue(value as number | undefined)}
                placeholder="Select a category..."
                showPath={true}
                allowClear={true}
                leafOnly={false}
                onScroll={(e) => console.log('Demo page scroll event:', e.target)}
              />
            </div>
            <div className="space-y-2">
              <label className="text-sm font-medium">Leaf Categories Only</label>
              <HierarchicalSelect
                categories={categories}
                value={hierarchicalSelectValue}
                onValueChange={(value) => setHierarchicalSelectValue(value as number | undefined)}
                placeholder="Select a category..."
                showPath={true}
                allowClear={true}
                leafOnly={true}
                onScroll={(e) => console.log('Demo page leaf scroll event:', e.target)}
              />
            </div>
          </div>

          {hierarchicalSelectValue && (
            <div className="p-3 bg-muted rounded-md">
              <p className="text-sm">
                <strong>Selected Category ID:</strong> {hierarchicalSelectValue}
              </p>
              <CategoryBreadcrumbDisplay
                categories={categories}
                categoryId={hierarchicalSelectValue}
                className="mt-2"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Breadcrumb Select */}
      <Card>
        <CardHeader>
          <CardTitle>Breadcrumb Select</CardTitle>
          <CardDescription>
            Step-by-step category selection with breadcrumb navigation
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CategoryBreadcrumbSelect
            categories={categories}
            value={breadcrumbSelectValue}
            onValueChange={setBreadcrumbSelectValue}
            placeholder="Choose category level by level..."
            allowClear={true}
          />

          {breadcrumbSelectValue && (
            <div className="p-3 bg-muted rounded-md">
              <p className="text-sm">
                <strong>Selected Category ID:</strong> {breadcrumbSelectValue}
              </p>
              <CategoryBreadcrumbDisplay
                categories={categories}
                categoryId={breadcrumbSelectValue}
                className="mt-2"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Category Tree - Single Select */}
      <Card>
        <CardHeader>
          <CardTitle>Category Tree (Single Select)</CardTitle>
          <CardDescription>
            Expandable tree view for single category selection
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CategoryTree
            categories={categories}
            selectedIds={treeSelectedIds}
            onSelectionChange={setTreeSelectedIds}
            multiSelect={false}
            showCheckboxes={false}
            expandedByDefault={false}
          />

          {treeSelectedIds.length > 0 && (
            <div className="p-3 bg-muted rounded-md">
              <p className="text-sm">
                <strong>Selected Category ID:</strong> {treeSelectedIds[0]}
              </p>
              <CategoryBreadcrumbDisplay
                categories={categories}
                categoryId={treeSelectedIds[0]}
                className="mt-2"
              />
            </div>
          )}
        </CardContent>
      </Card>

      {/* Category Tree - Multi Select */}
      <Card>
        <CardHeader>
          <CardTitle>Category Tree (Multi Select)</CardTitle>
          <CardDescription>
            Expandable tree view with checkboxes for multiple category selection
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <CategoryTree
            categories={categories}
            selectedIds={multiTreeSelectedIds}
            onSelectionChange={setMultiTreeSelectedIds}
            multiSelect={true}
            showCheckboxes={true}
            expandedByDefault={false}
          />

          {multiTreeSelectedIds.length > 0 && (
            <div className="p-3 bg-muted rounded-md">
              <p className="text-sm">
                <strong>Selected Categories ({multiTreeSelectedIds.length}):</strong>
              </p>
              <div className="mt-2 space-y-1">
                {multiTreeSelectedIds.map(id => (
                  <CategoryBreadcrumbDisplay
                    key={id}
                    categories={categories}
                    categoryId={id}
                    className="text-xs"
                  />
                ))}
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Reset All */}
      <Card>
        <CardHeader>
          <CardTitle>Actions</CardTitle>
        </CardHeader>
        <CardContent>
          <Button
            variant="outline"
            onClick={() => {
              setHierarchicalSelectValue(undefined)
              setBreadcrumbSelectValue(undefined)
              setTreeSelectedIds([])
              setMultiTreeSelectedIds([])
            }}
          >
            Reset All Selections
          </Button>
        </CardContent>
      </Card>
    </div>
  )
}
