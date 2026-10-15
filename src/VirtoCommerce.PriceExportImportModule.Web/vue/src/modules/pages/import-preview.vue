<template>
  <VcBlade
    :title="$t('PRICE_EXPORT_IMPORT.PREVIEW.TITLE')"
    :subtitle="$t('PRICE_EXPORT_IMPORT.PREVIEW.SUBTITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <div class="pei-preview">
      <div class="pei-preview__head">
        <VcRadioGroup
          v-model="importMode"
          :label="$t('PRICE_EXPORT_IMPORT.PREVIEW.STRATEGY')"
          :options="modes"
          orientation="horizontal"
        />
        <span class="pei-preview__count">
          {{ $t("PRICE_EXPORT_IMPORT.PREVIEW.COUNT", { count: n(rows.length), total: n(totalCount) }) }}
        </span>
      </div>
      <VcBanner
        v-if="unparsed"
        variant="warning"
      >
        {{ $t("PRICE_EXPORT_IMPORT.PREVIEW.UNPARSED", { count: n(unparsed), preview: n(previewCount) }) }}
      </VcBanner>
      <VcDataTable
        class="pei-preview__table"
        :items="rows"
        data-key="_row"
        :total-count="rows.length"
        :empty-state="{ icon: 'lucide-file-spreadsheet', title: $t('PRICE_EXPORT_IMPORT.PREVIEW.EMPTY') }"
      >
        <VcColumn
          id="sku"
          :title="$t('PRICE_EXPORT_IMPORT.PREVIEW.SKU')"
          always-visible
          mobile-role="title"
        />
        <VcColumn
          id="minQuantity"
          field="price.minQuantity"
          :title="$t('PRICE_EXPORT_IMPORT.PREVIEW.MIN_QUANTITY')"
          type="number"
          align="end"
          mobile-role="field"
        />
        <VcColumn
          id="list"
          :title="$t('PRICE_EXPORT_IMPORT.PREVIEW.LIST_PRICE')"
          align="end"
          mobile-role="field"
        >
          <template #body="{ data }">
            <!-- a number cell keeps its label on phones -->
            <span
              v-if="typeof data.price?.list === 'number'"
              class="vc-table-cell-number"
            >{{ money(data.price.list) }}</span>
          </template>
        </VcColumn>
        <VcColumn
          id="sale"
          :title="$t('PRICE_EXPORT_IMPORT.PREVIEW.SALE_PRICE')"
          align="end"
          mobile-role="field"
        >
          <template #body="{ data }">
            <!-- a number cell keeps its label on phones -->
            <span
              v-if="typeof data.price?.sale === 'number'"
              class="vc-table-cell-number"
            >{{ money(data.price.sale) }}</span>
          </template>
        </VcColumn>
      </VcDataTable>
    </div>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBanner, VcBlade, VcColumn, VcDataTable, VcRadioGroup } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { priceImport, type ImportMode, type ImportPrice } from "../api";

// The first lines of the uploaded CSV as the importer reads them, and the import strategy (legacy importPreviewController).
// Not routable: the uploaded file arrives only through options.
defineBlade({ url: "/price-import-preview", name: "PriceImportPreview", permissions: ["pricing:simpleImport"], routable: false });

const PREVIEW_SIZE = 10; // the server reads 10 lines
const { t, n } = useI18n({ useScope: "global" });
const { options, openBlade, closeSelf } = useBlade<{ pricelistId: string; filePath: string; reload?: () => Promise<void> }>();
const { showError } = usePopup();

const modes: { label: string; value: ImportMode }[] = [
  { label: t("PRICE_EXPORT_IMPORT.PREVIEW.CREATE_ONLY"), value: "createOnly" },
  { label: t("PRICE_EXPORT_IMPORT.PREVIEW.UPDATE_ONLY"), value: "updateOnly" },
  { label: t("PRICE_EXPORT_IMPORT.PREVIEW.CREATE_AND_UPDATE"), value: "createAndUpdate" },
];
const importMode = ref<ImportMode>("updateOnly");
const rows = ref<(ImportPrice & { _row: number })[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const previewCount = computed(() => Math.min(totalCount.value, PREVIEW_SIZE));
const unparsed = computed(() => Math.max(0, previewCount.value - rows.value.length));
const money = (v?: number | null) => (typeof v === "number" ? n(v, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) : "");

onMounted(async () => {
  if (!options.value?.filePath) return;
  loading.value = true;
  try {
    const r = await priceImport.preview(options.value.filePath);
    rows.value = (r.results ?? []).map((x, i) => ({ ...x, _row: i }));
    totalCount.value = r.totalCount ?? 0;
  } catch (e) {
    showError(String((e as Error)?.message ?? e));
  } finally {
    loading.value = false;
  }
});

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "import",
    title: t("PRICE_EXPORT_IMPORT.TOOLBAR.RUN_IMPORT"),
    icon: "lucide-upload",
    permissions: "pricing:simpleImport",
    disabled: loading.value || !totalCount.value,
    async clickHandler() {
      const o = options.value;
      if (!o) return;
      try {
        const notification = await priceImport.run({ pricelistId: o.pricelistId, importMode: importMode.value, filePath: o.filePath });
        await openBlade({ name: "PriceImportProcessing", options: { notification, reload: o.reload } });
      } catch (e) {
        showError(String((e as Error)?.message ?? e));
      }
    },
  },
  {
    id: "upload-new",
    title: t("PRICE_EXPORT_IMPORT.TOOLBAR.UPLOAD_NEW"),
    icon: "lucide-file-up",
    clickHandler: () => closeSelf(),
  },
]);
</script>

<style lang="scss">
.pei-preview {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 0;
  &__head {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-end;
    justify-content: space-between;
    gap: 12px 24px;
    padding: 16px;
    border-bottom: 1px solid var(--color-neutral-200);
  }
  &__count {
    font-size: 13px;
    font-weight: 500;
    color: var(--color-neutral-900);
  }
  &__table {
    flex: 1;
    min-height: 0;
  }
  .vc-banner {
    margin: 12px 16px 0;
  }
}
</style>
