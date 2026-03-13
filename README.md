# DDD.Tooling

Herramientas de análisis estático para validar reglas de Domain-Driven Design (DDD) en proyectos .NET mediante Roslyn Analyzers.

## 🎯 Objetivo

Proporcionar validación en tiempo de compilación de las reglas y patrones de DDD, ayudando a los desarrolladores a crear modelos de dominio consistentes y correctos.

## 📦 Estructura del Proyecto

```
DDD.Tooling/
├── DDD.Abstractions/           # Atributos DDD base
│   └── Attributes/
│       ├── EntityAttribute.cs
│       ├── EntityIdAttribute.cs
│       ├── AggregateRootAttribute.cs
│       └── ValueObjectAttribute.cs
├── DDD.Analyzers/              # Analizadores Roslyn
│   ├── DiagnosticDescriptors.cs
│   ├── EntityMustHaveEntityIdAnalyzer.cs
│   ├── AggregateRootMustHaveEntityIdAnalyzer.cs
│   ├── ValueObjectImmutabilityAnalyzer.cs
│   └── DddAttributeConflictsAnalyzer.cs
└── TestDomain/                 # Proyecto de prueba
    ├── Course.cs
    ├── Student.cs
    ├── Address.cs
    └── Examples/
        ├── InvalidValueObject.cs
        └── ErrorExamples.cs
```

## 🔍 Reglas Implementadas

### DDD001 - Entity debe tener EntityId ❌ Error

**Descripción**: Todas las clases decoradas con `[Entity]` deben tener al menos una propiedad decorada con `[EntityId]`.

**Ejemplo incorrecto:**

```csharp
[Entity]
public class Product
{
    public string Name { get; set; }  // ❌ Falta [EntityId]
}
```

**Ejemplo correcto:**

```csharp
[Entity]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }  // ✅ Correcto

    public string Name { get; set; }
}
```

---

### DDD002 - AggregateRoot debe tener EntityId ❌ Error

**Descripción**: Todas las clases decoradas con `[AggregateRoot]` deben tener al menos una propiedad decorada con `[EntityId]`.

**Ejemplo correcto:**

```csharp
[AggregateRoot]
public class Order
{
    [EntityId]
    public Guid OrderId { get; private set; }  // ✅ Correcto

    public DateTime CreatedAt { get; private set; }
}
```

---

### DDD004 - ValueObject debe ser inmutable ⚠️ Warning

**Descripción**: Los Value Objects no deben tener setters públicos en sus propiedades para garantizar inmutabilidad.

**Ejemplo incorrecto:**

```csharp
[ValueObject]
public class Money
{
    public decimal Amount { get; set; }  // ❌ Setter público
    public string Currency { get; set; }  // ❌ Setter público
}
```

**Ejemplo correcto:**

```csharp
[ValueObject]
public class Money
{
    public decimal Amount { get; }  // ✅ Solo getter
    public string Currency { get; }  // ✅ Solo getter

    public Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }
}
```

---

### DDD005 - No puede ser Entity y ValueObject simultáneamente ❌ Error

**Descripción**: Una clase no puede estar decorada con `[Entity]` y `[ValueObject]` al mismo tiempo.

---

### DDD006 - No puede ser AggregateRoot y ValueObject simultáneamente ❌ Error

**Descripción**: Una clase no puede estar decorada con `[AggregateRoot]` y `[ValueObject]` al mismo tiempo.

---

## 🚀 Uso

### 1. Instalar las abstracciones

Agrega referencia al proyecto `DDD.Abstractions`:

```xml
<ItemGroup>
  <ProjectReference Include="..\DDD.Abstractions\DDD.Abstractions.csproj" />
</ItemGroup>
```

### 2. Instalar los analizadores

Agrega referencia al proyecto `DDD.Analyzers` como analizador:

```xml
<ItemGroup>
  <ProjectReference Include="..\DDD.Analyzers\DDD.Analyzers.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

### 3. Usar los atributos en tu código

```csharp
using DDD.Abstractions;
using System;

[Entity]
public class Customer
{
    [EntityId]
    public Guid CustomerId { get; private set; }

    public string Name { get; set; }

    public Customer(Guid id, string name)
    {
        CustomerId = id;
        Name = name;
    }
}

[ValueObject]
public class Email
{
    public string Value { get; }

    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email no puede estar vacío");

        Value = value;
    }
}
```

## 🔧 Compilación

```bash
# Compilar toda la solución
dotnet build

# Compilar solo los analizadores
dotnet build DDD.Analyzers/DDD.Analyzers.csproj

# Empaquetar los analizadores
dotnet pack DDD.Analyzers/DDD.Analyzers.csproj
```

## 📝 Ejemplos en TestDomain

El proyecto `TestDomain` contiene ejemplos de uso correcto e incorrecto:

- ✅ `Course.cs` - Entity correcto con EntityId
- ✅ `Student.cs` - AggregateRoot correcto
- ✅ `Address.cs` - ValueObject inmutable correcto
- ⚠️ `InvalidValueObject.cs` - ValueObject con setter público (genera warning)
- 📚 `ErrorExamples.cs` - Ejemplos comentados de errores

## 🎯 Próximas Reglas a Implementar

- [ ] DDD003 - EntityId solo en propiedades (no en campos)
- [ ] DDD007 - Entities deben tener constructor privado/protected
- [ ] DDD008 - ValueObjects deben implementar Equals/GetHashCode
- [ ] DDD009 - AggregateRoot no debe exponer colecciones mutables
- [ ] DDD010 - Domain Events solo en AggregateRoot

## 📄 Licencia

MIT

## 👥 Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue antes de enviar un pull request.

---

Hecho con ❤️ para mejorar el modelado de dominios en DDD
