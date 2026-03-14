# Módulo Vacaciones / Autoservicio · Blueprint

## Objetivo
Relevar y modernizar el flujo de solicitudes de vacaciones de Nucleus RH 23.01 (`Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`, `Class/NucleusRH/Base/Vacaciones/lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs`) para ofrecer una experiencia autoservicio consistente con los nuevos módulos (Personal, Tiempos, Liquidación) y con un backend desacoplado.

## Alcance legado
- Workflow con etapas `SOLICITAR → PENDAPROB → FINALIZADA`, lógica en `lib_v11.WFSolicitud.SOLICITUD.NomadClass.cs` (cálculo de días, aprobación, actualización de legajo vacaciones).
- Formularios Nomad (tabs General, Domicilio, etc.) y validaciones CUIL/días bonificados.
- Dependencias con `LegajoVacaciones.PERSONAL_EMP` para saldo y actualización.

## Diseño moderno
1. **Vacaciones API** (ASP.NET Core) para solicitudes, aprobaciones, cálculos y sincronización con Tiempos/Liquidación.
2. **Autoservicio Portal** (SPA) para empleados: simulador de saldo, disponibilidad, historial.
3. **Workflow**: migrado a nuevo Nucleus WF (Temporal/Durable) con etapas configurables, SLA, delegaciones.
4. **Integraciones**: eventos hacia Tiempos (bloqueo de turnos) y Liquidación (liquidación de vacaciones), notificaciones (email/Teams).

## MVP implementado
- Portal Empleado inicia solicitudes vía Nucleus WF.
- Portal RH visualiza y aprueba/rechaza instancias de vacaciones.

## Estado (2026-03-13)
- Flujo validado end-to-end con Portal Empleado/RH y Nucleus WF.

---
*Fuentes: `Workflow/NucleusRH/Base/Vacaciones/Solicitud.WF.xml`, `Class/NucleusRH/Base/Vacaciones/lib_v11.*`, `docs/03_flujos_y_workflows.md`.*
