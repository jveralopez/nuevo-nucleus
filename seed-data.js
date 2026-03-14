// Seed data for Nucleus RH Next
// Run this to populate databases with test data for E2E tests

const fs = require('fs');
const path = require('path');

// Note: This is a conceptual seed script.
// In production, each service would have its own seed endpoint or migration.
// For now, this documents what data should exist for testing.

const seedData = {
  auth: {
    users: [
      { id: "admin", username: "admin", email: "admin@nucleus.com", role: "Admin" },
      { id: "rrhh", username: "rrhh", email: "rrhh@nucleus.com", role: "RRHH" },
      { id: "jefe", username: "jefe", email: "jefe@nucleus.com", role: "Jefe" },
      { id: "empleado", username: "empleado", email: "empleado@nucleus.com", role: "Empleado" }
    ]
  },
  organizacion: {
    empresas: [
      { id: "emp-001", nombre: "Nucleus SA", cuil: "30712345678", estado: "Activa" }
    ],
    unidades: [
      { id: "uni-001", nombre: "Gerencia General", empresaId: "emp-001", tipo: "Gerencia" },
      { id: "uni-002", nombre: "RRHH", empresaId: "emp-001", tipo: "Departamento", padreId: "uni-001" },
      { id: "uni-003", nombre: "TI", empresaId: "emp-001", tipo: "Departamento", padreId: "uni-001" }
    ],
    posiciones: [
      { id: "pos-001", nombre: "Gerente General", unidadId: "uni-001" },
      { id: "pos-002", nombre: "Encargado RRHH", unidadId: "uni-002" },
      { id: "pos-003", nombre: "Desarrollador", unidadId: "uni-003" }
    ]
  },
  personal: {
    legajos: [
      {
        id: "leg-001",
        numero: "10001",
        nombre: "Juan",
        apellido: "Perez",
        documentoTipo: "DNI",
        documentoNumero: "12345678",
        cuil: "20-12345678-9",
        email: "juan.perez@nucleus.com",
        empresaId: "emp-001",
        unidadId: "uni-002",
        posicionId: "pos-002",
        fechaIngreso: "2020-01-15",
        estado: "Activo"
      },
      {
        id: "leg-002",
        numero: "10002",
        nombre: "Maria",
        apellido: "Gonzalez",
        documentoTipo: "DNI",
        documentoNumero: "23456789",
        cuil: "27-23456789-5",
        email: "maria.gonzalez@nucleus.com",
        empresaId: "emp-001",
        unidadId: "uni-003",
        posicionId: "pos-003",
        fechaIngreso: "2021-03-01",
        estado: "Activo"
      }
    ]
  },
  liquidacion: {
    payrolls: [
      {
        id: "pay-001",
        periodo: "2026-02",
        tipo: "Mensual",
        estado: "Cerrado",
        descripcion: "Liquidacion Febrero 2026"
      }
    ]
  },
  tiempos: {
    turnos: [
      { id: "tur-001", nombre: "Turno Manana", entrada: "08:00", salida: "17:00" },
      { id: "tur-002", nombre: "Turno Tarde", entrada: "13:00", salida: "22:00" }
    ],
    horarios: [
      { id: "hor-001", nombre: "Horario Standard", turnoId: "tur-001", lunes: true, martes: true, miercoles: true, jueves: true, viernes: true }
    ],
    fichadas: [
      { id: "fic-001", legajoId: "leg-001", fechaHora: "2026-03-14T08:05:00", tipo: "Entrada" },
      { id: "fic-002", legajoId: "leg-001", fechaHora: "2026-03-14T17:00:00", tipo: "Salida" }
    ]
  },
  vacaciones: {
    solicitudes: [
      { id: "vac-001", legajoId: "leg-001", periodo: "2026", fechaDesde: "2026-04-15", fechaHasta: "2026-04-22", dias: 6, estado: "Aprobada" }
    ]
  },
  seleccion: {
    avisos: [
      { id: "aviso-001", titulo: "Desarrollador Senior", estado: "Activo", vacantes: 2 }
    ],
    candidates: [
      { id: "cand-001", nombre: "Pedro Martinez", avisoId: "aviso-001", estado: "En entrevista" }
    ]
  },
  evaluacion: {
    evaluaciones: [
      { id: "eval-001", legajoId: "leg-001", periodo: "2026-Q1", estado: "Completada", puntaje: 8 }
    ]
  },
  capacitacion: {
    cursos: [
      { id: "cur-001", nombre: "Liderazgo Efectivo", estado: "Activo", duracionHoras: 20 }
    ],
    inscripciones: [
      { id: "insc-001", cursoId: "cur-001", legajoId: "leg-001", estado: "En curso" }
    ]
  },
  clima: {
    encuestas: [
      { id: "enc-001", titulo: "Satisfaccion 2026", estado: "Activa", preguntas: 10 }
    ]
  }
};

console.log("Seed Data for Nucleus RH Next");
console.log("==============================");
console.log(JSON.stringify(seedData, null, 2));
console.log("\nTo seed data, use each service's API or database.");
console.log("Example: POST /api/rh/v1/test/seed in portal-bff-service");
