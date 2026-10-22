<script lang="ts" setup>
import { onBeforeUnmount, onMounted, ref, watch } from 'vue';

import { $t } from '@vben/locales';

import * as pdfjs from 'pdfjs-dist';
import pdfWorkerUrl from 'pdfjs-dist/build/pdf.worker.min.mjs?url';

const props = defineProps<{
  src: string;
}>();

pdfjs.GlobalWorkerOptions.workerSrc = pdfWorkerUrl;

const canvasContainer = ref<HTMLDivElement>();
const error = ref(false);
const loading = ref(false);
const renderedPageCount = ref(0);
const totalPageCount = ref(0);

let loadingTask: pdfjs.PDFDocumentLoadingTask | undefined;
let renderVersion = 0;

async function renderPdf() {
  const version = ++renderVersion;
  canvasContainer.value?.replaceChildren();
  error.value = false;
  loading.value = true;
  renderedPageCount.value = 0;
  totalPageCount.value = 0;

  if (!props.src || !canvasContainer.value) {
    loading.value = false;
    return;
  }

  try {
    const task = pdfjs.getDocument({ url: props.src });
    loadingTask = task;
    const pdf = await task.promise;
    if (version !== renderVersion) {
      await task.destroy();
      return;
    }

    totalPageCount.value = pdf.numPages;
    const maxPages = 1;

    for (let pageNumber = 1; pageNumber <= maxPages; pageNumber++) {
      if (version !== renderVersion) {
        break;
      }

      const page = await pdf.getPage(pageNumber);
      const viewport = page.getViewport({ scale: 1.6 });
      const canvas = document.createElement('canvas');
      const context = canvas.getContext('2d');
      if (!context) {
        throw new Error('Canvas is not supported');
      }

      const pixelRatio = Math.min(window.devicePixelRatio || 1, 2);
      canvas.width = Math.floor(viewport.width * pixelRatio);
      canvas.height = Math.floor(viewport.height * pixelRatio);
      canvas.className = 'h-auto w-full bg-white shadow-sm';
      context.scale(pixelRatio, pixelRatio);

      await page.render({
        canvas,
        canvasContext: context,
        viewport,
      }).promise;

      if (version !== renderVersion) {
        break;
      }
      canvasContainer.value?.append(canvas);
      renderedPageCount.value = pageNumber;
    }

    await task.destroy();
  } catch {
    if (version === renderVersion) {
      error.value = true;
    }
  } finally {
    if (version === renderVersion) {
      loadingTask = undefined;
      loading.value = false;
    }
  }
}

watch(
  () => props.src,
  () => {
    void renderPdf();
  },
  { flush: 'post' },
);

onMounted(() => {
  void renderPdf();
});

onBeforeUnmount(() => {
  renderVersion++;
  void loadingTask?.destroy();
  canvasContainer.value?.replaceChildren();
});
</script>

<template>
  <div class="flex h-full w-full flex-col">
    <div
      class="flex min-h-0 flex-1 flex-col items-center gap-3 overflow-auto bg-muted/20 p-3"
    >
      <div v-if="loading" class="py-12 text-sm text-muted-foreground">
        {{ $t('TestWorkshop.FileManager:LoadingPreview') }}
      </div>
      <div v-else-if="error" class="py-12 text-sm text-muted-foreground">
        {{ $t('TestWorkshop.FileManager:PreviewNotSupported') }}
      </div>
      <div
        ref="canvasContainer"
        class="flex w-full flex-col items-center gap-3"
      ></div>
    </div>
    <div
      v-if="renderedPageCount > 0 && totalPageCount > renderedPageCount"
      class="shrink-0 border-t px-4 py-2 text-xs text-muted-foreground"
    >
      {{ $t('page.fileManager.pdfPreviewNotice', [totalPageCount]) }}
    </div>
  </div>
</template>
