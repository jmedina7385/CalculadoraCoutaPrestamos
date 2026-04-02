<template>
  <form class="card shadow-sm" @submit.prevent="$emit('submit')">
    <div class="card-body d-flex flex-column gap-3">
      <div>
        <label class="form-label" for="fecha-nac">Fecha de nacimiento</label>
        <input
          id="fecha-nac"
          v-model="fechaNacimiento"
          type="date"
          class="form-control"
          required
          :disabled="disabled"
          :min="fechaMin"
          :max="fechaMax"
        />
        <small class="text-muted d-block">
          Las tasas de esta calculadora aplican a edades de 18 a 25 años; fuera de ese rango la evaluación es
          en sucursal.
        </small>
      </div>
      <div>
        <label class="form-label" for="monto">Monto del préstamo</label>
        <input
          id="monto"
          ref="montoInputRef"
          type="text"
          class="form-control"
          inputmode="decimal"
          autocomplete="off"
          placeholder="0.00"
          :disabled="disabled"
          aria-describedby="monto-hint"
        />
        <small id="monto-hint" class="text-muted">Formato: miles con coma y decimales con punto (ej. 12,500.50)</small>
      </div>
      <div>
        <label class="form-label" for="meses">Plazo (meses)</label>
        <select
          id="meses"
          v-model.number="meses"
          class="form-select"
          required
          :disabled="disabled || !plazos.length"
        >
          <option v-if="!plazos.length" disabled value="">
            Cargando plazos…
          </option>
          <option v-for="p in plazos" :key="p.valor" :value="p.valor">
            {{ p.descripcion }}
          </option>
        </select>
      </div>
      <button
        type="submit"
        class="btn btn-primary w-100"
        :disabled="disabled || !plazos.length || submitting || monto == null || monto <= 0"
      >
        <span
          v-if="submitting"
          class="spinner-border spinner-border-sm me-2"
          role="status"
          aria-hidden="true"
        />
        {{ submitting ? "Calculando…" : "Calcular cuota" }}
      </button>
    </div>
  </form>
</template>

<script setup>
import { ref, toRef } from "vue";
import { useImaskMoneyModel } from "@/composables/useImaskMoneyModel";

const props = defineProps({
  plazos: { type: Array, default: () => [] },
  disabled: { type: Boolean, default: false },
  submitting: { type: Boolean, default: false },
});

defineEmits(["submit"]);

const fechaNacimiento = defineModel("fechaNacimiento", { type: String, required: true });
const monto = defineModel("monto", { type: Number, default: null });
const meses = defineModel("meses", { type: Number, required: true });

const montoInputRef = ref(null);

const fechaMax = new Date().toISOString().slice(0, 10);
const fechaMin = (() => {
  const d = new Date();
  d.setFullYear(d.getFullYear() - 120);
  return d.toISOString().slice(0, 10);
})();

useImaskMoneyModel(montoInputRef, monto, toRef(props, "disabled"));
</script>
