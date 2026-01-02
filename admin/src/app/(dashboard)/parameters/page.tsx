"use client";

import { useState, useMemo } from "react";
import {
  Plus,
  Search,
  MoreHorizontal,
  Pencil,
  Trash2,
  Settings2,
  RefreshCw,
  ChevronDown,
  ChevronRight,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { ParameterForm } from "@/components/parameters/parameter-form";
import {
  useParameters,
  useParameterCategories,
  useCreateParameter,
  useUpdateParameter,
  useDeleteParameter,
} from "@/hooks/use-parameters";
import {
  type CreateParameterFormData,
  type UpdateParameterFormData,
} from "@/lib/validations/parameter";
import {
  type ParameterListItem,
  type ParameterFilters,
  type ParameterType,
  ParameterTypeLabels,
  ParameterValueTypeLabels,
} from "@/types/entities";
import { useDebounce } from "@/hooks/use-debounce";

export default function ParametersPage() {
  const [filters, setFilters] = useState<ParameterFilters>({
    page: 1,
    pageSize: 100, // Load more to enable grouping
  });
  const [searchQuery, setSearchQuery] = useState("");
  const debouncedSearch = useDebounce(searchQuery, 300);

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingParameter, setEditingParameter] = useState<ParameterListItem | null>(null);
  const [deletingParameter, setDeletingParameter] = useState<ParameterListItem | null>(null);
  const [expandedCategories, setExpandedCategories] = useState<Set<string>>(new Set());

  const { data, isLoading, isFetching, refetch } = useParameters({
    ...filters,
    search: debouncedSearch || undefined,
  });
  const { data: categories } = useParameterCategories();
  const createParameter = useCreateParameter();
  const updateParameter = useUpdateParameter();
  const deleteParameter = useDeleteParameter();

  // Group parameters by category
  const groupedParameters = useMemo(() => {
    if (!data?.items) return new Map<string, ParameterListItem[]>();

    const groups = new Map<string, ParameterListItem[]>();
    for (const param of data.items) {
      const category = param.category || "uncategorized";
      if (!groups.has(category)) {
        groups.set(category, []);
      }
      groups.get(category)!.push(param);
    }

    // Sort categories alphabetically
    return new Map([...groups.entries()].sort((a, b) => a[0].localeCompare(b[0])));
  }, [data?.items]);

  // Expand all categories by default when data loads
  useMemo(() => {
    if (groupedParameters.size > 0 && expandedCategories.size === 0) {
      setExpandedCategories(new Set(groupedParameters.keys()));
    }
  }, [groupedParameters, expandedCategories.size]);

  const toggleCategory = (category: string) => {
    setExpandedCategories((prev) => {
      const next = new Set(prev);
      if (next.has(category)) {
        next.delete(category);
      } else {
        next.add(category);
      }
      return next;
    });
  };

  const handleAddNew = () => {
    setIsCreateOpen(true);
  };

  const handleEdit = (param: ParameterListItem) => {
    setEditingParameter(param);
  };

  const handleCreateSubmit = async (formData: CreateParameterFormData) => {
    await createParameter.mutateAsync({
      key: formData.key,
      value: formData.value,
      description: formData.description || undefined,
      parameterType: formData.parameterType,
      valueType: formData.valueType,
      isEditable: formData.isEditable,
    });
    setIsCreateOpen(false);
  };

  const handleUpdateSubmit = async (formData: UpdateParameterFormData) => {
    if (editingParameter) {
      await updateParameter.mutateAsync({
        id: editingParameter.id,
        data: {
          value: formData.value,
          description: formData.description,
          isEditable: formData.isEditable,
        },
      });
      setEditingParameter(null);
    }
  };

  const handleDelete = async () => {
    if (deletingParameter) {
      await deleteParameter.mutateAsync(deletingParameter.id);
      setDeletingParameter(null);
    }
  };

  const handleTypeFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      parameterType: value === "all" ? undefined : (value as ParameterType),
      page: 1,
    }));
  };

  const handleCategoryFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      category: value === "all" ? undefined : value,
      page: 1,
    }));
  };

  const getValueTypeColor = (valueType: string) => {
    switch (valueType) {
      case "Number":
        return "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200";
      case "Boolean":
        return "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200";
      case "DateTime":
        return "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200";
      case "Json":
        return "bg-amber-100 text-amber-800 dark:bg-amber-900 dark:text-amber-200";
      default:
        return "bg-gray-100 text-gray-800 dark:bg-gray-800 dark:text-gray-200";
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Parameters</h1>
          <p className="text-muted-foreground">
            Manage system and business configuration parameters
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Parameter
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Parameters</CardTitle>
          <CardDescription>
            System and business configuration settings grouped by category
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search parameters..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select
              value={filters.parameterType || "all"}
              onValueChange={handleTypeFilterChange}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                <SelectItem value="System">System</SelectItem>
                <SelectItem value="Business">Business</SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={filters.category || "all"}
              onValueChange={handleCategoryFilterChange}
            >
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="Category" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Categories</SelectItem>
                {categories?.map((cat) => (
                  <SelectItem key={cat} value={cat}>
                    {cat}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Button
              variant="outline"
              size="icon"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`} />
            </Button>
          </div>

          {/* Parameters Table Grouped by Category */}
          {isLoading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : groupedParameters.size > 0 ? (
            <div className="space-y-4">
              {Array.from(groupedParameters.entries()).map(([category, params]) => (
                <Collapsible
                  key={category}
                  open={expandedCategories.has(category)}
                  onOpenChange={() => toggleCategory(category)}
                >
                  <CollapsibleTrigger asChild>
                    <Button
                      variant="ghost"
                      className="w-full justify-between h-12 px-4 bg-muted/50 hover:bg-muted"
                    >
                      <div className="flex items-center gap-2">
                        {expandedCategories.has(category) ? (
                          <ChevronDown className="h-4 w-4" />
                        ) : (
                          <ChevronRight className="h-4 w-4" />
                        )}
                        <span className="font-semibold capitalize">{category}</span>
                        <Badge variant="secondary" className="ml-2">
                          {params.length}
                        </Badge>
                      </div>
                    </Button>
                  </CollapsibleTrigger>
                  <CollapsibleContent>
                    <div className="rounded-md border mt-2">
                      <Table>
                        <TableHeader>
                          <TableRow>
                            <TableHead className="w-[300px]">Key</TableHead>
                            <TableHead>Value</TableHead>
                            <TableHead className="w-[100px] text-center">Type</TableHead>
                            <TableHead className="w-[100px] text-center">Value Type</TableHead>
                            <TableHead className="w-[100px] text-center">Editable</TableHead>
                            <TableHead className="w-[70px]"></TableHead>
                          </TableRow>
                        </TableHeader>
                        <TableBody>
                          {params.map((param) => (
                            <TableRow key={param.id}>
                              <TableCell>
                                <div>
                                  <div className="font-mono text-sm">{param.key}</div>
                                  {param.description && (
                                    <div className="text-xs text-muted-foreground mt-1 line-clamp-1">
                                      {param.description}
                                    </div>
                                  )}
                                </div>
                              </TableCell>
                              <TableCell>
                                <span className="font-mono text-sm line-clamp-1 max-w-[300px]">
                                  {param.valueType === "Boolean" ? (
                                    <Badge variant={param.value === "true" ? "default" : "secondary"}>
                                      {param.value}
                                    </Badge>
                                  ) : (
                                    param.value
                                  )}
                                </span>
                              </TableCell>
                              <TableCell className="text-center">
                                <Badge
                                  variant={param.parameterType === "System" ? "default" : "outline"}
                                >
                                  {ParameterTypeLabels[param.parameterType]}
                                </Badge>
                              </TableCell>
                              <TableCell className="text-center">
                                <Badge className={getValueTypeColor(param.valueType)}>
                                  {ParameterValueTypeLabels[param.valueType]}
                                </Badge>
                              </TableCell>
                              <TableCell className="text-center">
                                <Badge variant={param.isEditable ? "outline" : "secondary"}>
                                  {param.isEditable ? "Yes" : "No"}
                                </Badge>
                              </TableCell>
                              <TableCell>
                                <DropdownMenu>
                                  <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" size="icon">
                                      <MoreHorizontal className="h-4 w-4" />
                                    </Button>
                                  </DropdownMenuTrigger>
                                  <DropdownMenuContent align="end">
                                    <DropdownMenuItem onClick={() => handleEdit(param)}>
                                      <Pencil className="mr-2 h-4 w-4" />
                                      Edit
                                    </DropdownMenuItem>
                                    <DropdownMenuSeparator />
                                    <DropdownMenuItem
                                      className="text-destructive"
                                      onClick={() => setDeletingParameter(param)}
                                    >
                                      <Trash2 className="mr-2 h-4 w-4" />
                                      Delete
                                    </DropdownMenuItem>
                                  </DropdownMenuContent>
                                </DropdownMenu>
                              </TableCell>
                            </TableRow>
                          ))}
                        </TableBody>
                      </Table>
                    </div>
                  </CollapsibleContent>
                </Collapsible>
              ))}
            </div>
          ) : (
            <div className="text-center py-10">
              <Settings2 className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No parameters</h3>
              <p className="text-muted-foreground">
                {searchQuery
                  ? `No parameters match "${searchQuery}"`
                  : "Get started by creating your first parameter."}
              </p>
              {!searchQuery && (
                <Button className="mt-4" onClick={handleAddNew}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Parameter
                </Button>
              )}
            </div>
          )}

          {/* Total count */}
          {data && data.totalCount > 0 && (
            <div className="mt-4 text-sm text-muted-foreground">
              Total: {data.totalCount} parameters
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create Parameter Dialog */}
      <Dialog open={isCreateOpen} onOpenChange={setIsCreateOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Add Parameter</DialogTitle>
            <DialogDescription>
              Create a new system or business configuration parameter.
            </DialogDescription>
          </DialogHeader>
          <ParameterForm
            mode="create"
            onSubmit={handleCreateSubmit}
            onCancel={() => setIsCreateOpen(false)}
            isLoading={createParameter.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Edit Parameter Dialog */}
      <Dialog open={!!editingParameter} onOpenChange={(open) => !open && setEditingParameter(null)}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Edit Parameter</DialogTitle>
            <DialogDescription>
              Update the parameter value and settings.
            </DialogDescription>
          </DialogHeader>
          {editingParameter && (
            <ParameterForm
              mode="update"
              defaultValues={{
                key: editingParameter.key,
                value: editingParameter.value,
                description: editingParameter.description,
                parameterType: editingParameter.parameterType,
                valueType: editingParameter.valueType,
                isEditable: editingParameter.isEditable,
              }}
              onSubmit={handleUpdateSubmit}
              onCancel={() => setEditingParameter(null)}
              isLoading={updateParameter.isPending}
            />
          )}
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingParameter}
        onOpenChange={(open) => !open && setDeletingParameter(null)}
        title="Delete Parameter"
        description={`Are you sure you want to delete "${deletingParameter?.key}"? This action cannot be undone.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteParameter.isPending}
      />
    </div>
  );
}
