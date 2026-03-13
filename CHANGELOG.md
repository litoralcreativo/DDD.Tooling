# 📋 Changelog - DDD.Tooling

Todas las versiones y cambios notables del proyecto se documentan aquí.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

### 🎯 Planeado para v1.1.0

#### CI/CD

- GitHub Actions para build + test en cada PR
- GitHub Actions para publicar NuGet automáticamente en cada release tag

---

## [1.0.0] - 2026-03-13

Primera release pública. Publicada en NuGet.org como:

- [`DDD.Tooling.Abstractions`](https://www.nuget.org/packages/DDD.Tooling.Abstractions)
- [`DDD.Tooling.Analyzers`](https://www.nuget.org/packages/DDD.Tooling.Analyzers)

### ✨ Agregado

#### Atributos DDD (`DDD.Tooling.Abstractions`)

- `[Entity]` — Marca clases como entidades DDD
- `[EntityId]` — Marca propiedades como identificadores de entidad
- `[AggregateRoot]` — Marca clases como aggregate roots
- `[ValueObject]` — Marca clases como value objects
- `[BoundedContext("Nombre")]` — Declara el Bounded Context al que pertenece la clase
- `[SharedKernel]` — Marca la clase como parte del Shared Kernel (accesible desde cualquier BC)

#### Analizadores (`DDD.Tooling.Analyzers`)

- **DDD001** — Entity debe tener una propiedad con `[EntityId]` (Error)
- **DDD002** — AggregateRoot debe tener una propiedad con `[EntityId]` (Error)
- **DDD003** — `[EntityId]` solo puede aplicarse a propiedades (Error)
- **DDD004** — ValueObject debe ser inmutable; setters públicos no están permitidos (Warning)
    - Los accessors `init` (`{ get; init; }`) son válidos — inmutables tras construcción
- **DDD005** — Una clase no puede ser `[Entity]` y `[ValueObject]` simultáneamente (Error)
- **DDD006** — Una clase no puede ser `[AggregateRoot]` y `[ValueObject]` simultáneamente (Error)
- **DDD007** — ValueObject debe sobrescribir `Equals` (Warning)
- **DDD008** — ValueObject debe sobrescribir `GetHashCode` (Warning)
- **DDD009** — Entity/AggregateRoot debería usar Factory Method (Info)
    - Detecta constructor público sin factory method estático
    - El patrón correcto requiere: factory method `static` público + constructor `private`/`internal`
- **DDD010** — Entity/AggregateRoot/ValueObject debe declarar su Bounded Context (Warning)
    - Prerequisito para que funcione DDD011
- **DDD011** — No referencias directas entre Bounded Contexts (Error)
    - Soporta tipos simples y colecciones genéricas (`List<T>`, `IReadOnlyCollection<T>`, etc.)
- **DDD012** — Miembro privado usa tipo de otro Bounded Context (Warning)
- **DDD013** — Entity/AggregateRoot no puede tener más de un `[EntityId]` (Error)

#### Code Fixes

- **DDD001/002** — `EntityIdCodeFixProvider`: agrega propiedad `public Guid Id { get; private set; }` con `[EntityId]`; agrega `using System;` y `using DDD.Abstractions;` si faltan
- **DDD003** — `EntityIdOnPropertyCodeFixProvider`: mueve `[EntityId]` a la propiedad correcta
- **DDD004/005/006** — `ValueObjectMutabilityCodeFixProvider`:
    - DDD004: convierte setter público en `private set`
    - DDD005/006: elimina el atributo conflictivo de la clase
- **DDD007/008** — `ValueObjectEqualsCodeFixProvider`: genera `Equals` y `GetHashCode` con manejo inteligente de tipos (value types, nullables, reference types)
- **DDD009** — `EntityFactoryMethodCodeFixProvider` con 3 escenarios:
    - Constructor público sin factory method → hace privado el constructor + agrega `Create` estático
    - Constructor privado/internal sin factory method → agrega `Create` estático
    - Método `Create` existente no estático → agrega modificador `static`
- **DDD010** — `BoundedContextDeclarationCodeFixProvider`: agrega `[BoundedContext("NombreBC")]`
- **DDD011** — `CrossBoundedContextReferenceCodeFixProvider`:
    - Tipo simple: `public Course Course` → `public Guid CourseId`
    - Colección: `public List<Course> Courses` → `public List<Guid> CourseIds`
    - Infiere el tipo de Id desde `[EntityId]`; elimina `using` huérfano

#### Tests

- **74 tests unitarios** (100% verdes)
    - 55 tests de analizadores (DDD001–DDD013) con `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing v1.1.2`
    - 19 tests de Code Fixes (DDD001–DDD011) con `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing v1.1.2`

#### Infraestructura

- Target MSBuild `_AddAnalyzersToOutput`: coloca los DLLs en `analyzers/dotnet/cs/` dentro del `.nupkg`
- `DevelopmentDependency=true`: el paquete no genera dependencia transitiva en proyectos consumidores
- Soporte para `netstandard2.0` (compatible con .NET Framework 4.7.2+)
- Script PowerShell `reload-analyzers.ps1` para recargar analizadores en VS Code

---

## 🔗 Enlaces

- **Repositorio**: [github.com/litoralcreativo/DDD.Tooling](https://github.com/litoralcreativo/DDD.Tooling)
- **NuGet — Abstractions**: [DDD.Tooling.Abstractions](https://www.nuget.org/packages/DDD.Tooling.Abstractions)
- **NuGet — Analyzers**: [DDD.Tooling.Analyzers](https://www.nuget.org/packages/DDD.Tooling.Analyzers)
- **Issues**: [github.com/litoralcreativo/DDD.Tooling/issues](https://github.com/litoralcreativo/DDD.Tooling/issues)

---

## 🎓 Convenciones de Versionado

Este proyecto usa [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0) — Cambios incompatibles con versiones anteriores (ej: cambiar severidad de Error a Warning)
- **MINOR** (0.X.0) — Nueva funcionalidad compatible (ej: nuevo analizador)
- **PATCH** (0.0.X) — Correcciones de bugs (ej: fix en un Code Fix, nueva documentación)

---

## 📝 Formato de Commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` — Nueva funcionalidad
- `fix:` — Corrección de bug
- `docs:` — Cambios en documentación
- `test:` — Agregar o modificar tests
- `refactor:` — Refactorización sin cambio de comportamiento
- `chore:` — Tareas de build, CI, packaging

---

**Última actualización**: 13 de Marzo 2026
