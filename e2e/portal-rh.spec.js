const { test, expect } = require("@playwright/test");

const portalRhUrl = process.env.PORTAL_RH_URL || "http://localhost:3001/index.html";

test("Portal RH: login demo y refresh sin 401", async ({ page }) => {
  await page.goto(portalRhUrl, { waitUntil: "domcontentloaded" });

  await page.getByRole("textbox", { name: "Auth API" }).fill("http://localhost:5001");
  await page.getByRole("textbox", { name: "Organización API" }).fill("http://localhost:5100");
  await page.getByRole("textbox", { name: "Personal API" }).fill("http://localhost:5200");
  await page.getByRole("textbox", { name: "Liquidación API" }).fill("http://localhost:5188");
  await page.getByRole("textbox", { name: "Integration Hub API" }).fill("http://localhost:5050");
  await page.getByRole("textbox", { name: "Workflow API" }).fill("http://localhost:5051");
  await page.getByRole("textbox", { name: "Portal BFF" }).fill("http://localhost:5090");
  await page.getByRole("textbox", { name: "Configuración API" }).fill("http://localhost:5300");
  await page.getByRole("textbox", { name: "Tiempos API" }).fill("http://localhost:5400");

  await page.getByRole("button", { name: "Guardar configuración" }).click();
  await page.getByRole("button", { name: "Login demo" }).click();

  const payrollsResponse = page.waitForResponse(
    (resp) => resp.url().includes("/api/rh/v1/liquidacion/payrolls") && resp.status() !== 401
  );
  await page.getByRole("button", { name: "Refrescar datos" }).click();
  await payrollsResponse;

  await page.getByRole("button", { name: "Reclamos" }).click();
  await expect(page.locator("#list-reclamos")).toBeVisible();
  await page.getByRole("button", { name: "Sanciones" }).click();
  await expect(page.locator("#list-sanciones")).toBeVisible();

  await expect(page.locator("#status-pill")).toContainText("Autenticado");
  await expect(page.locator("#stat-empresas")).not.toContainText("--");
});
