# 🔧 Quick Fixes (Code Fixes) para DDD.Analyzers

## ✨ Nueva Funcionalidad: Correcciones Automáticas

¡Ahora puedes corregir automáticamente errores y warnings DDD con un solo clic!

---

## 🎯 Quick Fixes Disponibles

### **DDD001/DDD002 - Agregar propiedad Id con [EntityId]** ⭐ NUEVO

Cuando una `Entity` o `AggregateRoot` no tiene propiedad `[EntityId]`, puedes agregarla automáticamente.

**Ver guía detallada:** [ENTITYID_CODEFIX.md](ENTITYID_CODEFIX.md)

### **DDD007/DDD008 - Agregar Equals y GetHashCode**

Cuando un `ValueObject` no tiene `Equals` y/o `GetHashCode`, puedes agregarlos automáticamente.

**Ver guía detallada:** [TYPE_HANDLING.md](TYPE_HANDLING.md)

---

## 🚀 Cómo Usar

### **Opción 1: Usando el teclado (Rápido)**

1. Coloca el cursor en la clase con warning:

    ```csharp
    [ValueObject]
    public class Address  // ⚠️ cursor aquí
    ```

2. Presiona: **`Ctrl + .`** (punto)

3. Selecciona: **"Agregar Equals y GetHashCode"**

4. ¡Listo! El código se genera automáticamente

---

### **Opción 2: Usando el mouse**

1. Haz clic en el warning (subrayado amarillo)

2. Aparecerá una bombilla 💡

3. Clic en la bombilla

4. Selecciona: **"Agregar Equals y GetHashCode"**

---

## 📝 Ejemplo Antes y Después

### **ANTES** (con warnings):

```csharp
[ValueObject]
public class Address
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }
    // ⚠️ DDD007: Falta Equals
    // ⚠️ DDD008: Falta GetHashCode
}
```

### **DESPUÉS** (automático con Ctrl + .):

```csharp
[ValueObject]
public class Address
{
    public string Street { get; }
    public string City { get; }
    public string PostalCode { get; }
    public string Country { get; }

    public Address(string street, string city, string postalCode, string country)
    {
        Street = street;
        City = city;
        PostalCode = postalCode;
        Country = country;
    }

    // ✅ Agregado automáticamente
    public override bool Equals(object obj)
    {
        if (obj is Address other)
        {
            return Street == other.Street && City == other.City && PostalCode == other.PostalCode && Country == other.Country;
        }
        return false;
    }

    // ✅ Agregado automáticamente
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + (Street?.GetHashCode() ?? 0);
            hash = hash * 23 + (City?.GetHashCode() ?? 0);
            hash = hash * 23 + (PostalCode?.GetHashCode() ?? 0);
            hash = hash * 23 + (Country?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
```

---

## 🎨 Características del Code Fix

### ✅ **Inteligente**

- Detecta automáticamente todas las propiedades públicas
- Genera comparaciones para cada propiedad
- Maneja valores nulos correctamente (`?.GetHashCode() ?? 0`)

### ✅ **Completo**

- Agrega ambos métodos (`Equals` y `GetHashCode`)
- Usa el patrón correcto de ValueObject
- Incluye `unchecked` para evitar overflow

### ✅ **No Duplica**

- Si ya tienes `Equals`, solo agrega `GetHashCode`
- Si ya tienes `GetHashCode`, solo agrega `Equals`
- Si tienes ambos, no hace nada

---

## 🔄 Activar el Code Fix (Primera vez)

**Importante**: Después de compilar por primera vez, necesitas:

1. **Recargar VS Code**:

    ```
    Ctrl + Shift + P → "Developer: Reload Window"
    ```

2. O ejecutar el script:
    ```powershell
    .\reload-analyzers.ps1
    ```

Después de esto, los Quick Fixes funcionarán automáticamente.

---

## 📊 Teclas Rápidas

| Acción                   | Atajo              |
| ------------------------ | ------------------ |
| Mostrar Quick Fixes      | `Ctrl + .`         |
| Ver Problemas            | `Ctrl + Shift + M` |
| Ir al siguiente problema | `F8`               |
| Ir al problema anterior  | `Shift + F8`       |

---

## 💡 Tips Pro

### 1. **Aplicar en múltiples archivos**

- Los Code Fixes pueden aplicarse en batch
- Selecciona varios warnings en el panel de Problemas
- Haz clic derecho → "Fix All"

### 2. **Preview antes de aplicar**

- Algunos Code Fixes muestran preview
- Puedes ver los cambios antes de aplicarlos

### 3. **Workflow recomendado**

```
1. Crea tu ValueObject
2. Agrega propiedades
3. Presiona Ctrl + .
4. Selecciona "Agregar Equals y GetHashCode"
5. ¡Listo!
```

---

## 🎯 Próximos Code Fixes Planeados

- [x] ~~**DDD001/002**: Agregar automáticamente propiedad `[EntityId]`~~ ✅ **IMPLEMENTADO**
- [x] ~~**DDD007/008**: Agregar Equals y GetHashCode~~ ✅ **IMPLEMENTADO**
- [ ] **DDD004**: Convertir setter público a privado en ValueObjects
- [ ] **DDD005/006**: Remover atributos DDD conflictivos
- [ ] Agregar operadores `==` y `!=` a ValueObjects
- [ ] Implementar `IEquatable<T>` en ValueObjects
- [ ] Agregar constructor con inicialización de ID

---

## 🐛 Troubleshooting

### "No veo la opción de Quick Fix"

**Solución:**

1. Verifica que compilaste: `dotnet build`
2. Recarga VS Code: `Ctrl + Shift + P` → "Reload Window"
3. Asegúrate de estar en la línea con warning

### "El Quick Fix no funciona"

**Solución:**

1. Cierra y abre VS Code
2. Verifica que el analizador esté referenciado correctamente en `.csproj`
3. Ejecuta `.\reload-analyzers.ps1`

### "Solo veo los warnings en compilación"

**Solución:**

- Esto es normal la primera vez
- Recarga VS Code una vez
- Después funcionará en tiempo real

---

## 📚 Recursos Adicionales

- `ANALYZER_RELOAD.md` - Cómo recargar analizadores
- `README.md` - Documentación completa
- `ROADMAP.md` - Funcionalidades futuras

---

## ✨ Resultado Final

Con los Quick Fixes, crear ValueObjects correctos es **instantáneo**:

```
1. Escribe tu clase
2. Ctrl + .
3. ¡Listo! ✨
```

**¡Adiós al código repetitivo!** 🎉

---

**Última actualización:** 12 de Marzo 2026
