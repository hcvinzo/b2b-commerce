"use client";

import { useState } from "react";
import { Copy, Download, Check, AlertTriangle } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { CreateApiKeyResponse } from "@/types/entities";

interface ApiKeyCreatedDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  keyData: CreateApiKeyResponse | null;
}

export function ApiKeyCreatedDialog({
  open,
  onOpenChange,
  keyData,
}: ApiKeyCreatedDialogProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    if (keyData?.plainTextKey) {
      await navigator.clipboard.writeText(keyData.plainTextKey);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  const handleDownload = () => {
    if (!keyData) return;

    const content = `API Key Details
===============
Name: ${keyData.name}
Key: ${keyData.plainTextKey}
Prefix: ${keyData.keyPrefix}
Expires: ${keyData.expiresAt || "Never"}
Permissions: ${keyData.permissions.join(", ") || "None"}

WARNING: Store this key securely. It cannot be retrieved again.
Generated: ${new Date().toISOString()}
`;

    const blob = new Blob([content], { type: "text/plain" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = `api-key-${keyData.keyPrefix}.txt`;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    URL.revokeObjectURL(url);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertTriangle className="h-5 w-5 text-yellow-500" />
            API Key Created
          </DialogTitle>
          <DialogDescription>
            Your API key has been created successfully. Copy or download it now
            - this is the only time it will be shown.
          </DialogDescription>
        </DialogHeader>

        <Alert variant="destructive" className="bg-yellow-50 border-yellow-200 dark:bg-yellow-950 dark:border-yellow-800">
          <AlertTriangle className="h-4 w-4 text-yellow-600 dark:text-yellow-400" />
          <AlertDescription className="text-yellow-800 dark:text-yellow-200">
            {keyData?.warning || "Store this key securely. You won't be able to see it again."}
          </AlertDescription>
        </Alert>

        <div className="space-y-4">
          <div>
            <label className="text-sm font-medium text-muted-foreground">
              API Key
            </label>
            <div className="mt-1 flex items-center gap-2">
              <code className="flex-1 rounded bg-muted p-3 font-mono text-sm break-all">
                {keyData?.plainTextKey}
              </code>
              <Button variant="outline" size="icon" onClick={handleCopy}>
                {copied ? (
                  <Check className="h-4 w-4 text-green-500" />
                ) : (
                  <Copy className="h-4 w-4" />
                )}
              </Button>
            </div>
          </div>

          <div className="grid grid-cols-2 gap-4 text-sm">
            <div>
              <span className="text-muted-foreground">Name:</span>
              <p className="font-medium">{keyData?.name}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Prefix:</span>
              <p className="font-mono">{keyData?.keyPrefix}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Expires:</span>
              <p>{keyData?.expiresAt || "Never"}</p>
            </div>
            <div>
              <span className="text-muted-foreground">Permissions:</span>
              <p>{keyData?.permissions.length || 0} scopes</p>
            </div>
          </div>
        </div>

        <DialogFooter className="flex-col sm:flex-row gap-2">
          <Button variant="outline" onClick={handleDownload}>
            <Download className="mr-2 h-4 w-4" />
            Download as File
          </Button>
          <Button onClick={() => onOpenChange(false)}>
            I&apos;ve Saved the Key
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
