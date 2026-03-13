; Unshipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DDD009 | DDD.Design | Info | Entity or AggregateRoot with public constructor should use a Factory Method
DDD010 | DDD.Design | Warning | Entity/AggregateRoot/ValueObject must declare its Bounded Context
DDD011 | DDD.Design | Error | No direct references between different Bounded Contexts
DDD012 | DDD.Design | Info | Internal private field uses a type from another Bounded Context
