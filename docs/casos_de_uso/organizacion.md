# Casos de uso · Organización

## CU-ORG-01 Consultar empresas
- **Actor**: Usuario autenticado
- **Objetivo**: Ver la lista de empresas.
- **Precondiciones**: Token JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /empresas`.
  2. El sistema devuelve la lista.
- **Postcondicion**: Datos visibles.

## CU-ORG-02 Crear empresa
- **Actor**: Admin
- **Objetivo**: Registrar una empresa.
- **Precondiciones**: Rol Admin.
- **Flujo principal**:
  1. El actor envia datos obligatorios.
  2. El sistema crea la empresa.
- **Postcondicion**: Empresa creada.

## CU-ORG-03 Gestionar unidades
- **Actor**: Admin
- **Objetivo**: Crear/editar unidades organizativas.
- **Precondiciones**: Empresa existente.
- **Flujo principal**:
  1. El actor crea o edita unidad.
  2. El sistema valida empresa y guarda.
- **Postcondicion**: Unidad disponible.

## CU-ORG-03B Ver organigrama
- **Actor**: Usuario autenticado
- **Objetivo**: Consultar arbol de unidades.
- **Precondiciones**: Unidades cargadas.
- **Flujo principal**:
  1. El actor consulta `GET /unidades/tree`.
  2. El sistema devuelve el arbol.
- **Postcondicion**: Organigrama visible.

## CU-ORG-04 Gestionar posiciones
- **Actor**: Admin
- **Objetivo**: Crear/editar posiciones.
- **Precondiciones**: Unidad existente.
- **Flujo principal**:
  1. El actor crea o edita una posicion.
  2. El sistema valida y guarda.
- **Postcondicion**: Posicion disponible.

## CU-ORG-05 Asignar/desasignar legajo a posicion
- **Actor**: Admin
- **Objetivo**: Asociar o quitar legajo.
- **Precondiciones**: Legajo y posicion existentes.
- **Flujo principal**:
  1. El actor ejecuta asignar o desasignar.
  2. El sistema actualiza la posicion.
- **Postcondicion**: Posicion actualizada.

## CU-ORG-06 Gestionar centros de costo
- **Actor**: Admin
- **Objetivo**: Crear/editar centros de costo.
- **Precondiciones**: Empresa existente.
- **Flujo principal**:
  1. El actor crea o edita centro de costo.
  2. El sistema guarda.
- **Postcondicion**: Centro de costo disponible.

## CU-ORG-07 Gestionar sindicatos
- **Actor**: Admin
- **Objetivo**: Crear/editar sindicatos.
- **Precondiciones**: JWT Admin.
- **Flujo principal**:
  1. El actor registra o actualiza el sindicato.
  2. El sistema guarda la información.
- **Postcondicion**: Sindicato disponible.

## CU-ORG-08 Gestionar convenios
- **Actor**: Admin
- **Objetivo**: Crear/editar convenios colectivos.
- **Precondiciones**: Sindicato existente.
- **Flujo principal**:
  1. El actor registra o actualiza el convenio.
  2. El sistema valida sindicato y guarda.
- **Postcondicion**: Convenio disponible.

## CU-ORG-09 Versionar organigrama
- **Actor**: Admin
- **Objetivo**: Crear una nueva version del organigrama.
- **Precondiciones**: Empresa existente y unidades cargadas.
- **Flujo principal**:
  1. El actor solicita `POST /organigramas` con `EmpresaId` y `Nombre`.
  2. El sistema calcula la siguiente version y guarda el snapshot del arbol.
- **Postcondicion**: Version creada y disponible.

## CU-ORG-10 Consultar versiones de organigrama
- **Actor**: Usuario autenticado
- **Objetivo**: Listar versiones y consultar una version especifica.
- **Precondiciones**: Versiones existentes.
- **Flujo principal**:
  1. El actor consulta `GET /organigramas?empresaId={id}`.
  2. El actor consulta `GET /organigramas/{id}`.
- **Postcondicion**: Versiones visibles.
