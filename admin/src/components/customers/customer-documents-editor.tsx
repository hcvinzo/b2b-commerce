"use client";

import { useState, useRef } from "react";
import { FileText, Upload, Trash2, ExternalLink, File, Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
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
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger,
} from "@/components/ui/alert-dialog";
import {
  useCustomerDocuments,
  useUploadCustomerDocument,
  useDeleteCustomerDocument,
} from "@/hooks/use-customer-documents";
import type { CustomerDocumentType } from "@/types/entities";
import { formatDateTime } from "@/lib/utils";

// Document type labels in Turkish
const DOCUMENT_TYPE_LABELS: Record<CustomerDocumentType, string> = {
  TaxCertificate: "Vergi Levhası",
  SignatureCircular: "İmza Sirküleri",
  TradeRegistry: "Sicil Gazetesi",
  PartnershipAgreement: "İş Ortağı Sözleşmesi",
  AuthorizedIdCopy: "Yetkili Kimlik Fotokopisi",
  AuthorizedResidenceDocument: "Yetkili İkamet Belgesi",
};

// All available document types
const DOCUMENT_TYPES: CustomerDocumentType[] = [
  "TaxCertificate",
  "SignatureCircular",
  "TradeRegistry",
  "PartnershipAgreement",
  "AuthorizedIdCopy",
  "AuthorizedResidenceDocument",
];

// Format file size
function formatFileSize(bytes: number): string {
  if (bytes === 0) return "0 B";
  const k = 1024;
  const sizes = ["B", "KB", "MB", "GB"];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + " " + sizes[i];
}

// Get file icon based on type
function getFileIcon(fileType: string) {
  if (fileType === "application/pdf") {
    return <FileText className="h-8 w-8 text-red-500" />;
  }
  if (fileType.startsWith("image/")) {
    return <File className="h-8 w-8 text-blue-500" />;
  }
  return <File className="h-8 w-8 text-gray-500" />;
}

interface CustomerDocumentsEditorProps {
  customerId: string;
}

export function CustomerDocumentsEditor({ customerId }: CustomerDocumentsEditorProps) {
  const [selectedDocumentType, setSelectedDocumentType] = useState<CustomerDocumentType | "">("");
  const fileInputRef = useRef<HTMLInputElement>(null);

  const { data: documents, isLoading } = useCustomerDocuments(customerId);
  const uploadDocument = useUploadCustomerDocument();
  const deleteDocument = useDeleteCustomerDocument();

  const handleFileSelect = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !selectedDocumentType) return;

    // Validate file type
    const allowedTypes = ["application/pdf", "image/jpeg", "image/jpg", "image/png"];
    if (!allowedTypes.includes(file.type)) {
      return;
    }

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
      return;
    }

    await uploadDocument.mutateAsync({
      customerId,
      documentType: selectedDocumentType,
      file,
    });

    // Reset after upload
    setSelectedDocumentType("");
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  const handleUploadClick = () => {
    if (!selectedDocumentType) return;
    fileInputRef.current?.click();
  };

  const handleDelete = async (documentId: string) => {
    await deleteDocument.mutateAsync({ customerId, documentId });
  };

  if (isLoading) {
    return (
      <Card>
        <CardHeader>
          <Skeleton className="h-6 w-48" />
          <Skeleton className="h-4 w-72" />
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
            <Skeleton className="h-20 w-full" />
          </div>
        </CardContent>
      </Card>
    );
  }

  // Get already uploaded document types
  const uploadedTypes = new Set(documents?.map((d) => d.documentType) || []);
  const availableTypes = DOCUMENT_TYPES.filter((type) => !uploadedTypes.has(type));

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="h-5 w-5" />
          Belgeler
        </CardTitle>
        <CardDescription>
          Müşteriye ait evrak ve belgeleri yönetin
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Upload Section */}
        <div className="flex gap-4 items-end">
          <div className="flex-1">
            <Select
              value={selectedDocumentType}
              onValueChange={(value) => setSelectedDocumentType(value as CustomerDocumentType)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Belge türü seçin" />
              </SelectTrigger>
              <SelectContent>
                {availableTypes.map((type) => (
                  <SelectItem key={type} value={type}>
                    {DOCUMENT_TYPE_LABELS[type]}
                  </SelectItem>
                ))}
                {availableTypes.length === 0 && (
                  <SelectItem value="none" disabled>
                    Tüm belgeler yüklenmiş
                  </SelectItem>
                )}
              </SelectContent>
            </Select>
          </div>
          <Button
            onClick={handleUploadClick}
            disabled={!selectedDocumentType || uploadDocument.isPending}
          >
            {uploadDocument.isPending ? (
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
            ) : (
              <Upload className="h-4 w-4 mr-2" />
            )}
            Belge Yükle
          </Button>
          <input
            ref={fileInputRef}
            type="file"
            accept=".pdf,.jpg,.jpeg,.png"
            className="hidden"
            onChange={handleFileSelect}
          />
        </div>

        {/* Documents List */}
        <div className="space-y-3">
          {documents && documents.length > 0 ? (
            documents.map((doc) => (
              <div
                key={doc.id}
                className="flex items-center justify-between p-4 border rounded-lg hover:bg-muted/50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  {getFileIcon(doc.fileType)}
                  <div>
                    <p className="font-medium">{doc.documentTypeName}</p>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <span>{doc.fileName}</span>
                      <span>•</span>
                      <span>{formatFileSize(doc.fileSize)}</span>
                      <span>•</span>
                      <span>{formatDateTime(doc.createdAt)}</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant="outline">{doc.fileType.split("/")[1]?.toUpperCase()}</Badge>
                  <Button
                    variant="ghost"
                    size="icon"
                    asChild
                  >
                    <a
                      href={doc.contentUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      title="Önizle"
                    >
                      <ExternalLink className="h-4 w-4" />
                    </a>
                  </Button>
                  <AlertDialog>
                    <AlertDialogTrigger asChild>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="text-destructive hover:text-destructive"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </AlertDialogTrigger>
                    <AlertDialogContent>
                      <AlertDialogHeader>
                        <AlertDialogTitle>Belgeyi Sil</AlertDialogTitle>
                        <AlertDialogDescription>
                          &quot;{doc.documentTypeName}&quot; belgesini silmek istediğinize emin misiniz?
                          Bu işlem geri alınamaz.
                        </AlertDialogDescription>
                      </AlertDialogHeader>
                      <AlertDialogFooter>
                        <AlertDialogCancel>İptal</AlertDialogCancel>
                        <AlertDialogAction
                          onClick={() => handleDelete(doc.id)}
                          className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
                        >
                          Sil
                        </AlertDialogAction>
                      </AlertDialogFooter>
                    </AlertDialogContent>
                  </AlertDialog>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Henüz belge yüklenmemiş</p>
              <p className="text-sm">Yukarıdan belge türü seçerek yeni belge ekleyebilirsiniz</p>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
