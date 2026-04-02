import { computed, onMounted, onUnmounted, ref } from "vue";
import * as cuotasApi from "@/services/cuotasApi";

function fechaPorDefectoAnios(atras) {
  const d = new Date();
  d.setFullYear(d.getFullYear() - atras);
  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, "0");
  const day = String(d.getDate()).padStart(2, "0");
  return `${y}-${m}-${day}`;
}

export function useCalculadoraCuotas() {
  const fechaNacimiento = ref("");
  const monto = ref(null);
  const meses = ref(12);
  const plazos = ref([]);
  const error = ref("");
  const resultadoModal = ref(null);
  const cargandoPlazos = ref(false);
  const cargandoCalculo = ref(false);

  let calcularAbort = null;
  let plazosAbort = null;

  const cargando = computed(() => cargandoPlazos.value || cargandoCalculo.value);

  const formularioHabilitado = computed(
    () => !cargandoPlazos.value && plazos.value.length > 0,
  );

  async function cargarPlazos(signal) {
    error.value = "";
    cargandoPlazos.value = true;
    try {
      const data = await cuotasApi.fetchPlazos(signal);
      plazos.value = Array.isArray(data) ? data : [];
      if (plazos.value.length && !plazos.value.some((p) => p.valor === meses.value)) {
        meses.value = plazos.value[0].valor;
      }
    } catch (e) {
      plazos.value = [];
      error.value =
        e instanceof Error ? e.message : "No se pudieron cargar los plazos.";
    } finally {
      cargandoPlazos.value = false;
    }
  }

  async function enviarCalculo() {
    error.value = "";
    resultadoModal.value = null;
    calcularAbort?.abort();
    calcularAbort = new AbortController();
    cargandoCalculo.value = true;
    try {
      const data = await cuotasApi.calcularCuota(
        {
          fechaNacimiento: fechaNacimiento.value,
          monto: Number(monto.value),
          meses: meses.value,
        },
        calcularAbort.signal,
      );
      if (!data?.exito) {
        resultadoModal.value = {
          tipo: "danger",
          titulo: data?.titulo || "No se pudo calcular la cuota",
          mensaje: data?.mensaje || "No se pudo calcular la cuota.",
        };
        return;
      }
      resultadoModal.value = {
        tipo: "success",
        titulo: "Cálculo realizado",
        cuota: data.cuota ?? 0,
        tasaAplicada: data.tasaAplicada ?? null,
        tabla: Array.isArray(data.tablaAmortizacion)
          ? data.tablaAmortizacion
          : [],
        aviso: data.informacionSucursal ?? "",
      };
    } catch (e) {
      if (e instanceof DOMException && e.name === "AbortError") {
        return;
      }
      resultadoModal.value = {
        tipo: "danger",
        titulo: "Error",
        mensaje:
          e instanceof Error
            ? e.message
            : "Error de red. Compruebe que la API esté en ejecución.",
      };
    } finally {
      cargandoCalculo.value = false;
    }
  }

  onMounted(() => {
    fechaNacimiento.value = fechaPorDefectoAnios(20);
    plazosAbort = new AbortController();
    cargarPlazos(plazosAbort.signal);
  });

  onUnmounted(() => {
    plazosAbort?.abort();
    calcularAbort?.abort();
  });

  return {
    fechaNacimiento,
    monto,
    meses,
    plazos,
    error,
    resultadoModal,
    cargando,
    cargandoPlazos,
    formularioHabilitado,
    enviarCalculo,
    cargarPlazos,
  };
}
