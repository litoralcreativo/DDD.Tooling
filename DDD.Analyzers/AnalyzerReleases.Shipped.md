; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0.0

### New Rules

| Rule ID | Category | Severity | Notes                                                                             |
| ------- | -------- | -------- | --------------------------------------------------------------------------------- |
| DDD001  | Design   | Error    | Entity debe tener al menos una propiedad decorada con [EntityId]                  |
| DDD002  | Design   | Error    | AggregateRoot debe tener al menos una propiedad decorada con [EntityId]           |
| DDD004  | Design   | Warning  | ValueObject no debe tener propiedades con setters públicos (debe ser inmutable)   |
| DDD005  | Design   | Error    | Una clase no puede tener múltiples atributos DDD simultáneamente                  |
| DDD006  | Design   | Error    | Una clase no puede ser Entity y AggregateRoot al mismo tiempo                     |
| DDD007  | Design   | Warning  | ValueObject debe sobrescribir el método Equals(object) para comparación por valor |
| DDD008  | Design   | Warning  | ValueObject debe sobrescribir el método GetHashCode() consistente con Equals      |

### New Code Fixes

| Rule ID        | Code Fix Title               | Notes                                                                                                                         |
| -------------- | ---------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| DDD007, DDD008 | Agregar Equals y GetHashCode | Genera automáticamente métodos Equals y GetHashCode con manejo inteligente de tipos (value types, nullables, reference types) |
