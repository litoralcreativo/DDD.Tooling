# DDD.Tooling

Herramientas de análisis estático para validar reglas de Domain-Driven Design (DDD) en proyectos .NET mediante Roslyn Analyzers.

## 🎯 Objetivo

Proporcionar validación en tiempo de compilación de las reglas y patrones de DDD, ayudando a los desarrolladores a crear modelos de dominio consistentes y correctos.

## ✨ Características

- ✅ **9 Analizadores** para validar reglas DDD (DDD001-DDD009)
- ✅ **3 Code Fix Providers** para corregir automáticamente errores/warnings
- ✅ **Sugerencias educativas (Info)** para mejorar el diseño
- ✅ **Manejo inteligente de tipos** (detecta automáticamente structs, nullables, referencias)
- ✅ **Validación en tiempo real** en el IDE
- ✅ **Errores, Warnings e Infos claros** con mensajes descriptivos
- ✅ **Fácil integración** vía NuGet (próximamente)

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
│   ├── EntityMustHaveEntityIdAnalyzer.cs       # DDD001
│   ├── AggregateRootMustHaveEntityIdAnalyzer.cs # DDD002
│   ├── ValueObjectImmutabilityAnalyzer.cs      # DDD004
│   ├── DddAttributeConflictsAnalyzer.cs        # DDD005/006/007/008
│   ├── EntityFactoryMethodAnalyzer.cs          # DDD009
│   └── CodeFixes/
│       ├── EntityIdCodeFixProvider.cs          # Fix para DDD001/002
│       ├── ValueObjectEqualsCodeFixProvider.cs # Fix para DDD007/008
│       └── EntityFactoryMethodCodeFixProvider.cs # Fix para DDD009
└── TestDomain/                 # Proyecto de prueba
    ├── Course.cs
    ├── Student.cs
    ├── Address.cs
    └── Examples/
        ├── FactoryMethodExamples.cs
        ├── InvalidValueObject.cs
        └── ErrorExamples.cs
```

## 🔍 Reglas Implementadas

### DDD001 - Entity debe tener EntityId ❌ Error

**Descripción**: Todas las clases decoradas con `[Entity]` deben tener al menos una propiedad decorada con `[EntityId]`.

**Quick Fix disponible**: Agrega `[EntityId] public Guid Id { get; private set; }` automáticamente.

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

**Quick Fix disponible**: Agrega `[EntityId] public Guid Id { get; private set; }` automáticamente.

---

### DDD003 - EntityId solo en propiedades ❌ Error

**Descripción**: El atributo `[EntityId]` solo puede aplicarse a propiedades, no a campos u otros miembros.

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

### DDD007 - ValueObject debe sobrescribir Equals ⚠️ Warning

**Descripción**: Los ValueObjects deben compararse por valor, por lo que deben sobrescribir `Equals(object)`.

**Quick Fix disponible**: Genera `Equals` con comparación de todas las propiedades públicas.

---

### DDD008 - ValueObject debe sobrescribir GetHashCode ⚠️ Warning

**Descripción**: Los ValueObjects deben sobrescribir `GetHashCode` de forma consistente con `Equals`.

**Quick Fix disponible**: Genera `GetHashCode` con manejo inteligente de tipos (value types, nullables, referencias).

---

### DDD009 - Entity debería usar Factory Method ℹ️ Info

**Descripción**: Las Entities y AggregateRoots deben usar el patrón Factory Method: constructor privado/internal + método estático público que devuelva la instancia. Se reporta cuando el constructor es público o cuando no existe un factory method estático.

**Quick Fix disponible** (3 escenarios):

- Constructor público sin factory method → hace privado el constructor + agrega `Create` estático
- Constructor privado sin factory method estático → agrega `Create` estático
- Método `Create` existente pero no estático → agrega el modificador `static`

**Ejemplo que genera info:**

```csharp
[Entity]
public class Product  // ℹ️ DDD009
{
    [EntityId]
    public Guid Id { get; private set; }

    public Product(string name)  // ⬅️ Constructor público
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
```

**Ejemplo recomendado:**

```csharp
[Entity]
public class Product
{
    [EntityId]
    public Guid Id { get; private set; }

    public static Product Create(string name)  // ✅ Factory Method estático
    {
        return new Product(name);
    }

    private Product(string name)  // ✅ Constructor privado
    {
        Id = Guid.NewGuid();
        Name = name;
    }
}
```

**Ver documentación completa:** [DDD009_FACTORY_METHOD.md](DDD009_FACTORY_METHOD.md)

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

## 📚 Documentación Adicional

- 📖 [QUICKSTART.md](QUICKSTART.md) - Guía rápida de inicio
- 🔧 [QUICKFIX_GUIDE.md](QUICKFIX_GUIDE.md) - Cómo usar los Quick Fixes
- 🔍 [TYPE_HANDLING.md](TYPE_HANDLING.md) - Manejo inteligente de tipos en Code Fixes
- 🗺️ [ROADMAP.md](ROADMAP.md) - Roadmap del proyecto
- � [SUMMARY.md](SUMMARY.md) - Resumen de analizadores
- 🎬 [DEMO.md](DEMO.md) - Demo completo
- 🔄 [ANALYZER_RELOAD.md](ANALYZER_RELOAD.md) - Cómo recargar analizadores
- �📚 `ErrorExamples.cs` - Ejemplos comentados de errores

## 🎯 Próximas Reglas a Implementar

- [ ] DDD010 - AggregateRoot no debe exponer colecciones mutables (`List<T>` → `IReadOnlyCollection<T>`)
- [ ] DDD011 - Domain Events solo en AggregateRoot
- [ ] Code Fix para DDD004 - Convertir setters públicos a privados/init
- [ ] Tests unitarios para todos los analizadores

## 📄 Licencia

MIT

## 👥 Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue antes de enviar un pull request.

---

Hecho con ❤️ para mejorar el modelado de dominios en DDD
