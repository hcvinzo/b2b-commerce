import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCustomerDocuments,
  createCustomerDocument,
  deleteCustomerDocument,
  uploadDocument,
} from "@/lib/api/customers";
import type { CreateCustomerDocumentDto, CustomerDocumentType } from "@/types/entities";

export function useCustomerDocuments(customerId: string) {
  return useQuery({
    queryKey: ["customer-documents", customerId],
    queryFn: () => getCustomerDocuments(customerId),
    enabled: !!customerId,
  });
}

export function useUploadCustomerDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: async ({
      customerId,
      documentType,
      file,
    }: {
      customerId: string;
      documentType: CustomerDocumentType;
      file: File;
    }) => {
      // First upload the file
      const uploadResult = await uploadDocument(file);

      // Then create the document record
      const documentData: CreateCustomerDocumentDto = {
        documentType,
        fileName: uploadResult.fileName,
        fileType: uploadResult.contentType,
        contentUrl: uploadResult.url,
        fileSize: uploadResult.size,
      };

      return createCustomerDocument(customerId, documentData);
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["customer-documents", variables.customerId],
      });
      toast.success("Belge başarıyla yüklendi");
    },
    onError: (error: Error) => {
      toast.error(`Belge yüklenemedi: ${error.message}`);
    },
  });
}

export function useDeleteCustomerDocument() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      documentId,
    }: {
      customerId: string;
      documentId: string;
    }) => deleteCustomerDocument(customerId, documentId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: ["customer-documents", variables.customerId],
      });
      toast.success("Belge başarıyla silindi");
    },
    onError: (error: Error) => {
      toast.error(`Belge silinemedi: ${error.message}`);
    },
  });
}
