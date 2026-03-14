const KEYS = {
  auth: "rh_auth_url",
  liq: "rh_liq_url",
  org: "rh_org_url",
  personal: "rh_personal_url",
  hub: "rh_hub_url",
  wf: "rh_wf_url",
  bff: "rh_bff_url",
  config: "rh_config_url",
  tiempos: "rh_tiempos_url",
  token: "rh_token",
};

const el = (id) => document.getElementById(id);
const viewButtons = document.querySelectorAll(".nav-btn");
const views = document.querySelectorAll("main[data-view]");

const storedToken = localStorage.getItem(KEYS.token) || "";
const state = {
  authUrl: localStorage.getItem(KEYS.auth) || "http://localhost:5001",
  liqUrl: localStorage.getItem(KEYS.liq) || "http://localhost:5188",
  orgUrl: localStorage.getItem(KEYS.org) || "http://localhost:5100",
  personalUrl: localStorage.getItem(KEYS.personal) || "http://localhost:5200",
  hubUrl: localStorage.getItem(KEYS.hub) || "http://localhost:5050",
  wfUrl: localStorage.getItem(KEYS.wf) || "http://localhost:5051",
  bffUrl: localStorage.getItem(KEYS.bff) || "http://localhost:5090",
  configUrl: localStorage.getItem(KEYS.config) || "http://localhost:5300",
  tiemposUrl: localStorage.getItem(KEYS.tiempos) || "http://localhost:5400",
  token: normalizeToken(storedToken),
  empresas: [],
  unidades: [],
  posiciones: [],
  legajos: [],
  payrolls: [],
  templates: [],
  wfInstances: [],
  triggers: [],
  sindicatos: [],
  convenios: [],
  catalogos: [],
  parametros: [],
  turnos: [],
  horarios: [],
  fichadas: [],
  planillas: [],
};

const params = new URLSearchParams(window.location.search);
const tokenParam = params.get("token");
if (tokenParam) {
  state.token = normalizeToken(tokenParam);
  localStorage.setItem(KEYS.token, state.token);
} else if (storedToken && state.token !== storedToken) {
  localStorage.setItem(KEYS.token, state.token);
}

el("auth-url").value = state.authUrl;
el("liq-url").value = state.liqUrl;
el("org-url").value = state.orgUrl;
el("personal-url").value = state.personalUrl;
el("hub-url").value = state.hubUrl;
el("wf-url").value = state.wfUrl;
el("bff-url").value = state.bffUrl;
el("config-url").value = state.configUrl;
el("tiempos-url").value = state.tiemposUrl;
el("token").value = state.token;

function getOrgBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/organizacion`;
  return state.orgUrl.replace(/\/$/, "");
}

function getPersonalBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/personal`;
  return state.personalUrl.replace(/\/$/, "");
}

function getLiqBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/liquidacion`;
  return state.liqUrl.replace(/\/$/, "");
}

function getHubBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/integraciones`;
  return `${state.hubUrl.replace(/\/$/, "")}/integraciones`;
}

function getWfBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/wf`;
  return state.wfUrl.replace(/\/$/, "");
}

function getMedicinaBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/medicina`;
  return `${state.wfUrl.replace(/\/$/, "")}/medicina`;
}

function getConfigBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/configuracion`;
  return state.configUrl.replace(/\/$/, "");
}

function getTiemposBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/rh/v1/tiempos`;
  return state.tiemposUrl.replace(/\/$/, "");
}

function setStatus() {
  el("status-pill").textContent = state.token ? "Autenticado" : "Sin sesión";
}

function normalizeToken(value) {
  return value.replace(/^Bearer\s+/i, "").trim();
}

setStatus();

async function buildErrorMessage(response) {
  const statusLabel = `${response.status}${response.statusText ? ` ${response.statusText}` : ""}`.trim();
  let detail = "";
  try {
    const text = await response.text();
    if (text) {
      try {
        const data = JSON.parse(text);
        detail = data?.message || data?.error || text;
      } catch {
        detail = text;
      }
    }
  } catch {
    detail = "";
  }
  return detail ? `${statusLabel} · ${detail}` : statusLabel;
}

async function fetchJSON(base, path) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, { headers });
  if (!response.ok) throw new Error(await buildErrorMessage(response));
  return response.json();
}

async function postJSON(base, path, payload) {
  const headers = new Headers({ "Content-Type": "application/json" });
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, {
    method: "POST",
    headers,
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(await buildErrorMessage(response));
  }
  return response.json();
}

async function putJSON(base, path, payload) {
  const headers = new Headers({ "Content-Type": "application/json" });
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, {
    method: "PUT",
    headers,
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(await buildErrorMessage(response));
  }
  return response.json();
}

async function deleteJSON(base, path) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, { method: "DELETE", headers });
  if (!response.ok && response.status !== 204) {
    throw new Error(await buildErrorMessage(response));
  }
}

async function patchJSON(base, path, payload) {
  const headers = new Headers({ "Content-Type": "application/json" });
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, {
    method: "PATCH",
    headers,
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    throw new Error(await buildErrorMessage(response));
  }
  return response.json();
}

function getHealthTargets() {
  const targets = [
    { label: "Auth", url: `${state.authUrl.replace(/\/$/, "")}/health` },
    { label: "Organización", url: `${state.orgUrl.replace(/\/$/, "")}/health` },
    { label: "Personal", url: `${state.personalUrl.replace(/\/$/, "")}/health` },
    { label: "Liquidación", url: `${state.liqUrl.replace(/\/$/, "")}/health` },
    { label: "Integración", url: `${state.hubUrl.replace(/\/$/, "")}/health` },
    { label: "Workflow", url: `${state.wfUrl.replace(/\/$/, "")}/health` },
    { label: "Configuración", url: `${state.configUrl.replace(/\/$/, "")}/health` },
    { label: "Tiempos", url: `${state.tiemposUrl.replace(/\/$/, "")}/health` },
  ];
  if (state.bffUrl) {
    targets.push({ label: "Portal BFF", url: `${state.bffUrl.replace(/\/$/, "")}/health` });
  }
  return targets;
}

async function refreshHealthStatus() {
  const list = el("list-health");
  if (!list) return;
  const targets = getHealthTargets();
  const results = await Promise.all(
    targets.map(async (target) => {
      try {
        const resp = await fetch(target.url);
        return { ...target, ok: resp.ok, status: resp.status };
      } catch {
        return { ...target, ok: false, status: null };
      }
    })
  );
  list.innerHTML = results
    .map((item) => {
      const dotClass = item.ok ? "ok" : item.status ? "warn" : "fail";
      const statusLabel = item.ok ? "OK" : item.status ? `Error ${item.status}` : "Sin respuesta";
      return `
        <li class="status-item">
          <span>${item.label}</span>
          <span class="status-dot ${dotClass}"></span>
          <span>${statusLabel}</span>
        </li>
      `;
    })
    .join("");
}

function renderList(target, items, formatter) {
  const list = el(target);
  list.innerHTML = "";
  if (!items.length) {
    const li = document.createElement("li");
    li.textContent = "Sin datos";
    list.appendChild(li);
    return;
  }
  items.slice(0, 5).forEach((item) => {
    const li = document.createElement("li");
    li.innerHTML = formatter(item);
    list.appendChild(li);
  });
}

function showFormError(formId, message) {
  const form = el(formId);
  if (!form) return;
  let err = form.querySelector(".error-text");
  if (!err) {
    err = document.createElement("div");
    err.className = "error-text";
    form.appendChild(err);
  }
  err.textContent = message;
}

function clearFormError(formId) {
  const form = el(formId);
  const err = form?.querySelector(".error-text");
  if (err) err.textContent = "";
}

function fillSelect(selectId, items, getLabel) {
  const select = el(selectId);
  if (!select) return;
  select.innerHTML = "";
  if (!items.length) {
    const option = document.createElement("option");
    option.value = "";
    option.textContent = "Sin datos";
    select.appendChild(option);
    return;
  }
  items.forEach((item) => {
    const option = document.createElement("option");
    option.value = item.id;
    option.textContent = getLabel(item);
    select.appendChild(option);
  });
}

function getPayrollById(id) {
  return state.payrolls.find((p) => p.id === id);
}

function getSindicatoById(id) {
  return state.sindicatos.find((s) => s.id === id);
}

function normalizeDate(value) {
  return value ? value : null;
}

function normalizeBool(value) {
  if (value === undefined || value === null || value === "") return true;
  if (typeof value === "boolean") return value;
  return String(value).toLowerCase() !== "false";
}

function normalizeTime(value) {
  if (!value) return value;
  const text = String(value).trim();
  if (text.length === 5 && text.includes(":")) return `${text}:00`;
  return text;
}

function buildExportLinks(payroll) {
  if (!payroll) return [];
  const base = getLiqBase();
  const shortId = payroll.id.replace(/-/g, "").substring(0, 8);
  const fileBase = `${payroll.periodo}-${shortId}`;
  return [
    `${base}/exports/${fileBase}.json`,
    `${base}/exports/${fileBase}.csv`,
  ];
}

function buildExportLinksFromResponse(exports) {
  if (!exports?.length) return [];
  const base = getLiqBase();
  return exports.map((item) => {
    if (item.url) return `${base}${item.url}`;
    if (item.fileName) return `${base}/exports/${item.fileName}`;
    return "";
  }).filter(Boolean);
}

async function fetchExports(payrollId) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${getLiqBase()}/payrolls/${payrollId}/exports`, { headers });

  if (response.status === 401 || response.status === 403) {
    return { status: "unauthorized", exports: [] };
  }

  if (response.status === 404) {
    return { status: "not_found", exports: [] };
  }

  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  return { status: "ok", exports: await response.json() };
}

async function getExportInfo(payrollId) {
  const payroll = getPayrollById(payrollId);
  try {
    const result = await fetchExports(payrollId);
    if (result.status === "ok") {
      return { status: "ok", links: buildExportLinksFromResponse(result.exports) };
    }
    if (result.status === "unauthorized") {
      return { status: "unauthorized", links: [] };
    }
    if (result.status === "not_found") {
      if (!payroll) return { status: "not_found", links: [] };
      return { status: "fallback", links: buildExportLinks(payroll) };
    }
  } catch (err) {
    if (!payroll) return { status: "error", links: [] };
  }

  return { status: "fallback", links: buildExportLinks(payroll) };
}

function getFilenameFromResponse(response, url) {
  const disposition = response.headers.get("content-disposition");
  if (disposition) {
    const match = /filename="?([^";]+)"?/i.exec(disposition);
    if (match?.[1]) return match[1];
  }

  const fallback = url.split("/").pop();
  return fallback || "export";
}

async function downloadWithAuth(url) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(url, { headers });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }

  const blob = await response.blob();
  const fileName = getFilenameFromResponse(response, url);
  const objectUrl = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = objectUrl;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(objectUrl);
}

function attachExportLinkHandlers(root) {
  if (!root) return;
  root.querySelectorAll(".export-link").forEach((link) => {
    link.addEventListener("click", async (evt) => {
      evt.preventDefault();
      try {
        await downloadWithAuth(link.dataset.url || link.href);
      } catch (err) {
        alert(`Error descargando exporte: ${err.message}`);
      }
    });
  });
}

function renderExportLinks(targetId, links, status) {
  const list = el(targetId);
  if (!list) return;
  if (!links.length) {
    const message = status === "unauthorized" ? "Sin acceso" : "Sin exportes";
    list.innerHTML = `<li>${message}</li>`;
    return;
  }
  list.innerHTML = links
    .map((link) => `<li><a href="${link}" class="export-link" data-url="${link}">${link}</a></li>`)
    .join("");
  attachExportLinkHandlers(list);
}

function setView(view) {
  views.forEach((section) => {
    section.classList.toggle("hidden", section.dataset.view !== view);
  });
  viewButtons.forEach((btn) => {
    btn.classList.toggle("active", btn.dataset.view === view);
  });
}

viewButtons.forEach((btn) => {
  btn.addEventListener("click", () => setView(btn.dataset.view));
});

async function fetchAusenciasResumen() {
  const form = el("form-ausencias-resumen");
  const params = new URLSearchParams();
  if (form) {
    const fd = new FormData(form);
    const desde = fd.get("desde");
    const hasta = fd.get("hasta");
    if (desde) params.set("desde", desde);
    if (hasta) params.set("hasta", hasta);
  }
  const query = params.toString();
  const path = query ? `/ausencias/resumen?${query}` : "/ausencias/resumen";
  return fetchJSON(getTiemposBase(), path).catch(() => null);
}

async function refreshAll() {
  setStatus();
  const [empresas, unidades, legajos, posiciones, sindicatos, convenios, payrolls, jobs, templates, instances, parametros, turnos, horarios, fichadas, planillas, ausenciasResumen] = await Promise.all([
    fetchJSON(getOrgBase(), "/empresas").catch(() => []),
    fetchJSON(getOrgBase(), "/unidades").catch(() => []),
    fetchJSON(getPersonalBase(), "/legajos").catch(() => []),
    fetchJSON(getOrgBase(), "/posiciones").catch(() => []),
    fetchJSON(getOrgBase(), "/sindicatos").catch(() => []),
    fetchJSON(getOrgBase(), "/convenios").catch(() => []),
    fetchJSON(getLiqBase(), "/payrolls").catch(() => []),
    fetchJSON(getHubBase(), "/jobs").catch(() => []),
    fetchJSON(getHubBase(), "/templates").catch(() => []),
    fetchJSON(getWfBase(), "/instances").catch(() => []),
    fetchJSON(getConfigBase(), "/parametros").catch(() => []),
    fetchJSON(getTiemposBase(), "/turnos").catch(() => []),
    fetchJSON(getTiemposBase(), "/horarios").catch(() => []),
    fetchJSON(getTiemposBase(), "/fichadas").catch(() => []),
    fetchJSON(getTiemposBase(), "/planillas").catch(() => []),
    fetchAusenciasResumen(),
  ]);

  const triggers = await fetchJSON(getHubBase(), "/triggers").catch(() => []);

  state.empresas = empresas;
  state.unidades = unidades;
  state.posiciones = posiciones;
  state.legajos = legajos;
  state.payrolls = payrolls;
  state.templates = templates;
  state.wfInstances = instances;
  state.triggers = triggers;
  state.sindicatos = sindicatos;
  state.convenios = convenios;
  state.parametros = parametros;
  state.turnos = turnos;
  state.horarios = horarios;
  state.fichadas = fichadas;
  state.planillas = planillas;

  el("stat-empresas").textContent = empresas.length || "0";
  el("stat-unidades").textContent = unidades.length || "0";
  el("stat-legajos").textContent = legajos.length || "0";
  el("stat-payrolls").textContent = payrolls.length || "0";
  el("stat-jobs").textContent = jobs.length || "0";

  el("badge-org").textContent = empresas.length ? "Con datos" : "Sin datos";
  el("badge-personal").textContent = legajos.length ? "Con datos" : "Sin datos";
  el("badge-liq").textContent = payrolls.length ? "Con datos" : "Sin datos";
  el("badge-int").textContent = jobs.length ? "Con datos" : "Sin datos";

  renderList("list-payrolls", payrolls, (p) => `<strong>${p.periodo}</strong> · ${p.tipo} · ${p.estado}`);
  renderList("list-legajos", legajos, (l) => `<strong>${l.numero}</strong> · ${l.nombre}`);
  renderList("list-jobs", jobs, (j) => `<strong>${j.estado}</strong> · ${j.trigger} <button class="btn small btn-job-retry" data-id="${j.id}">Reintentar</button>`);
  renderList("list-empresas", empresas, (e) => `<strong>${e.nombre}</strong> · ${e.pais} <button class="btn small btn-delete-empresa" data-id="${e.id}">Eliminar</button>`);
  renderList("list-unidades", unidades, (u) => `<strong>${u.nombre}</strong> · ${u.tipo} <button class="btn small btn-delete-unidad" data-id="${u.id}">Eliminar</button>`);
  renderList("list-legajos-full", legajos, (l) => `<strong>${l.numero}</strong> · ${l.nombre} <button class="btn small btn-delete-legajo" data-id="${l.id}">Eliminar</button>`);
  renderList("list-posiciones", posiciones, (p) => `<strong>${p.nombre}</strong> · ${p.unidadId} <button class="btn small btn-delete-posicion" data-id="${p.id}">Eliminar</button>`);
  renderList("list-sindicatos", sindicatos, (s) => `<strong>${s.nombre}</strong> · ${s.codigo || "Sin código"} · ${s.jurisdiccion || "Sin jurisdicción"}`);
  renderList("list-convenios", convenios, (c) => {
    const sindicato = getSindicatoById(c.sindicatoId);
    const sindicatoLabel = sindicato ? sindicato.nombre : "Sin sindicato";
    const vigenciaDesde = c.vigenciaDesde ? new Date(c.vigenciaDesde).toLocaleDateString("es-AR") : "";
    const vigenciaHasta = c.vigenciaHasta ? new Date(c.vigenciaHasta).toLocaleDateString("es-AR") : "";
    const vigencia = vigenciaDesde || vigenciaHasta ? `${vigenciaDesde || ""}${vigenciaHasta ? ` → ${vigenciaHasta}` : ""}` : "Sin vigencia";
    return `<strong>${c.nombre}</strong> · ${c.numero || "Sin número"} · ${sindicatoLabel} · ${vigencia}`;
  });
  renderList("list-payrolls-full", payrolls, (p) => `<strong>${p.periodo}</strong> · ${p.tipo} · ${p.estado} <button class="btn small btn-process" data-id="${p.id}" data-export="false">Procesar</button> <button class="btn small btn-process" data-id="${p.id}" data-export="true">Exportar</button> <button class="btn small btn-download" data-id="${p.id}">Descargar</button>`);
  renderList("list-jobs-full", jobs, (j) => `<strong>${j.estado}</strong> · ${j.trigger} <button class="btn small btn-job-retry" data-id="${j.id}">Reintentar</button>`);
  renderList("list-triggers", triggers, (t) => `<strong>${t.eventName}</strong> · ${t.enabled ? "Activo" : "Inactivo"} <button class="btn small btn-trigger-edit" data-id="${t.id}">Editar</button>`);
  renderList("list-vacaciones", instances.filter((i) => i.key === "vacaciones"), (i) => `<strong>${i.estado}</strong> · ${i.id} <button class="btn small btn-vac-approve" data-id="${i.id}">Aprobar</button> <button class="btn small btn-vac-reject" data-id="${i.id}">Rechazar</button>`);
  renderList("list-reclamos", instances.filter((i) => i.key === "reclamos"), (i) => `<strong>${i.estado}</strong> · ${i.id} <button class="btn small btn-reclamo-approve" data-id="${i.id}">Aprobar</button> <button class="btn small btn-reclamo-reject" data-id="${i.id}">Rechazar</button>`);
  renderList("list-sanciones", instances.filter((i) => i.key === "sanciones"), (i) => `<strong>${i.estado}</strong> · ${i.id} <button class="btn small btn-sancion-approve" data-id="${i.id}">Resolver</button> <button class="btn small btn-sancion-reject" data-id="${i.id}">Descartar</button>`);
  renderList("list-medicina-examen", instances.filter((i) => i.key === "medicina-examen"), (i) => `<strong>${i.estado}</strong> · ${i.id} <button class="btn small btn-med-approve" data-id="${i.id}">Aprobar</button> <button class="btn small btn-med-reject" data-id="${i.id}">Rechazar</button>`);
  renderList("list-medicina-licencia", instances.filter((i) => i.key === "medicina-licencia"), (i) => `<strong>${i.estado}</strong> · ${i.id} <button class="btn small btn-med-approve" data-id="${i.id}">Aprobar</button> <button class="btn small btn-med-reject" data-id="${i.id}">Rechazar</button>`);
  renderList("list-parametros", parametros, (p) => `<strong>${p.clave}</strong> · ${p.valor}`);
  renderList("list-turnos", turnos, (t) => `<strong>${t.codigo}</strong> · ${t.nombre} · ${t.horaInicio} → ${t.horaFin} <button class="btn small btn-delete-turno" data-id="${t.id}">Eliminar</button>`);
  renderList("list-horarios", horarios, (h) => `<strong>${h.nombre}</strong> · ${h.diasSemana} · ${h.turnoId} <button class="btn small btn-delete-horario" data-id="${h.id}">Eliminar</button>`);
  renderList("list-fichadas", fichadas, (f) => `<strong>${f.tipo}</strong> · ${f.legajoId} · ${new Date(f.fechaHora).toLocaleString("es-AR")} · ${f.origen}`);
  renderList("list-planillas", planillas, (p) => `<strong>${p.periodo}</strong> · ${p.estado} · ${p.totalHoras} hs <button class="btn small btn-cerrar-planilla" data-id="${p.id}">Cerrar</button>`);
  if (ausenciasResumen && Array.isArray(ausenciasResumen.porLegajo)) {
    renderList("list-ausencias-resumen", ausenciasResumen.porLegajo, (item) => `<strong>${item.key}</strong> · ${item.totalDias} días · ${item.totalRegistros} registros`);
  } else {
    renderList("list-ausencias-resumen", [], () => "");
  }

  fillSelect("select-empresa", empresas, (e) => `${e.nombre} (${e.pais})`);
  fillSelect("select-unidad", unidades, (u) => `${u.nombre} (${u.tipo})`);
  fillSelect("select-posicion", posiciones, (p) => `${p.nombre}`);
  fillSelect("select-legajo", legajos, (l) => `${l.numero} · ${l.nombre}`);
  fillSelect("select-payroll", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-empresa-edit", empresas, (e) => `${e.nombre}`);
  fillSelect("select-unidad-edit", unidades, (u) => `${u.nombre}`);
  fillSelect("select-legajo-edit", legajos, (l) => `${l.numero} · ${l.nombre}`);
  fillSelect("select-posicion-edit", posiciones, (p) => `${p.nombre}`);
  fillSelect("select-sindicato-edit", sindicatos, (s) => `${s.nombre}`);
  fillSelect("select-convenio-sindicato", sindicatos, (s) => `${s.nombre}`);
  fillSelect("select-convenio-edit-sindicato", sindicatos, (s) => `${s.nombre}`);
  fillSelect("select-convenio-edit", convenios, (c) => `${c.nombre}`);
  fillSelect("select-payroll-recibos", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-payroll-edit", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-payroll-exportes", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-payroll-recibos", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-payroll-detalle", payrolls, (p) => `${p.periodo} · ${p.tipo}`);
  fillSelect("select-template", templates, (t) => `${t.name}`);
  fillSelect("select-job", jobs, (j) => `${j.estado} · ${j.trigger}`);
  fillSelect("select-job-evento", jobs, (j) => `${j.estado} · ${j.trigger}`);
  fillSelect("select-template-trigger", templates, (t) => `${t.name}`);
  fillSelect("select-trigger", triggers, (t) => `${t.eventName}`);
  fillSelect("select-trigger-edit", triggers, (t) => `${t.eventName}`);
  fillSelect("select-template-trigger-edit", templates, (t) => `${t.name}`);
  fillSelect("select-turno", turnos, (t) => `${t.codigo} · ${t.nombre}`);
  fillSelect("select-turno-edit", turnos, (t) => `${t.codigo} · ${t.nombre}`);
  fillSelect("select-turno-horario-edit", turnos, (t) => `${t.codigo} · ${t.nombre}`);
  fillSelect("select-horario-edit", horarios, (h) => `${h.nombre}`);
  fillSelect("select-legajo-fichada", legajos, (l) => `${l.numero} · ${l.nombre}`);
  fillSelect("select-empresa-planilla", empresas, (e) => `${e.nombre}`);
  fillSelect("select-legajo-planilla", legajos, (l) => `${l.numero} · ${l.nombre}`);

  await refreshHealthStatus();

  document.querySelectorAll(".btn-delete-empresa").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getOrgBase(), `/empresas/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando empresa: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-delete-unidad").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getOrgBase(), `/unidades/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando unidad: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-delete-legajo").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getPersonalBase(), `/legajos/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando legajo: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-delete-posicion").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getOrgBase(), `/posiciones/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando posición: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-delete-turno").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getTiemposBase(), `/turnos/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando turno: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-delete-horario").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await deleteJSON(getTiemposBase(), `/horarios/${btn.dataset.id}`);
        refreshAll();
      } catch (err) {
        alert(`Error eliminando horario: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-cerrar-planilla").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getTiemposBase(), `/planillas/${btn.dataset.id}/cerrar`, {});
        refreshAll();
      } catch (err) {
        alert(`Error cerrando planilla: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-process").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getLiqBase(), `/payrolls/${btn.dataset.id}/procesar`, {
          exportar: btn.dataset.export === "true",
        });
        refreshAll();
      } catch (err) {
        alert(`Error procesando liquidación: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-download").forEach((btn) => {
    btn.addEventListener("click", async () => {
      const info = await getExportInfo(btn.dataset.id);
      if (info.status === "unauthorized") {
        alert("No tenés permisos para descargar exportes.");
        return;
      }
      if (!info.links.length) {
        alert("No hay exportes disponibles.");
        return;
      }
      try {
        for (const link of info.links) {
          await downloadWithAuth(link);
        }
      } catch (err) {
        alert(`Error descargando exportes: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-vac-approve").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "aprobar",
          actor: el("vac-actor").value || "rrhh",
          datos: { comentario: el("vac-comment").value || "" },
        });
        // Notificar al empleado
        const instance = (state.wfInstances || []).find((item) => item.id === btn.dataset.id);
        if (state.bffUrl && instance)
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fechaDesde = instance.datos?.fechaDesde || instance.datos?.fecha || "";
          const fechaHasta = instance.datos?.fechaHasta || instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Vacaciones aprobadas",
            detail: `Legajo ${legajoNumero} · ${fechaDesde} → ${fechaHasta}`,
            sourceId: instance.id
          });
          // Integracion con Tiempos: registrar ausencia
          if (legajoNumero && fechaDesde && fechaHasta)
          {
            await postJSON(getTiemposBase(), "/ausencias", {
              legajoId: null,
              legajoNumero,
              tipo: "Vacaciones",
              fechaDesde,
              fechaHasta,
              origen: "Vacaciones",
              observaciones: "Vacaciones aprobadas"
            });
          }
        }
        refreshAll();
      } catch (err) {
        alert(`Error aprobando vacaciones: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-vac-reject").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "rechazar",
          actor: el("vac-actor").value || "rrhh",
          datos: { comentario: el("vac-comment").value || "" },
        });
        // Notificar al empleado
        const instance = (state.wfInstances || []).find((item) => item.id === btn.dataset.id);
        if (state.bffUrl && instance)
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fechaDesde = instance.datos?.fechaDesde || instance.datos?.fecha || "";
          const fechaHasta = instance.datos?.fechaHasta || instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Vacaciones rechazadas",
            detail: `Legajo ${legajoNumero} · ${fechaDesde} → ${fechaHasta}`,
            sourceId: instance.id
          });
        }
        refreshAll();
      } catch (err) {
        alert(`Error rechazando vacaciones: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-reclamo-approve").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "aprobar",
          actor: el("reclamo-actor").value || "mesa-ayuda",
          datos: { comentario: el("reclamo-comment").value || "" },
        });
        refreshAll();
      } catch (err) {
        alert(`Error aprobando reclamo: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-reclamo-reject").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "rechazar",
          actor: el("reclamo-actor").value || "mesa-ayuda",
          datos: { comentario: el("reclamo-comment").value || "" },
        });
        refreshAll();
      } catch (err) {
        alert(`Error rechazando reclamo: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-sancion-approve").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "aprobar",
          actor: el("sancion-actor").value || "rrhh",
          datos: { comentario: el("sancion-comment").value || "" },
        });
        refreshAll();
      } catch (err) {
        alert(`Error resolviendo sanción: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-sancion-reject").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "rechazar",
          actor: el("sancion-actor").value || "rrhh",
          datos: { comentario: el("sancion-comment").value || "" },
        });
        refreshAll();
      } catch (err) {
        alert(`Error descartando sanción: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-med-approve").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "aprobar",
          actor: el("medicina-actor").value || "medico",
          datos: { comentario: el("medicina-comment").value || "" },
        });
        const instance = (state.wfInstances || []).find((item) => item.id === btn.dataset.id);
        if (instance && instance.key === "medicina-licencia")
        {
          const fechaDesde = instance.datos?.fechaDesde || instance.datos?.fecha || null;
          const fechaHasta = instance.datos?.fechaHasta || instance.datos?.fecha || null;
          const legajoNumero = instance.datos?.legajoNumero || "";
          if (fechaDesde && fechaHasta)
          {
            if (legajoNumero)
            {
              await postJSON(getTiemposBase(), "/ausencias", {
                legajoId: null,
                legajoNumero,
                tipo: instance.datos?.tipo || "Licencia médica",
                fechaDesde,
                fechaHasta,
                origen: "Medicina",
                observaciones: "Licencia médica aprobada"
              });
            }
          }
        }
        if (state.bffUrl && instance && instance.key === "medicina-licencia")
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fechaDesde = instance.datos?.fechaDesde || instance.datos?.fecha || "";
          const fechaHasta = instance.datos?.fechaHasta || instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Licencia médica aprobada",
            detail: `Legajo ${legajoNumero} · ${fechaDesde} → ${fechaHasta}`,
            sourceId: instance.id
          });
        }
        if (state.bffUrl && instance && instance.key === "medicina-examen")
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fecha = instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Examen médico aprobado",
            detail: `Legajo ${legajoNumero} · ${fecha}`,
            sourceId: instance.id
          });
        }
        refreshAll();
      } catch (err) {
        alert(`Error aprobando solicitud médica: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-med-reject").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getWfBase(), `/instances/${btn.dataset.id}/transitions`, {
          evento: "rechazar",
          actor: el("medicina-actor").value || "medico",
          datos: { comentario: el("medicina-comment").value || "" },
        });
        const instance = (state.wfInstances || []).find((item) => item.id === btn.dataset.id);
        if (state.bffUrl && instance && instance.key === "medicina-examen")
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fecha = instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Examen médico rechazado",
            detail: `Legajo ${legajoNumero} · ${fecha}`,
            sourceId: instance.id
          });
        }
        if (state.bffUrl && instance && instance.key === "medicina-licencia")
        {
          const legajoNumero = instance.datos?.legajoNumero || "";
          const fechaDesde = instance.datos?.fechaDesde || instance.datos?.fecha || "";
          const fechaHasta = instance.datos?.fechaHasta || instance.datos?.fecha || "";
          await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
            title: "Licencia médica rechazada",
            detail: `Legajo ${legajoNumero} · ${fechaDesde} → ${fechaHasta}`,
            sourceId: instance.id
          });
        }
        refreshAll();
      } catch (err) {
        alert(`Error rechazando solicitud médica: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-job-retry").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await postJSON(getHubBase(), `/jobs/${btn.dataset.id}/retry`, {
          reason: "Manual",
        });
        refreshAll();
      } catch (err) {
        alert(`Error reintentando job: ${err.message}`);
      }
    });
  });

  document.querySelectorAll(".btn-trigger-edit").forEach((btn) => {
    btn.addEventListener("click", () => {
      const trigger = state.triggers.find((t) => t.id === btn.dataset.id);
      if (!trigger) return;
      el("select-trigger-edit").value = trigger.id;
      el("form-trigger-edit").eventName.value = trigger.eventName;
      el("select-template-trigger-edit").value = trigger.templateId;
      el("form-trigger-edit").enabled.value = trigger.enabled ? "true" : "false";
    });
  });
}

el("btn-save").addEventListener("click", () => {
  state.authUrl = el("auth-url").value.trim();
  state.liqUrl = el("liq-url").value.trim();
  state.orgUrl = el("org-url").value.trim();
  state.personalUrl = el("personal-url").value.trim();
  state.hubUrl = el("hub-url").value.trim();
  state.wfUrl = el("wf-url").value.trim();
  state.bffUrl = el("bff-url").value.trim();
  state.configUrl = el("config-url").value.trim();
  state.tiemposUrl = el("tiempos-url").value.trim();
  state.token = normalizeToken(el("token").value);
  localStorage.setItem(KEYS.auth, state.authUrl);
  localStorage.setItem(KEYS.liq, state.liqUrl);
  localStorage.setItem(KEYS.org, state.orgUrl);
  localStorage.setItem(KEYS.personal, state.personalUrl);
  localStorage.setItem(KEYS.hub, state.hubUrl);
  localStorage.setItem(KEYS.wf, state.wfUrl);
  localStorage.setItem(KEYS.bff, state.bffUrl);
  localStorage.setItem(KEYS.config, state.configUrl);
  localStorage.setItem(KEYS.tiempos, state.tiemposUrl);
  localStorage.setItem(KEYS.token, state.token);
  refreshAll();
});

el("btn-refresh").addEventListener("click", refreshAll);

el("btn-refresh-medicina-stats")?.addEventListener("click", async () => {
  try {
    const stats = await fetchJSON(getMedicinaBase(), "/estadisticas");
    if (stats) {
      el("stat-examenes-aprobados").textContent = stats.examenes?.aprobados ?? 0;
      el("stat-examenes-pendientes").textContent = stats.examenes?.pendientes ?? 0;
      el("stat-licencias-aprobadas").textContent = stats.licencias?.aprobadas ?? 0;
      el("stat-licencias-pendientes").textContent = stats.licencias?.pendientes ?? 0;
    }
    // Cargar alertas
    await loadMedicinaAlertas();
  } catch (err) {
    alert(`Error cargando estadísticas: ${err.message}`);
  }
});

async function loadMedicinaAlertas() {
  const lista = el("list-medicina-alertas");
  const hint = el("medicina-alertas-hint");
  if (!lista) return;
  
  const instances = state.wfInstances || [];
  const medicinaInstances = instances.filter(i => 
    i.key === "medicina-examen" || i.key === "medicina-licencia"
  );
  
  const alertas = [];
  
  // Verificar licencias pendientes hace más de 30 días
  const thirtyDaysAgo = new Date();
  thirtyDaysAgo.setDate(thirtyDaysAgo.getDate() - 30);
  
  medicinaInstances.forEach(inst => {
    const created = new Date(inst.createdAt || inst.created);
    if (inst.estado === "Pendiente" && created < thirtyDaysAgo) {
      alertas.push({
        tipo: "warning",
        mensaje: `Solicitud pendiente hace más de 30 días: ${inst.key} (${inst.id?.slice(0,8)})`
      });
    }
  });
  
  // Verificar licencias extensas (más de 60 días)
  medicinaInstances.forEach(inst => {
    if (inst.key === "medicina-licencia" && inst.estado === "Aprobada") {
      const fechaDesde = inst.datos?.fechaDesde || inst.datos?.fecha;
      const fechaHasta = inst.datos?.fechaHasta;
      if (fechaDesde && fechaHasta) {
        const desde = new Date(fechaDesde);
        const hasta = new Date(fechaHasta);
        const dias = Math.ceil((hasta - desde) / (1000 * 60 * 60 * 24));
        if (dias > 60) {
          alertas.push({
            tipo: "danger",
            mensaje: `Licencia extensa (${dias} días): legajo ${inst.datos?.legajoNumero} (${inst.id?.slice(0,8)})`
          });
        }
      }
    }
  });
  
  // Renderizar alertas
  if (alertas.length === 0) {
    lista.innerHTML = "";
    if (hint) hint.textContent = "Sin alertas.";
  } else {
    if (hint) hint.textContent = "";
    lista.innerHTML = alertas.map(a => 
      `<li class="alerta-${a.tipo}">${a.mensaje}</li>`
    ).join("");
  }
}

el("btn-login-demo").addEventListener("click", async () => {
  const payload = { username: "admin", password: "admin123" };
  const response = await fetch(`${state.authUrl}/login`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(payload),
  });
  if (!response.ok) {
    alert("Login falló");
    return;
  }
  const data = await response.json();
  state.token = normalizeToken(data.token);
  el("token").value = state.token;
  localStorage.setItem(KEYS.token, state.token);
  refreshAll();
});

el("form-empresa")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-empresa");
  try {
    await postJSON(getOrgBase(), "/empresas", {
      nombre: fd.get("nombre"),
      pais: fd.get("pais"),
      moneda: fd.get("moneda"),
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-empresa", `Error creando empresa: ${err.message}`);
  }
});

el("form-empresa-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("empresaId");
  clearFormError("form-empresa-edit");
  if (!id) {
    showFormError("form-empresa-edit", "Seleccioná una empresa");
    return;
  }
  try {
    await putJSON(getOrgBase(), `/empresas/${id}`, {
      nombre: fd.get("nombre"),
      pais: fd.get("pais"),
      moneda: fd.get("moneda"),
      estado: fd.get("estado") || "Activa",
    });
    await refreshAll();
  } catch (err) {
    showFormError("form-empresa-edit", `Error actualizando empresa: ${err.message}`);
  }
});

el("form-unidad")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-unidad");
  try {
    await postJSON(getOrgBase(), "/unidades", {
      empresaId: fd.get("empresaId"),
      nombre: fd.get("nombre"),
      tipo: fd.get("tipo"),
      padreId: fd.get("padreId") || null,
      centroCostoId: null,
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-unidad", `Error creando unidad: ${err.message}`);
  }
});

el("form-unidad-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("unidadId");
  clearFormError("form-unidad-edit");
  if (!id) {
    showFormError("form-unidad-edit", "Seleccioná una unidad");
    return;
  }
  try {
    await putJSON(getOrgBase(), `/unidades/${id}`, {
      nombre: fd.get("nombre"),
      tipo: fd.get("tipo"),
      padreId: fd.get("padreId") || null,
      centroCostoId: null,
      estado: fd.get("estado") || "Activa",
    });
    await refreshAll();
  } catch (err) {
    showFormError("form-unidad-edit", `Error actualizando unidad: ${err.message}`);
  }
});

el("form-sindicato")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-sindicato");
  try {
    await postJSON(getOrgBase(), "/sindicatos", {
      nombre: fd.get("nombre"),
      codigo: fd.get("codigo") || null,
      jurisdiccion: fd.get("jurisdiccion") || null,
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-sindicato", `Error creando sindicato: ${err.message}`);
  }
});

el("form-sindicato-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("sindicatoId");
  clearFormError("form-sindicato-edit");
  if (!id) {
    showFormError("form-sindicato-edit", "Seleccioná un sindicato");
    return;
  }
  try {
    await putJSON(getOrgBase(), `/sindicatos/${id}`,
      {
        nombre: fd.get("nombre"),
        codigo: fd.get("codigo") || null,
        jurisdiccion: fd.get("jurisdiccion") || null,
        estado: fd.get("estado") || "Activo",
      });
    await refreshAll();
  } catch (err) {
    showFormError("form-sindicato-edit", `Error actualizando sindicato: ${err.message}`);
  }
});

el("form-convenio")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-convenio");
  const sindicatoId = fd.get("sindicatoId");
  if (!sindicatoId) {
    showFormError("form-convenio", "Seleccioná un sindicato");
    return;
  }
  try {
    await postJSON(getOrgBase(), "/convenios", {
      sindicatoId,
      nombre: fd.get("nombre"),
      numero: fd.get("numero") || null,
      vigenciaDesde: normalizeDate(fd.get("vigenciaDesde")),
      vigenciaHasta: normalizeDate(fd.get("vigenciaHasta")),
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-convenio", `Error creando convenio: ${err.message}`);
  }
});

el("form-convenio-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("convenioId");
  clearFormError("form-convenio-edit");
  if (!id) {
    showFormError("form-convenio-edit", "Seleccioná un convenio");
    return;
  }
  const sindicatoId = fd.get("sindicatoId");
  if (!sindicatoId) {
    showFormError("form-convenio-edit", "Seleccioná un sindicato");
    return;
  }
  try {
    await putJSON(getOrgBase(), `/convenios/${id}`,
      {
        sindicatoId,
        nombre: fd.get("nombre"),
        numero: fd.get("numero") || null,
        vigenciaDesde: normalizeDate(fd.get("vigenciaDesde")),
        vigenciaHasta: normalizeDate(fd.get("vigenciaHasta")),
        estado: fd.get("estado") || "Activo",
      });
    await refreshAll();
  } catch (err) {
    showFormError("form-convenio-edit", `Error actualizando convenio: ${err.message}`);
  }
});

el("form-legajo")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-legajo");
  try {
    await postJSON(getPersonalBase(), "/legajos", {
      numero: fd.get("numero"),
      nombre: fd.get("nombre"),
      apellido: fd.get("apellido"),
      documento: fd.get("documento"),
      cuil: fd.get("cuil"),
      email: fd.get("email"),
      estado: "Activo",
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-legajo", `Error creando legajo: ${err.message}`);
  }
});

el("form-legajo-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("legajoId");
  clearFormError("form-legajo-edit");
  if (!id) {
    showFormError("form-legajo-edit", "Seleccioná un legajo");
    return;
  }
  try {
    await putJSON(getPersonalBase(), `/legajos/${id}`, {
      numero: fd.get("numero"),
      nombre: fd.get("nombre"),
      apellido: fd.get("apellido"),
      documento: fd.get("documento"),
      cuil: fd.get("cuil"),
      email: fd.get("email"),
      estado: fd.get("estado") || "Activo",
    });
    await refreshAll();
  } catch (err) {
    showFormError("form-legajo-edit", `Error actualizando legajo: ${err.message}`);
  }
});

el("form-posicion")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-posicion");
  try {
    await postJSON(getOrgBase(), "/posiciones", {
      unidadId: fd.get("unidadId"),
      nombre: fd.get("nombre"),
      nivel: fd.get("nivel"),
      perfil: fd.get("perfil"),
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-posicion", `Error creando posición: ${err.message}`);
  }
});

el("form-posicion-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("posicionId");
  clearFormError("form-posicion-edit");
  if (!id) {
    showFormError("form-posicion-edit", "Seleccioná una posición");
    return;
  }
  try {
    await putJSON(getOrgBase(), `/posiciones/${id}`, {
      nombre: fd.get("nombre"),
      nivel: fd.get("nivel"),
      perfil: fd.get("perfil"),
      estado: fd.get("estado") || "Activa",
    });
    await refreshAll();
  } catch (err) {
    showFormError("form-posicion-edit", `Error actualizando posición: ${err.message}`);
  }
});

el("form-payroll")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-payroll");
  try {
    await postJSON(getLiqBase(), "/payrolls", {
      periodo: fd.get("periodo"),
      tipo: fd.get("tipo"),
      descripcion: fd.get("descripcion"),
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-payroll", `Error creando liquidación: ${err.message}`);
  }
});

el("form-payroll-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const id = fd.get("payrollId");
  clearFormError("form-payroll-edit");
  if (!id) {
    showFormError("form-payroll-edit", "Seleccioná una liquidación");
    return;
  }
  try {
    await patchJSON(getLiqBase(), `/payrolls/${id}`, {
      periodo: fd.get("periodo"),
      tipo: fd.get("tipo"),
      descripcion: fd.get("descripcion"),
    });
    await refreshAll();
  } catch (err) {
    showFormError("form-payroll-edit", `Error actualizando liquidación: ${err.message}`);
  }
});

el("form-legajo-payroll")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const payrollId = fd.get("payrollId");
  clearFormError("form-legajo-payroll");
  if (!payrollId) {
    showFormError("form-legajo-payroll", "Seleccioná una liquidación");
    return;
  }
  try {
    await postJSON(getLiqBase(), `/payrolls/${payrollId}/legajos`, {
      numero: fd.get("numero"),
      nombre: fd.get("nombre"),
      cuil: fd.get("cuil"),
      basico: Number(fd.get("basico") || 0),
      antiguedad: Number(fd.get("antiguedad") || 0),
      adicionales: Number(fd.get("adicionales") || 0),
      descuentos: Number(fd.get("descuentos") || 0),
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-legajo-payroll", `Error agregando legajo: ${err.message}`);
  }
});

el("form-recibos")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const payrollId = fd.get("payrollId");
  clearFormError("form-recibos");
  if (!payrollId) {
    showFormError("form-recibos", "Seleccioná una liquidación");
    return;
  }
  try {
    const recibos = await fetchJSON(getLiqBase(), `/payrolls/${payrollId}/recibos`);
    renderList("list-recibos", recibos, (r) => `<strong>${r.legajoNumero}</strong> · ${r.legajoNombre} · ${r.neto}`);
  } catch (err) {
    showFormError("form-recibos", `Error cargando recibos: ${err.message}`);
  }
});

el("form-exportes")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const payrollId = fd.get("payrollId");
  clearFormError("form-exportes");
  if (!payrollId) {
    showFormError("form-exportes", "Seleccioná una liquidación");
    return;
  }
  try {
    await postJSON(getLiqBase(), `/payrolls/${payrollId}/procesar`, { exportar: true });
    const info = await getExportInfo(payrollId);
    renderExportLinks("list-exportes", info.links, info.status);
  } catch (err) {
    showFormError("form-exportes", `Error exportando: ${err.message}`);
  }
});

el("form-detalle")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const payrollId = fd.get("payrollId");
  if (!payrollId) {
    alert("Seleccioná una liquidación");
    return;
  }
  try {
    const recibos = await fetchJSON(getLiqBase(), `/payrolls/${payrollId}/recibos`);
    const payroll = getPayrollById(payrollId);
    const info = await getExportInfo(payrollId);
    const exportsMarkup = info.links.length
      ? info.links.map((link) => `<li><a href="${link}" class="export-link" data-url="${link}">${link}</a></li>`).join("")
      : `<li>${info.status === "unauthorized" ? "Sin acceso" : "Sin exportes"}</li>`;
    el("detalle-payroll").innerHTML = `
      <p><strong>${payroll?.periodo || ""}</strong> · ${payroll?.tipo || ""}</p>
      <p>Estado: ${payroll?.estado || ""}</p>
      <p>Exportes:</p>
      <ul class="list">${exportsMarkup}</ul>
      <p>Recibos:</p>
      <ul class="list">${recibos.map((r) => `<li>${r.legajoNumero} · ${r.legajoNombre} · ${r.neto}</li>`).join("")}</ul>
    `;
    attachExportLinkHandlers(el("detalle-payroll"));
  } catch (err) {
    alert(`Error cargando detalle: ${err.message}`);
  }
});

el("form-tiempo-export")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-tiempo-export");
  const templateId = fd.get("templateId");
  if (!templateId) {
    showFormError("form-tiempo-export", "Seleccioná un template");
    return;
  }
  try {
    await postJSON(getHubBase(), "/jobs", {
      templateId,
      periodo: fd.get("periodo") || null,
      trigger: fd.get("trigger"),
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-tiempo-export", `Error iniciando job: ${err.message}`);
  }
});

el("form-job-detalle")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const jobId = fd.get("jobId");
  if (!jobId) {
    showFormError("form-job-detalle", "Seleccioná un job");
    return;
  }
  clearFormError("form-job-detalle");
  try {
    const job = await fetchJSON(getHubBase(), `/jobs/${jobId}`);
    el("detalle-job").innerHTML = `
      <p><strong>${job.estado}</strong> · ${job.trigger}</p>
      <p>Periodo: ${job.periodo || ""}</p>
      <p>Archivo: ${job.archivoGenerado || ""}</p>
      <p>Reintentos: ${job.retryCount || 0}</p>
      <p>Error: ${job.error || ""}</p>
    `;
  } catch (err) {
    showFormError("form-job-detalle", `Error cargando job: ${err.message}`);
  }
});

el("form-eventos")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const jobId = fd.get("jobId");
  clearFormError("form-eventos");
  try {
    const query = jobId ? `?jobId=${jobId}` : "";
    const events = await fetchJSON(getHubBase(), `/eventos${query}`);
    renderList("list-eventos", events, (e) => `<strong>${e.tipo}</strong> · ${e.detalle} · ${e.createdAt}`);
  } catch (err) {
    showFormError("form-eventos", `Error cargando eventos: ${err.message}`);
  }
});

el("form-trigger")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-trigger");
  if (!fd.get("eventName") || !fd.get("templateId")) {
    showFormError("form-trigger", "Completá eventName y template");
    return;
  }
  try {
    await postJSON(getHubBase(), "/triggers", {
      eventName: fd.get("eventName"),
      templateId: fd.get("templateId"),
      enabled: fd.get("enabled") === "true",
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-trigger", `Error creando trigger: ${err.message}`);
  }
});

el("form-ausencias-resumen")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  await refreshAll();
});

el("form-trigger-exec")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-trigger-exec");
  const triggerId = fd.get("triggerId");
  if (!triggerId) {
    showFormError("form-trigger-exec", "Seleccioná un trigger");
    return;
  }
  if (!fd.get("trigger")) {
    showFormError("form-trigger-exec", "Completá el trigger");
    return;
  }
  try {
    await postJSON(getHubBase(), `/triggers/${triggerId}/execute`, {
      periodo: fd.get("periodo") || null,
      trigger: fd.get("trigger"),
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-trigger-exec", `Error ejecutando trigger: ${err.message}`);
  }
});

el("form-trigger-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-trigger-edit");
  const triggerId = fd.get("triggerId");
  if (!triggerId) {
    showFormError("form-trigger-edit", "Seleccioná un trigger");
    return;
  }
  if (!fd.get("eventName") || !fd.get("templateId")) {
    showFormError("form-trigger-edit", "Completá eventName y template");
    return;
  }
  try {
    await putJSON(getHubBase(), `/triggers/${triggerId}`, {
      eventName: fd.get("eventName"),
      templateId: fd.get("templateId"),
      enabled: fd.get("enabled") === "true",
    });
    refreshAll();
  } catch (err) {
    showFormError("form-trigger-edit", `Error actualizando trigger: ${err.message}`);
  }
});

el("form-asignar-posicion")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  const posicionId = fd.get("posicionId");
  const legajoId = fd.get("legajoId");
  clearFormError("form-asignar-posicion");
  if (!posicionId || !legajoId) {
    showFormError("form-asignar-posicion", "Seleccioná posición y legajo");
    return;
  }
  try {
    await postJSON(getOrgBase(), `/posiciones/${posicionId}/asignar`, {
      legajoId,
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-asignar-posicion", `Error asignando legajo: ${err.message}`);
  }
});

el("form-catalogo-buscar")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-catalogo-buscar");
  try {
    const tipo = String(fd.get("tipo") || "").trim();
    if (!tipo) throw new Error("Completá el tipo");
    const items = await fetchJSON(getConfigBase(), `/catalogos/${encodeURIComponent(tipo)}`);
    state.catalogos = items;
    renderList("list-catalogos", items, (c) => `<strong>${c.tipo}</strong> · ${c.codigo} · ${c.nombre}`);
  } catch (err) {
    showFormError("form-catalogo-buscar", `Error cargando catálogo: ${err.message}`);
  }
});

el("form-catalogo")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-catalogo");
  try {
    const payload = {
      tipo: String(fd.get("tipo") || "").trim(),
      codigo: String(fd.get("codigo") || "").trim(),
      nombre: String(fd.get("nombre") || "").trim(),
      activo: normalizeBool(fd.get("activo")),
      metadataJson: String(fd.get("metadata") || "").trim() || null,
    };
    await postJSON(getConfigBase(), "/catalogos", payload);
    evt.target.reset();
  } catch (err) {
    showFormError("form-catalogo", `Error creando catálogo: ${err.message}`);
  }
});

el("form-catalogo-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-catalogo-edit");
  try {
    const id = String(fd.get("id") || "").trim();
    if (!id) throw new Error("Completá el ID");
    const payload = {
      codigo: String(fd.get("codigo") || "").trim(),
      nombre: String(fd.get("nombre") || "").trim(),
      activo: normalizeBool(fd.get("activo")),
      metadataJson: String(fd.get("metadata") || "").trim() || null,
    };
    await putJSON(getConfigBase(), `/catalogos/${id}`, payload);
  } catch (err) {
    showFormError("form-catalogo-edit", `Error actualizando catálogo: ${err.message}`);
  }
});

el("form-catalogo-delete")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-catalogo-delete");
  try {
    const id = String(fd.get("id") || "").trim();
    if (!id) throw new Error("Completá el ID");
    await deleteJSON(getConfigBase(), `/catalogos/${id}`);
  } catch (err) {
    showFormError("form-catalogo-delete", `Error eliminando catálogo: ${err.message}`);
  }
});

el("form-parametros")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-parametros");
  try {
    const items = await fetchJSON(getConfigBase(), "/parametros");
    state.parametros = items;
    renderList("list-parametros", items, (p) => `<strong>${p.clave}</strong> · ${p.valor}`);
  } catch (err) {
    showFormError("form-parametros", `Error cargando parámetros: ${err.message}`);
  }
});

el("form-parametro")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-parametro");
  try {
    const payload = {
      clave: String(fd.get("clave") || "").trim(),
      valor: String(fd.get("valor") || "").trim(),
      descripcion: String(fd.get("descripcion") || "").trim() || null,
      activo: normalizeBool(fd.get("activo")),
    };
    await postJSON(getConfigBase(), "/parametros", payload);
    evt.target.reset();
  } catch (err) {
    showFormError("form-parametro", `Error guardando parámetro: ${err.message}`);
  }
});

el("form-parametro-buscar")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-parametro-buscar");
  try {
    const clave = String(fd.get("clave") || "").trim();
    if (!clave) throw new Error("Completá la clave");
    const item = await fetchJSON(getConfigBase(), `/parametros/${encodeURIComponent(clave)}`);
    const target = el("parametro-detalle");
    if (target) target.innerHTML = `<strong>${item.clave}</strong> · ${item.valor}`;
  } catch (err) {
    showFormError("form-parametro-buscar", `Error buscando parámetro: ${err.message}`);
  }
});

el("form-turno")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-turno");
  try {
    await postJSON(getTiemposBase(), "/turnos", {
      codigo: fd.get("codigo"),
      nombre: fd.get("nombre"),
      horaInicio: normalizeTime(fd.get("horaInicio")),
      horaFin: normalizeTime(fd.get("horaFin")),
      toleranciaMinutos: Number(fd.get("toleranciaMinutos") || 0),
      activo: normalizeBool(fd.get("activo")),
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-turno", `Error creando turno: ${err.message}`);
  }
});

el("form-turno-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-turno-edit");
  const id = fd.get("turnoId");
  if (!id) {
    showFormError("form-turno-edit", "Seleccioná un turno");
    return;
  }
  try {
    await putJSON(getTiemposBase(), `/turnos/${id}`, {
      codigo: fd.get("codigo"),
      nombre: fd.get("nombre"),
      horaInicio: normalizeTime(fd.get("horaInicio")),
      horaFin: normalizeTime(fd.get("horaFin")),
      toleranciaMinutos: Number(fd.get("toleranciaMinutos") || 0),
      activo: normalizeBool(fd.get("activo")),
    });
    refreshAll();
  } catch (err) {
    showFormError("form-turno-edit", `Error actualizando turno: ${err.message}`);
  }
});

el("form-horario")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-horario");
  try {
    await postJSON(getTiemposBase(), "/horarios", {
      nombre: fd.get("nombre"),
      diasSemana: fd.get("diasSemana"),
      turnoId: fd.get("turnoId"),
      activo: normalizeBool(fd.get("activo")),
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-horario", `Error creando horario: ${err.message}`);
  }
});

el("form-horario-edit")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-horario-edit");
  const id = fd.get("horarioId");
  if (!id) {
    showFormError("form-horario-edit", "Seleccioná un horario");
    return;
  }
  try {
    await putJSON(getTiemposBase(), `/horarios/${id}`, {
      nombre: fd.get("nombre"),
      diasSemana: fd.get("diasSemana"),
      turnoId: fd.get("turnoId"),
      activo: normalizeBool(fd.get("activo")),
    });
    refreshAll();
  } catch (err) {
    showFormError("form-horario-edit", `Error actualizando horario: ${err.message}`);
  }
});

el("form-fichada")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-fichada");
  const legajoId = fd.get("legajoId");
  if (!legajoId) {
    showFormError("form-fichada", "Seleccioná un legajo");
    return;
  }
  try {
    await postJSON(getTiemposBase(), "/fichadas", {
      legajoId,
      fechaHora: fd.get("fechaHora"),
      tipo: fd.get("tipo"),
      origen: fd.get("origen"),
      observaciones: fd.get("observaciones"),
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-fichada", `Error registrando fichada: ${err.message}`);
  }
});

el("form-planilla")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  const fd = new FormData(evt.target);
  clearFormError("form-planilla");
  const empresaId = fd.get("empresaId");
  const legajoId = fd.get("legajoId");
  if (!empresaId || !legajoId) {
    showFormError("form-planilla", "Seleccioná empresa y legajo");
    return;
  }
  try {
    await postJSON(getTiemposBase(), "/planillas", {
      periodo: fd.get("periodo"),
      empresaId,
      detalles: [
        {
          legajoId,
          horasNormales: Number(fd.get("horasNormales") || 0),
          horasExtra: Number(fd.get("horasExtra") || 0),
          horasAusencia: Number(fd.get("horasAusencia") || 0),
          observaciones: fd.get("observaciones"),
        },
      ],
    });
    evt.target.reset();
    refreshAll();
  } catch (err) {
    showFormError("form-planilla", `Error creando planilla: ${err.message}`);
  }
});

refreshAll();
setView("dashboard");
