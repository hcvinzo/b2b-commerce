"use client";

import { useRouter } from "next/navigation";
import Link from "next/link";
import { ArrowLeft } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { CampaignForm } from "@/components/forms/campaign-form";
import { useCreateCampaign } from "@/hooks/use-campaigns";
import { CampaignFormData } from "@/lib/validations/campaign";

export default function NewCampaignPage() {
  const router = useRouter();
  const createCampaign = useCreateCampaign();

  const handleSubmit = async (data: CampaignFormData) => {
    const result = await createCampaign.mutateAsync({
      name: data.name,
      description: data.description,
      startDate: data.startDate,
      endDate: data.endDate,
      priority: data.priority,
      totalBudgetLimitAmount: data.totalBudgetLimitAmount ?? undefined,
      totalUsageLimit: data.totalUsageLimit ?? undefined,
      perCustomerBudgetLimitAmount: data.perCustomerBudgetLimitAmount ?? undefined,
      perCustomerUsageLimit: data.perCustomerUsageLimit ?? undefined,
      currency: data.currency,
    });
    router.push(`/campaigns/${result.id}`);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link href="/campaigns">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Create Campaign</h1>
          <p className="text-muted-foreground">
            Set up a new discount campaign
          </p>
        </div>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Campaign Information</CardTitle>
          <CardDescription>
            Configure the basic settings for your campaign. You can add discount
            rules after creating the campaign.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <CampaignForm
            onSubmit={handleSubmit}
            onCancel={() => router.push("/campaigns")}
            isLoading={createCampaign.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
