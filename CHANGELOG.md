# 📋 Changelog - DDD.Tooling

Todas las versiones y cambios notables del proyecto se documentan aquí.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [Unreleased]

---

## [1.1.0] - 2026-03-22

### ✨ Agregado

#### Atributo DDD (`DDD.Tooling.Abstractions`)

- `[DomainEvent]` — Marca clases como Domain Events DDD. Activa las reglas DDD014, DDD015 y DDD016.

#### Reglas de análisis (`DDD.Tooling.Analyzers`)

- **DDD014** (`Error`) — `DomainEvent` debe ser inmutable: todas las propiedades deben tener `get` sin setter público (se permiten `private set` e `init`).
- **DDD015** (`Warning`) — `DomainEvent` debe tener una propiedad `OccurredOn` de tipo `DateTime` o `DateTimeOffset`.
- **DDD016** (`Warning`) — `DomainEvent` debe declarar su `Bounded Context` con `[BoundedContext("Nombre")]` o `[SharedKernel]`.

#### Code Fixes

- **DDD014** — `DomainEventImmutabilityCodeFixProvider`: convierte el setter público a `private set` o `init` (C# 9+).
- **DDD015** — `DomainEventOccurredOnCodeFixProvider`: agrega automáticamente `public DateTime OccurredOn { get; }` como primer miembro. Añade `using System;` si no existe.
- **DDD016** — `BoundedContextDeclarationCodeFixProvider` extendido: ahora también maneja `DomainEvent` (aplica `[BoundedContext("BC")]` o `[SharedKernel]` inferido del namespace).

---

## [1.0.1] - 2026-03-17

### 🔧 Cambiado

- **DDD007/008** — Eliminado `ValueObjectEqualsCodeFixProvider`: el Code Fix generado era incorrecto (no comparaba las propiedades en `Equals`, y `GetHashCode` devolvía siempre el mismo valor). DDD007 y DDD008 permanecen como **warnings sin Quick Fix** — los IDEs modernos (Visual Studio, Rider) ofrecen generar `Equals` y `GetHashCode` con resultados correctos.

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
- **DDD007/008** — DDD007 y DDD008 son warnings sin Quick Fix (ver v1.0.1)
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

- **74 tests unitarios** (100% verdes) — ver v1.0.1 para estado actualizado

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

**Última actualización**: 17 de Marzo 2026
