(function () {
  var payloadEl = document.getElementById("calc-modal-payload");
  var modalEl = document.getElementById("modalResultadoMvc");
  if (!payloadEl || !modalEl || typeof bootstrap === "undefined") {
    return;
  }

  var payload;
  try {
    payload = JSON.parse(payloadEl.textContent || "{}");
  } catch {
    return;
  }

  if (!payload || !payload.tipo) {
    return;
  }

  function escapeHtml(s) {
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function formatMoney(n) {
    return new Intl.NumberFormat("es-DO", {
      style: "currency",
      currency: "DOP",
    }).format(Number(n));
  }

  var titleEl = modalEl.querySelector(".modal-title");
  var bodyEl = modalEl.querySelector(".modal-body");
  var headerEl = modalEl.querySelector(".modal-header");

  if (titleEl) {
    titleEl.textContent = payload.titulo || "";
  }

  if (payload.tipo === "danger" && bodyEl) {
    if (headerEl) {
      headerEl.className =
        "modal-header border-danger text-danger border-bottom";
    }
    bodyEl.innerHTML =
      '<p class="mb-0">' + escapeHtml(payload.mensaje || "") + "</p>";
  } else if (payload.tipo === "success" && bodyEl) {
    if (headerEl) {
      headerEl.className =
        "modal-header border-success text-success border-bottom";
    }
    var cuotaTxt = formatMoney(payload.cuota);
    var tasaTxt =
      payload.tasaAplicada != null
        ? "<p class=\"mb-2 small\"><strong>Factor de tasa aplicado (por edad):</strong> " +
          escapeHtml(String(payload.tasaAplicada)) +
          "</p>"
        : "";
    var nota =
      '<p class="small text-muted mb-2">La cuota es fija. En cada mes se reparte el capital al préstamo; el interés del periodo es el remanente de esa cuota respecto al capital abonado.</p>';
    var tablaHtml = "";
    if (Array.isArray(payload.tabla) && payload.tabla.length > 0) {
      var rows = payload.tabla
        .map(function (f) {
          return (
            "<tr><td>" +
            escapeHtml(String(f.numeroCuota)) +
            "</td><td class=\"text-end\">" +
            formatMoney(f.saldoInicial) +
            "</td><td class=\"text-end\">" +
            formatMoney(f.cuota) +
            "</td><td class=\"text-end\">" +
            formatMoney(f.capital) +
            "</td><td class=\"text-end\">" +
            formatMoney(f.interes) +
            "</td><td class=\"text-end\">" +
            formatMoney(f.saldoFinal) +
            "</td></tr>"
          );
        })
        .join("");
      tablaHtml =
        '<div class="table-responsive mt-2" style="max-height: 280px; overflow-y: auto">' +
        '<table class="table table-sm table-striped table-bordered align-middle mb-0">' +
        "<thead class=\"table-light\"><tr>" +
        "<th>#</th><th class=\"text-end\">Saldo inicial</th><th class=\"text-end\">Cuota</th>" +
        "<th class=\"text-end\">Capital</th><th class=\"text-end\">Interés</th><th class=\"text-end\">Saldo final</th>" +
        "</tr></thead><tbody>" +
        rows +
        "</tbody></table></div>" +
        '<p class="small text-muted mt-2 mb-0"><strong>Tabla de amortización</strong> (referencia según la cuota calculada).</p>';
    }
    bodyEl.innerHTML =
      '<p class="mb-2"><strong>Cuota mensual:</strong> ' +
      cuotaTxt +
      "</p>" +
      tasaTxt +
      nota +
      '<p class="mb-0 text-muted small">' +
      escapeHtml(payload.aviso || "") +
      "</p>" +
      tablaHtml;
  }

  var instance = bootstrap.Modal.getOrCreateInstance(modalEl);
  instance.show();
})();
