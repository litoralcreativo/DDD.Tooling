# DDD.Tooling

Herramientas de análisis estático para validar reglas de Domain-Driven Design (DDD) en proyectos .NET mediante Roslyn Analyzers.

## 🎯 Objetivo

Proporcionar validación en tiempo de compilación de las reglas y patrones de DDD, ayudando a los desarrolladores a crear modelos de dominio consistentes y correctos.

## ✨ Características

- ✅ **12 Reglas** de análisis DDD (DDD001-DDD012)
- ✅ **5 Code Fix Providers** para corregir automáticamente errores/warnings
- ✅ **Sugerencias educativas (Info)** para mejorar el diseño
- ✅ **Manejo inteligente de tipos** (detecta automáticamente structs, nullables, referencias)
- ✅ **Detección de referencias cruzadas** entre Bounded Contexts (DDD010-DDD012)
- ✅ **Soporte para colecciones genéricas** (`List<T>`, `IReadOnlyCollection<T>`, etc.)
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
│       ├── ValueObjectAttribute.cs
│       ├── BoundedContextAttribute.cs
│       └── SharedKernelAttribute.cs
├── DDD.Analyzers/              # Analizadores Roslyn
│   ├── DiagnosticDescriptors.cs
│   ├── EntityMustHaveEntityIdAnalyzer.cs             # DDD001
│   ├── AggregateRootMustHaveEntityIdAnalyzer.cs      # DDD002
│   ├── ValueObjectImmutabilityAnalyzer.cs            # DDD004, DDD007, DDD008
│   ├── DddAttributeUsageAnalyzer.cs                  # DDD003, DDD005, DDD006
│   ├── EntityFactoryMethodAnalyzer.cs                # DDD009
│   ├── BoundedContextDeclarationAnalyzer.cs          # DDD010
│   ├── CrossBoundedContextReferenceAnalyzer.cs       # DDD011, DDD012
│   └── CodeFixes/
│       ├── EntityIdCodeFixProvider.cs                # Fix para DDD001/002
│       ├── ValueObjectEqualsCodeFixProvider.cs       # Fix para DDD007/008
│       ├── EntityFactoryMethodCodeFixProvider.cs     # Fix para DDD009
│       ├── BoundedContextDeclarationCodeFixProvider.cs # Fix para DDD010
│       └── CrossBoundedContextReferenceCodeFixProvider.cs # Fix para DDD011
└── TestDomain/                 # Proyecto de prueba
    ├── Catalog/
    │   ├── Course.cs
    │   └── CourseModule.cs
    ├── StudentManagment/
    │   └── Student.cs
    └── SharedKernel/
        └── Address.cs
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

### DDD010 - Entity/AggregateRoot/ValueObject debe declarar su Bounded Context ⚠️ Warning

**Descripción**: Todas las clases DDD deben estar decoradas con `[BoundedContext("NombreBC")]` o `[SharedKernel]` para indicar a qué Bounded Context pertenecen. Esto es el prerequisito para que funcione DDD011.

**Quick Fix disponible**: Agrega `[BoundedContext("NombreBC")]` encima de la clase.

```csharp
[AggregateRoot]
// ⚠️ DDD010: Falta declarar el Bounded Context
public class Course { ... }

// ✅ Correcto:
[AggregateRoot]
[BoundedContext("Catalog")]
public class Course { ... }
```

---

### DDD011 - No referencias directas entre Bounded Contexts ❌ Error

**Descripción**: Una clase de un BC no puede tener propiedades públicas que referencien directamente tipos de otro BC. Se deben usar los identificadores (IDs) en su lugar.

**Quick Fix disponible** (2 casos):

- **Tipo simple**: `public Course Course { get; set; }` → `public Guid CourseId { get; set; }`
- **Colección genérica**: `public List<Course> Courses { get; set; }` → `public List<Guid> CourseIds { get; set; }`

El mensaje varía según el tipo referenciado:

- **AggregateRoot** → "usa el `CourseId` (el identificador del agregado)"
- **Entity interna** → "las entidades internas no deben exponerse fuera del BC"
- **ValueObject** → "los value objects deben copiarse o abstraerse en el BC destino"

```csharp
[AggregateRoot]
[BoundedContext("StudentManagement")]
public class Student
{
    [EntityId]
    public Guid Id { get; private set; }

    public Course Course { get; set; }            // ❌ DDD011 Error
    public List<Course> Courses { get; set; }     // ❌ DDD011 Error (genérico)

    public Guid CourseId { get; set; }            // ✅ Correcto: solo el Id
    public List<Guid> CourseIds { get; set; }     // ✅ Correcto: colección de Ids
}
```

---

### DDD012 - Miembro privado usa tipo de otro Bounded Context ⚠️ Warning

**Descripción**: Campos y propiedades privadas/protegidas que referencian tipos de otro BC. Menos severo que DDD011 (privados no forman parte del contrato público), pero aún es una dependencia a revisar.

> ⚠️ No tiene Code Fix asociado — requiere decisión de diseño por parte del desarrollador.

```csharp
[AggregateRoot]
[BoundedContext("StudentManagement")]
public class Student
{
    private Course _currentCourse;          // ⚠️ DDD012 Warning
    private List<Course> _history;          // ⚠️ DDD012 Warning
}
```

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

El proyecto `TestDomain` contiene ejemplos de uso correcto e incorrecto, organizados por Bounded Context:

- ✅ `Catalog/Course.cs` - AggregateRoot con `[BoundedContext("Catalog")]`
- ✅ `Catalog/CourseModule.cs` - Entity interna del BC Catalog
- ✅ `StudentManagment/Student.cs` - AggregateRoot con referencias cruzadas (activa DDD011)
- ✅ `SharedKernel/Address.cs` - ValueObject con `[SharedKernel]`

## 📚 Documentación Adicional

- 🚀 [QUICKSTART.md](QUICKSTART.md) - Guía rápida de inicio
- 🗺️ [ROADMAP.md](ROADMAP.md) - Roadmap del proyecto
- 🎬 [DEMO.md](DEMO.md) - Demo completo con ejemplos
- 📋 [CHANGELOG.md](CHANGELOG.md) - Historial de cambios

## 🎯 Próximas Reglas a Implementar

- [ ] Code Fix para DDD004 - Convertir setters públicos a privados/init
- [ ] Code Fix para DDD005/006 - Remover atributos conflictivos
- [ ] Tests unitarios para todos los analizadores

## 📄 Licencia

Este proyecto está bajo la licencia **MIT**. Ver el archivo [LICENSE](LICENSE) para más detalles.

## 👤 Autor

**Gastón Chatelet** — Creador y mantenedor principal del proyecto.

## Contribuciones

Las contribuciones son bienvenidas. Por favor, abre un issue antes de enviar un pull request.

---

Hecho con ❤️ por [Gastón Chatelet](https://github.com/gastonchatelet) para mejorar el modelado de dominios en DDD
