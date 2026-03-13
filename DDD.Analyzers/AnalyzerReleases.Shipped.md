; Shipped analyzer releases
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

## Release 1.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
DDD001 | Design | Error | Entity must have EntityId property - includes Code Fix
DDD002 | Design | Error | AggregateRoot must have EntityId property - includes Code Fix
DDD004 | Design | Warning | ValueObject must be immutable
DDD005 | Design | Error | Cannot use multiple DDD attributes simultaneously
DDD006 | Design | Error | Cannot be Entity and AggregateRoot simultaneously
DDD007 | Design | Warning | ValueObject must override Equals - includes Code Fix
DDD008 | Design | Warning | ValueObject must override GetHashCode - includes Code Fix
