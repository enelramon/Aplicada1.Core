# Aplicada1.Core
# Aplicada1.Core

Biblioteca base para la creación de servicios genéricos en proyectos .NET.

Esta librería define la interfaz base que todos los servicios entregados por los estudiantes deben implementar para mantener una estructura uniforme y consistente en el desarrollo del curso.

## Instalación

Puedes instalar la librería con el siguiente comando:

```bash
dotnet add package Aplicada1.Core
```

## Qué se espera de cada servicio
**Todo servicio entregado debe:**

- Implementar `IService<T, TKey>`

- Usar el tipo correcto para `T` y `TKey`

- Eefinir la lógica real de `Guardar`, `Buscar`, `Eliminar` y `GetList`

- Mantener la convención de nombres y firmas establecidas por la interfaz

## Beneficios
**Al heredar de esta interfaz:**

- Todos los servicios siguen la misma estructura
- Se facilita la evaluación y revisión del código
- Se reduce la variabilidad entre entregas
- De promueve un diseño más ordenado y reutilizable

> Importante: no basta con crear una clase de servicio; debe heredar de IService<T, TKey> para cumplir con el contrato esperado.