const API_URL = window.localStorage.getItem("liquidacion_api") || "http://localhost:5188";
const TOKEN_KEY = "liquidacion_token";
const payrollList = document.getElementById("payroll-list");
const detail = document.getElementById("payroll-detail");
const refreshBtn = document.getElementById("btn-refresh");
const createForm = document.getElementById("form-create");
const tokenInput = document.getElementById("input-token");
const saveTokenBtn = document.getElementById("btn-save-token");

const state = {
  payrolls: [],
  selected: null,
  token: window.localStorage.getItem(TOKEN_KEY) || "",
};

if (tokenInput) tokenInput.value = state.token;
saveTokenBtn?.addEventListener("click", () => {
  state.token = tokenInput?.value?.trim() || "";
  window.localStorage.setItem(TOKEN_KEY, state.token);
  alert("Token actualizado");
});

async function fetchJSON(path, options) {
  const headers = new Headers(options?.headers || {});
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${API_URL}${path}`, { ...options, headers });
  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || response.statusText);
  }
  return response.status === 204 ? null : response.json();
}

async function loadPayrolls() {
  const data = await fetchJSON("/payrolls");
  state.payrolls = data;
  renderPayrollList();
  if (state.selected) {
    const exists = state.payrolls.find((p) => p.id === state.selected.id);
    if (!exists) state.selected = null;
  }
}

function renderPayrollList() {
  payrollList.innerHTML = "";
  if (!state.payrolls.length) {
    const empty = document.createElement("li");
    empty.className = "payroll-item";
    empty.textContent = "Sin liquidaciones";
    payrollList.appendChild(empty);
    return;
  }
  state.payrolls.forEach((p) => {
    const li = document.createElement("li");
    li.className = `payroll-item ${state.selected?.id === p.id ? "active" : ""}`;
    li.innerHTML = `<strong>${p.periodo}</strong><br/><small>${p.tipo} · ${p.estado}</small>`;
    li.addEventListener("click", () => selectPayroll(p.id));
    payrollList.appendChild(li);
  });
}

async function selectPayroll(id) {
  state.selected = await fetchJSON(`/payrolls/${id}`);
  renderDetail();
}

function renderDetail() {
  if (!state.selected) {
    detail.className = "card empty";
    detail.innerHTML = `<p>Seleccioná una liquidación para ver el detalle.</p>`;
    return;
  }
  detail.className = "card";
  const p = state.selected;
  detail.innerHTML = `
    <div class="header">
      <h2>${p.periodo} · ${p.tipo}</h2>
      <span class="pill status">${p.estado}</span>
    </div>
    <p>${p.descripcion || "Sin descripción"}</p>
    <div class="detail-grid">
      <div>
        <h3>Legajos (${p.legajos.length})</h3>
        ${renderLegajosTable(p)}
        ${renderLegajoForm()}
      </div>
      <div>
        <h3>Acciones</h3>
        <div class="actions">
          <button id="btn-process">Procesar recibos</button>
          <button id="btn-export">Procesar + exportar</button>
        </div>
        <div class="divider"></div>
        <h3>Recibos (${p.recibos.length})</h3>
        ${renderRecibosTable(p)}
      </div>
    </div>
  `;

  const form = detail.querySelector("#form-legajo");
  form?.addEventListener("submit", async (evt) => {
    evt.preventDefault();
    const fd = new FormData(form);
    const payload = Object.fromEntries(fd.entries());
    try {
      await fetchJSON(`/payrolls/${p.id}/legajos`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          numero: payload.numero,
          nombre: payload.nombre,
          cuil: payload.cuil,
          basico: Number(payload.basico ?? 0),
          antiguedad: Number(payload.antiguedad ?? 0),
          adicionales: Number(payload.adicionales ?? 0),
          descuentos: Number(payload.descuentos ?? 0),
        }),
      });
      form.reset();
      await refreshSelected();
    } catch (err) {
      alert(`Error agregando legajo: ${err.message}`);
    }
  });

  detail.querySelectorAll(".btn-remove-legajo").forEach((btn) => {
    btn.addEventListener("click", async () => {
      const legajoId = btn.dataset.legajo;
      try {
        await fetchJSON(`/payrolls/${p.id}/legajos/${legajoId}`, { method: "DELETE" });
        await refreshSelected();
      } catch (err) {
        alert(`No se pudo eliminar: ${err.message}`);
      }
    });
  });

  detail.querySelector("#btn-process")?.addEventListener("click", () => triggerProcess(false));
  detail.querySelector("#btn-export")?.addEventListener("click", () => triggerProcess(true));

  async function triggerProcess(exportar) {
    try {
      await fetchJSON(`/payrolls/${p.id}/procesar`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ exportar }),
      });
      await refreshSelected();
    } catch (err) {
      alert(`Error procesando: ${err.message}`);
    }
  }
}

function renderLegajosTable(payroll) {
  if (!payroll.legajos.length) {
    return `<p class="muted">Sin legajos cargados.</p>`;
  }
  const rows = payroll.legajos
    .map(
      (l) => `<tr>
        <td>${l.numero}</td>
        <td>${l.nombre}</td>
        <td>$${l.basico.toLocaleString()}</td>
        <td><button class="btn-remove-legajo" data-legajo="${l.id}">Quitar</button></td>
      </tr>`
    )
    .join("");
  return `<table class="table">
    <thead><tr><th>Número</th><th>Nombre</th><th>Básico</th><th></th></tr></thead>
    <tbody>${rows}</tbody>
  </table>`;
}

function renderRecibosTable(payroll) {
  if (!payroll.recibos.length) {
    return `<p class="muted">Aún no se generaron recibos.</p>`;
  }
  const rows = payroll.recibos
    .map(
      (r) => `<tr>
        <td>${r.legajoNumero}</td>
        <td>${r.legajoNombre}</td>
        <td>$${r.remunerativo.toLocaleString()}</td>
        <td>$${r.neto.toLocaleString()}</td>
      </tr>`
    )
    .join("");
  return `<table class="table">
    <thead><tr><th>Legajo</th><th>Nombre</th><th>Remunerativo</th><th>Neto</th></tr></thead>
    <tbody>${rows}</tbody>
  </table>`;
}

function renderLegajoForm() {
  return `
    <form id="form-legajo" class="form-inline">
      <label>Número<input name="numero" required /></label>
      <label>Nombre<input name="nombre" required /></label>
      <label>CUIL<input name="cuil" required /></label>
      <label>Básico<input name="basico" type="number" step="0.01" required /></label>
      <label>Antigüedad<input name="antiguedad" type="number" step="0.01" /></label>
      <label>Adicionales<input name="adicionales" type="number" step="0.01" /></label>
      <label>Descuentos<input name="descuentos" type="number" step="0.01" /></label>
      <button type="submit">Agregar legajo</button>
    </form>
  `;
}

async function refreshSelected() {
  if (!state.selected) return;
  state.selected = await fetchJSON(`/payrolls/${state.selected.id}`);
  await loadPayrolls();
  renderDetail();
}

createForm.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(createForm);
  const payload = Object.fromEntries(fd.entries());
  try {
    const created = await fetchJSON("/payrolls", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(payload),
    });
    createForm.reset();
    await loadPayrolls();
    await selectPayroll(created.id);
  } catch (err) {
    alert(`Error creando liquidación: ${err.message}`);
  }
});

refreshBtn.addEventListener("click", loadPayrolls);

loadPayrolls()
  .then(renderDetail)
  .catch((err) => alert(`No se pudieron cargar las liquidaciones: ${err.message}`));
