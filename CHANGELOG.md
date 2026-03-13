# 📋 Changelog - DDD.Tooling

Todas las versiones y cambios notables del proyecto se documentan aquí.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

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

- **DDD007/DDD008** - Agregar Equals y GetHashCode automáticamente
    - Manejo inteligente de tipos:
        - Value types (int, decimal, DateTime) → sin operador `?.`
        - Nullable types (int?, decimal?) → con operador `?.`
        - Reference types (string, classes) → con operador `?.`

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
- 🚀 `QUICKSTART.md` - Guía rápida de inicio
- 🔧 `QUICKFIX_GUIDE.md` - Guía de Quick Fixes
- 🔍 `TYPE_HANDLING.md` - Manejo inteligente de tipos
- 🗺️ `ROADMAP.md` - Roadmap del proyecto
- 📝 `SUMMARY.md` - Resumen de analizadores
- 🎬 `DEMO.md` - Demo completo con ejemplos
- 🔄 `ANALYZER_RELOAD.md` - Cómo recargar analizadores
- 📋 `CHANGELOG.md` - Este archivo
- 🧹 `GIT_CLEANUP.md` - Guía de limpieza de Git

#### Ejemplos

- `TestDomain/Currency.cs` - ValueObject completo con validaciones
- `TestDomain/Address.cs` - ValueObject simple
- `TestDomain/Student.cs` - Entity con EntityId
- `TestDomain/Course.cs` - AggregateRoot con EntityId
- `TestDomain/Examples/Price.cs` - ValueObject con tipos mixtos
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

### 🎯 Planeado para v1.1.0

#### Analizadores

- **DDD003** - EntityId solo en propiedades, no en campos
- **DDD009** - AggregateRoot no debe exponer colecciones mutables
- **DDD010** - Domain Events solo en AggregateRoots

#### Code Fixes

- **DDD001/002** - Agregar propiedad EntityId automáticamente
- **DDD004** - Convertir setter público a privado/init
- **DDD005/006** - Remover atributos conflictivos

#### Mejoras

- Soporte para `IEquatable<T>` en ValueObjects
- Detección de colecciones inmutables (ImmutableList, ReadOnlyCollection)
- Sugerencias para operadores `==` y `!=` en ValueObjects
- Análisis de constructores en Entities

### 🎯 Planeado para v1.2.0

#### Analizadores

- **DDD011** - Validation rules (Domain invariants)
- **DDD012** - Repository pattern validation
- **DDD013** - Factory pattern validation

#### Infraestructura

- Publicación en NuGet
- Paquete analyzer como .nupkg
- CI/CD con GitHub Actions
- Unit tests para analizadores

---

## 📊 Estadísticas de la Versión Actual

### v1.0.0

- **Analizadores**: 6 reglas (DDD001-DDD008)
- **Code Fixes**: 1 provider (DDD007/008)
- **Atributos**: 4 (Entity, EntityId, AggregateRoot, ValueObject)
- **Ejemplos**: 7 archivos de ejemplo
- **Documentación**: 11 archivos markdown
- **Líneas de código**: ~1,500 líneas
- **Cobertura**: Reglas básicas de DDD

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

**Última actualización**: 12 de Marzo 2026
