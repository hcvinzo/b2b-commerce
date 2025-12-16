"use client";

import { useState } from "react";
import { Plus, Trash2, Star, Image as ImageIcon } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
} from "@/components/ui/card";

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

  const validateUrl = (url: string): boolean => {
    if (!url) return false;
    try {
      new URL(url);
      return true;
    } catch {
      return false;
    }
  };

  const handleAddImage = () => {
    if (!newImageUrl.trim()) {
      setUrlError("Please enter a URL");
      return;
    }

    if (!validateUrl(newImageUrl)) {
      setUrlError("Please enter a valid URL");
      return;
    }

    // If no main image, set this as main
    if (!mainImageUrl) {
      onMainImageChange(newImageUrl);
    } else {
      // Add to additional images
      onImageUrlsChange([...imageUrls, newImageUrl]);
    }

    setNewImageUrl("");
    setUrlError(null);
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
    ? [{ url: mainImageUrl, isMain: true }, ...imageUrls.map((url) => ({ url, isMain: false }))]
    : imageUrls.map((url) => ({ url, isMain: false }));

  return (
    <div className="space-y-4">
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
                handleAddImage();
              }
            }}
          />
          {urlError && <p className="mt-1 text-sm text-destructive">{urlError}</p>}
        </div>
        <Button type="button" variant="outline" onClick={handleAddImage}>
          <Plus className="h-4 w-4 mr-1" />
          Add
        </Button>
      </div>

      {allImages.length === 0 ? (
        <Card>
          <CardContent className="flex flex-col items-center justify-center py-8 text-muted-foreground">
            <ImageIcon className="h-12 w-12 mb-2" />
            <p>No images added yet</p>
            <p className="text-sm">Add image URLs to display product images</p>
          </CardContent>
        </Card>
      ) : (
        <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
          {allImages.map((image, index) => (
            <Card key={image.url + index} className="group relative overflow-hidden">
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
