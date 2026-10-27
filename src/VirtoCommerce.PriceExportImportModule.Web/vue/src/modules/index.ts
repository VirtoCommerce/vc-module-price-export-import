import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, useExtensionPoint } from "@vc-shell/framework";
import { markRaw } from "vue";
import type { PricingProductsToolbarCommand } from "./toolbar";

// CSV export / import on the price list products toolbar (legacy: toolbarService on pricelistItemListController).
// Named "… CSV": the list already has Pricing's own (advanced) Export.
const none = markRaw({ render: () => null });
const toolbar = useExtensionPoint("pricing-pricelist-products-toolbar");
toolbar.add({
  id: "price-csv-export",
  priority: 10,
  component: none,
  meta: {
    title: "PRICE_EXPORT_IMPORT.TOOLBAR.EXPORT",
    icon: "lucide-download",
    permissions: "export:access",
    onClick: (ctx) =>
      ctx.openBlade({
        name: "PriceExportCsv",
        options: {
          pricelistId: ctx.pricelistId,
          pricelist: ctx.pricelist,
          selection: ctx.selection,
          keyword: ctx.keyword,
          totalCount: ctx.totalCount,
        },
      }),
  } satisfies PricingProductsToolbarCommand,
});
toolbar.add({
  id: "price-csv-import",
  priority: 20,
  component: none,
  meta: {
    title: "PRICE_EXPORT_IMPORT.TOOLBAR.IMPORT",
    icon: "lucide-upload",
    permissions: "pricing:simpleImport",
    onClick: (ctx) => ctx.openBlade({ name: "PriceImportUpload", options: { pricelistId: ctx.pricelistId, reload: ctx.reload } }),
  } satisfies PricingProductsToolbarCommand,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
