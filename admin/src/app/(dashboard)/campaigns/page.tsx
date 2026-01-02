"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  Plus,
  Search,
  MoreHorizontal,
  Eye,
  Trash2,
  Play,
  Pause,
  Calendar,
  XCircle,
  Clock,
  CheckCircle2,
  Ban,
  FileEdit,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import {
  useCampaigns,
  useDeleteCampaign,
  useActivateCampaign,
  usePauseCampaign,
  useCancelCampaign,
  useScheduleCampaign,
} from "@/hooks/use-campaigns";
import {
  CampaignFilters,
  CampaignStatus,
  CampaignStatusLabels,
} from "@/types/entities";
import { formatDate, formatCurrency } from "@/lib/utils";

const statusIcons: Record<CampaignStatus, React.ReactNode> = {
  Draft: <FileEdit className="h-3 w-3" />,
  Scheduled: <Clock className="h-3 w-3" />,
  Active: <CheckCircle2 className="h-3 w-3" />,
  Paused: <Pause className="h-3 w-3" />,
  Ended: <Calendar className="h-3 w-3" />,
  Cancelled: <Ban className="h-3 w-3" />,
};

const statusVariants: Record<CampaignStatus, "default" | "secondary" | "outline" | "destructive"> = {
  Draft: "secondary",
  Scheduled: "outline",
  Active: "default",
  Paused: "secondary",
  Ended: "outline",
  Cancelled: "destructive",
};

export default function CampaignsPage() {
  const router = useRouter();
  const [filters, setFilters] = useState<CampaignFilters>({
    page: 1,
    pageSize: 10,
    sortBy: "CreatedAt",
    sortDirection: "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [deleteId, setDeleteId] = useState<string | null>(null);
  const [cancelId, setCancelId] = useState<string | null>(null);

  const { data, isLoading } = useCampaigns(filters);
  const deleteCampaign = useDeleteCampaign();
  const activateCampaign = useActivateCampaign();
  const pauseCampaign = usePauseCampaign();
  const cancelCampaign = useCancelCampaign();
  const scheduleCampaign = useScheduleCampaign();

  const handleSearch = () => {
    setFilters((prev) => ({ ...prev, search: searchInput, page: 1 }));
  };

  const handleStatusChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      status: value === "all" ? undefined : (value as CampaignStatus),
      page: 1,
    }));
  };

  const handleDelete = async () => {
    if (deleteId) {
      await deleteCampaign.mutateAsync(deleteId);
      setDeleteId(null);
    }
  };

  const handleCancel = async () => {
    if (cancelId) {
      await cancelCampaign.mutateAsync(cancelId);
      setCancelId(null);
    }
  };

  const handleActivate = async (id: string) => {
    await activateCampaign.mutateAsync(id);
  };

  const handlePause = async (id: string) => {
    await pauseCampaign.mutateAsync(id);
  };

  const handleSchedule = async (id: string) => {
    await scheduleCampaign.mutateAsync(id);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Campaigns</h1>
          <p className="text-muted-foreground">
            Manage discount campaigns and promotional offers
          </p>
        </div>
        <Button asChild>
          <Link href="/campaigns/new">
            <Plus className="mr-2 h-4 w-4" />
            Create Campaign
          </Link>
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Campaigns</CardTitle>
          <CardDescription>
            Browse and manage your discount campaigns
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 md:flex-row md:items-center mb-6">
            <div className="flex flex-1 gap-2">
              <div className="relative flex-1 max-w-sm">
                <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
                <Input
                  type="search"
                  placeholder="Search campaigns..."
                  className="pl-8"
                  value={searchInput}
                  onChange={(e) => setSearchInput(e.target.value)}
                  onKeyDown={(e) => e.key === "Enter" && handleSearch()}
                />
              </div>
              <Button variant="secondary" onClick={handleSearch}>
                Search
              </Button>
            </div>
            <div className="flex gap-2">
              <Select
                value={filters.status || "all"}
                onValueChange={handleStatusChange}
              >
                <SelectTrigger className="w-[150px]">
                  <SelectValue placeholder="All Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="Draft">Draft</SelectItem>
                  <SelectItem value="Scheduled">Scheduled</SelectItem>
                  <SelectItem value="Active">Active</SelectItem>
                  <SelectItem value="Paused">Paused</SelectItem>
                  <SelectItem value="Ended">Ended</SelectItem>
                  <SelectItem value="Cancelled">Cancelled</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* Table */}
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Campaign</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Schedule</TableHead>
                  <TableHead className="text-right">Budget</TableHead>
                  <TableHead className="text-right">Usage</TableHead>
                  <TableHead className="text-right">Rules</TableHead>
                  <TableHead className="w-[70px]"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  [...Array(5)].map((_, i) => (
                    <TableRow key={i}>
                      <TableCell colSpan={7}>
                        <Skeleton className="h-10 w-full" />
                      </TableCell>
                    </TableRow>
                  ))
                ) : data?.items.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={7} className="text-center py-10">
                      <p className="text-muted-foreground">No campaigns found</p>
                    </TableCell>
                  </TableRow>
                ) : (
                  data?.items.map((campaign) => (
                    <TableRow
                      key={campaign.id}
                      className="cursor-pointer"
                      onClick={() => router.push(`/campaigns/${campaign.id}`)}
                    >
                      <TableCell>
                        <div>
                          <span className="font-medium">{campaign.name}</span>
                          {campaign.description && (
                            <p className="text-sm text-muted-foreground line-clamp-1">
                              {campaign.description}
                            </p>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={statusVariants[campaign.status]}
                          className="gap-1"
                        >
                          {statusIcons[campaign.status]}
                          {CampaignStatusLabels[campaign.status]}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <div className="flex items-center gap-1 text-sm text-muted-foreground">
                          <Calendar className="h-3 w-3" />
                          {formatDate(campaign.startDate)} →{" "}
                          {formatDate(campaign.endDate)}
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        {campaign.totalBudgetLimitAmount ? (
                          <div className="text-sm">
                            <span className="font-medium">
                              {formatCurrency(
                                campaign.totalDiscountUsedAmount,
                                campaign.totalDiscountUsedCurrency
                              )}
                            </span>
                            <span className="text-muted-foreground">
                              {" / "}
                              {formatCurrency(
                                campaign.totalBudgetLimitAmount,
                                campaign.totalBudgetLimitCurrency
                              )}
                            </span>
                          </div>
                        ) : (
                          <span className="text-sm text-muted-foreground">
                            Unlimited
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        {campaign.totalUsageLimit ? (
                          <span className="text-sm">
                            {campaign.totalUsageCount} / {campaign.totalUsageLimit}
                          </span>
                        ) : (
                          <span className="text-sm">
                            {campaign.totalUsageCount}
                          </span>
                        )}
                      </TableCell>
                      <TableCell className="text-right">
                        <Badge variant="outline">
                          {campaign.discountRuleCount} rules
                        </Badge>
                      </TableCell>
                      <TableCell onClick={(e) => e.stopPropagation()}>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem asChild>
                              <Link href={`/campaigns/${campaign.id}`}>
                                <Eye className="mr-2 h-4 w-4" />
                                View Details
                              </Link>
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            {campaign.status === "Draft" && (
                              <DropdownMenuItem
                                onClick={() => handleSchedule(campaign.id)}
                              >
                                <Clock className="mr-2 h-4 w-4" />
                                Schedule
                              </DropdownMenuItem>
                            )}
                            {(campaign.status === "Scheduled" ||
                              campaign.status === "Paused") && (
                              <DropdownMenuItem
                                onClick={() => handleActivate(campaign.id)}
                              >
                                <Play className="mr-2 h-4 w-4" />
                                Activate
                              </DropdownMenuItem>
                            )}
                            {campaign.status === "Active" && (
                              <DropdownMenuItem
                                onClick={() => handlePause(campaign.id)}
                              >
                                <Pause className="mr-2 h-4 w-4" />
                                Pause
                              </DropdownMenuItem>
                            )}
                            {(campaign.status === "Draft" ||
                              campaign.status === "Scheduled" ||
                              campaign.status === "Active" ||
                              campaign.status === "Paused") && (
                              <>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                  onClick={() => setCancelId(campaign.id)}
                                >
                                  <XCircle className="mr-2 h-4 w-4" />
                                  Cancel Campaign
                                </DropdownMenuItem>
                              </>
                            )}
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-destructive"
                              onClick={() => setDeleteId(campaign.id)}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))
                )}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
                {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
                {data.totalCount} campaigns
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasPreviousPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      page: (prev.page || 1) - 1,
                    }))
                  }
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasNextPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      page: (prev.page || 1) + 1,
                    }))
                  }
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deleteId}
        onOpenChange={(open) => !open && setDeleteId(null)}
        title="Delete Campaign"
        description="Are you sure you want to delete this campaign? This action cannot be undone."
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteCampaign.isPending}
      />

      {/* Cancel Confirmation */}
      <ConfirmDialog
        open={!!cancelId}
        onOpenChange={(open) => !open && setCancelId(null)}
        title="Cancel Campaign"
        description="Are you sure you want to cancel this campaign? This action cannot be undone and the campaign will no longer apply discounts."
        confirmText="Cancel Campaign"
        variant="destructive"
        onConfirm={handleCancel}
        isLoading={cancelCampaign.isPending}
      />
    </div>
  );
}
