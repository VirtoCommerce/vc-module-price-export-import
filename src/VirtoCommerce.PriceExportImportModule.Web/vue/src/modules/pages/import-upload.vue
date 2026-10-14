<template>
  <VcBlade
    :title="$t('PRICE_EXPORT_IMPORT.UPLOAD.TITLE')"
    :subtitle="$t('PRICE_EXPORT_IMPORT.UPLOAD.SUBTITLE')"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <VcForm class="pei-page">
        <VcFileUpload
          variant="file-upload"
          accept=".csv"
          :disabled="loading"
          :custom-text="{ dragHere: $t('PRICE_EXPORT_IMPORT.UPLOAD.DROP'), browse: $t('PRICE_EXPORT_IMPORT.UPLOAD.BROWSE') }"
          @upload="upload"
        />
        <VcCard
          v-if="result"
          :header="$t(result.failed ? 'PRICE_EXPORT_IMPORT.UPLOAD.FAILED' : 'PRICE_EXPORT_IMPORT.UPLOAD.SUCCESS')"
        >
          <div class="pei-card">
            <div class="pei-file">
              <VcIcon
                :icon="result.failed ? 'lucide-file-x' : errors.length ? 'lucide-file-warning' : 'lucide-file-check'"
                size="xl"
                :variant="result.failed ? 'danger' : errors.length ? 'warning' : 'success'"
                class="pei-file__icon"
              />
              <div class="pei-file__info">
                <div class="pei-file__name">{{ result.name }}</div>
                <div
                  v-if="result.failed || !errors.length"
                  class="pei-file__hint"
                >
                  {{ result.failed ?? result.size }}
                </div>
                <ul
                  v-else
                  class="pei-file__errors"
                >
                  <li
                    v-for="(e, i) in errors"
                    :key="i"
                  >
                    {{ errorText(e) }}
                  </li>
                </ul>
              </div>
            </div>
            <div
              v-if="file"
              class="pei-file__actions"
            >
              <VcButton
                variant="outline"
                icon="lucide-trash-2"
                @click="remove"
              >
                {{ $t("PRICE_EXPORT_IMPORT.UPLOAD.DELETE") }}
              </VcButton>
              <VcButton
                v-if="!errors.length"
                icon="lucide-eye"
                @click="preview"
              >
                {{ $t("PRICE_EXPORT_IMPORT.UPLOAD.PREVIEW") }}
              </VcButton>
            </div>
          </div>
        </VcCard>
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { readableSize, useBlade, usePopup } from "@vc-shell/framework";
import { VcBlade, VcButton, VcCard, VcContainer, VcFileUpload, VcForm, VcIcon } from "@vc-shell/framework/ui";
import { ref } from "vue";
import { useI18n } from "vue-i18n";
import { deleteAsset, numberSetting, priceImport, uploadTmp, type ValidationError } from "../api";

// Upload + validation of the prices CSV to import into a price list (legacy: "Import" on
// pricelistItemListController + fileUploadController); not routable: the price list arrives only through options
defineBlade({ url: "/price-import", name: "PriceImportUpload", permissions: ["pricing:simpleImport"], routable: false });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade, closeChildren, onBeforeClose } = useBlade<{ pricelistId: string; reload?: () => Promise<void> }>();
const { showConfirmation, showError } = usePopup();

const file = ref<string>(); // the uploaded CSV (asset relative url)
const result = ref<{ name: string; size?: string; failed?: string }>();
const errors = ref<ValidationError[]>([]);
const loading = ref(false);
const maxSize = numberSetting("PriceExportImport.Import.FileMaxSize", 1).then((mb) => mb * 1024 * 1024);

const errorText = (e: ValidationError) => {
  const key = `PRICE_EXPORT_IMPORT.VALIDATION.${e.errorCode}`;
  const text = t(key, e.properties ?? {});
  return text === key ? e.errorCode : text;
};

async function busy(action: () => Promise<unknown>) {
  loading.value = true;
  try {
    await action();
  } catch (e) {
    showError(String((e as Error)?.message ?? e));
  } finally {
    loading.value = false;
  }
}

async function dropFile() {
  const url = file.value;
  file.value = undefined;
  errors.value = [];
  if (url) await deleteAsset(url);
}

// a new file replaces the previous one; its preview closes (legacy removed the old CSV before uploading)
async function upload(files: FileList) {
  const picked = files[0];
  if (!picked) return;
  await closeChildren();
  await busy(async () => {
    await dropFile();
    if (!/\.csv$/i.test(picked.name)) {
      result.value = { name: picked.name, failed: t("PRICE_EXPORT_IMPORT.UPLOAD.FILE_TYPE_ERROR") };
    } else if (picked.size > (await maxSize)) {
      result.value = { name: picked.name, failed: t("PRICE_EXPORT_IMPORT.UPLOAD.MAX_SIZE_ERROR") };
    } else {
      result.value = undefined;
      file.value = await uploadTmp(picked);
      errors.value = await priceImport.validate(file.value);
      result.value = { name: picked.name, size: readableSize(picked.size) };
    }
  });
}

async function remove() {
  if (!(await showConfirmation(t("PRICE_EXPORT_IMPORT.UPLOAD.DELETE_CONFIRMATION")))) return;
  await closeChildren();
  await busy(dropFile);
  result.value = undefined;
}

const preview = () =>
  openBlade({
    name: "PriceImportPreview",
    options: { pricelistId: options.value?.pricelistId, filePath: file.value, reload: options.value?.reload },
  });

// closing keeps no orphan file: confirm, then delete the uploaded CSV (legacy bladeClose)
onBeforeClose(async () => {
  if (!file.value) return false;
  if (!(await showConfirmation(t("PRICE_EXPORT_IMPORT.UPLOAD.DELETE_CONFIRMATION")))) return true;
  await dropFile().catch(() => undefined);
  return false;
});
</script>

<style lang="scss">
.pei-file {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  &__icon {
    flex-shrink: 0;
  }
  &__info {
    display: flex;
    flex-direction: column;
    gap: 4px;
    min-width: 0;
  }
  &__name {
    font-weight: 500;
    word-break: break-all;
  }
  &__hint {
    color: var(--color-neutral-500);
    font-size: 13px;
  }
  &__errors {
    margin: 0;
    padding-left: 18px;
    color: var(--color-danger-600);
    font-size: 13px;
  }
  &__actions {
    display: flex;
    justify-content: flex-end;
    gap: 12px;
  }
}
</style>
