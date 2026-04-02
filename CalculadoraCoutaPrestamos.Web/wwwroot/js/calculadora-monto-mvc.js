(function () {
  if (typeof IMask === "undefined") return;

  const form = document.querySelector(".calculadora-app form");
  const hidden = document.querySelector('#Monto[type="hidden"]');
  const visible = document.getElementById("MontoMasked");

  if (!form || !hidden || !visible) return;

  const maskOpts = {
    mask: Number,
    scale: 2,
    signed: false,
    thousandsSeparator: ",",
    radix: ".",
    mapToRadix: [","],
    min: 0,
    normalizeZeros: true,
    padFractionalZeros: false,
  };

  const im = IMask(visible, maskOpts);

  function parseHiddenToNumber() {
    let raw = (hidden.value || "").trim();
    if (raw === "") return null;
    if (raw.includes(",") && !/^\d{1,3}(,\d{3})+\./.test(raw)) {
      raw = raw.replace(/\./g, "").replace(",", ".");
    } else {
      raw = raw.replace(/,/g, "");
    }
    const n = Number.parseFloat(raw);
    return Number.isFinite(n) ? n : null;
  }

  function syncHiddenFromMask() {
    const v = im.typedValue;
    if (v === "" || v == null || (typeof v === "number" && Number.isNaN(v))) {
      hidden.value = "";
      return;
    }
    if (typeof v === "number") {
      hidden.value = v.toFixed(2).replace(".", ",");
    }
  }

  let initial = parseHiddenToNumber();
  if (initial === 0) {
    initial = null;
    hidden.value = "";
  }
  if (initial != null && initial > 0) {
    im.typedValue = initial;
    syncHiddenFromMask();
  }

  im.on("accept", syncHiddenFromMask);
  form.addEventListener("submit", function () {
    syncHiddenFromMask();
  });
})();
