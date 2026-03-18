; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.2.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|---------
DDD017 | DDD.Design | Error | AggregateRoot debe implementar IAggregateRoot&lt;TId&gt;
DDD018 | DDD.Design | Error | Entity debe implementar IEntity&lt;TId&gt;
DDD019 | DDD.Design | Error | DomainEvent debe implementar IDomainEvent
DDD020 | DDD.Design | Error | ValueObject debe heredar de ValueObject base class

## Release 1.1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|---------
DDD014 | DDD.Design | Error | DomainEvent debe ser inmutable
DDD015 | DDD.Design | Warning | DomainEvent debe tener propiedad OccurredOn
DDD016 | DDD.Design | Warning | DomainEvent debe declarar su Bounded Context

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|---------
DDD001 | DDD.Design | Error | Entity debe tener una propiedad con EntityId
DDD002 | DDD.Design | Error | AggregateRoot debe tener una propiedad con EntityId
DDD003 | DDD.Design | Error | EntityId solo puede aplicarse a propiedades
DDD004 | DDD.Design | Warning | ValueObject debe ser inmutable (accessors init son validos)
DDD005 | DDD.Design | Error | Una clase no puede ser Entity y ValueObject simultaneamente
DDD006 | DDD.Design | Error | Una clase no puede ser AggregateRoot y ValueObject simultaneamente
DDD007 | DDD.Design | Warning | ValueObject debe sobrescribir Equals
DDD008 | DDD.Design | Warning | ValueObject debe sobrescribir GetHashCode
DDD009 | DDD.Design | Info | Entity deberia usar Factory Method
DDD010 | DDD.Design | Warning | Tipo DDD debe declarar su Bounded Context
DDD011 | DDD.Design | Error | No referencias directas entre Bounded Contexts
DDD012 | DDD.Design | Warning | Uso interno de tipo de otro Bounded Context
DDD013 | DDD.Design | Error | Entity/AggregateRoot no puede tener mas de un EntityId