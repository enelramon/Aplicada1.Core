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
- Definir la lógica real de `Guardar`, `Buscar`, `Eliminar` y `GetList`
- Mantener la convención de nombres y firmas establecidas por la interfaz

## Beneficios
**Al heredar de esta interfaz:**

- Todos los servicios siguen la misma estructura
- Se facilita la evaluación y revisión del código
- Se reduce la variabilidad entre entregas
- Se promueve un diseño más ordenado y reutilizable

> Importante: no basta con crear una clase de servicio; debe heredar de `IService<T, TKey>` para cumplir con el contrato esperado.

---

## Result pattern

El `Result` pattern es una forma de representar el resultado de una operación sin lanzar excepciones para errores esperados del dominio. En lugar de devolver `null`, `false` o lanzar una excepción cuando el caso de error es manejable, la operación devuelve un objeto con el estado y el detalle del error.

### ¿Cuándo usarlo?

Usa `Result` cuando:

- Una operación puede fallar por validación, regla de negocio o inconsistencia de datos.
- Necesitas devolver información útil del error sin romper el flujo normal.
- Quieres mantener una API clara, expresiva y consistente.
- Deseas encadenar validaciones y transformaciones de manera segura.

### Modelo base

La biblioteca define:

- `Result` para operaciones sin valor de retorno
- `Result<T>` para operaciones con valor de retorno
- `Error` como estructura con `Code` y `Description`

```csharp
var result = Result.Success();
var errorResult = Result.Failure(new Error("E001", "No se pudo guardar"));

var userResult = Result.Success(new Usuario { Id = 1 });
```

### Crear resultados simples

```csharp
var ok = Result.Success();
var bad = Result.Failure(new Error("VALIDATION", "El nombre es obligatorio"));
```

Cuando la operación retorna un valor:

```csharp
var saved = Result.Success("usuario-guardado");
var invalid = Result.Failure<string>(new Error("INVALID", "La entidad no es válida"));
```

### Validar condiciones con conversion directa

```csharp
var isValid = true;
var result = isValid.ToResult(new Error("VALIDATION", "La validación falló"));
```

Esto es útil cuando decides resolver un flujo en una sola línea:

```csharp
return cliente != null
    ? Result.Success()
    : Result.Failure(new Error("NOT_FOUND", "Cliente no existe"));
```

### Ejecutar acciones según el estado

#### `OnSuccess` y `OnFailure`

```csharp
Result.Success()
    .OnSuccess(() => Console.WriteLine("Operación exitosa"))
    .OnFailure(() => Console.WriteLine("Operación fallida"));
```

```csharp
Result.Failure<string>(new Error("E002", "Fallo de validación"))
    .OnFailure(error => Console.WriteLine(error.Description));
```

Es útil cuando deseas ejecutar efectos secundarios o registrar eventos sin romper el flujo del resultado.

#### Versión asíncrona

```csharp
await Result.Success()
    .OnSuccessAsync(async () =>
    {
        await Task.Delay(100);
        Console.WriteLine("Proceso completado");
    });
```

```csharp
await Result.Failure(new Error("E003", "Error de red"))
    .OnFailureAsync(async () =>
    {
        await Task.Delay(50);
        Console.WriteLine("Registrar error");
    });
```

### Transformar valores con `Map`

```csharp
var result = Result.Success(10)
    .Map(x => x * 2);

Console.WriteLine(result.Value); // 20
```

Si falla, se conserva el mismo error sin ejecutar la transformación:

```csharp
var result = Result.Failure<int>(new Error("E004", "No hay valor"))
    .Map(x => x + 10);

Console.WriteLine(result.IsFailure); // true
```

### Encadenar operaciones con `Bind`

`Bind` sirve para ejecutar la siguiente operación solo si el resultado anterior fue exitoso:

```csharp
var result = Result.Success(5)
    .Bind(value => Result.Success(value + 3))
    .Bind(value => Result.Success(value * 2));

Console.WriteLine(result.Value); // 16
```

Esto es ideal para flujos de negocio donde cada paso depende del anterior:

```csharp
var proceso = Result.Success(new Usuario())
    .Bind(user => ValidarUsuario(user))
    .Bind(user => GuardarUsuario(user))
    .Bind(user => EnviarCorreo(user));
```

### Seleccionar comportamiento con `Match`

`Match` transforma un resultado en un valor final según el estado:

```csharp
var mensaje = Result.Success("usuario")
    .Match(
        value => $"OK: {value}",
        error => $"ERROR: {error.Description}");
```

```csharp
var errorDescripcion = Result.Failure<string>(new Error("E005", "Nombre inválido"))
    .Match(
        value => value,
        error => error.Description);
```

### Efectos laterales con `Tap` y `TapError`

```csharp
var result = Result.Success("producto")
    .Tap(value => Console.WriteLine($"Producto recibido: {value}"));
```

```csharp
var result = Result.Failure<string>(new Error("E006", "Producto no existe"))
    .TapError(error => Console.WriteLine(error.Description));
```

Estas operaciones son útiles para logging, trazabilidad o métricas sin cambiar el resultado real.

### Finalización con `Finally`

`Finally` siempre ejecuta una acción, sin importar si el resultado fue success o failure:

```csharp
var result = Result.Success()
    .Finally(() => Console.WriteLine("Se ejecutó siempre"));
```

Se usa para liberar recursos, cerrar conexiones, registrar métricas o limpiar estado.

### Manejo de excepciones con `Try`

`Try` convierte una excepción en un `Result` para que el error quede representado en el patrón y no salga como excepción no controlada:

```csharp
var result = Result.Try(() =>
{
    var value = int.Parse("abc");
    return value;
});

if (result.IsFailure)
{
    Console.WriteLine(result.Error.Code);        // EXCEPTION
    Console.WriteLine(result.Error.Description); // Input string was not in a correct format.
}
```

Esto es útil cuando el método puede fallar debido a entrada inválida, acceso a archivos, servicios externos o IO.

### Extensors rápidos para retorno directo

También puedes devolver resultados de una manera compacta:

```csharp
return true.ToResult(new Error("VALIDATION", "El campo es obligatorio"));
return "usuario".ToSuccess();
return new Error("NOT_FOUND", "No existe").ToFailure<Usuario>();
```

Estos helpers reducen ruido en el código y hacen los métodos más expresivos.

### Ejemplo práctico completo

```csharp
public Result<string> RegistrarUsuario(string nombre)
{
    if (string.IsNullOrWhiteSpace(nombre))
    {
        return Result.Failure<string>(new Error("VALIDATION", "Nombre requerido"));
    }

    return Result.Try(() =>
    {
        var usuario = $"Usuario: {nombre}";
        return usuario;
    })
    .Map(value => value.Trim())
    .Then(value => $"{value} registrado")
    .OnSuccess(value => Console.WriteLine($"OK: {value}"))
    .OnFailure(error => Console.WriteLine($"ERROR: {error.Description}"));
}
```

Este patrón evita el uso de `throw` para errores esperados y permite mantener el flujo más claro y predecible.

---

## Estructura recomendada para servicios

En servicios de negocio suele trabajarse así:

```csharp
public interface IService<T, TKey> where T : class
{
    Task<bool> Guardar(T entidad);
    Task<T?> Buscar(TKey id);
    Task<bool> Eliminar(TKey id);
    Task<List<T>> GetList(Expression<Func<T, bool>> criterio);
}
```

Y la lógica interna puede devolver `Result` para los casos de validación y error del dominio.

## Recomendación final

El `Result` pattern debe usarse para manejar errores esperados y caminos de negocio claramente definidos. No reemplaza excepciones para fallos inesperados, sino que complementa el diseño para dejar la API más legible y segura.

- Use `Result` para errores esperados del dominio.
- Use `Exception` para fallos técnicos o inesperados.
- Use `Map`, `Bind` y `Match` para mantener el flujo limpio.
- Use `OnSuccess`, `OnFailure`, `Tap` y `Finally` para efectos y trazabilidad.
- Use `Try` para encapsular código que puede lanzar excepciones.

---

## Conclusión

`Aplicada1.Core` combina una base de servicios con un Result pattern robusto para convertir el manejo de errores del código en un flujo más claro, más controlado y más fácil de mantener en proyectos académicos y profesionales.
