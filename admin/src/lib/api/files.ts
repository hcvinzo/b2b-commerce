import { apiClient } from "./client";

export interface FileUploadResponse {
  url: string;
  fileName: string;
  contentType: string;
  size: number;
}

export interface FileUploadError {
  fileName: string;
  error: string;
}

export interface MultipleFileUploadResponse {
  uploaded: FileUploadResponse[];
  errors: FileUploadError[];
}

/**
 * Upload a single image file
 */
export async function uploadImage(
  file: File,
  folder: string = "products"
): Promise<FileUploadResponse> {
  const formData = new FormData();
  formData.append("file", file);

  const response = await apiClient.post<FileUploadResponse>(
    `/files/upload/image?folder=${encodeURIComponent(folder)}`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );

  return response.data;
}

/**
 * Upload multiple image files
 */
export async function uploadImages(
  files: File[],
  folder: string = "products"
): Promise<MultipleFileUploadResponse> {
  const formData = new FormData();
  files.forEach((file) => {
    formData.append("files", file);
  });

  const response = await apiClient.post<MultipleFileUploadResponse>(
    `/files/upload/images?folder=${encodeURIComponent(folder)}`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    }
  );

  return response.data;
}

/**
 * Delete an uploaded file
 */
export async function deleteFile(url: string): Promise<void> {
  await apiClient.delete("/files", {
    data: { url },
  });
}
