# Roadmap - DDD.Tooling

## ✅ Implementado (v1.0)

- [x] **DDD001** - Validar que Entity tenga EntityId
- [x] **DDD002** - Validar que AggregateRoot tenga EntityId
- [x] **DDD004** - Validar inmutabilidad de ValueObjects
- [x] **DDD005** - Prevenir Entity + ValueObject
- [x] **DDD006** - Prevenir AggregateRoot + ValueObject

## 🚧 Próximas Implementaciones

### Fase 2 - Code Fixes (Auto-correcciones)

- [ ] **Code Fix para DDD001/DDD002**: Agregar automáticamente propiedad `[EntityId] public Guid Id { get; private set; }`
- [ ] **Code Fix para DDD004**: Convertir setters públicos a `{ get; }` o `{ get; private set; }`
- [ ] **Code Fix para DDD005/DDD006**: Ofrecer remover uno de los atributos conflictivos

### Fase 3 - Analizadores Adicionales

#### DDD003 - EntityId solo en propiedades

```csharp
[Entity]
public class Product
{
    [EntityId]
    private Guid _id;  // ❌ Error: EntityId debe estar en propiedad, no campo
}
```

#### DDD007 - Constructor validations

```csharp
[Entity]
public class Order
{
    [EntityId]
    public Guid Id { get; private set; }

    public Order() { }  // ❌ Warning: Entity debería tener constructor privado/protected
}
```

#### DDD008 - ValueObject debe implementar Equals/GetHashCode

```csharp
[ValueObject]
public class Money
{
    public decimal Amount { get; }
    public string Currency { get; }

    // ❌ Warning: Falta override de Equals y GetHashCode
}
```

#### DDD009 - AggregateRoot no debe exponer colecciones mutables

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

#### DDD010 - Domain Events solo en AggregateRoot

```csharp
[Entity]
public class OrderItem
{
    public void RaiseDomainEvent() { }  // ❌ Warning: Solo AggregateRoot debe lanzar eventos
}
```

#### DDD011 - Validar nombres de EntityId

```csharp
[Entity]
public class Product
{
    [EntityId]
    public Guid Identifier { get; private set; }  // ⚠️ Warning: Considere usar 'ProductId' o 'Id'
}
```

#### DDD012 - Repository pattern

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
