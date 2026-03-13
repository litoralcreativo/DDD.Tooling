# Roadmap - DDD.Tooling

## ✅ Implementado (v1.0)

- [x] **DDD001** - Validar que Entity tenga EntityId ❌ Error
    - [x] Code Fix: agrega `[EntityId] public Guid Id { get; private set; }` automáticamente
- [x] **DDD002** - Validar que AggregateRoot tenga EntityId ❌ Error
    - [x] Code Fix: agrega `[EntityId] public Guid Id { get; private set; }` automáticamente
- [x] **DDD003** - EntityId solo en propiedades, no en campos ❌ Error
    - [x] Code Fix: convierte el campo en propiedad automáticamente
- [x] **DDD004** - Validar inmutabilidad de ValueObjects ⚠️ Warning
    - [x] Code Fix: convierte setter público a `{ get; private set; }` o `{ get; init; }`
    - [x] Los accessors `init` no disparan DDD004 (son válidos en ValueObjects)
- [x] **DDD005** - Prevenir Entity + ValueObject simultáneamente ❌ Error
    - [x] Code Fix: elimina el atributo conflictivo (preserva separadores correctamente)
- [x] **DDD006** - Prevenir AggregateRoot + ValueObject simultáneamente ❌ Error
    - [x] Code Fix: elimina el atributo conflictivo (preserva separadores correctamente)
- [x] **DDD007** - ValueObject debe sobrescribir Equals ⚠️ Warning
    - [x] Code Fix: genera `Equals` con comparación de todas las propiedades
- [x] **DDD008** - ValueObject debe sobrescribir GetHashCode ⚠️ Warning
    - [x] Code Fix: genera `GetHashCode` con manejo inteligente de tipos

## ✅ Implementado (v1.1)

- [x] **DDD009** - Entity/AggregateRoot debería usar Factory Method ℹ️ Info
    - [x] Code Fix (3 escenarios): hacer constructor privado + agregar `Create` estático / agregar `static` a método existente
- [x] **DDD010** - Entity/AggregateRoot/ValueObject debe declarar su Bounded Context ⚠️ Warning
    - [x] Code Fix: agrega `[BoundedContext("NombreBC")]` automáticamente
    - [x] Soporte para `[SharedKernel]` como alternativa
- [x] **DDD011** - No referencias directas entre Bounded Contexts ❌ Error
    - [x] Mensajes diferenciados: AggregateRoot / Entity interna / ValueObject
    - [x] Soporte para colecciones genéricas (`List<T>`, `IReadOnlyCollection<T>`, etc.)
    - [x] Code Fix: tipo simple (`Course` → `Guid CourseId`) y colección (`List<Course>` → `List<Guid> CourseIds`)
    - [x] Code Fix elimina `using` huérfano y agrega `using System` si falta
- [x] **DDD012** - Miembro privado usa tipo de otro Bounded Context ⚠️ Warning
    - [x] Detecta campos y propiedades privadas/protegidas
    - [ ] Code Fix: sin fix por decisión de diseño (requiere juicio del desarrollador)

## ✅ Implementado (v1.2)

- [x] **DDD013** - Solo puede haber un `[EntityId]` por clase ❌ Error
    - [x] Detecta múltiples propiedades con `[EntityId]` en la misma entidad o agregado
    - [ ] Code Fix: sin fix por decisión de diseño (¿cuál ID es el correcto?)

## ✅ Implementado (v1.3) — Testing

- [x] **74 tests unitarios** cubriendo todos los analizadores y Code Fixes
    - [x] Fase 3 — Tests para DDD001–DDD013 (55 tests, todos verdes)
    - [x] Fase 4 — Tests de Code Fixes para DDD001–DDD011 (19 tests adicionales, todos verdes)
    - [x] Stack: xUnit + `Microsoft.CodeAnalysis.CSharp.Analyzer.Testing` + `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing`

## 🚧 Próximas Implementaciones

### Packaging NuGet

- [ ] Configurar `DDD.Abstractions.csproj` para publicar `DDD.Tooling.Abstractions`
- [ ] Configurar `DDD.Analyzers.csproj` para publicar `DDD.Tooling.Analyzers`
- [ ] Publicar en NuGet.org
- [ ] Logo y branding

### CI/CD

- [ ] GitHub Actions para build + test en cada PR
- [ ] GitHub Actions para publicar NuGet en cada release

### Documentación adicional

- [ ] Wiki con patrones DDD comunes
- [ ] Video tutoriales

### Fase 4 - Integración con IDEs

- [ ] Snippets para VS Code
- [ ] Snippets para Visual Studio
- [ ] Plantillas de Item Templates para Entity, ValueObject, AggregateRoot
- [ ] Extension VS Code con comandos personalizados

### Fase 5 - Analizadores Adicionales

#### DDD014 - Repository pattern

```csharp
// Validar que los repositorios solo trabajen con AggregateRoots
public interface IProductRepository  // ⚠️ Warning: Product no es AggregateRoot
{
    Product GetById(Guid id);
}
```

### Fase 6 - Características Avanzadas

- [ ] Análisis de dependencias entre Aggregates
- [ ] Detector de Anemic Domain Model
- [ ] Validación de arquitectura hexagonal/clean
- [ ] Métricas de complejidad del dominio
- [ ] Generación automática de diagramas de dominio

## 💡 Ideas Futuras

- Integración con ArchUnit para validaciones arquitecturales
- Soporte para F# (Algebraic Data Types)
- Plugin para Rider
- Dashboard con métricas del dominio
- AI-powered suggestions para refactoring

## 🤝 Contribuciones

¿Tienes ideas para nuevas reglas o mejoras?

1. Abre un issue describiendo la regla DDD que quieres validar
2. Proporciona ejemplos de código correcto e incorrecto
3. Indica la severidad sugerida (error/warning/suggestion)

---

**Última actualización**: Marzo 2026
