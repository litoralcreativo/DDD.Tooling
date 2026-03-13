# Demo Visual - DDD.Tooling Analyzers

## 🎬 Demostración del Sistema

### Escenario 1: Entity sin EntityId ❌

```csharp
using DDD.Abstractions;

[Entity]
public class Invoice
{
    public string Number { get; set; }
    public decimal Amount { get; set; }
}
```

**Resultado en compilación:**

```
Error DDD001: La clase 'Invoice' está decorada con [Entity] pero no tiene ninguna propiedad decorada con [EntityId]
```

### Escenario 2: Agregando EntityId ✅

```csharp
using DDD.Abstractions;
using System;

[Entity]
public class Invoice
{
    [EntityId]
    public Guid InvoiceId { get; private set; }  // ✅ Ahora compila!

    public string Number { get; set; }
    public decimal Amount { get; set; }

    public Invoice(Guid id, string number, decimal amount)
    {
        InvoiceId = id;
        Number = number;
        Amount = amount;
    }
}
```

**Resultado:** ✅ Compila sin errores

---

### Escenario 3: ValueObject mutable ⚠️

```csharp
using DDD.Abstractions;

[ValueObject]
public class Price
{
    public decimal Amount { get; set; }      // ⚠️ Warning
    public string Currency { get; set; }     // ⚠️ Warning
}
```

**Resultado en compilación:**

```
Warning DDD004: La propiedad 'Amount' en el ValueObject 'Price' tiene un setter público. Los ValueObjects deben ser inmutables
Warning DDD004: La propiedad 'Currency' en el ValueObject 'Price' tiene un setter público. Los ValueObjects deben ser inmutables
```

### Escenario 4: ValueObject inmutable ✅

```csharp
using DDD.Abstractions;

[ValueObject]
public class Price
{
    public decimal Amount { get; }       // ✅ Inmutable
    public string Currency { get; }      // ✅ Inmutable

    public Price(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public override bool Equals(object obj)
    {
        return obj is Price other &&
               Amount == other.Amount &&
               Currency == other.Currency;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Amount, Currency);
    }
}
```

**Resultado:** ✅ Compila sin warnings

---

### Escenario 5: Atributos conflictivos ❌

```csharp
using DDD.Abstractions;
using System;

[Entity]
[ValueObject]  // ❌ Error: No puede ser ambos!
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }
}
```

**Resultado en compilación:**

```
Error DDD005: La clase 'Product' está decorada con [Entity] y [ValueObject]. Debe ser uno u otro, no ambos
```

---

### Escenario 6: AggregateRoot completo ✅

```csharp
using DDD.Abstractions;
using System;
using System.Collections.Generic;

[AggregateRoot]
public class ShoppingCart
{
    [EntityId]
    public Guid CartId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private readonly List<CartItem> _items = new List<CartItem>();
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();

    public static ShoppingCart Create(Guid cartId)  // ✅ Factory Method estático
    {
        return new ShoppingCart(cartId);
    }

    private ShoppingCart(Guid cartId)  // ✅ Constructor privado
    {
        CartId = cartId;
        CreatedAt = DateTime.UtcNow;
    }

    public void AddItem(CartItem item)
    {
        _items.Add(item);
    }
}
```

**Resultado:** ✅ Compila perfectamente - Modelo DDD válido!

---

### Escenario 7: Entity con Factory Method ℹ️ DDD009

```csharp
[Entity]
public class Product  // ℹ️ DDD009 - Constructor público sin factory method estático
{
    [EntityId]
    public Guid Id { get; private set; }

    public Product(string name) { ... }  // ⬅️ Constructor público
}
```

**Resultado:**

```
Info DDD009: La clase 'Product' tiene constructor público. Considera usar un Factory Method estático...
```

**Después del Quick Fix:**

```csharp
[Entity]
public class Product  // ✅ Sin DDD009
{
    [EntityId]
    public Guid Id { get; private set; }

    public static Product Create(string name)  // ✅ Factory Method estático
    {
        return new Product(name);
    }

    private Product(string name) { ... }  // ✅ Constructor privado
}
```

---

## 📊 Estadísticas de Validación

Después de compilar el proyecto `TestDomain`:

```
Build succeeded with 1 warning(s) in 1.1s

Warnings:
  - DDD004: InvalidValueObject.cs(13,3) - ValueObject con setter público

Errors:
  - Ninguno (todos los modelos principales son válidos)
```

---

## 🎯 Beneficios

1. **Detección temprana**: Los errores se detectan al escribir código, no en runtime
2. **Consistencia**: Todo el equipo sigue las mismas reglas DDD
3. **Documentación viva**: Los atributos documentan la intención del diseño
4. **Refactoring seguro**: Los analizadores previenen romper invariantes DDD
5. **Educación**: Los mensajes de error enseñan buenas prácticas DDD

---

## 🚀 Próximos Pasos

1. **Tests unitarios** para cada analizador
2. **Publicar NuGet package** para uso público
3. **Nuevas reglas** según el ROADMAP.md (DDD014+)

---

**¿Preguntas?** Abre un issue en el repositorio!
