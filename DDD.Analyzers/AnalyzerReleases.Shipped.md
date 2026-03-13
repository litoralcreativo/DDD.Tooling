; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DDD001 | DDD.Design | Error | Entity must have EntityId property
DDD002 | DDD.Design | Error | AggregateRoot must have EntityId property
DDD003 | DDD.Design | Error | EntityId attribute must only be applied to properties
DDD004 | DDD.Design | Warning | ValueObject must be immutable
DDD005 | DDD.Design | Error | Cannot use Entity and ValueObject simultaneously
DDD006 | DDD.Design | Error | Cannot use AggregateRoot and ValueObject simultaneously
DDD007 | DDD.Design | Warning | ValueObject must override Equals
DDD008 | DDD.Design | Warning | ValueObject must override GetHashCode
