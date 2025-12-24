"use client";

import { useState, useCallback, useEffect } from "react";
import { Plus, Trash2, Save, Users, Building, Package, Landmark, Shield, CreditCard, AlertCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import {
  useCustomerAttributes,
  useUpsertCustomerAttributes,
} from "@/hooks/use-customers";
import {
  ShareholderData,
  BusinessPartnerData,
  ProductCategoryData,
  BankAccountData,
  CollateralData,
  PaymentPreferenceData,
} from "@/types/entities";
import { toast } from "sonner";

// Extended types that include the attribute ID for updates
interface AttributeItem<T> {
  id?: string;
  data: T;
}

// Dirty state for each section
interface DirtyState {
  shareholders: boolean;
  businessPartners: boolean;
  productCategories: boolean;
  bankAccounts: boolean;
  collaterals: boolean;
  paymentPreferences: boolean;
}

// Predefined options for checkboxes
const PRODUCT_CATEGORIES = [
  "Bilgisayarlar",
  "Bilgisayar Parçaları",
  "Oyuncu Ürünleri",
  "Yazılım",
  "İş İstasyonları",
  "Çevre Birimler",
  "Ağ Ürünleri",
  "Aksesuarlar",
  "Sunucular",
  "Veri Depolama",
  "Baskı Çözümleri",
  "Diğer",
];

const PAYMENT_PREFERENCES = [
  "Nakit & Kredi Kartı",
  "Açık Hesap & Kredili",
  "Çek",
];

const PAYMENT_TERMS = [
  "Peşin",
  "15 Gün",
  "30 Gün",
  "45 Gün",
  "60 Gün",
  "90 Gün",
];

const COLLATERAL_TYPES = [
  "Nakit Teminat",
  "Banka Teminat Mektubu",
  "Gayrimenkul İpoteği",
  "Kefalet",
  "Diğer",
];

const CURRENCIES = ["TRY", "USD", "EUR"];

interface CustomerAttributesEditorProps {
  customerId: string;
}

export function CustomerAttributesEditor({
  customerId,
}: CustomerAttributesEditorProps) {
  const { data: attributes, isLoading } = useCustomerAttributes(customerId);
  const upsertMutation = useUpsertCustomerAttributes();

  // Local state for each attribute type - now includes IDs for updates
  const [shareholders, setShareholders] = useState<AttributeItem<ShareholderData>[]>([]);
  const [businessPartners, setBusinessPartners] = useState<AttributeItem<BusinessPartnerData>[]>([]);
  const [productCategories, setProductCategories] = useState<{ id?: string; categories: string[] }>({ categories: [] });
  const [bankAccounts, setBankAccounts] = useState<AttributeItem<BankAccountData>[]>([]);
  const [collaterals, setCollaterals] = useState<AttributeItem<CollateralData>[]>([]);
  const [paymentPreferences, setPaymentPreferences] = useState<{ id?: string; preferences: string[] }>({ preferences: [] });

  // Track dirty state per section
  const [dirtyState, setDirtyState] = useState<DirtyState>({
    shareholders: false,
    businessPartners: false,
    productCategories: false,
    bankAccounts: false,
    collaterals: false,
    paymentPreferences: false,
  });

  // Helper to mark a section as dirty
  const markDirty = (section: keyof DirtyState) => {
    setDirtyState(prev => ({ ...prev, [section]: true }));
  };

  // Helper to mark a section as clean
  const markClean = (section: keyof DirtyState) => {
    setDirtyState(prev => ({ ...prev, [section]: false }));
  };

  // Check if any section has unsaved changes
  const hasUnsavedChanges = Object.values(dirtyState).some(Boolean);

  // Count of dirty sections
  const dirtyCount = Object.values(dirtyState).filter(Boolean).length;

  // Parse attributes on load
  useEffect(() => {
    if (attributes) {
      const shareholderAttrs = attributes.filter(
        (a) => a.attributeType === "ShareholderOrDirector"
      );
      const partnerAttrs = attributes.filter(
        (a) => a.attributeType === "BusinessPartner"
      );
      const categoryAttrs = attributes.filter(
        (a) => a.attributeType === "ProductCategory"
      );
      const bankAttrs = attributes.filter(
        (a) => a.attributeType === "BankAccount"
      );
      const collateralAttrs = attributes.filter(
        (a) => a.attributeType === "Collateral"
      );
      const preferenceAttrs = attributes.filter(
        (a) => a.attributeType === "PaymentPreference"
      );

      setShareholders(
        shareholderAttrs.map((a) => ({
          id: a.id,
          data: JSON.parse(a.jsonData) as ShareholderData,
        }))
      );
      setBusinessPartners(
        partnerAttrs.map((a) => ({
          id: a.id,
          data: JSON.parse(a.jsonData) as BusinessPartnerData,
        }))
      );
      setBankAccounts(
        bankAttrs.map((a) => ({
          id: a.id,
          data: JSON.parse(a.jsonData) as BankAccountData,
        }))
      );
      setCollaterals(
        collateralAttrs.map((a) => ({
          id: a.id,
          data: JSON.parse(a.jsonData) as CollateralData,
        }))
      );

      // Product categories and payment preferences are stored as single items with arrays
      if (categoryAttrs.length > 0) {
        const data = JSON.parse(categoryAttrs[0].jsonData) as ProductCategoryData;
        setProductCategories({ id: categoryAttrs[0].id, categories: data.categories || [] });
      } else {
        setProductCategories({ categories: [] });
      }

      if (preferenceAttrs.length > 0) {
        const data = JSON.parse(preferenceAttrs[0].jsonData) as PaymentPreferenceData;
        setPaymentPreferences({ id: preferenceAttrs[0].id, preferences: data.preferences || [] });
      } else {
        setPaymentPreferences({ preferences: [] });
      }

      // Reset all dirty states
      setDirtyState({
        shareholders: false,
        businessPartners: false,
        productCategories: false,
        bankAccounts: false,
        collaterals: false,
        paymentPreferences: false,
      });
    }
  }, [attributes]);

  // Save handlers
  const saveShareholders = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "ShareholderOrDirector",
        items: shareholders.map((s, i) => ({
          id: s.id,
          displayOrder: i,
          jsonData: JSON.stringify(s.data),
        })),
      },
    });
    markClean("shareholders");
  }, [customerId, shareholders, upsertMutation]);

  const saveBusinessPartners = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "BusinessPartner",
        items: businessPartners.map((p, i) => ({
          id: p.id,
          displayOrder: i,
          jsonData: JSON.stringify(p.data),
        })),
      },
    });
    markClean("businessPartners");
  }, [customerId, businessPartners, upsertMutation]);

  const saveProductCategories = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "ProductCategory",
        items:
          productCategories.categories.length > 0
            ? [
                {
                  id: productCategories.id,
                  displayOrder: 0,
                  jsonData: JSON.stringify({ categories: productCategories.categories }),
                },
              ]
            : [],
      },
    });
    markClean("productCategories");
  }, [customerId, productCategories, upsertMutation]);

  const saveBankAccounts = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "BankAccount",
        items: bankAccounts.map((b, i) => ({
          id: b.id,
          displayOrder: i,
          jsonData: JSON.stringify(b.data),
        })),
      },
    });
    markClean("bankAccounts");
  }, [customerId, bankAccounts, upsertMutation]);

  const saveCollaterals = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "Collateral",
        items: collaterals.map((c, i) => ({
          id: c.id,
          displayOrder: i,
          jsonData: JSON.stringify(c.data),
        })),
      },
    });
    markClean("collaterals");
  }, [customerId, collaterals, upsertMutation]);

  const savePaymentPreferences = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "PaymentPreference",
        items:
          paymentPreferences.preferences.length > 0
            ? [
                {
                  id: paymentPreferences.id,
                  displayOrder: 0,
                  jsonData: JSON.stringify({ preferences: paymentPreferences.preferences }),
                },
              ]
            : [],
      },
    });
    markClean("paymentPreferences");
  }, [customerId, paymentPreferences, upsertMutation]);

  // Save all sections with unsaved changes
  const saveAll = useCallback(async () => {
    const promises: Promise<void>[] = [];

    if (dirtyState.shareholders) promises.push(saveShareholders());
    if (dirtyState.businessPartners) promises.push(saveBusinessPartners());
    if (dirtyState.productCategories) promises.push(saveProductCategories());
    if (dirtyState.bankAccounts) promises.push(saveBankAccounts());
    if (dirtyState.collaterals) promises.push(saveCollaterals());
    if (dirtyState.paymentPreferences) promises.push(savePaymentPreferences());

    try {
      await Promise.all(promises);
      toast.success("Tüm değişiklikler kaydedildi");
    } catch {
      toast.error("Bazı değişiklikler kaydedilemedi");
    }
  }, [
    dirtyState,
    saveShareholders,
    saveBusinessPartners,
    saveProductCategories,
    saveBankAccounts,
    saveCollaterals,
    savePaymentPreferences,
  ]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-[300px]" />
        <Skeleton className="h-[300px]" />
      </div>
    );
  }

  // Dirty indicator badge component
  const DirtyBadge = ({ isDirty }: { isDirty: boolean }) =>
    isDirty ? (
      <Badge variant="outline" className="ml-2 text-orange-600 border-orange-300 bg-orange-50">
        <AlertCircle className="h-3 w-3 mr-1" />
        Kaydedilmedi
      </Badge>
    ) : null;

  return (
    <div className="space-y-6">
      {/* Global Save All Button */}
      {hasUnsavedChanges && (
        <div className="sticky top-0 z-10 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 border rounded-lg p-4 flex items-center justify-between shadow-sm">
          <div className="flex items-center gap-2 text-orange-600">
            <AlertCircle className="h-5 w-5" />
            <span className="font-medium">
              {dirtyCount} bölümde kaydedilmemiş değişiklik var
            </span>
          </div>
          <Button onClick={saveAll} disabled={upsertMutation.isPending}>
            <Save className="h-4 w-4 mr-2" />
            Tümünü Kaydet
          </Button>
        </div>
      )}

      {/* Shareholders & Directors */}
      <Card className={dirtyState.shareholders ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Yetkililer & Ortaklar</CardTitle>
                  <DirtyBadge isDirty={dirtyState.shareholders} />
                </div>
                <CardDescription>Şirket ortakları ve yöneticileri</CardDescription>
              </div>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  setShareholders([
                    ...shareholders,
                    { data: { fullName: "", identityNumber: "", sharePercentage: 0 } },
                  ]);
                  markDirty("shareholders");
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveShareholders}
                disabled={upsertMutation.isPending || !dirtyState.shareholders}
                variant={dirtyState.shareholders ? "default" : "outline"}
              >
                <Save className="h-4 w-4 mr-1" />
                Kaydet
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {shareholders.length === 0 ? (
            <p className="text-sm text-muted-foreground">Kayıt bulunamadı</p>
          ) : (
            <div className="space-y-4">
              {shareholders.map((s, idx) => (
                <div key={s.id || idx} className="flex gap-4 items-end">
                  <div className="flex-1">
                    <Label>Adı Soyadı</Label>
                    <Input
                      value={s.data.fullName}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = { ...s, data: { ...s.data, fullName: e.target.value } };
                        setShareholders(updated);
                        markDirty("shareholders");
                      }}
                      placeholder="Ad Soyad"
                    />
                  </div>
                  <div className="flex-1">
                    <Label>T.C. Kimlik Numarası</Label>
                    <Input
                      value={s.data.identityNumber}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = { ...s, data: { ...s.data, identityNumber: e.target.value } };
                        setShareholders(updated);
                        markDirty("shareholders");
                      }}
                      placeholder="T.C. Kimlik No"
                    />
                  </div>
                  <div className="w-32">
                    <Label>Pay Oranı (%)</Label>
                    <Input
                      type="number"
                      value={s.data.sharePercentage}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = {
                          ...s,
                          data: { ...s.data, sharePercentage: parseFloat(e.target.value) || 0 },
                        };
                        setShareholders(updated);
                        markDirty("shareholders");
                      }}
                      placeholder="%"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setShareholders(shareholders.filter((_, i) => i !== idx));
                      markDirty("shareholders");
                    }}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Business Partners */}
      <Card className={dirtyState.businessPartners ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Çalışmakta Olduğunuz Şirketler</CardTitle>
                  <DirtyBadge isDirty={dirtyState.businessPartners} />
                </div>
                <CardDescription>İş ortakları ve çalışma koşulları</CardDescription>
              </div>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  setBusinessPartners([
                    ...businessPartners,
                    { data: { companyName: "", paymentTerm: "", creditLimitUsd: 0 } },
                  ]);
                  markDirty("businessPartners");
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveBusinessPartners}
                disabled={upsertMutation.isPending || !dirtyState.businessPartners}
                variant={dirtyState.businessPartners ? "default" : "outline"}
              >
                <Save className="h-4 w-4 mr-1" />
                Kaydet
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {businessPartners.length === 0 ? (
            <p className="text-sm text-muted-foreground">Kayıt bulunamadı</p>
          ) : (
            <div className="space-y-4">
              {businessPartners.map((p, idx) => (
                <div key={p.id || idx} className="flex gap-4 items-end">
                  <div className="flex-1">
                    <Label>Şirket Adı</Label>
                    <Input
                      value={p.data.companyName}
                      onChange={(e) => {
                        const updated = [...businessPartners];
                        updated[idx] = { ...p, data: { ...p.data, companyName: e.target.value } };
                        setBusinessPartners(updated);
                        markDirty("businessPartners");
                      }}
                      placeholder="Şirket Adı"
                    />
                  </div>
                  <div className="w-40">
                    <Label>Vade</Label>
                    <Select
                      value={p.data.paymentTerm}
                      onValueChange={(value) => {
                        const updated = [...businessPartners];
                        updated[idx] = { ...p, data: { ...p.data, paymentTerm: value } };
                        setBusinessPartners(updated);
                        markDirty("businessPartners");
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Seçiniz" />
                      </SelectTrigger>
                      <SelectContent>
                        {PAYMENT_TERMS.map((term) => (
                          <SelectItem key={term} value={term}>
                            {term}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="w-40">
                    <Label>Limit Tutarı (USD)</Label>
                    <Input
                      type="number"
                      value={p.data.creditLimitUsd}
                      onChange={(e) => {
                        const updated = [...businessPartners];
                        updated[idx] = {
                          ...p,
                          data: { ...p.data, creditLimitUsd: parseFloat(e.target.value) || 0 },
                        };
                        setBusinessPartners(updated);
                        markDirty("businessPartners");
                      }}
                      placeholder="Limit"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setBusinessPartners(
                        businessPartners.filter((_, i) => i !== idx)
                      );
                      markDirty("businessPartners");
                    }}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Product Categories */}
      <Card className={dirtyState.productCategories ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Satışını Gerçekleştirdiğiniz Ürün Kategorileri</CardTitle>
                  <DirtyBadge isDirty={dirtyState.productCategories} />
                </div>
                <CardDescription>Birden fazla seçenek işaretleyebilirsiniz</CardDescription>
              </div>
            </div>
            <Button
              size="sm"
              onClick={saveProductCategories}
              disabled={upsertMutation.isPending || !dirtyState.productCategories}
              variant={dirtyState.productCategories ? "default" : "outline"}
            >
              <Save className="h-4 w-4 mr-1" />
              Kaydet
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            {PRODUCT_CATEGORIES.map((cat) => (
              <div key={cat} className="flex items-center space-x-2">
                <Checkbox
                  id={`cat-${cat}`}
                  checked={productCategories.categories.includes(cat)}
                  onCheckedChange={(checked) => {
                    if (checked) {
                      setProductCategories({
                        ...productCategories,
                        categories: [...productCategories.categories, cat],
                      });
                    } else {
                      setProductCategories({
                        ...productCategories,
                        categories: productCategories.categories.filter((c) => c !== cat),
                      });
                    }
                    markDirty("productCategories");
                  }}
                />
                <Label htmlFor={`cat-${cat}`} className="text-sm cursor-pointer">
                  {cat}
                </Label>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>

      {/* Bank Accounts */}
      <Card className={dirtyState.bankAccounts ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Landmark className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Banka Hesap Bilgileri</CardTitle>
                  <DirtyBadge isDirty={dirtyState.bankAccounts} />
                </div>
                <CardDescription>Şirket banka hesapları</CardDescription>
              </div>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  setBankAccounts([
                    ...bankAccounts,
                    { data: { bankName: "", iban: "" } },
                  ]);
                  markDirty("bankAccounts");
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveBankAccounts}
                disabled={upsertMutation.isPending || !dirtyState.bankAccounts}
                variant={dirtyState.bankAccounts ? "default" : "outline"}
              >
                <Save className="h-4 w-4 mr-1" />
                Kaydet
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {bankAccounts.length === 0 ? (
            <p className="text-sm text-muted-foreground">Kayıt bulunamadı</p>
          ) : (
            <div className="space-y-4">
              {bankAccounts.map((b, idx) => (
                <div key={b.id || idx} className="flex gap-4 items-end">
                  <div className="w-64">
                    <Label>Banka Adı</Label>
                    <Input
                      value={b.data.bankName}
                      onChange={(e) => {
                        const updated = [...bankAccounts];
                        updated[idx] = { ...b, data: { ...b.data, bankName: e.target.value } };
                        setBankAccounts(updated);
                        markDirty("bankAccounts");
                      }}
                      placeholder="Banka Adı"
                    />
                  </div>
                  <div className="flex-1">
                    <Label>IBAN</Label>
                    <Input
                      value={b.data.iban}
                      onChange={(e) => {
                        const updated = [...bankAccounts];
                        updated[idx] = { ...b, data: { ...b.data, iban: e.target.value } };
                        setBankAccounts(updated);
                        markDirty("bankAccounts");
                      }}
                      placeholder="TRXX XXXX XXXX XXXX XXXX XXXX XX"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setBankAccounts(bankAccounts.filter((_, i) => i !== idx));
                      markDirty("bankAccounts");
                    }}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Collaterals */}
      <Card className={dirtyState.collaterals ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Teminatlar</CardTitle>
                  <DirtyBadge isDirty={dirtyState.collaterals} />
                </div>
                <CardDescription>Şirket teminat bilgileri</CardDescription>
              </div>
            </div>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => {
                  setCollaterals([
                    ...collaterals,
                    { data: { type: "", amount: 0, currency: "USD" } },
                  ]);
                  markDirty("collaterals");
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveCollaterals}
                disabled={upsertMutation.isPending || !dirtyState.collaterals}
                variant={dirtyState.collaterals ? "default" : "outline"}
              >
                <Save className="h-4 w-4 mr-1" />
                Kaydet
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent>
          {collaterals.length === 0 ? (
            <p className="text-sm text-muted-foreground">Kayıt bulunamadı</p>
          ) : (
            <div className="space-y-4">
              {collaterals.map((c, idx) => (
                <div key={c.id || idx} className="flex gap-4 items-end">
                  <div className="w-48">
                    <Label>Teminat Türü</Label>
                    <Select
                      value={c.data.type}
                      onValueChange={(value) => {
                        const updated = [...collaterals];
                        updated[idx] = { ...c, data: { ...c.data, type: value } };
                        setCollaterals(updated);
                        markDirty("collaterals");
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Seçiniz" />
                      </SelectTrigger>
                      <SelectContent>
                        {COLLATERAL_TYPES.map((type) => (
                          <SelectItem key={type} value={type}>
                            {type}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="flex-1">
                    <Label>Tutar</Label>
                    <Input
                      type="number"
                      value={c.data.amount}
                      onChange={(e) => {
                        const updated = [...collaterals];
                        updated[idx] = {
                          ...c,
                          data: { ...c.data, amount: parseFloat(e.target.value) || 0 },
                        };
                        setCollaterals(updated);
                        markDirty("collaterals");
                      }}
                      placeholder="Tutar"
                    />
                  </div>
                  <div className="w-28">
                    <Label>Para Birimi</Label>
                    <Select
                      value={c.data.currency}
                      onValueChange={(value) => {
                        const updated = [...collaterals];
                        updated[idx] = { ...c, data: { ...c.data, currency: value } };
                        setCollaterals(updated);
                        markDirty("collaterals");
                      }}
                    >
                      <SelectTrigger>
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {CURRENCIES.map((curr) => (
                          <SelectItem key={curr} value={curr}>
                            {curr}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setCollaterals(collaterals.filter((_, i) => i !== idx));
                      markDirty("collaterals");
                    }}
                  >
                    <Trash2 className="h-4 w-4 text-destructive" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Payment Preferences */}
      <Card className={dirtyState.paymentPreferences ? "ring-2 ring-orange-200" : ""}>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              <div>
                <div className="flex items-center">
                  <CardTitle>Talep Ettiğiniz Çalışma Koşulları</CardTitle>
                  <DirtyBadge isDirty={dirtyState.paymentPreferences} />
                </div>
                <CardDescription>Birden fazla seçenek işaretleyebilirsiniz</CardDescription>
              </div>
            </div>
            <Button
              size="sm"
              onClick={savePaymentPreferences}
              disabled={upsertMutation.isPending || !dirtyState.paymentPreferences}
              variant={dirtyState.paymentPreferences ? "default" : "outline"}
            >
              <Save className="h-4 w-4 mr-1" />
              Kaydet
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          <div className="flex flex-wrap gap-6">
            {PAYMENT_PREFERENCES.map((pref) => (
              <div key={pref} className="flex items-center space-x-2">
                <Checkbox
                  id={`pref-${pref}`}
                  checked={paymentPreferences.preferences.includes(pref)}
                  onCheckedChange={(checked) => {
                    if (checked) {
                      setPaymentPreferences({
                        ...paymentPreferences,
                        preferences: [...paymentPreferences.preferences, pref],
                      });
                    } else {
                      setPaymentPreferences({
                        ...paymentPreferences,
                        preferences: paymentPreferences.preferences.filter((p) => p !== pref),
                      });
                    }
                    markDirty("paymentPreferences");
                  }}
                />
                <Label htmlFor={`pref-${pref}`} className="text-sm cursor-pointer">
                  {pref}
                </Label>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
