import { onMounted, onBeforeUnmount, watch } from "vue";
import IMask from "imask";
import { moneyMaskOptions } from "@/utils/moneyMask";

export function useImaskMoneyModel(inputRef, modelMonto, disabledRef) {
  let im = null;

  function typedToModel() {
    if (!im) return;
    const v = im.typedValue;
    if (v === "" || v == null || (typeof v === "number" && Number.isNaN(v))) {
      modelMonto.value = null;
      return;
    }
    if (typeof v === "number") {
      modelMonto.value = v;
    }
  }

  onMounted(() => {
    const el = inputRef.value;
    if (!el) return;
    im = IMask(el, {
      ...moneyMaskOptions,
      disable: disabledRef?.value ?? false,
    });
    if (modelMonto.value != null && typeof modelMonto.value === "number" && !Number.isNaN(modelMonto.value)) {
      im.typedValue = modelMonto.value;
    }
    im.on("accept", typedToModel);
  });

  if (disabledRef) {
    watch(disabledRef, (d) => {
      im?.updateOptions({ ...moneyMaskOptions, disable: d });
    });
  }

  onBeforeUnmount(() => {
    im?.destroy();
    im = null;
  });
}
