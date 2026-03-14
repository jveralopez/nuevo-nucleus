const { test, expect } = require("@playwright/test");

const portalEmpleadoUrl = process.env.PORTAL_EMPLEADO_URL || "http://localhost:3002/index.html";

test("Portal Empleado: login demo y refresh sin 401", async ({ page }) => {
  await page.goto(portalEmpleadoUrl, { waitUntil: "domcontentloaded" });

  await page.getByRole("textbox", { name: "Auth API" }).fill("http://localhost:5001");
  await page.getByRole("textbox", { name: "Liquidación API" }).fill("http://localhost:5188");
  await page.getByRole("textbox", { name: "Workflow API" }).fill("http://localhost:5051");
  await page.getByRole("textbox", { name: "Portal BFF" }).fill("http://localhost:5090");

  await page.getByRole("button", { name: "Guardar configuración" }).click();
  await page.getByRole("button", { name: "Login demo" }).click();

  await page.getByRole("button", { name: "Solicitudes" }).click();
  await page.getByRole("button", { name: "Cargar workflows" }).click();

  const wfResponse = page.waitForResponse(
    (resp) => resp.url().includes("/api/portal/v1/wf/definitions") && resp.status() !== 401
  );
  await page.getByRole("button", { name: "Refrescar datos" }).click();
  await wfResponse;

  await expect(page.locator("#status-pill")).toContainText("Autenticado");
  await expect(page.locator("#stat-recibos")).toContainText(/\d+/);
});
