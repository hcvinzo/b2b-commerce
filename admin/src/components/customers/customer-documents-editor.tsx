"use client";

import { FileText, ExternalLink, File } from "lucide-react";

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

// Document URL structure stored in Customer.documentUrls JSON field
interface DocumentUrl {
  document_type: string;
  file_type: string;
  file_url: string;
  file_name: string;
  file_size: number;
}

// Document type labels in Turkish
const DOCUMENT_TYPE_LABELS: Record<string, string> = {
  taxCertificate: "Vergi Levhası",
  signatureCircular: "İmza Sirküleri",
  tradeRegistry: "Sicil Gazetesi",
  partnershipAgreement: "İş Ortağı Sözleşmesi",
  authorizedIdCopy: "Yetkili Kimlik Fotokopisi",
  authorizedResidenceDocument: "Yetkili İkamet Belgesi",
};

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

// Get document type label
function getDocumentTypeLabel(documentType: string): string {
  return DOCUMENT_TYPE_LABELS[documentType] || documentType;
}

// Get file extension from content type
function getFileExtension(fileType: string): string {
  const ext = fileType.split("/")[1];
  return ext?.toUpperCase() || "FILE";
}

interface CustomerDocumentsEditorProps {
  customerId: string;
  documentUrls?: string;
}

export function CustomerDocumentsEditor({ documentUrls }: CustomerDocumentsEditorProps) {
  // Parse documentUrls JSON string
  let documents: DocumentUrl[] = [];
  if (documentUrls) {
    try {
      documents = JSON.parse(documentUrls);
    } catch {
      console.error("Failed to parse documentUrls JSON");
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center gap-2">
          <FileText className="h-5 w-5" />
          Belgeler
        </CardTitle>
        <CardDescription>
          Müşteri başvurusu sırasında yüklenen belgeler
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-6">
        {/* Documents List */}
        <div className="space-y-3">
          {documents.length > 0 ? (
            documents.map((doc, index) => (
              <div
                key={`${doc.document_type}-${index}`}
                className="flex items-center justify-between p-4 border rounded-lg hover:bg-muted/50 transition-colors"
              >
                <div className="flex items-center gap-4">
                  {getFileIcon(doc.file_type)}
                  <div>
                    <p className="font-medium">{getDocumentTypeLabel(doc.document_type)}</p>
                    <div className="flex items-center gap-2 text-sm text-muted-foreground">
                      <span>{doc.file_name}</span>
                      <span>•</span>
                      <span>{formatFileSize(doc.file_size)}</span>
                    </div>
                  </div>
                </div>
                <div className="flex items-center gap-2">
                  <Badge variant="outline">{getFileExtension(doc.file_type)}</Badge>
                  <Button
                    variant="ghost"
                    size="icon"
                    asChild
                  >
                    <a
                      href={doc.file_url}
                      target="_blank"
                      rel="noopener noreferrer"
                      title="Önizle"
                    >
                      <ExternalLink className="h-4 w-4" />
                    </a>
                  </Button>
                </div>
              </div>
            ))
          ) : (
            <div className="text-center py-8 text-muted-foreground">
              <FileText className="h-12 w-12 mx-auto mb-4 opacity-50" />
              <p>Henüz belge yüklenmemiş</p>
              <p className="text-sm">Müşteri başvurusu sırasında belge yüklememiş</p>
            </div>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
