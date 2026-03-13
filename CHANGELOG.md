# 📋 Changelog - DDD.Tooling

Todas las versiones y cambios notables del proyecto se documentan aquí.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

---

## [1.1.0] - 2026-03-13

### ✨ Agregado

#### Atributos

- **`[BoundedContext("Nombre")]`** - Marca la clase con el nombre de su Bounded Context
- **`[SharedKernel]`** - Marca la clase como parte del Shared Kernel (accesible desde cualquier BC)

#### Analizadores

- **DDD009** - Entity/AggregateRoot debería usar Factory Method (Info)
    - Detecta constructor público sin factory method estático
    - Detecta factory method de instancia (no estático) que devuelve la clase
    - El patrón correcto requiere: factory method estático público + constructor privado/internal
    - Constructor `internal` + factory method estático = patrón válido (uso en mismo assembly)

- **DDD010** - Entity/AggregateRoot/ValueObject debe declarar su Bounded Context (Warning)
    - Detecta clases DDD sin `[BoundedContext]` ni `[SharedKernel]`
    - Prerequisito obligatorio para que funcione la regla DDD011

- **DDD011** - No referencias directas entre Bounded Contexts (Error)
    - Detecta propiedades públicas que referencian tipos de otro BC
    - Soporta tipos simples (`Course`) y colecciones genéricas (`List<Course>`, `IReadOnlyCollection<Course>`, etc.)
    - Mensajes distintos según el tipo referenciado: AggregateRoot / Entity interna / ValueObject

- **DDD012** - Miembro privado usa tipo de otro Bounded Context (Warning)
    - Detecta campos y propiedades privadas/protegidas que referencian tipos de otro BC
    - No tiene Code Fix (requiere decisión de diseño)

#### Code Fixes

- **DDD009** - `EntityFactoryMethodCodeFixProvider` con 3 escenarios:
    - **Escenario 1**: Constructor público sin factory method → hace privado el constructor + agrega `Create` estático
    - **Escenario 2**: Constructor privado/internal sin factory method estático → agrega `Create` estático
    - **Escenario 3**: Método `Create` existente no estático → agrega modificador `static`

- **DDD010** - `BoundedContextDeclarationCodeFixProvider`:
    - Agrega `[BoundedContext("NombreBC")]` encima de la clase

- **DDD011** - `CrossBoundedContextReferenceCodeFixProvider` con 2 casos:
    - **Tipo simple**: `public Course Course { get; set; }` → `public Guid CourseId { get; set; }`
    - **Colección genérica**: `public List<Course> Courses { get; set; }` → `public List<Guid> CourseIds { get; set; }`
    - Infiere el tipo del Id desde el atributo `[EntityId]` (fallback: `Guid`)
    - Elimina el `using` huérfano del BC referenciado si ya no es necesario
    - Agrega `using System` si falta (para `Guid`)

#### TestDomain reorganizado

- Reorganizado en carpetas por Bounded Context: `Catalog/`, `StudentManagement/`, `SharedKernel/`
- `Course.cs` → `Catalog/` con `[BoundedContext("Catalog")]`
- `Student.cs` → `StudentManagement/` con `[BoundedContext("StudentManagement")]`
- `Address.cs` → `SharedKernel/` con `[SharedKernel]`

#### Documentación

- 📖 `README.md` - Documentación principal
- ️🗺️ `ROADMAP.md` - Roadmap del proyecto
- 📋 `CHANGELOG.md` - Este archivo
- 🎬 `DEMO.md` - Demo completo con ejemplos
- 🚀 `QUICKSTART.md` - Guía rápida de inicio

### 🔧 Cambiado

- Lógica del analizador DDD009: el patrón correcto ahora requiere **ambas** condiciones: factory method estático **Y** constructor no público (no solo una de las dos)
- Renombrado `DddAttributeConflictsAnalyzer` → `DddAttributeUsageAnalyzer` (refleja mejor que detecta DDD003, DDD005 y DDD006)

---

## [1.0.0] - 2026-03-12

### ✨ Agregado

#### Analizadores

- **DDD001** - Entity debe tener EntityId (Error)
- **DDD002** - AggregateRoot debe tener EntityId (Error)
- **DDD004** - ValueObject debe ser inmutable (Warning)
- **DDD005** - No usar múltiples atributos DDD (Error)
- **DDD006** - No usar Entity y AggregateRoot juntos (Error)
- **DDD007** - ValueObject debe sobrescribir Equals (Warning)
- **DDD008** - ValueObject debe sobrescribir GetHashCode (Warning)

#### Code Fixes (Quick Fixes)

- **DDD001/DDD002** - Agregar propiedad Id con [EntityId] automáticamente
    - Genera propiedad `public Guid Id { get; private set; }`
    - Agrega atributo `[EntityId]` automáticamente
    - Agrega `using DDD.Abstractions` si no existe
    - Posiciona la propiedad al inicio de la clase
- **DDD007/DDD008** - Agregar Equals y GetHashCode automáticamente
    - Manejo inteligente de tipos:
        - Value types (int, decimal, DateTime) → sin operador `?.`
        - Nullable types (int?, decimal?) → con operador `?.`
        - Reference types (string, classes) → con operador `?.`
    - Genera comparaciones para todas las propiedades públicas
    - Implementa hash code correcto con algoritmo seguro

#### Atributos DDD

- `[Entity]` - Marca clases como entidades
- `[EntityId]` - Marca propiedades como identificadores
- `[AggregateRoot]` - Marca clases como aggregate roots
- `[ValueObject]` - Marca clases como value objects

#### Infraestructura

- Script PowerShell `reload-analyzers.ps1` para recargar analizadores
- Configuración `.editorconfig` para el proyecto
- `.gitignore` completo para proyectos .NET
- Soporte para netstandard2.0 (compatible con .NET Framework 4.7.2+)

#### Documentación

- 📖 `README.md` - Documentación principal
- ️ `ROADMAP.md` - Roadmap del proyecto
- � `CHANGELOG.md` - Este archivo
- 🎬 `DEMO.md` - Demo completo con ejemplos
- � `QUICKSTART.md` - Guía rápida de inicio

#### Ejemplos

- `TestDomain/Currency.cs` - ValueObject completo con validaciones
- `TestDomain/Address.cs` - ValueObject simple
- `TestDomain/Student.cs` - Entity con EntityId
- `TestDomain/Course.cs` - AggregateRoot con EntityId
- `TestDomain/Examples/Price.cs` - ValueObject con tipos mixtos
- `TestDomain/Examples/MissingEntityId.cs` - Entities sin EntityId para probar Code Fix
- `TestDomain/Examples/InvalidValueObject.cs` - Ejemplos de errores
- `TestDomain/Examples/IncompleteValueObject.cs` - Para testing

### 🔧 Cambiado

- Migrado de C# 9 records a classes para compatibilidad con netstandard2.0
- Mejorado generación de GetHashCode para manejar diferentes tipos correctamente

### 🐛 Corregido

- Error CS0023 cuando se usaba `?.` con tipos de valor (decimal, int, etc.)
- Warning RS1036 agregando `EnforceExtendedAnalyzerRules=true`
- Problemas con recarga de analizadores en VS Code

### 🧹 Limpieza

- Removidos 84 archivos de build (bin/, obj/, .vs/)
- Agregado `.gitignore` completo
- Limpieza de repositorio Git

---

## [Unreleased] - Próximas versiones

### 🎯 Planeado para v1.2.0

#### Code Fixes

- **DDD004** - Convertir setter público a privado/init
- **DDD005/006** - Remover atributos conflictivos

#### Infraestructura

- Publicación en NuGet
- Paquete analyzer como .nupkg
- CI/CD con GitHub Actions
- Unit tests para analizadores

---

## 📊 Estadísticas de la Versión Actual

### v1.1.0

- **Analizadores**: 7 clases, 12 reglas (DDD001-DDD012)
- **Code Fixes**: 5 providers
    - `EntityIdCodeFixProvider` (DDD001/002)
    - `ValueObjectEqualsCodeFixProvider` (DDD007/008)
    - `EntityFactoryMethodCodeFixProvider` (DDD009)
    - `BoundedContextDeclarationCodeFixProvider` (DDD010)
    - `CrossBoundedContextReferenceCodeFixProvider` (DDD011)
- **Atributos**: 6 (Entity, EntityId, AggregateRoot, ValueObject, BoundedContext, SharedKernel)
- **Ejemplos**: 3 BCs en TestDomain (Catalog, StudentManagement, SharedKernel)
- **Documentación**: 12+ archivos markdown

---

## 🔗 Enlaces

- **Repositorio**: [GitHub](https://github.com/tu-usuario/DDD.Tooling) _(pendiente)_
- **NuGet Package**: _(pendiente)_
- **Issues**: [GitHub Issues](https://github.com/tu-usuario/DDD.Tooling/issues) _(pendiente)_
- **Wiki**: [GitHub Wiki](https://github.com/tu-usuario/DDD.Tooling/wiki) _(pendiente)_

---

## 🎓 Convenciones de Versionado

Este proyecto usa [Semantic Versioning](https://semver.org/):

- **MAJOR** (X.0.0) - Cambios incompatibles con versiones anteriores
- **MINOR** (0.X.0) - Nueva funcionalidad compatible con versiones anteriores
- **PATCH** (0.0.X) - Correcciones de bugs compatibles con versiones anteriores

### Ejemplos:

- Nuevo analizador → MINOR version
- Cambiar severidad de Error a Warning → MAJOR version
- Fix de bug en Code Fix → PATCH version
- Nueva documentación → PATCH version

---

## 📝 Formato de Commits

Usamos [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` - Nueva funcionalidad
- `fix:` - Corrección de bug
- `docs:` - Cambios en documentación
- `style:` - Formato, falta de punto y coma, etc.
- `refactor:` - Refactorización de código
- `test:` - Agregar tests
- `chore:` - Actualización de tareas de build, etc.

---

**Última actualización**: 13 de Marzo 2026
