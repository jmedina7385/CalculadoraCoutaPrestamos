const jsonHeaders = { Accept: "application/json" };

async function parseJsonBody(res) {
  const text = await res.text();
  if (!text) {
    return null;
  }
  try {
    return JSON.parse(text);
  } catch {
    throw new Error("La respuesta del servidor no es JSON válido.");
  }
}

async function handleResponse(res, allowAnyOkStatus = false) {
  const data = await parseJsonBody(res);
  if (!res.ok && !allowAnyOkStatus) {
    const msg =
      (data && typeof data === "object" && ("title" in data || "detail" in data)
        ? [data.title, data.detail].filter(Boolean).join(" ")
        : null) || `Solicitud fallida (${res.status}).`;
    throw new Error(msg);
  }
  return data;
}

export function fetchPlazos(signal) {
  return fetch("/api/cuotas/plazos", {
    method: "GET",
    headers: jsonHeaders,
    signal,
  }).then((res) => handleResponse(res));
}

export function calcularCuota(payload, signal) {
  return fetch("/api/cuotas/calcular", {
    method: "POST",
    headers: { ...jsonHeaders, "Content-Type": "application/json" },
    body: JSON.stringify(payload),
    signal,
  }).then((res) => handleResponse(res, true));
}
