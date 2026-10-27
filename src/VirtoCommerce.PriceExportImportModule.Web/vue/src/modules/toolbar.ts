// The price list products toolbar extension point "pricing-pricelist-products-toolbar" (contract documented in the
// Pricing plugin, vc-module-pricing .../vue/src/modules/pages/pricelist-products.vue). Types repeated here: the contract is by shape.

export interface Pricelist {
  id?: string;
  name?: string;
  currency?: string;
}

export interface ProductPrice {
  productId?: string;
  prices?: { id?: string }[];
}

export interface PricingProductsToolbarContext {
  pricelistId: string;
  pricelist?: Pricelist;
  selection: ProductPrice[];
  keyword?: string;
  totalCount: number;
  openBlade: (event: { name: string; param?: string; options?: Record<string, unknown> }) => Promise<void>;
  reload: () => Promise<void>;
}

export interface PricingProductsToolbarCommand {
  title: string;
  icon?: string;
  permissions?: string | string[];
  isVisible?: (ctx: PricingProductsToolbarContext) => boolean;
  disabled?: (ctx: PricingProductsToolbarContext) => boolean;
  onClick: (ctx: PricingProductsToolbarContext) => unknown;
}
