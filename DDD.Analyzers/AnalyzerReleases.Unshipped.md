; Unshipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.1.0 (En Desarrollo)

### Planned New Rules

| Rule ID | Category | Severity | Notes                                                           |
| ------- | -------- | -------- | --------------------------------------------------------------- |
| DDD003  | Design   | Warning  | EntityId solo debe aplicarse a propiedades, no a campos         |
| DDD009  | Design   | Warning  | AggregateRoot no debe exponer colecciones mutables públicamente |
| DDD010  | Design   | Info     | Domain Events solo deben usarse en AggregateRoots               |

### Planned New Code Fixes

| Rule ID | Code Fix Title                 | Notes                                                  |
| ------- | ------------------------------ | ------------------------------------------------------ |
| DDD001  | Agregar propiedad EntityId     | Genera automáticamente una propiedad Id con [EntityId] |
| DDD002  | Agregar propiedad EntityId     | Genera automáticamente una propiedad Id con [EntityId] |
| DDD004  | Convertir setter a privado     | Cambia setters públicos a privados o init              |
| DDD005  | Remover atributo conflictivo   | Remueve uno de los atributos DDD en conflicto          |
| DDD006  | Remover Entity o AggregateRoot | Permite elegir cuál atributo mantener                  |
