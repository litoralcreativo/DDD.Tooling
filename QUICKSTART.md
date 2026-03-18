# 🚀 Quick Start - DDD.Tooling

## ⚡ Inicio Rápido en 5 Minutos

### Paso 1: Agrega los paquetes NuGet (30 segundos)

```bash
dotnet add package DDD.Tooling.Abstractions
dotnet add package DDD.Tooling.Analyzers
```

O en tu proyecto `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="DDD.Tooling.Abstractions" Version="1.2.0" />
  <PackageReference Include="DDD.Tooling.Analyzers" Version="1.2.0">
    <PrivateAssets>all</PrivateAssets>
    <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
  </PackageReference>
</ItemGroup>
```

### Paso 2: Crea tu primera Entity (2 minutos)

```csharp
using DDD.Abstractions;
using System;

[Entity]
public class Customer
{
    [EntityId]
    public Guid CustomerId { get; private set; }

    public string Name { get; set; }
    public string Email { get; set; }

    public Customer(Guid id, string name, string email)
    {
        CustomerId = id;
        Name = name;
        Email = email;
    }
}
```

### Paso 3: Crea tu primer ValueObject (2 minutos)

```csharp
using DDD.Abstractions;

[ValueObject]
public class EmailAddress
{
    public string Value { get; }

    public EmailAddress(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty");

        Value = value;
    }

    public override bool Equals(object obj) =>
        obj is EmailAddress other && Value == other.Value;

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;
}
```

### Paso 4: Compila y verifica (30 segundos)

```bash
dotnet build
```

✅ Si todo está correcto, compilará sin errores!

---

## 🎯 Casos de Uso Comunes

### ✅ Entity Correcto

```csharp
[Entity]
public class Order
{
    [EntityId]
    public Guid OrderId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Order(Guid id)
    {
        OrderId = id;
        CreatedAt = DateTime.UtcNow;
    }
}
```

### ❌ Entity Incorrecto (Sin EntityId)

```csharp
[Entity]
public class Order  // ❌ Error DDD001
{
    public DateTime CreatedAt { get; private set; }
}
```

**Error:** `Error DDD001: La clase 'Order' está decorada con [Entity] pero no tiene ninguna propiedad decorada con [EntityId]`

---

### ✅ ValueObject Correcto (Inmutable)

```csharp
[ValueObject]
public class Money
{
    public decimal Amount { get; }  // ✅ Solo getter
    public string Currency { get; }

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
```

### ⚠️ ValueObject con Warning (Mutable)

```csharp
[ValueObject]
public class Money
{
    public decimal Amount { get; set; }  // ⚠️ Warning DDD004
    public string Currency { get; set; }
}
```

**Warning:** `Warning DDD004: La propiedad 'Amount' en el ValueObject 'Money' tiene un setter público. Los ValueObjects deben ser inmutables`

---

### ✅ AggregateRoot Correcto

```csharp
[AggregateRoot]
public class ShoppingCart
{
    [EntityId]
    public Guid CartId { get; private set; }

    private readonly List<CartItem> _items = new();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public ShoppingCart(Guid id)
    {
        CartId = id;
    }

    public void AddItem(CartItem item) => _items.Add(item);
}
```

---

## 🔍 Cheatsheet de Atributos y Base Classes

| Atributo / Base class | Uso                    | Requiere EntityId | Debe ser Inmutable  |
| --------------------- | ---------------------- | ----------------- | ------------------- |
| `[Entity]`            | Clase con identidad    | ✅ Sí             | ❌ No               |
| `Entity<TId>`         | Base class para Entity | automático        | ❌ No               |
| `[AggregateRoot]`     | Raíz de agregado       | ✅ Sí             | ❌ No               |
| `AggregateRoot<TId>`  | Base class para AR     | automático        | ❌ No               |
| `[ValueObject]`       | Objeto sin identidad   | ❌ No             | ✅ Sí (recomendado) |
| `ValueObject`         | Base class para VO     | ❌ No             | ✅ Sí (recomendado) |
| `[EntityId]`          | Identificador único    | N/A               | ❌ No               |
| `[DomainEvent]`       | Evento de dominio      | ❌ No             | ✅ Sí (obligatorio) |

---

## 🎨 Patrones Recomendados

### Pattern 1: Entity con Factory Method

```csharp
[Entity]
public class Product
{
    [EntityId]
    public Guid ProductId { get; private set; }
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    private Product() { }  // Constructor privado

    public static Product Create(string name, decimal price)
    {
        return new Product
        {
            ProductId = Guid.NewGuid(),
            Name = name,
            Price = price
        };
    }
}
```

### Pattern 2: ValueObject con Validación

```csharp
[ValueObject]
public class Age
{
    public int Value { get; }

    public Age(int value)
    {
        if (value < 0 || value > 150)
            throw new ArgumentException("Invalid age");

        Value = value;
    }

    public static implicit operator int(Age age) => age.Value;
    public static explicit operator Age(int value) => new Age(value);
}
```

### Pattern 3: Aggregate Root con Domain Events

```csharp
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid OrderId { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private readonly List<object> _domainEvents = new();
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    public Order(Guid id)
    {
        OrderId = id;
        _domainEvents.Add(new OrderCreatedEvent(id));
    }

    public void AddItem(OrderItem item)
    {
        _items.Add(item);
        _domainEvents.Add(new ItemAddedEvent(OrderId, item.ProductId));
    }
}
```

---

## 🛠️ Troubleshooting

### Problema: El analizador no funciona

**Solución:**

1. Verifica que la referencia tenga `OutputItemType="Analyzer"` y `ReferenceOutputAssembly="false"`
2. Limpia y reconstruye: `dotnet clean && dotnet build`
3. Cierra y abre Visual Studio / VS Code

### Problema: Muchos warnings en proyecto legacy

**Solución:** Desactiva temporalmente las reglas en `.editorconfig`:

```ini
[*.cs]
dotnet_diagnostic.DDD004.severity = none  # Desactiva warning de inmutabilidad
```

### Problema: Quiero ignorar un warning específico

**Solución:** Usa `#pragma warning disable`:

```csharp
#pragma warning disable DDD004
[ValueObject]
public class LegacyValue { public string Value { get; set; } }
#pragma warning restore DDD004
```

---

## 📖 Recursos Adicionales

- **README.md** - Documentación completa
- **DEMO.md** - Ejemplos visuales
- **ROADMAP.md** - Funcionalidades futuras
- **Configuration.md** - Configuración avanzada

---

## 🎉 ¡Listo!

Ahora tienes un sistema completo de validación DDD funcionando.

**Siguiente:** Lee el `README.md` para conocer todas las reglas disponibles.

---

**Tips Pro:**

- 💡 Usa los atributos para documentar tu intención de diseño
- 🔄 Los analizadores te ayudan durante refactoring
- 📚 Los mensajes de error son educativos
- 🚀 Compila frecuentemente para detectar problemas temprano

**¡Happy DDD Modeling!** 🎯
