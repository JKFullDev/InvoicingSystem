# 📊 Sistema de Facturación

Sistema de gestión de facturas con **Blazor WebAssembly** y **ASP.NET Core 9**.

## 🚀 Qué hace

- ✅ Crear y editar facturas con líneas de detalle
- ✅ Generar PDFs profesionales
- ✅ Gestionar clientes y productos
- ✅ Calcular IVA automáticamente (21%, 10%, 4%, etc.)
- ✅ Dashboard con métricas en tiempo real

## 📋 Requisitos

- .NET 9 SDK
- SQL Server (Express vale)
- Visual Studio 2022 (opcional)

## 🔧 Instalación

### 1. Clonar
```
git clone https://github.com/JKFullDev/InvoicingSystem.git
cd InvoicingSystem
```

### 2. Base de datos
Ejecuta en SQL Server Management Studio el script: `InvoicingSystem.sql`

### 3. Configurar
Edita `Server/appsettings.json` y cambia `TU_SERVIDOR` por:
- `localhost` (SQL Server normal)
- `.\SQLEXPRESS` (SQL Express)

### 4. Ejecutar
**Visual Studio:** Abre `InvoicingSystem.sln` y presiona `F5`

**Terminal:** 
```
cd Server
dotnet run
```

Abre tu navegador en: `https://localhost:7085`

## 📖 Uso

### Nueva factura
1. **Facturas** → **Nueva Factura**
2. Rellena datos básicos (ID, cliente, fecha, condiciones de pago)
3. **Añadir Línea**: Elige producto, IVA y cantidad
4. **Guardar**

### Generar PDF
En la lista de facturas, click en el botón **PDF**. El documento incluye desglose de IVA por tipo y cálculo automático de totales.

## 🐛 Problemas comunes

### Error "Could not load Radzen.Blazor"
Windows Defender puede bloquear archivos. Solución:
1. Cierra Visual Studio
2. Borra carpetas `bin` y `obj`
3. Ejecuta:
   ```
   dotnet clean
   dotnet restore
   dotnet build
   ```

### No conecta a la base de datos
1. Verifica que SQL Server esté corriendo
2. Comprueba la cadena de conexión en `appsettings.json`
3. Asegúrate de haber ejecutado el script SQL

## 🛠️ Tecnologías

- Blazor WebAssembly
- ASP.NET Core 9
- Entity Framework Core
- SQL Server
- Radzen UI
- QuestPDF

## 👤 Autor

Juan Carlos Alonso Hernando

## 📄 Licencia

MIT
