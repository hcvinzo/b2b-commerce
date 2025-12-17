"use client";

import { useState, useRef } from "react";
import {
  Plus,
  Trash2,
  Star,
  Image as ImageIcon,
  Upload,
  Link as LinkIcon,
  Loader2,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { uploadImage, uploadImages } from "@/lib/api/files";
import { toast } from "sonner";

interface ProductImageManagerProps {
  mainImageUrl?: string;
  imageUrls: string[];
  onMainImageChange: (url: string | undefined) => void;
  onImageUrlsChange: (urls: string[]) => void;
}

export function ProductImageManager({
  mainImageUrl,
  imageUrls,
  onMainImageChange,
  onImageUrlsChange,
}: ProductImageManagerProps) {
  const [newImageUrl, setNewImageUrl] = useState("");
  const [urlError, setUrlError] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const validateUrl = (url: string): boolean => {
    if (!url) return false;
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  };

  const addImageUrl = (url: string) => {
    // If no main image, set this as main
    if (!mainImageUrl) {
      onMainImageChange(url);
    } else {
      // Add to additional images
      onImageUrlsChange([...imageUrls, url]);
    }
  };

  const handleAddImageUrl = () => {
    if (!newImageUrl.trim()) {
      setUrlError("Please enter a URL");
      return;
    }

    if (!validateUrl(newImageUrl)) {
      setUrlError("Please enter a valid URL");
      return;
    }

    addImageUrl(newImageUrl);
    setNewImageUrl("");
    setUrlError(null);
  };

  const handleFileSelect = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const files = event.target.files;
    if (!files || files.length === 0) return;

    setIsUploading(true);
    setUploadProgress(`Uploading ${files.length} file(s)...`);

    try {
      if (files.length === 1) {
        // Single file upload
        const result = await uploadImage(files[0]);
        addImageUrl(result.url);
        toast.success("Image uploaded", {
          description: `${files[0].name} uploaded successfully`,
        });
      } else {
        // Multiple files upload
        const result = await uploadImages(Array.from(files));

        // Add all successful uploads
        result.uploaded.forEach((file) => {
          addImageUrl(file.url);
        });

        // Show results
        if (result.uploaded.length > 0) {
          toast.success("Images uploaded", {
            description: `${result.uploaded.length} image(s) uploaded successfully`,
          });
        }

        if (result.errors.length > 0) {
          toast.error("Some uploads failed", {
            description: result.errors.map((e) => e.fileName).join(", "),
          });
        }
      }
    } catch (error) {
      console.error("Upload failed:", error);
      toast.error("Upload failed", {
        description: "Failed to upload image(s). Please try again.",
      });
    } finally {
      setIsUploading(false);
      setUploadProgress(null);
      // Reset file input
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    }
  };

  const handleDrop = async (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();

    const files = event.dataTransfer.files;
    if (!files || files.length === 0) return;

    // Filter to only image files
    const imageFiles = Array.from(files).filter((file) =>
      file.type.startsWith("image/")
    );

    if (imageFiles.length === 0) {
      toast.error("Invalid files", {
        description: "Please drop image files only",
      });
      return;
    }

    setIsUploading(true);
    setUploadProgress(`Uploading ${imageFiles.length} file(s)...`);

    try {
      if (imageFiles.length === 1) {
        const result = await uploadImage(imageFiles[0]);
        addImageUrl(result.url);
        toast.success("Image uploaded", {
          description: `${imageFiles[0].name} uploaded successfully`,
        });
      } else {
        const result = await uploadImages(imageFiles);

        result.uploaded.forEach((file) => {
          addImageUrl(file.url);
        });

        if (result.uploaded.length > 0) {
          toast.success("Images uploaded", {
            description: `${result.uploaded.length} image(s) uploaded successfully`,
          });
        }

        if (result.errors.length > 0) {
          toast.error("Some uploads failed", {
            description: result.errors.map((e) => e.fileName).join(", "),
          });
        }
      }
    } catch (error) {
      console.error("Upload failed:", error);
      toast.error("Upload failed", {
        description: "Failed to upload image(s). Please try again.",
      });
    } finally {
      setIsUploading(false);
      setUploadProgress(null);
    }
  };

  const handleDragOver = (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    event.stopPropagation();
  };

  const handleRemoveImage = (url: string, isMain: boolean) => {
    if (isMain) {
      // If removing main image, promote first additional image to main
      if (imageUrls.length > 0) {
        onMainImageChange(imageUrls[0]);
        onImageUrlsChange(imageUrls.slice(1));
      } else {
        onMainImageChange(undefined);
      }
    } else {
      onImageUrlsChange(imageUrls.filter((u) => u !== url));
    }
  };

  const handleSetAsMain = (url: string) => {
    // Move current main to additional images if exists
    if (mainImageUrl) {
      onImageUrlsChange([...imageUrls.filter((u) => u !== url), mainImageUrl]);
    } else {
      onImageUrlsChange(imageUrls.filter((u) => u !== url));
    }
    onMainImageChange(url);
  };

  const allImages = mainImageUrl
    ? [
        { url: mainImageUrl, isMain: true },
        ...imageUrls.map((url) => ({ url, isMain: false })),
      ]
    : imageUrls.map((url) => ({ url, isMain: false }));

  return (
    <div className="space-y-4">
      <Tabs defaultValue="upload" className="w-full">
        <TabsList className="grid w-full grid-cols-2">
          <TabsTrigger value="upload" className="flex items-center gap-2">
            <Upload className="h-4 w-4" />
            Upload File
          </TabsTrigger>
          <TabsTrigger value="url" className="flex items-center gap-2">
            <LinkIcon className="h-4 w-4" />
            Image URL
          </TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="mt-4">
          <div
            className={`border-2 border-dashed rounded-lg p-6 text-center transition-colors ${
              isUploading
                ? "border-primary bg-primary/5"
                : "border-muted-foreground/25 hover:border-primary/50"
            }`}
            onDrop={handleDrop}
            onDragOver={handleDragOver}
          >
            {isUploading ? (
              <div className="flex flex-col items-center gap-2">
                <Loader2 className="h-8 w-8 animate-spin text-primary" />
                <p className="text-sm text-muted-foreground">{uploadProgress}</p>
              </div>
            ) : (
              <>
                <Upload className="h-8 w-8 mx-auto text-muted-foreground mb-2" />
                <p className="text-sm text-muted-foreground mb-2">
                  Drag and drop images here, or click to select
                </p>
                <p className="text-xs text-muted-foreground mb-4">
                  Supports: JPG, PNG, GIF, WebP (max 5MB each)
                </p>
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => fileInputRef.current?.click()}
                >
                  <Upload className="h-4 w-4 mr-2" />
                  Select Files
                </Button>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept="image/jpeg,image/jpg,image/png,image/gif,image/webp"
                  multiple
                  className="hidden"
                  onChange={handleFileSelect}
                />
              </>
            )}
          </div>
        </TabsContent>

        <TabsContent value="url" className="mt-4">
          <div className="flex gap-2">
            <div className="flex-1">
              <Input
                placeholder="Enter image URL (e.g., https://example.com/image.jpg)"
                value={newImageUrl}
                onChange={(e) => {
                  setNewImageUrl(e.target.value);
                  setUrlError(null);
                }}
                onKeyDown={(e) => {
                  if (e.key === "Enter") {
                    e.preventDefault();
                    handleAddImageUrl();
                  }
                }}
              />
              {urlError && (
                <p className="mt-1 text-sm text-destructive">{urlError}</p>
              )}
            </div>
            <Button type="button" variant="outline" onClick={handleAddImageUrl}>
              <Plus className="h-4 w-4 mr-1" />
              Add
            </Button>
          </div>
        </TabsContent>
      </Tabs>

      {allImages.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-8 text-muted-foreground">
            <ImageIcon className="h-12 w-12 mb-2" />
            <p>No images added yet</p>
            <p className="text-sm">
              Upload files or add image URLs to display product images
            </p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {allImages.map((image, index) => (
            <Card
              key={image.url + index}
              className="group relative overflow-hidden"
            >
              <CardContent className="p-2">
                <div className="aspect-square relative bg-muted rounded-md overflow-hidden">
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img
                    src={image.url}
                    alt={`Product image ${index + 1}`}
                    className="object-cover w-full h-full"
                    onError={(e) => {
                      const target = e.target as HTMLImageElement;
                      target.style.display = "none";
                      const placeholder = target.nextElementSibling;
                      if (placeholder) {
                        (placeholder as HTMLElement).style.display = "flex";
                      }
                    }}
                  />
                  <div className="absolute inset-0 items-center justify-center bg-muted hidden">
                    <ImageIcon className="h-8 w-8 text-muted-foreground" />
                  </div>
                  {image.isMain && (
                    <div className="absolute top-1 left-1">
                      <span className="inline-flex items-center gap-1 bg-primary text-primary-foreground text-xs px-2 py-1 rounded-md">
                        <Star className="h-3 w-3 fill-current" />
                        Main
                      </span>
                    </div>
                  )}
                </div>
                <div className="flex gap-1 mt-2">
                  {!image.isMain && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="sm"
                      className="flex-1 h-8 text-xs"
                      onClick={() => handleSetAsMain(image.url)}
                    >
                      <Star className="h-3 w-3 mr-1" />
                      Set as Main
                    </Button>
                  )}
                  <Button
                    type="button"
                    variant="ghost"
                    size="sm"
                    className="h-8 text-destructive hover:text-destructive"
                    onClick={() => handleRemoveImage(image.url, image.isMain)}
                  >
                    <Trash2 className="h-3 w-3" />
                  </Button>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>
      )}

      <p className="text-sm text-muted-foreground">
        {allImages.length} image{allImages.length !== 1 ? "s" : ""} added
        {mainImageUrl && " (main image set)"}
      </p>
    </div>
  );
}
