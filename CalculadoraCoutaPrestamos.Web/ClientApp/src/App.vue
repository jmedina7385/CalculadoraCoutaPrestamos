<template>
  <div class="vue-shell d-flex flex-column min-vh-100">
    <header>
      <nav
        class="navbar navbar-expand-sm navbar-toggleable-sm navbar-light bg-white border-bottom box-shadow mb-0"
      >
        <div class="container-fluid">
          <a class="navbar-brand" href="/Prestamos">Calculadora de préstamos</a>
          <button
            class="navbar-toggler"
            type="button"
            data-bs-toggle="collapse"
            data-bs-target="#navbarVue"
            aria-controls="navbarVue"
            aria-expanded="false"
            aria-label="Alternar navegación"
          >
            <span class="navbar-toggler-icon"></span>
          </button>
          <div id="navbarVue" class="collapse navbar-collapse d-sm-inline-flex justify-content-between">
            <ul class="navbar-nav flex-grow-1">
              <li class="nav-item">
                <a class="nav-link text-dark" href="/Prestamos">Inicio (MVC)</a>
              </li>
              <li class="nav-item">
                <a
                  class="nav-link text-dark active"
                  href="/app/"
                  aria-current="page"
                >
                  Calculadora (Vue)
                </a>
              </li>
              <li v-if="mostrarSwagger" class="nav-item">
                <a
                  class="nav-link text-dark"
                  href="/swagger/index.html"
                  target="_blank"
                  rel="noopener noreferrer"
                >
                  Swagger
                </a>
              </li>
              <li class="nav-item">
                <a class="nav-link text-dark" href="/Home/Privacy">Privacidad</a>
              </li>
            </ul>
          </div>
        </div>
      </nav>
    </header>

    <main class="container flex-grow-1 py-4">
      <div class="calculadora-app">
        <div class="mb-4">
          <h1 class="h3 mb-1">Calculadora de cuotas</h1>
          <p class="text-muted small mb-0">
            Vue 3 · API REST · Cuota = (Monto × Tasa) ÷ meses
          </p>
        </div>

        <CuotaForm
          v-model:fechaNacimiento="fechaNacimiento"
          v-model:monto="monto"
          v-model:meses="meses"
          :plazos="plazos"
          :disabled="!formularioHabilitado"
          :submitting="cargandoCalculoSolo"
          @submit="enviarCalculo"
        />

        <div
          v-if="error"
          class="alert alert-danger mt-3"
          role="alert"
        >
          {{ error }}
        </div>

        <div
          ref="modalResultadoEl"
          class="modal fade"
          id="modalResultadoVue"
          tabindex="-1"
          aria-labelledby="modalResultadoVueLabel"
          aria-hidden="true"
        >
          <div
            class="modal-dialog modal-dialog-centered modal-lg modal-dialog-scrollable"
          >
            <div class="modal-content">
              <div
                class="modal-header"
                :class="
                  resultadoModal?.tipo === 'danger'
                    ? 'border-danger text-danger border-bottom'
                    : 'border-success text-success border-bottom'
                "
              >
                <h5 id="modalResultadoVueLabel" class="modal-title">
                  {{ resultadoModal?.titulo }}
                </h5>
                <button
                  type="button"
                  class="btn-close"
                  data-bs-dismiss="modal"
                  aria-label="Cerrar"
                />
              </div>
              <div class="modal-body">
                <template v-if="resultadoModal?.tipo === 'danger'">
                  <p class="mb-0">{{ resultadoModal.mensaje }}</p>
                </template>
                <template v-else-if="resultadoModal?.tipo === 'success'">
                  <p class="mb-2">
                    <strong>Cuota mensual:</strong> {{ formatMoney(resultadoModal.cuota) }}
                  </p>
                  <p
                    v-if="resultadoModal.tasaAplicada != null"
                    class="mb-2 small"
                  >
                    <strong>Factor de tasa aplicado (por edad):</strong>
                    {{ resultadoModal.tasaAplicada }}
                  </p>
                  <p class="small text-muted mb-2">
                    La cuota es fija. En cada mes se reparte el capital al préstamo; el interés del
                    periodo es el remanente de esa cuota respecto al capital abonado.
                  </p>
                  <p class="mb-2 text-muted small">{{ resultadoModal.aviso }}</p>
                  <div
                    v-if="resultadoModal.tabla?.length"
                    class="table-responsive mt-2"
                    style="max-height: 280px; overflow-y: auto"
                  >
                    <table
                      class="table table-sm table-striped table-bordered align-middle mb-0"
                    >
                      <thead class="table-light">
                        <tr>
                          <th>#</th>
                          <th class="text-end">Saldo inicial</th>
                          <th class="text-end">Cuota</th>
                          <th class="text-end">Capital</th>
                          <th class="text-end">Interés</th>
                          <th class="text-end">Saldo final</th>
                        </tr>
                      </thead>
                      <tbody>
                        <tr v-for="f in resultadoModal.tabla" :key="f.numeroCuota">
                          <td>{{ f.numeroCuota }}</td>
                          <td class="text-end">{{ formatMoney(f.saldoInicial) }}</td>
                          <td class="text-end">{{ formatMoney(f.cuota) }}</td>
                          <td class="text-end">{{ formatMoney(f.capital) }}</td>
                          <td class="text-end">{{ formatMoney(f.interes) }}</td>
                          <td class="text-end">{{ formatMoney(f.saldoFinal) }}</td>
                        </tr>
                      </tbody>
                    </table>
                  </div>
                  <p
                    v-if="resultadoModal.tabla?.length"
                    class="small text-muted mt-2 mb-0"
                  >
                    <strong>Tabla de amortización</strong> (referencia según la cuota calculada).
                  </p>
                </template>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">
                  Aceptar
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <footer class="border-top footer text-muted mt-auto py-3">
      <div class="container">
        &copy; 2026 - CalculadoraCoutaPrestamos.Web —
        <a href="/Home/Privacy">Privacidad</a>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { Modal } from "bootstrap";
import { computed, nextTick, ref, watch } from "vue";
import CuotaForm from "@/components/CuotaForm.vue";
import { useCalculadoraCuotas } from "@/composables/useCalculadoraCuotas";

const mostrarSwagger = import.meta.env.DEV;

const {
  fechaNacimiento,
  monto,
  meses,
  plazos,
  error,
  resultadoModal,
  formularioHabilitado,
  enviarCalculo,
  cargandoPlazos,
  cargando,
} = useCalculadoraCuotas();

const cargandoCalculoSolo = computed(
  () => cargando.value && !cargandoPlazos.value,
);

const modalResultadoEl = ref(null);

watch(resultadoModal, async (payload) => {
  if (!payload) {
    return;
  }
  await nextTick();
  const el = modalResultadoEl.value;
  if (!el) {
    return;
  }
  Modal.getOrCreateInstance(el).show();
});

function formatMoney(value) {
  return new Intl.NumberFormat("es-DO", {
    style: "currency",
    currency: "DOP",
  }).format(value);
}
</script>

<style scoped>
.vue-shell .footer a {
  color: inherit;
  text-decoration: underline;
}
</style>
