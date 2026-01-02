"use client";

import { useParams, useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import {
  ArrowLeft,
  Pencil,
  Calendar,
  Play,
  Pause,
  XCircle,
  Clock,
  CheckCircle2,
  Ban,
  FileEdit,
  TrendingUp,
  DollarSign,
  Users,
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
import { Separator } from "@/components/ui/separator";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Progress } from "@/components/ui/progress";
import { CampaignForm } from "@/components/forms/campaign-form";
import { DiscountRulesEditor } from "@/components/campaigns/discount-rules-editor";
import {
  useCampaign,
  useUpdateCampaign,
  useScheduleCampaign,
  useActivateCampaign,
  usePauseCampaign,
  useCancelCampaign,
} from "@/hooks/use-campaigns";
import { formatDateTime, formatDate, formatCurrency } from "@/lib/utils";
import { CampaignFormData } from "@/lib/validations/campaign";
import {
  CampaignStatus,
  CampaignStatusLabels,
} from "@/types/entities";

const statusIcons: Record<CampaignStatus, React.ReactNode> = {
  Draft: <FileEdit className="h-4 w-4" />,
  Scheduled: <Clock className="h-4 w-4" />,
  Active: <CheckCircle2 className="h-4 w-4" />,
  Paused: <Pause className="h-4 w-4" />,
  Ended: <Calendar className="h-4 w-4" />,
  Cancelled: <Ban className="h-4 w-4" />,
};

const statusVariants: Record<CampaignStatus, "default" | "secondary" | "outline" | "destructive"> = {
  Draft: "secondary",
  Scheduled: "outline",
  Active: "default",
  Paused: "secondary",
  Ended: "outline",
  Cancelled: "destructive",
};

export default function CampaignDetailPage() {
  const params = useParams();
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const isEditing = searchParams.get("edit") === "true";

  const { data: campaign, isLoading } = useCampaign(id);
  const updateCampaign = useUpdateCampaign();
  const scheduleCampaign = useScheduleCampaign();
  const activateCampaign = useActivateCampaign();
  const pauseCampaign = usePauseCampaign();
  const cancelCampaign = useCancelCampaign();

  const handleSubmit = async (data: CampaignFormData) => {
    await updateCampaign.mutateAsync({
      id,
      data: {
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
      },
    });
    router.push(`/campaigns/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-[400px]" />
      </div>
    );
  }

  if (!campaign) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/campaigns">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">
            Campaign not found
          </h1>
        </div>
      </div>
    );
  }

  if (isEditing) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href={`/campaigns/${id}`}>
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Edit Campaign</h1>
            <p className="text-muted-foreground">Update campaign information</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Campaign Information</CardTitle>
          </CardHeader>
          <CardContent>
            <CampaignForm
              isEditing
              defaultValues={{
                name: campaign.name,
                description: campaign.description || "",
                startDate: campaign.startDate,
                endDate: campaign.endDate,
                priority: campaign.priority,
                totalBudgetLimitAmount: campaign.totalBudgetLimitAmount ?? null,
                totalUsageLimit: campaign.totalUsageLimit ?? null,
                perCustomerBudgetLimitAmount:
                  campaign.perCustomerBudgetLimitAmount ?? null,
                perCustomerUsageLimit: campaign.perCustomerUsageLimit ?? null,
                currency: campaign.totalDiscountUsedCurrency || "TRY",
              }}
              onSubmit={handleSubmit}
              onCancel={() => router.push(`/campaigns/${id}`)}
              isLoading={updateCampaign.isPending}
            />
          </CardContent>
        </Card>
      </div>
    );
  }

  const budgetProgress = campaign.totalBudgetLimitAmount
    ? (campaign.totalDiscountUsedAmount / campaign.totalBudgetLimitAmount) * 100
    : 0;

  const usageProgress = campaign.totalUsageLimit
    ? (campaign.totalUsageCount / campaign.totalUsageLimit) * 100
    : 0;

  const canEdit = campaign.status === "Draft" || campaign.status === "Scheduled";
  const canSchedule = campaign.status === "Draft";
  const canActivate =
    campaign.status === "Scheduled" || campaign.status === "Paused";
  const canPause = campaign.status === "Active";
  const canCancel =
    campaign.status === "Draft" ||
    campaign.status === "Scheduled" ||
    campaign.status === "Active" ||
    campaign.status === "Paused";

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/campaigns">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <div className="flex items-center gap-3">
              <h1 className="text-3xl font-bold tracking-tight">
                {campaign.name}
              </h1>
              <Badge
                variant={statusVariants[campaign.status]}
                className="gap-1"
              >
                {statusIcons[campaign.status]}
                {CampaignStatusLabels[campaign.status]}
              </Badge>
            </div>
            <div className="flex items-center gap-2 text-muted-foreground">
              <Calendar className="h-4 w-4" />
              {formatDate(campaign.startDate)} → {formatDate(campaign.endDate)}
            </div>
          </div>
        </div>
        <div className="flex gap-2">
          {canSchedule && (
            <Button
              variant="outline"
              onClick={() => scheduleCampaign.mutate(id)}
              disabled={scheduleCampaign.isPending}
            >
              <Clock className="mr-2 h-4 w-4" />
              Schedule
            </Button>
          )}
          {canActivate && (
            <Button
              variant="outline"
              onClick={() => activateCampaign.mutate(id)}
              disabled={activateCampaign.isPending}
            >
              <Play className="mr-2 h-4 w-4" />
              Activate
            </Button>
          )}
          {canPause && (
            <Button
              variant="outline"
              onClick={() => pauseCampaign.mutate(id)}
              disabled={pauseCampaign.isPending}
            >
              <Pause className="mr-2 h-4 w-4" />
              Pause
            </Button>
          )}
          {canCancel && (
            <Button
              variant="outline"
              onClick={() => cancelCampaign.mutate(id)}
              disabled={cancelCampaign.isPending}
            >
              <XCircle className="mr-2 h-4 w-4" />
              Cancel
            </Button>
          )}
          {canEdit && (
            <Button asChild>
              <Link href={`/campaigns/${id}?edit=true`}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Link>
            </Button>
          )}
        </div>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Priority</CardDescription>
          </CardHeader>
          <CardContent>
            <span className="text-2xl font-bold">{campaign.priority}</span>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Discount Rules</CardDescription>
          </CardHeader>
          <CardContent>
            <span className="text-2xl font-bold">
              {campaign.discountRules.length}
            </span>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Budget Used</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <span className="text-2xl font-bold">
                {formatCurrency(
                  campaign.totalDiscountUsedAmount,
                  campaign.totalDiscountUsedCurrency
                )}
              </span>
              {campaign.totalBudgetLimitAmount && (
                <>
                  <span className="text-sm text-muted-foreground">
                    {" / "}
                    {formatCurrency(
                      campaign.totalBudgetLimitAmount,
                      campaign.totalBudgetLimitCurrency
                    )}
                  </span>
                  <Progress value={budgetProgress} className="h-2" />
                </>
              )}
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Usage Count</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="space-y-2">
              <span className="text-2xl font-bold">
                {campaign.totalUsageCount}
              </span>
              {campaign.totalUsageLimit && (
                <>
                  <span className="text-sm text-muted-foreground">
                    {" / "}
                    {campaign.totalUsageLimit}
                  </span>
                  <Progress value={usageProgress} className="h-2" />
                </>
              )}
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="rules">Discount Rules</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-3">
            {/* Main Info */}
            <Card className="md:col-span-2">
              <CardHeader>
                <CardTitle>Campaign Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <p className="text-sm text-muted-foreground">Name</p>
                    <p className="font-medium">{campaign.name}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Status</p>
                    <Badge
                      variant={statusVariants[campaign.status]}
                      className="gap-1"
                    >
                      {statusIcons[campaign.status]}
                      {CampaignStatusLabels[campaign.status]}
                    </Badge>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Start Date</p>
                    <p className="font-medium">
                      {formatDateTime(campaign.startDate)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">End Date</p>
                    <p className="font-medium">
                      {formatDateTime(campaign.endDate)}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Priority</p>
                    <p className="font-medium">{campaign.priority}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Currency</p>
                    <p className="font-medium">
                      {campaign.totalDiscountUsedCurrency}
                    </p>
                  </div>
                </div>

                {campaign.description && (
                  <>
                    <Separator />
                    <div>
                      <p className="text-sm text-muted-foreground mb-2">
                        Description
                      </p>
                      <p className="text-sm">{campaign.description}</p>
                    </div>
                  </>
                )}

                <Separator />

                {/* Budget Limits */}
                <div>
                  <p className="text-sm font-medium mb-3">Budget Limits</p>
                  <div className="grid gap-4 md:grid-cols-2">
                    <div className="flex items-center gap-3">
                      <DollarSign className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Total Budget
                        </p>
                        <p className="font-medium">
                          {campaign.totalBudgetLimitAmount
                            ? formatCurrency(
                                campaign.totalBudgetLimitAmount,
                                campaign.totalBudgetLimitCurrency
                              )
                            : "Unlimited"}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <TrendingUp className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Total Usage
                        </p>
                        <p className="font-medium">
                          {campaign.totalUsageLimit
                            ? `${campaign.totalUsageLimit} uses`
                            : "Unlimited"}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <Users className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Per-Customer Budget
                        </p>
                        <p className="font-medium">
                          {campaign.perCustomerBudgetLimitAmount
                            ? formatCurrency(
                                campaign.perCustomerBudgetLimitAmount,
                                campaign.perCustomerBudgetLimitCurrency
                              )
                            : "Unlimited"}
                        </p>
                      </div>
                    </div>
                    <div className="flex items-center gap-3">
                      <Users className="h-5 w-5 text-muted-foreground" />
                      <div>
                        <p className="text-sm text-muted-foreground">
                          Per-Customer Usage
                        </p>
                        <p className="font-medium">
                          {campaign.perCustomerUsageLimit
                            ? `${campaign.perCustomerUsageLimit} uses`
                            : "Unlimited"}
                        </p>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Metadata */}
            <Card>
              <CardHeader>
                <CardTitle>Metadata</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Created</span>
                  <span>{formatDateTime(campaign.createdAt)}</span>
                </div>
                {campaign.createdBy && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Created By</span>
                    <span>{campaign.createdBy}</span>
                  </div>
                )}
                {campaign.updatedAt && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Updated</span>
                    <span>{formatDateTime(campaign.updatedAt)}</span>
                  </div>
                )}
                {campaign.updatedBy && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Updated By</span>
                    <span>{campaign.updatedBy}</span>
                  </div>
                )}
                {campaign.externalId && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">External ID</span>
                    <span className="font-mono text-xs">
                      {campaign.externalId}
                    </span>
                  </div>
                )}
                {campaign.externalCode && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">External Code</span>
                    <span className="font-mono text-xs">
                      {campaign.externalCode}
                    </span>
                  </div>
                )}
                {campaign.lastSyncedAt && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Last Synced</span>
                    <span>{formatDateTime(campaign.lastSyncedAt)}</span>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="rules">
          <DiscountRulesEditor campaign={campaign} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
