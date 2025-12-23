"use client";

import { useState, useCallback, useMemo, useEffect } from "react";
import { Plus, Trash2, Save, Users, Building, Package, Landmark, Shield, CreditCard } from "lucide-react";

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
import {
  useCustomerAttributes,
  useUpsertCustomerAttributes,
} from "@/hooks/use-customers";
import {
  CustomerAttribute,
  CustomerAttributeType,
  ShareholderData,
  BusinessPartnerData,
  ProductCategoryData,
  BankAccountData,
  CollateralData,
  PaymentPreferenceData,
} from "@/types/entities";

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

  // Local state for each attribute type
  const [shareholders, setShareholders] = useState<ShareholderData[]>([]);
  const [businessPartners, setBusinessPartners] = useState<BusinessPartnerData[]>([]);
  const [productCategories, setProductCategories] = useState<string[]>([]);
  const [bankAccounts, setBankAccounts] = useState<BankAccountData[]>([]);
  const [collaterals, setCollaterals] = useState<CollateralData[]>([]);
  const [paymentPreferences, setPaymentPreferences] = useState<string[]>([]);

  // Track dirty state
  const [isDirty, setIsDirty] = useState(false);

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
        shareholderAttrs.map((a) => JSON.parse(a.jsonData) as ShareholderData)
      );
      setBusinessPartners(
        partnerAttrs.map((a) => JSON.parse(a.jsonData) as BusinessPartnerData)
      );
      setBankAccounts(
        bankAttrs.map((a) => JSON.parse(a.jsonData) as BankAccountData)
      );
      setCollaterals(
        collateralAttrs.map((a) => JSON.parse(a.jsonData) as CollateralData)
      );

      // Product categories and payment preferences are stored as single items with arrays
      if (categoryAttrs.length > 0) {
        const data = JSON.parse(categoryAttrs[0].jsonData) as ProductCategoryData;
        setProductCategories(data.categories || []);
      } else {
        setProductCategories([]);
      }

      if (preferenceAttrs.length > 0) {
        const data = JSON.parse(preferenceAttrs[0].jsonData) as PaymentPreferenceData;
        setPaymentPreferences(data.preferences || []);
      } else {
        setPaymentPreferences([]);
      }

      setIsDirty(false);
    }
  }, [attributes]);

  // Save handlers
  const saveShareholders = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "ShareholderOrDirector",
        items: shareholders.map((s, i) => ({
          displayOrder: i,
          jsonData: JSON.stringify(s),
        })),
      },
    });
    setIsDirty(false);
  }, [customerId, shareholders, upsertMutation]);

  const saveBusinessPartners = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "BusinessPartner",
        items: businessPartners.map((p, i) => ({
          displayOrder: i,
          jsonData: JSON.stringify(p),
        })),
      },
    });
    setIsDirty(false);
  }, [customerId, businessPartners, upsertMutation]);

  const saveProductCategories = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "ProductCategory",
        items:
          productCategories.length > 0
            ? [
                {
                  displayOrder: 0,
                  jsonData: JSON.stringify({ categories: productCategories }),
                },
              ]
            : [],
      },
    });
    setIsDirty(false);
  }, [customerId, productCategories, upsertMutation]);

  const saveBankAccounts = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "BankAccount",
        items: bankAccounts.map((b, i) => ({
          displayOrder: i,
          jsonData: JSON.stringify(b),
        })),
      },
    });
    setIsDirty(false);
  }, [customerId, bankAccounts, upsertMutation]);

  const saveCollaterals = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "Collateral",
        items: collaterals.map((c, i) => ({
          displayOrder: i,
          jsonData: JSON.stringify(c),
        })),
      },
    });
    setIsDirty(false);
  }, [customerId, collaterals, upsertMutation]);

  const savePaymentPreferences = useCallback(async () => {
    await upsertMutation.mutateAsync({
      customerId,
      data: {
        attributeType: "PaymentPreference",
        items:
          paymentPreferences.length > 0
            ? [
                {
                  displayOrder: 0,
                  jsonData: JSON.stringify({ preferences: paymentPreferences }),
                },
              ]
            : [],
      },
    });
    setIsDirty(false);
  }, [customerId, paymentPreferences, upsertMutation]);

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-[300px]" />
        <Skeleton className="h-[300px]" />
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Shareholders & Directors */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Users className="h-5 w-5" />
              <div>
                <CardTitle>Yetkililer & Ortaklar</CardTitle>
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
                    { fullName: "", identityNumber: "", sharePercentage: 0 },
                  ]);
                  setIsDirty(true);
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveShareholders}
                disabled={upsertMutation.isPending}
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
                <div key={idx} className="flex gap-4 items-end">
                  <div className="flex-1">
                    <Label>Adı Soyadı</Label>
                    <Input
                      value={s.fullName}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = { ...s, fullName: e.target.value };
                        setShareholders(updated);
                        setIsDirty(true);
                      }}
                      placeholder="Ad Soyad"
                    />
                  </div>
                  <div className="flex-1">
                    <Label>T.C. Kimlik Numarası</Label>
                    <Input
                      value={s.identityNumber}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = { ...s, identityNumber: e.target.value };
                        setShareholders(updated);
                        setIsDirty(true);
                      }}
                      placeholder="T.C. Kimlik No"
                    />
                  </div>
                  <div className="w-32">
                    <Label>Pay Oranı (%)</Label>
                    <Input
                      type="number"
                      value={s.sharePercentage}
                      onChange={(e) => {
                        const updated = [...shareholders];
                        updated[idx] = {
                          ...s,
                          sharePercentage: parseFloat(e.target.value) || 0,
                        };
                        setShareholders(updated);
                        setIsDirty(true);
                      }}
                      placeholder="%"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setShareholders(shareholders.filter((_, i) => i !== idx));
                      setIsDirty(true);
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
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Building className="h-5 w-5" />
              <div>
                <CardTitle>Çalışmakta Olduğunuz Şirketler</CardTitle>
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
                    { companyName: "", paymentTerm: "", creditLimitUsd: 0 },
                  ]);
                  setIsDirty(true);
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveBusinessPartners}
                disabled={upsertMutation.isPending}
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
                <div key={idx} className="flex gap-4 items-end">
                  <div className="flex-1">
                    <Label>Şirket Adı</Label>
                    <Input
                      value={p.companyName}
                      onChange={(e) => {
                        const updated = [...businessPartners];
                        updated[idx] = { ...p, companyName: e.target.value };
                        setBusinessPartners(updated);
                        setIsDirty(true);
                      }}
                      placeholder="Şirket Adı"
                    />
                  </div>
                  <div className="w-40">
                    <Label>Vade</Label>
                    <Select
                      value={p.paymentTerm}
                      onValueChange={(value) => {
                        const updated = [...businessPartners];
                        updated[idx] = { ...p, paymentTerm: value };
                        setBusinessPartners(updated);
                        setIsDirty(true);
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
                      value={p.creditLimitUsd}
                      onChange={(e) => {
                        const updated = [...businessPartners];
                        updated[idx] = {
                          ...p,
                          creditLimitUsd: parseFloat(e.target.value) || 0,
                        };
                        setBusinessPartners(updated);
                        setIsDirty(true);
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
                      setIsDirty(true);
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
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Package className="h-5 w-5" />
              <div>
                <CardTitle>Satışını Gerçekleştirdiğiniz Ürün Kategorileri</CardTitle>
                <CardDescription>Birden fazla seçenek işaretleyebilirsiniz</CardDescription>
              </div>
            </div>
            <Button
              size="sm"
              onClick={saveProductCategories}
              disabled={upsertMutation.isPending}
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
                  checked={productCategories.includes(cat)}
                  onCheckedChange={(checked) => {
                    if (checked) {
                      setProductCategories([...productCategories, cat]);
                    } else {
                      setProductCategories(
                        productCategories.filter((c) => c !== cat)
                      );
                    }
                    setIsDirty(true);
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
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Landmark className="h-5 w-5" />
              <div>
                <CardTitle>Banka Hesap Bilgileri</CardTitle>
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
                    { bankName: "", iban: "" },
                  ]);
                  setIsDirty(true);
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveBankAccounts}
                disabled={upsertMutation.isPending}
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
                <div key={idx} className="flex gap-4 items-end">
                  <div className="w-64">
                    <Label>Banka Adı</Label>
                    <Input
                      value={b.bankName}
                      onChange={(e) => {
                        const updated = [...bankAccounts];
                        updated[idx] = { ...b, bankName: e.target.value };
                        setBankAccounts(updated);
                        setIsDirty(true);
                      }}
                      placeholder="Banka Adı"
                    />
                  </div>
                  <div className="flex-1">
                    <Label>IBAN</Label>
                    <Input
                      value={b.iban}
                      onChange={(e) => {
                        const updated = [...bankAccounts];
                        updated[idx] = { ...b, iban: e.target.value };
                        setBankAccounts(updated);
                        setIsDirty(true);
                      }}
                      placeholder="TRXX XXXX XXXX XXXX XXXX XXXX XX"
                    />
                  </div>
                  <Button
                    variant="ghost"
                    size="icon"
                    onClick={() => {
                      setBankAccounts(bankAccounts.filter((_, i) => i !== idx));
                      setIsDirty(true);
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
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              <div>
                <CardTitle>Teminatlar</CardTitle>
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
                    { type: "", amount: 0, currency: "USD" },
                  ]);
                  setIsDirty(true);
                }}
              >
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
              <Button
                size="sm"
                onClick={saveCollaterals}
                disabled={upsertMutation.isPending}
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
                <div key={idx} className="flex gap-4 items-end">
                  <div className="w-48">
                    <Label>Teminat Türü</Label>
                    <Select
                      value={c.type}
                      onValueChange={(value) => {
                        const updated = [...collaterals];
                        updated[idx] = { ...c, type: value };
                        setCollaterals(updated);
                        setIsDirty(true);
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
                      value={c.amount}
                      onChange={(e) => {
                        const updated = [...collaterals];
                        updated[idx] = {
                          ...c,
                          amount: parseFloat(e.target.value) || 0,
                        };
                        setCollaterals(updated);
                        setIsDirty(true);
                      }}
                      placeholder="Tutar"
                    />
                  </div>
                  <div className="w-28">
                    <Label>Para Birimi</Label>
                    <Select
                      value={c.currency}
                      onValueChange={(value) => {
                        const updated = [...collaterals];
                        updated[idx] = { ...c, currency: value };
                        setCollaterals(updated);
                        setIsDirty(true);
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
                      setIsDirty(true);
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
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              <div>
                <CardTitle>Talep Ettiğiniz Çalışma Koşulları</CardTitle>
                <CardDescription>Birden fazla seçenek işaretleyebilirsiniz</CardDescription>
              </div>
            </div>
            <Button
              size="sm"
              onClick={savePaymentPreferences}
              disabled={upsertMutation.isPending}
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
                  checked={paymentPreferences.includes(pref)}
                  onCheckedChange={(checked) => {
                    if (checked) {
                      setPaymentPreferences([...paymentPreferences, pref]);
                    } else {
                      setPaymentPreferences(
                        paymentPreferences.filter((p) => p !== pref)
                      );
                    }
                    setIsDirty(true);
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
