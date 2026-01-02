"use client";

import { useState } from "react";
import {
  Plus,
  Trash2,
  Percent,
  DollarSign,
  Package,
  Tag,
  Building2,
  Users,
  ShieldCheck,
  Pencil,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { DiscountRuleForm } from "./discount-rule-form";
import { CategorySelectorDialog } from "./category-selector-dialog";
import { BrandSelectorDialog } from "./brand-selector-dialog";
import { CustomerSelectorDialog } from "./customer-selector-dialog";
import {
  useRemoveDiscountRule,
  useAddDiscountRule,
  useAddCategoriesToRule,
  useAddBrandsToRule,
  useAddCustomersToRule,
} from "@/hooks/use-campaigns";
import {
  Campaign,
  DiscountRule,
  DiscountTypeLabels,
  ProductTargetTypeLabels,
  CustomerTargetTypeLabels,
  PriceTierLabels,
  CreateDiscountRuleDto,
} from "@/types/entities";
import { formatCurrency } from "@/lib/utils";

interface DiscountRulesEditorProps {
  campaign: Campaign;
}

export function DiscountRulesEditor({ campaign }: DiscountRulesEditorProps) {
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);
  const [deleteRuleId, setDeleteRuleId] = useState<string | null>(null);

  const addRule = useAddDiscountRule();
  const removeRule = useRemoveDiscountRule();

  const handleAddRule = async (data: CreateDiscountRuleDto) => {
    await addRule.mutateAsync({
      campaignId: campaign.id,
      data,
    });
    setIsAddDialogOpen(false);
  };

  const handleRemoveRule = async () => {
    if (deleteRuleId) {
      await removeRule.mutateAsync({
        campaignId: campaign.id,
        ruleId: deleteRuleId,
      });
      setDeleteRuleId(null);
    }
  };

  const canEdit = campaign.status === "Draft" || campaign.status === "Scheduled";

  return (
    <Card>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <CardTitle>Discount Rules</CardTitle>
            <CardDescription>
              Configure which products and customers receive discounts
            </CardDescription>
          </div>
          {canEdit && (
            <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
              <DialogTrigger asChild>
                <Button>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Rule
                </Button>
              </DialogTrigger>
              <DialogContent className="max-w-2xl max-h-[90vh] overflow-y-auto">
                <DialogHeader>
                  <DialogTitle>Add Discount Rule</DialogTitle>
                  <DialogDescription>
                    Configure the discount type, targeting, and conditions
                  </DialogDescription>
                </DialogHeader>
                <DiscountRuleForm
                  onSubmit={handleAddRule}
                  onCancel={() => setIsAddDialogOpen(false)}
                  isLoading={addRule.isPending}
                />
              </DialogContent>
            </Dialog>
          )}
        </div>
      </CardHeader>
      <CardContent>
        {campaign.discountRules.length === 0 ? (
          <div className="flex flex-col items-center justify-center py-10 text-center">
            <Package className="h-12 w-12 text-muted-foreground mb-4" />
            <p className="text-muted-foreground">No discount rules configured</p>
            {canEdit && (
              <p className="text-sm text-muted-foreground mt-1">
                Add a discount rule to start applying discounts
              </p>
            )}
          </div>
        ) : (
          <div className="space-y-4">
            {campaign.discountRules.map((rule, index) => (
              <DiscountRuleCard
                key={rule.id}
                rule={rule}
                index={index}
                canEdit={canEdit}
                onDelete={() => setDeleteRuleId(rule.id)}
                currency={campaign.totalDiscountUsedCurrency}
                campaignId={campaign.id}
              />
            ))}
          </div>
        )}
      </CardContent>

      <ConfirmDialog
        open={!!deleteRuleId}
        onOpenChange={(open) => !open && setDeleteRuleId(null)}
        title="Remove Discount Rule"
        description="Are you sure you want to remove this discount rule? This action cannot be undone."
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleRemoveRule}
        isLoading={removeRule.isPending}
      />
    </Card>
  );
}

interface DiscountRuleCardProps {
  rule: DiscountRule;
  index: number;
  canEdit: boolean;
  onDelete: () => void;
  currency: string;
  campaignId: string;
}

function DiscountRuleCard({
  rule,
  index,
  canEdit,
  onDelete,
  currency,
  campaignId,
}: DiscountRuleCardProps) {
  const [categorySelectorOpen, setCategorySelectorOpen] = useState(false);
  const [brandSelectorOpen, setBrandSelectorOpen] = useState(false);
  const [customerSelectorOpen, setCustomerSelectorOpen] = useState(false);

  const addCategories = useAddCategoriesToRule();
  const addBrands = useAddBrandsToRule();
  const addCustomers = useAddCustomersToRule();

  const handleSaveCategories = async (categoryIds: string[]) => {
    await addCategories.mutateAsync({
      campaignId,
      ruleId: rule.id,
      categoryIds,
    });
    setCategorySelectorOpen(false);
  };

  const handleSaveBrands = async (brandIds: string[]) => {
    await addBrands.mutateAsync({
      campaignId,
      ruleId: rule.id,
      brandIds,
    });
    setBrandSelectorOpen(false);
  };

  const handleSaveCustomers = async (customerIds: string[]) => {
    await addCustomers.mutateAsync({
      campaignId,
      ruleId: rule.id,
      customerIds,
    });
    setCustomerSelectorOpen(false);
  };

  // Check if we need to show edit buttons
  const needsCategorySelection =
    rule.productTargetType === "Categories" && canEdit;
  const needsBrandSelection = rule.productTargetType === "Brands" && canEdit;
  const needsCustomerSelection =
    rule.customerTargetType === "SpecificCustomers" && canEdit;

  return (
    <>
      <div className="rounded-lg border p-4 space-y-4">
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-3">
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-primary/10 text-primary font-medium text-sm">
              {index + 1}
            </div>
            <div>
              <div className="flex items-center gap-2">
                {rule.discountType === "Percentage" ? (
                  <Percent className="h-4 w-4 text-muted-foreground" />
                ) : (
                  <DollarSign className="h-4 w-4 text-muted-foreground" />
                )}
                <span className="font-semibold">
                  {rule.discountType === "Percentage"
                    ? `${rule.discountValue}% off`
                    : formatCurrency(rule.discountValue, currency)}
                </span>
                {rule.maxDiscountAmount && rule.discountType === "Percentage" && (
                  <Badge variant="outline" className="text-xs">
                    Max: {formatCurrency(rule.maxDiscountAmount, currency)}
                  </Badge>
                )}
              </div>
              <p className="text-sm text-muted-foreground">
                {DiscountTypeLabels[rule.discountType]}
              </p>
            </div>
          </div>
          {canEdit && (
            <Button variant="ghost" size="icon" onClick={onDelete}>
              <Trash2 className="h-4 w-4 text-muted-foreground hover:text-destructive" />
            </Button>
          )}
        </div>

        <div className="grid gap-4 md:grid-cols-2">
          {/* Product Targeting */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-sm font-medium">
                <Package className="h-4 w-4 text-muted-foreground" />
                Products
              </div>
              {needsCategorySelection && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 text-xs"
                  onClick={() => setCategorySelectorOpen(true)}
                >
                  <Pencil className="h-3 w-3 mr-1" />
                  Edit
                </Button>
              )}
              {needsBrandSelection && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 text-xs"
                  onClick={() => setBrandSelectorOpen(true)}
                >
                  <Pencil className="h-3 w-3 mr-1" />
                  Edit
                </Button>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              <Badge variant="secondary">
                {ProductTargetTypeLabels[rule.productTargetType]}
              </Badge>
              {rule.productTargetType === "SpecificProducts" &&
                rule.products.slice(0, 3).map((p) => (
                  <Badge key={p.productId} variant="outline" className="text-xs">
                    {p.productName || p.productSku}
                  </Badge>
                ))}
              {rule.productTargetType === "SpecificProducts" &&
                rule.products.length > 3 && (
                  <Badge variant="outline" className="text-xs">
                    +{rule.products.length - 3} more
                  </Badge>
                )}
              {rule.productTargetType === "Categories" &&
                (rule.categories.length > 0 ? (
                  rule.categories.map((c) => (
                    <Badge key={c.categoryId} variant="outline" className="text-xs">
                      <Tag className="h-3 w-3 mr-1" />
                      {c.categoryName}
                    </Badge>
                  ))
                ) : (
                  <Badge variant="destructive" className="text-xs">
                    No categories selected
                  </Badge>
                ))}
              {rule.productTargetType === "Brands" &&
                (rule.brands.length > 0 ? (
                  rule.brands.map((b) => (
                    <Badge key={b.brandId} variant="outline" className="text-xs">
                      <Building2 className="h-3 w-3 mr-1" />
                      {b.brandName}
                    </Badge>
                  ))
                ) : (
                  <Badge variant="destructive" className="text-xs">
                    No brands selected
                  </Badge>
                ))}
            </div>
          </div>

          {/* Customer Targeting */}
          <div className="space-y-2">
            <div className="flex items-center justify-between">
              <div className="flex items-center gap-2 text-sm font-medium">
                <Users className="h-4 w-4 text-muted-foreground" />
                Customers
              </div>
              {needsCustomerSelection && (
                <Button
                  variant="ghost"
                  size="sm"
                  className="h-7 text-xs"
                  onClick={() => setCustomerSelectorOpen(true)}
                >
                  <Pencil className="h-3 w-3 mr-1" />
                  Edit
                </Button>
              )}
            </div>
            <div className="flex flex-wrap gap-2">
              <Badge variant="secondary">
                {CustomerTargetTypeLabels[rule.customerTargetType]}
              </Badge>
              {rule.customerTargetType === "SpecificCustomers" &&
                (rule.customers.length > 0 ? (
                  <>
                    {rule.customers.slice(0, 3).map((c) => (
                      <Badge key={c.customerId} variant="outline" className="text-xs">
                        {c.customerTitle}
                      </Badge>
                    ))}
                    {rule.customers.length > 3 && (
                      <Badge variant="outline" className="text-xs">
                        +{rule.customers.length - 3} more
                      </Badge>
                    )}
                  </>
                ) : (
                  <Badge variant="destructive" className="text-xs">
                    No customers selected
                  </Badge>
                ))}
              {rule.customerTargetType === "CustomerTiers" &&
                rule.customerTiers.map((t) => (
                  <Badge key={t.priceTier} variant="outline" className="text-xs">
                    <ShieldCheck className="h-3 w-3 mr-1" />
                    {PriceTierLabels[t.priceTier]}
                  </Badge>
                ))}
            </div>
          </div>
        </div>

        {/* Conditions */}
        {(rule.minOrderAmount || rule.minQuantity) && (
          <div className="flex gap-4 text-sm text-muted-foreground">
            {rule.minOrderAmount && (
              <span>Min order: {formatCurrency(rule.minOrderAmount, currency)}</span>
            )}
            {rule.minQuantity && <span>Min quantity: {rule.minQuantity}</span>}
          </div>
        )}
      </div>

      {/* Selector Dialogs */}
      <CategorySelectorDialog
        open={categorySelectorOpen}
        onOpenChange={setCategorySelectorOpen}
        selectedIds={rule.categories.map((c) => c.categoryId)}
        onSave={handleSaveCategories}
        isLoading={addCategories.isPending}
      />

      <BrandSelectorDialog
        open={brandSelectorOpen}
        onOpenChange={setBrandSelectorOpen}
        selectedIds={rule.brands.map((b) => b.brandId)}
        onSave={handleSaveBrands}
        isLoading={addBrands.isPending}
      />

      <CustomerSelectorDialog
        open={customerSelectorOpen}
        onOpenChange={setCustomerSelectorOpen}
        selectedIds={rule.customers.map((c) => c.customerId)}
        onSave={handleSaveCustomers}
        isLoading={addCustomers.isPending}
      />
    </>
  );
}
