# Roadmap - DDD.Tooling

## ✅ Implementado (v1.0)

- [x] **DDD001** - Validar que Entity tenga EntityId ❌ Error
  - [x] Code Fix: agrega `[EntityId] public Guid Id { get; private set; }` automáticamente
- [x] **DDD002** - Validar que AggregateRoot tenga EntityId ❌ Error
  - [x] Code Fix: agrega `[EntityId] public Guid Id { get; private set; }` automáticamente
- [x] **DDD003** - EntityId solo en propiedades, no en campos ❌ Error
- [x] **DDD004** - Validar inmutabilidad de ValueObjects ⚠️ Warning
- [x] **DDD005** - Prevenir Entity + ValueObject simultáneamente ❌ Error
- [x] **DDD006** - Prevenir AggregateRoot + ValueObject simultáneamente ❌ Error
- [x] **DDD007** - ValueObject debe sobrescribir Equals ⚠️ Warning
  - [x] Code Fix: genera `Equals` con comparación de todas las propiedades
- [x] **DDD008** - ValueObject debe sobrescribir GetHashCode ⚠️ Warning
  - [x] Code Fix: genera `GetHashCode` con manejo inteligente de tipos
- [x] **DDD009** - Entity/AggregateRoot debería usar Factory Method ℹ️ Info
  - [x] Code Fix (3 escenarios): hacer constructor privado + agregar `Create` estático / agregar `static` a método existente

## 🚧 Próximas Implementaciones

### Fase 2 - Code Fixes Pendientes

- [ ] **Code Fix para DDD004**: Convertir setters públicos a `{ get; }` o `{ get; private set; }`
- [ ] **Code Fix para DDD005/DDD006**: Ofrecer remover uno de los atributos conflictivos

### Fase 3 - Analizadores Adicionales

#### DDD010 - AggregateRoot no debe exponer colecciones mutables

```csharp
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid Id { get; private set; }

    public List<OrderItem> Items { get; set; }  // ❌ Error: No exponer List<T> directamente

    // ✅ Correcto: IReadOnlyCollection<OrderItem>
}
```

#### DDD011 - Domain Events solo en AggregateRoot

```csharp
[Entity]
public class OrderItem
{
    public void RaiseDomainEvent() { }  // ❌ Warning: Solo AggregateRoot debe lanzar eventos
}
```

#### DDD012 - Validar nombres de EntityId

```csharp
[Entity]
public class Product
{
    [EntityId]
    public Guid Identifier { get; private set; }  // ⚠️ Warning: Considere usar 'ProductId' o 'Id'
}
```

#### DDD013 - Repository pattern

```csharp
// Validar que los repositorios solo trabajen con AggregateRoots
public interface IProductRepository  // ⚠️ Warning: Product no es AggregateRoot
{
    Product GetById(Guid id);
}
```

### Fase 4 - Testing Framework

- [ ] Crear proyecto `DDD.Analyzers.Tests`
- [ ] Tests unitarios para cada analizador
- [ ] Tests de verificación de Code Fixes
- [ ] Integración con GitHub Actions / Azure Pipelines

### Fase 5 - Documentación y Packaging

- [ ] Generar documentación completa con ejemplos
- [ ] Crear package NuGet público
- [ ] Logo y branding
- [ ] Wiki con patrones DDD comunes
- [ ] Video tutoriales

### Fase 6 - Integración con IDEs

- [ ] Snippets para VS Code
- [ ] Snippets para Visual Studio
- [ ] Plantillas de Item Templates para Entity, ValueObject, AggregateRoot
- [ ] Extension VS Code con comandos personalizados

### Fase 7 - Características Avanzadas

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
