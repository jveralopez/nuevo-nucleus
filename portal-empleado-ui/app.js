const KEYS = {
  auth: "portal_auth_url",
  liq: "portal_liq_url",
  wf: "portal_wf_url",
  bff: "portal_bff_url",
  token: "portal_token",
};

const el = (id) => document.getElementById(id);
const viewButtons = document.querySelectorAll(".nav-btn");
const views = document.querySelectorAll("main[data-view]");

const storedToken = localStorage.getItem(KEYS.token) || "";
const state = {
  authUrl: localStorage.getItem(KEYS.auth) || "http://localhost:5001",
  liqUrl: localStorage.getItem(KEYS.liq) || "http://localhost:5188",
  wfUrl: localStorage.getItem(KEYS.wf) || "http://localhost:5051",
  bffUrl: localStorage.getItem(KEYS.bff) || "http://localhost:5090",
  token: normalizeToken(storedToken),
  instances: [],
  payrolls: [],
  definitions: [],
  recibosDetalle: [],
  notifications: [],
  notifUnreadOnly: false,
};

const NOTIF_KEY = "empleado_notificaciones";

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
el("wf-url").value = state.wfUrl;
el("bff-url").value = state.bffUrl;
el("token").value = state.token;

function setStatus() {
  el("status-pill").textContent = state.token ? "Autenticado" : "Sin sesión";
}

function normalizeToken(value) {
  return value.replace(/^Bearer\s+/i, "").trim();
}

setStatus();

async function fetchJSON(base, path) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, { headers });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
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
    const msg = await response.text();
    throw new Error(msg || `${response.status}`);
  }
  return response.json();
}

function getWfBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/portal/v1/wf`;
  return state.wfUrl.replace(/\/$/, "");
}

function getLiqBase() {
  if (state.bffUrl) return `${state.bffUrl.replace(/\/$/, "")}/api/portal/v1/liquidacion`;
  return state.liqUrl.replace(/\/$/, "");
}

async function deleteJSON(base, path) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(`${base}${path}`, { method: "DELETE", headers });
  if (!response.ok) {
    const msg = await response.text();
    throw new Error(msg || `${response.status}`);
  }
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

function renderInstancesFull(instances) {
  const list = el("list-instancias-full");
  list.innerHTML = "";
  if (!instances.length) {
    const li = document.createElement("li");
    li.textContent = "Sin datos";
    list.appendChild(li);
    return;
  }
  instances.forEach((i) => {
    const li = document.createElement("li");
    li.innerHTML = `
      <strong>${i.key}</strong> · ${i.estado}
      <div class="actions">
        <button class="btn small btn-approve" data-id="${i.id}">Aprobar</button>
        <button class="btn small btn-reject" data-id="${i.id}">Rechazar</button>
        <button class="btn small btn-history" data-id="${i.id}">Historial</button>
      </div>
    `;
    list.appendChild(li);
  });
}

function renderSolicitudes(instances) {
  const list = el("list-solicitudes");
  if (!list) return;
  list.innerHTML = "";
  const items = instances.filter((i) => ["vacaciones", "datos-personales", "reclamos"].includes(i.key));
  if (!items.length) {
    const li = document.createElement("li");
    li.textContent = "Sin solicitudes";
    list.appendChild(li);
    return;
  }
  items.forEach((i) => {
    const li = document.createElement("li");
    li.innerHTML = `<strong>${i.key}</strong> · ${i.estado} · ${i.id}`;
    list.appendChild(li);
  });
}

function buildNotifications(payrolls, instances) {
  const items = [];
  payrolls.slice(0, 3).forEach((p) => {
    items.push({
      id: `recibo-${p.id}`,
      title: `Recibo disponible · ${p.periodo}`,
      detail: `${p.tipo} · ${p.estado}`,
    });
  });
  instances
    .filter((i) => i.estado.toLowerCase() === "aprobacion" || i.estado.toLowerCase() === "inicio")
    .slice(0, 3)
    .forEach((i) => {
      items.push({
        id: `wf-${i.id}-${i.estado}`,
        title: `Tarea pendiente · ${i.key}`,
        detail: `Estado ${i.estado}`,
      });
    });
  return items.slice(0, 5);
}

async function syncNotificationsWithBff(items) {
  if (!state.bffUrl) return [];
  for (const item of items) {
    await postJSON(state.bffUrl, "/api/portal/v1/notificaciones", {
      title: item.title,
      detail: item.detail,
      sourceId: item.id,
    });
  }
  const query = state.notifUnreadOnly ? "?unreadOnly=true" : "";
  return fetchJSON(state.bffUrl, `/api/portal/v1/notificaciones${query}`);
}

function renderNotificationsFromList(list, target) {
  target.innerHTML = "";
  if (!list.length) {
    const li = document.createElement("li");
    li.textContent = "Sin notificaciones";
    target.appendChild(li);
    return;
  }
  list.forEach((item) => {
    const li = document.createElement("li");
    const readBadge = item.readAt ? "Leída" : "Nueva";
    const readButton = item.readAt ? "" : `<button class="btn small btn-notif-read" data-id="${item.id}">Marcar leída</button>`;
    li.innerHTML = `<strong>${item.title}</strong> · ${item.detail} · ${readBadge} ${readButton}`;
    target.appendChild(li);
  });
}

async function renderNotifications(payrolls, instances) {
  const list = el("list-notificaciones");
  if (!list) return;
  list.innerHTML = "";
  const localItems = buildNotifications(payrolls, instances);
  if (state.bffUrl) {
    try {
      const remote = await syncNotificationsWithBff(localItems);
      state.notifications = remote;
      renderNotificationsFromList(remote, list);
      await renderNotificationSummary();
      return;
    } catch (err) {
      // Fallback a local storage.
    }
  }

  const stored = JSON.parse(localStorage.getItem(NOTIF_KEY) || "[]");
  const items = [...localItems, ...stored].slice(0, 8);
  state.notifications = items;
  renderNotificationsFromList(items, list);
  localStorage.setItem(NOTIF_KEY, JSON.stringify(items));
  renderNotificationSummary(items.length, items.filter((n) => !n.readAt).length);
}

async function renderNotificationSummary(totalOverride, unreadOverride) {
  const summaryEl = el("notif-resumen");
  if (!summaryEl) return;
  if (!state.bffUrl) {
    const total = totalOverride ?? 0;
    const unread = unreadOverride ?? 0;
    summaryEl.textContent = `${unread}/${total}`;
    return;
  }
  try {
    const summary = await fetchJSON(state.bffUrl, "/api/portal/v1/notificaciones/resumen");
    summaryEl.textContent = `${summary.unread}/${summary.total}`;
  } catch (err) {
    summaryEl.textContent = "0/0";
  }
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

async function applyQuickTransition(instanceId, eventName) {
  const actor = el("task-actor").value || "empleado";
  const comment = el("task-comment").value || "";
  await postJSON(getWfBase(), `/instances/${instanceId}/transitions`, {
    evento: eventName,
    actor,
    datos: { comentario: comment },
  });
}

function renderHistory(instance) {
  const list = el("list-historial");
  list.innerHTML = "";
  const items = instance?.historial || [];
  if (!items.length) {
    const li = document.createElement("li");
    li.textContent = "Sin historial";
    list.appendChild(li);
    return;
  }
  items.forEach((h) => {
    const li = document.createElement("li");
    li.textContent = `${h.timestamp} · ${h.evento} · ${h.from} → ${h.to}`;
    list.appendChild(li);
  });
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

function buildWorkflowDefinition(key, nombre) {
  return {
    key,
    version: "1.0.0",
    nombre,
    estadoInicial: "Inicio",
    transiciones: [
      { from: "Inicio", to: "Aprobacion", evento: "enviar" },
      { from: "Aprobacion", to: "Aprobado", evento: "aprobar" },
      { from: "Aprobacion", to: "Rechazado", evento: "rechazar" },
      { from: "Aprobado", to: "Cerrado", evento: "cerrar" },
      { from: "Rechazado", to: "Cerrado", evento: "cerrar" },
    ],
  };
}

async function seedWorkflows() {
  const defs = [
    buildWorkflowDefinition("vacaciones", "Solicitud de Vacaciones"),
    buildWorkflowDefinition("datos-personales", "Actualizacion de Datos"),
    buildWorkflowDefinition("reclamos", "Gestion de Reclamos"),
  ];

  for (const def of defs) {
    try {
      await postJSON(getWfBase(), "/definitions", def);
    } catch (err) {
      if (!err.message?.toLowerCase().includes("existe")) {
        throw err;
      }
    }
  }
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

async function refreshAll() {
  setStatus();
  const wfBase = getWfBase();
  const [payrolls, definitions, instances] = await Promise.all([
    fetchJSON(getLiqBase(), state.bffUrl ? "" : "/payrolls").catch(() => []),
    fetchJSON(wfBase, "/definitions").catch(() => []),
    fetchJSON(wfBase, "/instances").catch(() => []),
  ]);

  state.instances = instances;
  state.payrolls = payrolls;
  state.definitions = definitions;

  el("stat-recibos").textContent = payrolls.length || "0";
  el("stat-wf").textContent = definitions.length || "0";
  el("stat-instancias").textContent = instances.length || "0";
  el("stat-tareas").textContent = instances.length || "0";
  el("stat-notif").textContent = payrolls.length ? "1" : "0";

  el("badge-wf").textContent = definitions.length ? "Con datos" : "Sin datos";
  el("badge-recibos").textContent = payrolls.length ? "Con datos" : "Sin datos";

  renderList("list-recibos", payrolls, (p) => `<strong>${p.periodo}</strong> · ${p.tipo} · ${p.estado}`);
  renderList("list-wf", definitions, (d) => `<strong>${d.nombre}</strong> · ${d.version}`);
  renderList("list-instancias", instances, (i) => `<strong>${i.estado}</strong> · ${i.key}`);

  renderList("list-recibos-full", payrolls, (p) => `<strong>${p.periodo}</strong> · ${p.tipo} · ${p.estado}`);
  renderList("list-wf-full", definitions, (d) => `<strong>${d.nombre}</strong> · ${d.version}`);
  renderInstancesFull(instances);
  renderSolicitudes(instances);
  await renderNotifications(payrolls, instances);

  fillSelect("select-instancia", instances, (i) => `${i.key} · ${i.estado}`);
  fillSelect("select-recibo-payroll", payrolls, (p) => `${p.periodo} · ${p.tipo}`);

  document.querySelectorAll(".btn-approve").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await applyQuickTransition(btn.dataset.id, "aprobar");
        await refreshAll();
      } catch (err) {
        alert(`Error aprobando: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-reject").forEach((btn) => {
    btn.addEventListener("click", async () => {
      try {
        await applyQuickTransition(btn.dataset.id, "rechazar");
        await refreshAll();
      } catch (err) {
        alert(`Error rechazando: ${err.message}`);
      }
    });
  });
  document.querySelectorAll(".btn-history").forEach((btn) => {
    btn.addEventListener("click", () => {
      const instance = state.instances.find((i) => i.id === btn.dataset.id);
      renderHistory(instance);
    });
  });

  document.querySelectorAll(".btn-notif-read").forEach((btn) => {
    btn.addEventListener("click", async () => {
      if (!state.bffUrl) {
        return;
      }
      try {
        await postJSON(state.bffUrl, `/api/portal/v1/notificaciones/${btn.dataset.id}/read`, {});
        await renderNotifications(state.payrolls, state.instances);
      } catch (err) {
        alert(`Error marcando notificación: ${err.message}`);
      }
    });
  });
}

el("btn-save").addEventListener("click", () => {
  state.authUrl = el("auth-url").value.trim();
  state.liqUrl = el("liq-url").value.trim();
  state.wfUrl = el("wf-url").value.trim();
  state.bffUrl = el("bff-url").value.trim();
  state.token = normalizeToken(el("token").value);
  localStorage.setItem(KEYS.auth, state.authUrl);
  localStorage.setItem(KEYS.liq, state.liqUrl);
  localStorage.setItem(KEYS.wf, state.wfUrl);
  localStorage.setItem(KEYS.bff, state.bffUrl);
  localStorage.setItem(KEYS.token, state.token);
  refreshAll();
});

el("btn-refresh").addEventListener("click", refreshAll);

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

el("form-wf-start")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-wf-start");
  const fd = new FormData(evt.target);
  let datos = {};
  const raw = fd.get("datos");
  if (raw) {
    try {
      datos = JSON.parse(raw);
    } catch (err) {
      showFormError("form-wf-start", "JSON inválido");
      return;
    }
  }
  try {
    await postJSON(getWfBase(), "/instances", {
      key: fd.get("key"),
      version: fd.get("version"),
      datos,
    });
    evt.target.reset();
    await refreshAll();
    setView("tareas");
  } catch (err) {
    showFormError("form-wf-start", `Error iniciando workflow: ${err.message}`);
  }
});

el("btn-seed-wf")?.addEventListener("click", async () => {
  try {
    await seedWorkflows();
    await refreshAll();
    alert("Workflows demo creados.");
  } catch (err) {
    alert(`Error creando workflows: ${err.message}`);
  }
});

el("form-wf-transition")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-wf-transition");
  const fd = new FormData(evt.target);
  let datos = {};
  const raw = fd.get("datos");
  if (raw) {
    try {
      datos = JSON.parse(raw);
    } catch (err) {
      showFormError("form-wf-transition", "JSON inválido");
      return;
    }
  }
  const instanceId = fd.get("instanceId");
  if (!instanceId) {
    showFormError("form-wf-transition", "Seleccioná una instancia");
    return;
  }
  try {
    await postJSON(getWfBase(), `/instances/${instanceId}/transitions`, {
      evento: fd.get("evento"),
      actor: fd.get("actor"),
      datos,
    });
    evt.target.reset();
    await refreshAll();
  } catch (err) {
    showFormError("form-wf-transition", `Error aplicando transición: ${err.message}`);
  }
});

el("form-vacaciones")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-vacaciones");
  const fd = new FormData(evt.target);
  try {
    await postJSON(getWfBase(), "/instances", {
      key: "vacaciones",
      version: "1.0.0",
      datos: {
        legajoNumero: fd.get("legajoNumero") || "",
        inicio: fd.get("inicio"),
        fin: fd.get("fin"),
        dias: Number(fd.get("dias")) || 0,
        motivo: fd.get("motivo") || "",
      },
    });
    evt.target.reset();
    await refreshAll();
    setView("tareas");
  } catch (err) {
    showFormError("form-vacaciones", `Error enviando solicitud: ${err.message}`);
  }
});

el("form-datos-personales")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-datos-personales");
  const fd = new FormData(evt.target);
  try {
    await postJSON(getWfBase(), "/instances", {
      key: "datos-personales",
      version: "1.0.0",
      datos: {
        motivo: fd.get("motivo"),
        detalle: fd.get("detalle"),
      },
    });
    evt.target.reset();
    await refreshAll();
    setView("tareas");
  } catch (err) {
    showFormError("form-datos-personales", `Error enviando solicitud: ${err.message}`);
  }
});

el("form-reclamo")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-reclamo");
  const fd = new FormData(evt.target);
  try {
    await postJSON(getWfBase(), "/instances", {
      key: "reclamos",
      version: "1.0.0",
      datos: {
        asunto: fd.get("asunto"),
        detalle: fd.get("detalle"),
      },
    });
    evt.target.reset();
    await refreshAll();
    setView("tareas");
  } catch (err) {
    showFormError("form-reclamo", `Error enviando reclamo: ${err.message}`);
  }
});

el("form-recibos-detalle")?.addEventListener("submit", async (evt) => {
  evt.preventDefault();
  clearFormError("form-recibos-detalle");
  const fd = new FormData(evt.target);
  const payrollId = fd.get("payrollId");
  if (!payrollId) {
    showFormError("form-recibos-detalle", "Seleccioná una liquidación");
    return;
  }
  try {
    const recibosPath = state.bffUrl ? `/${payrollId}/recibos` : `/payrolls/${payrollId}/recibos`;
    const recibos = await fetchJSON(getLiqBase(), recibosPath);
    state.recibosDetalle = recibos;
    const totalNeto = recibos.reduce((acc, r) => acc + (Number(r.neto) || 0), 0);
    const totalRem = recibos.reduce((acc, r) => acc + (Number(r.remunerativo) || 0), 0);
    const totalDed = recibos.reduce((acc, r) => acc + (Number(r.deducciones) || 0), 0);
    el("detalle-recibos").innerHTML = `
      <p>Total remunerativo: ${totalRem.toFixed(2)} · Total deducciones: ${totalDed.toFixed(2)} · Total neto: ${totalNeto.toFixed(2)}</p>
      <ul class="list">
        ${recibos.map((r) => `<li>${r.legajoNumero} · ${r.legajoNombre} · Rem: ${r.remunerativo} · Ded: ${r.deducciones} · Neto: ${r.neto}</li>`).join("")}
      </ul>
    `;
  } catch (err) {
    showFormError("form-recibos-detalle", `Error cargando recibos: ${err.message}`);
  }
});

el("btn-notif-clear")?.addEventListener("click", () => {
  if (state.bffUrl) {
    deleteJSON(state.bffUrl, "/api/portal/v1/notificaciones")
      .then(() => renderNotifications(state.payrolls, state.instances))
      .catch(() => {
        localStorage.removeItem(NOTIF_KEY);
        renderNotifications(state.payrolls, state.instances);
      });
    return;
  }
  localStorage.removeItem(NOTIF_KEY);
  renderNotifications(state.payrolls, state.instances);
});

el("btn-notif-readall")?.addEventListener("click", () => {
  if (!state.bffUrl) return;
  postJSON(state.bffUrl, "/api/portal/v1/notificaciones/read-all", {})
    .then(() => renderNotifications(state.payrolls, state.instances))
    .catch((err) => alert(`Error marcando todas: ${err.message}`));
});

el("btn-notif-unread")?.addEventListener("click", () => {
  state.notifUnreadOnly = !state.notifUnreadOnly;
  el("btn-notif-unread").textContent = state.notifUnreadOnly ? "Mostrar todas" : "Solo no leídas";
  renderNotifications(state.payrolls, state.instances);
});

el("btn-notif-refresh")?.addEventListener("click", () => {
  renderNotifications(state.payrolls, state.instances);
});

function downloadBlob(content, filename, type) {
  const blob = content instanceof Blob ? content : new Blob([content], { type });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = filename;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

function buildRecibosCsv(recibos) {
  const header = "Legajo,Nombre,Remunerativo,Deducciones,Neto";
  const rows = recibos.map((r) =>
    `${r.legajoNumero},${r.legajoNombre},${r.remunerativo},${r.deducciones},${r.neto}`);
  return [header, ...rows].join("\n");
}

async function fetchExportLinks(payrollId) {
  const base = state.bffUrl ? state.bffUrl.replace(/\/$/, "") : state.liqUrl.replace(/\/$/, "");
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const path = state.bffUrl ? `/api/portal/v1/liquidacion/${payrollId}/exports` : `/payrolls/${payrollId}/exports/empleado`;
  const response = await fetch(`${base}${path}`, { headers });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  const exports = await response.json();
  return exports.map((item) => {
    if (state.bffUrl) {
      return `${base}/api/portal/v1/liquidacion/exports/${item.fileName}`;
    }
    return `${base}${item.url || `/exports/${item.fileName}`}`;
  });
}

async function downloadWithAuth(url) {
  const headers = new Headers();
  if (state.token) headers.set("Authorization", `Bearer ${state.token}`);
  const response = await fetch(url, { headers });
  if (!response.ok) {
    throw new Error(`${response.status}`);
  }
  const blob = await response.blob();
  const fileName = url.split("/").pop() || "export";
  downloadBlob(blob, fileName, blob.type || "application/octet-stream");
}

el("btn-recibos-json")?.addEventListener("click", () => {
  if (!state.recibosDetalle.length) {
    alert("Primero cargá el detalle de recibos.");
    return;
  }
  downloadBlob(JSON.stringify(state.recibosDetalle, null, 2), "recibos.json", "application/json");
});

el("btn-recibos-csv")?.addEventListener("click", () => {
  if (!state.recibosDetalle.length) {
    alert("Primero cargá el detalle de recibos.");
    return;
  }
  downloadBlob(buildRecibosCsv(state.recibosDetalle), "recibos.csv", "text/csv");
});

el("btn-recibos-export")?.addEventListener("click", async () => {
  const select = el("select-recibo-payroll");
  const payrollId = select?.value;
  if (!payrollId) {
    alert("Seleccioná una liquidación.");
    return;
  }
  if (!state.token) {
    alert("Necesitás login para descargar exportes.");
    return;
  }
  try {
    const links = await fetchExportLinks(payrollId);
    if (!links.length) {
      alert("No hay exportes disponibles.");
      return;
    }
    for (const link of links) {
      await downloadWithAuth(link);
    }
  } catch (err) {
    alert(`Error descargando exportes: ${err.message}`);
  }
});

refreshAll();
setView("dashboard");
