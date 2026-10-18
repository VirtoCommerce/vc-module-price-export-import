<template>
  <VcBlade
    :title="$t('PRICE_EXPORT_IMPORT.EXPORT.TITLE')"
    :subtitle="options?.pricelist?.name"
    :toolbar-items="toolbar"
    :loading="limit === undefined"
    width="50%"
  >
    <VcContainer>
      <div
        v-if="limit !== undefined"
        class="pei-page"
      >
        <VcBanner
          v-if="tooMany"
          variant="warning"
        >
          {{ $t("PRICE_EXPORT_IMPORT.EXPORT.TOO_MANY", { count, limit }) }}
        </VcBanner>
        <VcCard
          v-else
          :header="$t('PRICE_EXPORT_IMPORT.EXPORT.SUMMARY')"
        >
          <div class="pei-card">
            <div>{{ $t(allSelected ? "PRICE_EXPORT_IMPORT.EXPORT.ALL" : "PRICE_EXPORT_IMPORT.EXPORT.SELECTED", { count }) }}</div>
            <div class="pei-card__hint">{{ $t("PRICE_EXPORT_IMPORT.EXPORT.ADVANCED_HINT") }}</div>
            <div>
              <VcButton
                variant="outline"
                icon="lucide-settings-2"
                @click="advanced"
              >
                {{ $t("PRICE_EXPORT_IMPORT.EXPORT.ADVANCED") }}
              </VcButton>
            </div>
          </div>
        </VcCard>
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcButton, VcCard, VcContainer } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { numberSetting } from "../api";
import type { Pricelist, ProductPrice } from "../toolbar";

// CSV export of a price list's prices (legacy: "Export" on pricelistItemListController + price-export dialog), run by the
// Export module: "Start export" opens its progress blade, "Advanced export" its settings blade, both pre-filled.
// Not routable: the price list and selection arrive only through options.
defineBlade({ url: "/price-export", name: "PriceExportCsv", permissions: ["export:access"], routable: false });

const EXPORT_TYPE = "VirtoCommerce.PricingModule.Data.ExportImport.ExportablePrice";
// the columns of the simple export (legacy csvPropertyInfo)
const COLUMNS: [string, string][] = [
  ["Code", "SKU"],
  ["ProductName", "Product Name"],
  ["Currency", "Currency"],
  ["List", "List price"],
  ["Sale", "Sale price"],
  ["MinQuantity", "Min quantity"],
  ["ModifiedDate", "Modified"],
  ["StartDate", "Valid from"],
  ["EndDate", "Valid to"],
  ["CreatedDate", "Created date"],
  ["CreatedBy", "Created by"],
  ["ModifiedBy", "Modified By"],
];

const { t } = useI18n({ useScope: "global" });
const { options, replaceWith } = useBlade<{
  pricelistId: string;
  pricelist?: Pricelist;
  selection: ProductPrice[];
  keyword?: string;
  totalCount: number;
}>();

const limit = ref<number>();
const selection = computed(() => options.value?.selection ?? []);
const allSelected = computed(() => !selection.value.length);
const count = computed(() => (allSelected.value ? (options.value?.totalCount ?? 0) : selection.value.length));
const tooMany = computed(() => limit.value !== undefined && count.value > limit.value);

// nothing selected: every price of the price list the search matches (legacy dataQuery)
const dataQuery = () => ({
  exportTypeName: "PriceExportDataQuery",
  isAllSelected: allSelected.value,
  isAnyFilterApplied: true,
  objectIds: allSelected.value ? [] : selection.value.flatMap((x) => (x.prices ?? []).map((p) => p.id)),
  productIds: allSelected.value ? [] : selection.value.map((x) => x.productId),
  groupByProducts: true,
  pricelistIds: [options.value?.pricelistId],
  keyword: options.value?.keyword,
});

onMounted(async () => (limit.value = await numberSetting("PriceExportImport.Export.LimitOfLines", 10000)));

const advanced = () =>
  replaceWith({
    name: "ExportSettings",
    options: {
      exportDataRequest: { exportTypeName: EXPORT_TYPE, dataQuery: dataQuery() },
      totalItemsCount: allSelected.value ? undefined : count.value,
      title: t("PRICE_EXPORT_IMPORT.EXPORT.TITLE"),
      subtitle: options.value?.pricelist?.name,
    },
  });

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "start",
    title: t("PRICE_EXPORT_IMPORT.TOOLBAR.START_EXPORT"),
    icon: "lucide-play",
    disabled: limit.value === undefined || tooMany.value,
    clickHandler: () =>
      replaceWith({
        name: "ExportProgress",
        options: {
          request: {
            exportTypeName: EXPORT_TYPE,
            providerName: "CsvExportProvider",
            providerConfig: { type: "CsvProviderConfiguration", delimiter: ";" },
            dataQuery: {
              ...dataQuery(),
              includedProperties: COLUMNS.map(([fullName, displayName]) => ({ fullName, displayName, group: "TabularPrice", isRequired: false })),
            },
          },
        },
      }),
  },
]);
</script>

<style lang="scss">
.pei-page {
  padding: 20px 22px 36px;
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.pei-card {
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
  font-size: 14px;
  color: var(--color-neutral-900);
  &__hint {
    color: var(--color-neutral-500);
    font-size: 13px;
  }
}
</style>
