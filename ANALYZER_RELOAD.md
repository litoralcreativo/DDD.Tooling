# 🔍 Por qué los analizadores no se detectan en tiempo real en VS Code

## 🤔 El Problema

Has notado que:

- ✅ **DDD004** (setter público) se detecta **en tiempo real** en el IDE
- ❌ **DDD007/DDD008** (Equals/GetHashCode) solo se detectan **al compilar**

## 📖 Explicación

### Comportamiento de los Analizadores Roslyn:

1. **Analizadores ya existentes** (como DDD004):
    - Están cargados en memoria del Language Server
    - Se ejecutan en tiempo real mientras escribes
    - Aparecen instantáneamente como subrayados en el código

2. **Analizadores nuevos** (como DDD007/DDD008):
    - VS Code carga los analizadores **una vez** al iniciar
    - Los cambios en los analizadores **no se recargan automáticamente**
    - Necesitan reiniciar el Language Server para activarse

---

## ✅ Soluciones

### Opción 1: Recargar la ventana de VS Code (Más rápido)

```
1. Presiona: Ctrl + Shift + P
2. Busca: "Developer: Reload Window"
3. Presiona Enter
```

Esto recarga el Language Server y detecta los nuevos analizadores.

---

### Opción 2: Cerrar y abrir VS Code

```
1. Cierra VS Code completamente
2. Abre de nuevo el proyecto
```

---

### Opción 3: Usar el script (Automático)

```powershell
# Ejecutar desde la raíz del proyecto
.\reload-analyzers.ps1
```

Luego recarga VS Code con `Ctrl + Shift + P` → "Developer: Reload Window"

---

## 🔬 Verificar que los analizadores funcionan

Aunque no veas los subrayados en tiempo real, puedes verificar que funcionan:

### 1️⃣ Compilar manualmente:

```bash
dotnet build TestDomain/TestDomain.csproj
```

Verás los warnings:

```
warning DDD007: La clase 'Address' debe sobrescribir Equals(object)
warning DDD008: La clase 'Address' debe sobrescribir GetHashCode()
```

### 2️⃣ Ver el panel de problemas:

```
View → Problems (Ctrl + Shift + M)
```

Después de compilar, aparecerán allí.

---

## 🎯 Por qué DDD004 funciona en tiempo real

El analizador DDD004 (setter público) probablemente funcionó desde el principio porque:

1. Se agregó cuando abriste VS Code por primera vez
2. Ya estaba cargado en el Language Server
3. No requiere recarga

Los nuevos analizadores (DDD007/DDD008) necesitan ser cargados por primera vez.

---

## 💡 Tips para desarrolladores de analizadores

Si estás desarrollando analizadores Roslyn:

1. **Durante desarrollo:**
    - Compila frecuentemente para ver warnings
    - Usa `dotnet build` para validar
    - Recarga VS Code después de cambios importantes

2. **Para usuarios finales:**
    - Los analizadores funcionarán automáticamente
    - Solo la primera vez después de instalar
    - No requieren configuración especial

---

## 🚀 Próximos pasos

1. Ejecuta el script de recarga:

    ```powershell
    .\reload-analyzers.ps1
    ```

2. Recarga VS Code:

    ```
    Ctrl + Shift + P → "Developer: Reload Window"
    ```

3. Abre `Address.cs` y verás los subrayados amarillos en:
    ```csharp
    [ValueObject]
    public class Address  // ⚠️ Aquí verás warnings DDD007/DDD008
    ```

---

## 📊 Comparación

| Aspecto            | DDD004 (Setter)   | DDD007/008 (Equals) |
| ------------------ | ----------------- | ------------------- |
| Detección en IDE   | ✅ Sí (inmediata) | ❌ Requiere recarga |
| Detección en build | ✅ Sí             | ✅ Sí               |
| ¿Por qué?          | Ya cargado        | Recién agregado     |
| Solución           | N/A               | Recargar VS Code    |

---

## 🎓 Conclusión

Es **normal** que los nuevos analizadores no aparezcan inmediatamente en el IDE. Esto es una característica de cómo VS Code y el Language Server de C# cargan los analizadores.

**Después de recargar VS Code, funcionarán perfectamente en tiempo real.** ✨

---

**Última actualización:** 12 de Marzo 2026
