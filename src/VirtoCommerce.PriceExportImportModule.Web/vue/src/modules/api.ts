// The module's API (api/pricing/import) and the platform calls its blades need, as small same-origin fetches
// (cookie auth, like the generated clients).
import type { JobNotification } from "./composables/useJob";

export async function http<T = unknown>(method: string, url: string, body?: unknown): Promise<T> {
  const res = await fetch(url, {
    method,
    headers: { Accept: "application/json", ...(body !== undefined && { "Content-Type": "application/json" }) },
    body: body !== undefined ? JSON.stringify(body) : undefined,
  });
  const text = await res.text();
  if (!res.ok) throw new Error(`${method} ${url}: ${res.status} ${text}`.trim());
  return (text ? JSON.parse(text) : undefined) as T;
}

export type ImportMode = "createOnly" | "updateOnly" | "createAndUpdate";

export interface ValidationError {
  errorCode: string;
  properties?: Record<string, string>;
}

export interface ImportNotification extends JobNotification {
  createdCount?: number;
  updatedCount?: number;
  reportUrl?: string;
}

export interface ImportPrice {
  sku?: string;
  productId?: string;
  price?: { minQuantity?: number; list?: number; sale?: number | null };
}

/** A positive integer platform setting (legacy settings.getValues). */
export const numberSetting = (name: string, fallback: number) =>
  http<unknown[]>("GET", `/api/platform/settings/values/${encodeURIComponent(name)}`).then(
    (v) => Number(v?.[0]) || fallback,
    () => fallback,
  );

/** Uploads a file to the asset store's tmp folder (as the AngularJS uploader did); returns its relative url. */
export async function uploadTmp(file: File): Promise<string> {
  const body = new FormData();
  body.append("file", file);
  const res = await fetch("/api/assets?folderUrl=tmp", { method: "POST", headers: { Accept: "application/json" }, body });
  if (!res.ok) throw new Error(`${file.name}: ${res.status} ${await res.text()}`.trim());
  const [asset] = (await res.json()) as { relativeUrl?: string; url?: string }[];
  return asset.relativeUrl ?? asset.url!;
}

export const deleteAsset = (url: string) => http("DELETE", `/api/assets?urls=${encodeURIComponent(url)}`);

export const priceImport = {
  validate: (filePath: string) =>
    http<{ errors?: ValidationError[] }>("POST", "/api/pricing/import/validate", { filePath }).then((r) => r.errors ?? []),
  preview: (filePath: string) =>
    http<{ totalCount?: number; results?: ImportPrice[] }>("POST", "/api/pricing/import/preview", { filePath }),
  run: (request: { pricelistId: string; importMode: ImportMode; filePath: string }) =>
    http<ImportNotification>("POST", "/api/pricing/import/run", request),
  cancel: (jobId: string) => http("POST", "/api/pricing/import/cancel", { jobId }),
};

/** Opens a file of the asset store (relative or absolute url) in a download. */
export function download(url: string) {
  const link = document.createElement("a");
  link.href = new URL(url, `${location.origin}/`).href;
  link.download = "";
  document.body.appendChild(link);
  link.click();
  link.remove();
}

export const fileName = (url?: string) => url?.split(/[\/]/).pop() ?? "";
