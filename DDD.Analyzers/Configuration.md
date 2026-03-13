# Configuración de Analizadores DDD

Este archivo permite personalizar el comportamiento de los analizadores DDD.

## Severidades Disponibles

- `none` - Desactivado
- `silent` - No muestra nada
- `suggestion` - Sugerencia (línea punteada)
- `warning` - Advertencia (amarillo)
- `error` - Error (rojo, bloquea compilación)

## Configuración Global

Crea un archivo `.editorconfig` en la raíz de tu proyecto:

```ini
# Configuración de analizadores DDD
root = true

[*.cs]

# DDD001: Entity debe tener EntityId
dotnet_diagnostic.DDD001.severity = error

# DDD002: AggregateRoot debe tener EntityId
dotnet_diagnostic.DDD002.severity = error

# DDD003: EntityId solo en propiedades
dotnet_diagnostic.DDD003.severity = error

# DDD004: ValueObject debe ser inmutable
dotnet_diagnostic.DDD004.severity = warning

# DDD005: No puede ser Entity y ValueObject
dotnet_diagnostic.DDD005.severity = error

# DDD006: No puede ser AggregateRoot y ValueObject
dotnet_diagnostic.DDD006.severity = error
```

## Desactivar reglas específicas

```ini
# Desactivar validación de inmutabilidad en ValueObjects
dotnet_diagnostic.DDD004.severity = none
```

## Configuración por archivo

```csharp
#pragma warning disable DDD004 // Desactiva DDD004 para este archivo
[ValueObject]
public class LegacyValueObject
{
    public string Value { get; set; }
}
#pragma warning restore DDD004
```

## Configuración por región

```csharp
public class MyClass
{
#pragma warning disable DDD001
    [Entity]
    public class TemporaryEntity
    {
        // Temporalmente sin EntityId
    }
#pragma warning restore DDD001
}
```
