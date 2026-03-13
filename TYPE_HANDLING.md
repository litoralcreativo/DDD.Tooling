# 🔍 Manejo Inteligente de Tipos en Code Fix

## Problema Detectado

**Pregunta del usuario:**

> "podemos llegar a encontrarnos con valores q no tengan GetHashCode, como decimal"

**Realidad:** `decimal` **SÍ tiene** `GetHashCode()`, pero es un **tipo de valor** (struct), no un tipo de referencia.

---

## ⚠️ El Error Original

### Código INCORRECTO (versión anterior):

```csharp
public override int GetHashCode()
{
    unchecked
    {
        int hash = 17;
        hash = hash * 23 + (Symbol?.GetHashCode() ?? 0);   // ✅ OK (string es referencia)
        hash = hash * 23 + (Value?.GetHashCode() ?? 0);    // ❌ ERROR! (decimal no es nullable)
        return hash;
    }
}
```

### Error de compilación:

```
error CS0023: Operator '?' cannot be applied to operand of type 'decimal'
```

**Explicación:** El operador `?.` (null-conditional) solo funciona con tipos nullable. `decimal` es un struct no nullable.

---

## ✅ La Solución Implementada

### Mejora en `ValueObjectEqualsCodeFixProvider.cs`:

```csharp
private string GenerateHashCodeForProperty(IPropertySymbol property)
{
    var typeInfo = property.Type;

    // 1️⃣ Si es un tipo nullable (int?, decimal?, etc.)
    if (typeInfo.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T)
    {
        return $"hash = hash * 23 + ({property.Name}?.GetHashCode() ?? 0);";
    }

    // 2️⃣ Si es un tipo de valor no nullable (int, decimal, DateTime, etc.)
    if (typeInfo.IsValueType)
    {
        return $"hash = hash * 23 + {property.Name}.GetHashCode();";
    }

    // 3️⃣ Si es un tipo de referencia (string, clases, etc.)
    return $"hash = hash * 23 + ({property.Name}?.GetHashCode() ?? 0);";
}
```

---

## 📊 Ejemplos de Código Generado

### Ejemplo completo con diferentes tipos:

```csharp
[ValueObject]
public class Price
{
    public string Currency { get; }        // Tipo de referencia
    public decimal Amount { get; }         // Tipo de valor
    public DateTime CreatedAt { get; }     // Tipo de valor
    public int? DiscountPercent { get; }   // Tipo de valor nullable
    public string Description { get; }     // Tipo de referencia
}
```

### Quick Fix genera (Ctrl + .):

```csharp
public override int GetHashCode()
{
    unchecked
    {
        int hash = 17;
        hash = hash * 23 + (Currency?.GetHashCode() ?? 0);       // string (referencia)
        hash = hash * 23 + Amount.GetHashCode();                 // decimal (valor)
        hash = hash * 23 + CreatedAt.GetHashCode();              // DateTime (valor)
        hash = hash * 23 + (DiscountPercent?.GetHashCode() ?? 0); // int? (nullable)
        hash = hash * 23 + (Description?.GetHashCode() ?? 0);    // string (referencia)
        return hash;
    }
}
```

---

## 🎯 Tipos Comunes y Su Tratamiento

| Tipo        | Categoría      | Código Generado              | Razón          |
| ----------- | -------------- | ---------------------------- | -------------- |
| `string`    | Referencia     | `(Name?.GetHashCode() ?? 0)` | Puede ser null |
| `decimal`   | Valor          | `Name.GetHashCode()`         | Nunca es null  |
| `int`       | Valor          | `Name.GetHashCode()`         | Nunca es null  |
| `DateTime`  | Valor          | `Name.GetHashCode()`         | Nunca es null  |
| `Guid`      | Valor          | `Name.GetHashCode()`         | Nunca es null  |
| `bool`      | Valor          | `Name.GetHashCode()`         | Nunca es null  |
| `int?`      | Valor Nullable | `(Name?.GetHashCode() ?? 0)` | Puede ser null |
| `decimal?`  | Valor Nullable | `(Name?.GetHashCode() ?? 0)` | Puede ser null |
| `DateTime?` | Valor Nullable | `(Name?.GetHashCode() ?? 0)` | Puede ser null |
| `YourClass` | Referencia     | `(Name?.GetHashCode() ?? 0)` | Puede ser null |

---

## 🔬 Detalles Técnicos

### ¿Qué es un tipo de valor (ValueType)?

En C#, los tipos de valor son structs que:

- Se almacenan en el stack (normalmente)
- No pueden ser `null` (a menos que sean `Nullable<T>`)
- Siempre tienen valor (se inicializan con valor por defecto)
- Incluyen: `int`, `decimal`, `DateTime`, `bool`, `char`, `enum`, etc.

### ¿Qué es un tipo de referencia?

Los tipos de referencia son clases que:

- Se almacenan en el heap
- Pueden ser `null`
- Incluyen: `string`, `object`, tus clases personalizadas, etc.

### Verificación en Roslyn:

```csharp
// Verificar si es tipo de valor
bool isValueType = property.Type.IsValueType;

// Verificar si es nullable
bool isNullable = property.Type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;
```

---

## ✨ Beneficios de la Mejora

1. **✅ Código correcto para todos los tipos**
    - Maneja correctamente `decimal`, `int`, `DateTime`, etc.
    - No genera operadores `?.` innecesarios

2. **✅ Sin errores de compilación**
    - El código generado siempre compila
    - No más `CS0023: Operator '?' cannot be applied...`

3. **✅ Performance óptimo**
    - No usa `?.` en tipos que nunca son null
    - Evita boxing innecesario

4. **✅ Semántica correcta**
    - Respeta la naturaleza de cada tipo
    - Código más limpio y claro

---

## 🧪 Casos de Prueba

### Test 1: ValueObject solo con tipos de valor

```csharp
[ValueObject]
public class Point
{
    public int X { get; }
    public int Y { get; }
}

// Genera:
hash = hash * 23 + X.GetHashCode();
hash = hash * 23 + Y.GetHashCode();
```

### Test 2: ValueObject con tipos mixtos

```csharp
[ValueObject]
public class Money
{
    public string Currency { get; }
    public decimal Amount { get; }
}

// Genera:
hash = hash * 23 + (Currency?.GetHashCode() ?? 0);
hash = hash * 23 + Amount.GetHashCode();
```

### Test 3: ValueObject con nullable

```csharp
[ValueObject]
public class Score
{
    public int Value { get; }
    public int? Bonus { get; }
}

// Genera:
hash = hash * 23 + Value.GetHashCode();
hash = hash * 23 + (Bonus?.GetHashCode() ?? 0);
```

---

## 📝 Archivo de Prueba

Creamos `TestDomain/Examples/Price.cs` para probar esta funcionalidad:

```csharp
[ValueObject]
public class Price
{
    public string Currency { get; }
    public decimal Amount { get; }
    public DateTime CreatedAt { get; }
    public int? DiscountPercent { get; }
    public string Description { get; }
}
```

**Uso:**

1. Abre `Price.cs`
2. Presiona `Ctrl + .` en la clase
3. Selecciona "Agregar Equals y GetHashCode"
4. Verifica el código generado ✨

---

## 🎓 Lecciones Aprendidas

1. **No todos los tipos son nullables**
    - Los structs no son null por defecto
    - Solo `Nullable<T>` (ej: `int?`) puede ser null

2. **El operador `?.` tiene restricciones**
    - Solo funciona con tipos de referencia y nullable
    - Causa error de compilación en tipos de valor no nullable

3. **Roslyn provee información de tipos**
    - `property.Type.IsValueType` detecta structs
    - `SpecialType.System_Nullable_T` detecta nullable

4. **El Code Fix debe ser inteligente**
    - Adaptar el código según el tipo de dato
    - Generar código idiomático y correcto

---

## 🚀 Próximas Mejoras

- [ ] Detectar colecciones (`List<T>`, `Array`) y usar hash especial
- [ ] Soportar propiedades de tipos custom (otros ValueObjects)
- [ ] Generar `IEquatable<T>` además de `Equals(object)`
- [ ] Optimizar hash para estructuras grandes

---

**Actualizado:** 12 de Marzo 2026  
**Archivo modificado:** `DDD.Analyzers/CodeFixes/ValueObjectEqualsCodeFixProvider.cs`  
**Método nuevo:** `GenerateHashCodeForProperty(IPropertySymbol property)`
