# Casos de uso · Nucleus WF

## CU-WF-01 Consultar definiciones
- **Actor**: Usuario autenticado
- **Objetivo**: Ver definiciones disponibles.
- **Precondiciones**: Token JWT valido.
- **Flujo principal**:
  1. El actor consulta `GET /definitions`.
  2. El sistema devuelve la lista.
- **Postcondicion**: Definiciones visibles.

## CU-WF-02 Crear definicion
- **Actor**: Admin
- **Objetivo**: Registrar un workflow.
- **Precondiciones**: Rol Admin.
- **Flujo principal**:
  1. El actor envia key/version/estado inicial/transiciones.
  2. El sistema crea la definicion.
- **Postcondicion**: Definicion creada.

## CU-WF-03 Iniciar instancia
- **Actor**: Admin
- **Objetivo**: Crear una instancia de workflow.
- **Precondiciones**: Definicion existente.
- **Flujo principal**:
  1. El actor inicia instancia con key/version.
  2. El sistema crea la instancia en estado inicial.
- **Postcondicion**: Instancia disponible.
