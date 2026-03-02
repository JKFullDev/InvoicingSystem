# 📘 InvoicingSystem - Sistema de Facturación con Blazor WebAssembly

## 🎯 Introducción

He desarrollado **InvoicingSystem**, una aplicación de facturación empresarial usando tecnologías modernas de Microsoft. Este proyecto me ha permitido implementar una arquitectura completa cliente-servidor con **Blazor WebAssembly**, **ASP.NET Core 9**, **Entity Framework Core** y **OData**.

---

## 🏗️ Arquitectura General del Proyecto

Mi sistema está dividido en **dos proyectos principales**:

### 1️⃣ **InvoicingSystem.Server** (Backend)
El servidor es una **API REST** construida con **ASP.NET Core 9**. Es el cerebro de la aplicación, responsable de:
- Gestionar la base de datos SQL Server mediante **Entity Framework Core**
- Exponer endpoints **OData** para operaciones CRUD avanzadas
- Generar PDFs de facturas con **QuestPDF**
- Validar reglas de negocio (ej: no permitir borrar productos facturados)

### 2️⃣ **InvoicingSystem.Client** (Frontend)
El cliente es una **Single Page Application (SPA)** con **Blazor WebAssembly**. Se ejecuta completamente en el navegador del usuario y:
- Consume la API del servidor mediante HTTP
- Renderiza interfaces interactivas usando **Radzen Blazor Components**
- Gestiona el estado de la aplicación en memoria
- Implementa validaciones del lado del cliente

---

## 🗃️ Modelo de Datos (Entity Framework Core)

He diseñado un modelo relacional normalizado con las siguientes entidades principales:

### **Customers (Clientes)**
Almaceno la información básica de cada cliente:
```csharp
public class Customers
{
    public required string CustomerId { get; set; }  // Clave primaria (string personalizable)
    public required string Name { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string Nif { get; set; }
}
```

### **Products (Productos)**
Catalogo de productos/servicios facturables:
```csharp
public class Products
{
    public Guid ProductId { get; set; }  // Clave primaria (GUID autogenerado)
    public required string Name { get; set; }
    public required string Description { get; set; }
    public decimal CurrentPrice { get; set; }  // Precio actual del producto
}
```

### **TaxRates (Tipos de IVA)**
Diferentes tipos impositivos aplicables:
```csharp
public class TaxRates
{
    public Guid TaxRateId { get; set; }
    public required string Name { get; set; }  // Ej: "IVA 21%"
    public decimal Percentage { get; set; }    // Ej: 21
}
```

### **PaymentTerms (Condiciones de Pago)**
Defino los plazos de pago:
```csharp
public class PaymentTerms
{
    public Guid PaymentTermsId { get; set; }
    public required string Description { get; set; }  // Ej: "30 días"
    public int PaymentDays { get; set; }              // Ej: 30
}
```

### **SalesInvoiceHeaders (Cabeceras de Factura)**
La entidad principal de facturación:
```csharp
public class SalesInvoiceHeaders
{
    public required string SalesInvoiceHeaderId { get; set; }  // Ej: "FA2024-001"
    public required string CustomerReference { get; set; }
    public DateTime InvoiceDate { get; set; }
    public DateTime DueDate { get; set; }  // Se calcula: InvoiceDate + PaymentDays
    public required string QuoteReference { get; set; }
    
    // Relaciones (Foreign Keys)
    public required string CustomerId { get; set; }
    public Customers? Customer { get; set; }  // Propiedad de navegación
    
    public Guid PaymentTermsId { get; set; }
    public PaymentTerms? PaymentTerms { get; set; }
    
    public List<SalesInvoiceLines> Lines { get; set; } = new();  // Relación 1-N
}
```

### **SalesInvoiceLines (Líneas de Factura)**
Cada línea representa un producto/servicio facturado:
```csharp
public class SalesInvoiceLines
{
    public Guid SalesInvoiceLineId { get; set; }
    public required string SalesInvoiceHeaderId { get; set; }  // FK a SalesInvoiceHeaders
    
    public Guid ProductId { get; set; }
    public Products? Product { get; set; }
    
    public Guid TaxRateId { get; set; }
    public TaxRates? TaxRate { get; set; }
    
    public decimal UnitPrice { get; set; }  // Precio en el momento de facturación
    public int Quantity { get; set; }
    public required string CustomDescription { get; set; }
    
    public decimal TotalLine => UnitPrice * Quantity;  // Propiedad calculada
}
```

---

## 🔧 Entity Framework Core: Cómo Gestiono la Base de Datos

### **DbContext**
Mi clase `InvoicingSystemDbContext` es el puente entre el código C# y la base de datos SQL Server:

```csharp
public class InvoicingSystemDbContext : DbContext
{
    public DbSet<Customers> Customers { get; set; }
    public DbSet<Products> Products { get; set; }
    public DbSet<TaxRates> TaxRates { get; set; }
    public DbSet<PaymentTerms> PaymentTerms { get; set; }
    public DbSet<SalesInvoiceHeaders> SalesInvoiceHeaders { get; set; }
    public DbSet<SalesInvoiceLines> SalesInvoiceLines { get; set; }
}
```

Cada `DbSet<T>` representa una **tabla** en la base de datos. EF Core traduce automáticamente operaciones LINQ en **consultas SQL**.

### **Migrations (Migraciones)**
Para evolucionar el esquema de la base de datos de forma controlada, uso migraciones:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

Esto genera scripts SQL que transforman la base de datos sin perder datos.

---

## 🌐 OData: API Potente para el Cliente

He implementado **OData v8** en mis controladores para exponer una API REST super flexible.

### **¿Qué es OData?**
OData (Open Data Protocol) es un estándar que permite al **cliente** construir consultas complejas desde la URL. Por ejemplo:

```http
GET /odata/InvoicingSystem/Products?$filter=CurrentPrice gt 100&$orderby=Name&$top=10&$expand=Category
```

Esta URL significa:
- Filtrar productos con precio > 100
- Ordenar por nombre
- Tomar solo 10 resultados
- Expandir la relación `Category` (eager loading)

### **Configuración en Program.cs**
Registro OData en el pipeline de ASP.NET Core:

```csharp
builder.Services.AddControllers()
    .AddOData(options => options
        .AddRouteComponents("odata/InvoicingSystem", GetEdmModel())
        .Filter()   // Habilita $filter
        .OrderBy()  // Habilita $orderby
        .Expand()   // Habilita $expand
        .Count()    // Habilita $count
        .SetMaxTop(1000));
```

El **EDM (Entity Data Model)** define la estructura de datos expuesta:

```csharp
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();
    builder.EntitySet<Customers>("Customers");
    builder.EntitySet<Products>("Products");
    builder.EntitySet<SalesInvoiceHeaders>("SalesInvoiceHeaders");
    // ... resto de entidades
    return builder.GetEdmModel();
}
```

---

## 🎮 Controladores: La Lógica de Negocio

Los controladores heredan de `ODataController` y gestionan las peticiones HTTP.

### **Ejemplo: ProductsController**

#### **GET (Listar productos)**
```csharp
[HttpGet]
[EnableQuery(MaxExpansionDepth = 10)]
public IEnumerable<Products> GetProducts()
{
    return context.Products.AsNoTracking().AsQueryable();
}
```
- `AsNoTracking()`: Optimiza rendimiento (EF no vigila cambios)
- `AsQueryable()`: Permite que OData añada filtros, ordenación, etc.

#### **POST (Crear producto)**
```csharp
[HttpPost]
public IActionResult Post([FromBody] ProductsDTO dto)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);
    
    var newProduct = new Products
    {
        ProductId = Guid.NewGuid(),
        Name = dto.Name,
        Description = dto.Description,
        CurrentPrice = dto.CurrentPrice
    };
    
    context.Products.Add(newProduct);
    context.SaveChanges();
    
    return Created(newProduct);
}
```

#### **PUT (Actualizar producto)**
```csharp
[HttpPut("/odata/InvoicingSystem/Products({key})")]
public IActionResult Put(Guid key, [FromBody] ProductsDTO dto)
{
    var existingProduct = context.Products.Find(key);
    if (existingProduct == null) return NotFound();
    
    existingProduct.Name = dto.Name;
    existingProduct.Description = dto.Description;
    existingProduct.CurrentPrice = dto.CurrentPrice;
    
    context.SaveChanges();
    return Ok();
}
```

#### **DELETE (Eliminar producto)**
```csharp
[HttpDelete("/odata/InvoicingSystem/Products({key})")]
public IActionResult Delete(Guid key)
{
    try
    {
        var product = context.Products.Find(key);
        if (product == null) return NotFound();
        
        context.Products.Remove(product);
        context.SaveChanges();
        return NoContent();
    }
    catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
    {
        return Conflict(new { message = "No se puede eliminar: producto facturado" });
    }
}
```

---

## 🔄 DTOs: Data Transfer Objects

Uso **DTOs** para separar el modelo de base de datos del modelo de transferencia:

### **¿Por qué DTOs?**
1. **Seguridad**: No expongo la estructura interna de la BD
2. **Flexibilidad**: Puedo cambiar la BD sin romper la API
3. **Validaciones**: Aplico reglas específicas para entrada/salida

### **Ejemplo: ProductsDTO**
```csharp
public class ProductsDTO
{
    [Key]
    public Guid ProductId { get; set; }
    
    [Required]
    public required string Name { get; set; }
    
    [Required]
    public required string Description { get; set; }
    
    [Required]
    public decimal CurrentPrice { get; set; }
}
```

En los controladores, **mapeo manualmente** entre entidades y DTOs:
```csharp
var dto = new ProductsDTO
{
    ProductId = entity.ProductId,
    Name = entity.Name,
    Description = entity.Description,
    CurrentPrice = entity.CurrentPrice
};
```

---

## 🚀 Servicios en el Cliente (Client Services)

Los **servicios** en Blazor WebAssembly encapsulan las llamadas HTTP al servidor.

### **Interfaz: IProductsService**
Defino el contrato que debe cumplir el servicio:
```csharp
public interface IProductsService
{
    Task<ODataServiceResult<Products>?> GetProducts(Query query);
    Task<Products?> CreateProducts(ProductsDTO dto);
    Task<HttpResponseMessage> UpdateProducts(Guid productId, Products product);
    Task<HttpResponseMessage> ReplaceProducts(Guid productId, ProductsDTO dto);
    Task<HttpResponseMessage> DeleteProducts(Guid productId);
    Task<(bool, int)> IsProductInvoiced(Guid productId);
}
```

### **Implementación: ProductsService**
Implemento la interfaz usando `HttpClient`:

```csharp
public class ProductsService : IProductsService
{
    private readonly HttpClient httpClient;
    private readonly Uri baseUri;
    
    public ProductsService(HttpClient client, NavigationManager nav)
    {
        httpClient = client;
        baseUri = new Uri($"{nav.BaseUri}odata/InvoicingSystem/");
    }
    
    public async Task<ODataServiceResult<Products>?> GetProducts(Query query)
    {
        var uri = new Uri(baseUri, "Products");
        
        // Radzen construye la URL con parámetros OData
        uri = Radzen.ODataExtensions.GetODataUri(
            uri: uri,
            filter: query.Filter,
            top: query.Top,
            skip: query.Skip,
            orderby: query.OrderBy,
            count: true
        );
        
        var response = await httpClient.GetAsync(uri);
        return await response.ReadAsync<ODataServiceResult<Products>>();
    }
    
    public async Task<Products?> CreateProducts(ProductsDTO dto)
    {
        var uri = new Uri(baseUri, "Products");
        var response = await httpClient.PostAsJsonAsync(uri, dto);
        return await response.ReadAsync<Products>();
    }
}
```

### **Inyección de Dependencias**
Registro los servicios en `Program.cs` del cliente:

```csharp
builder.Services.AddHttpClient<IProductsService, ProductsService>()
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

builder.Services.AddScoped<IProductsService, ProductsService>();
```

Esto permite **inyectar** el servicio en componentes Blazor:
```csharp
[Inject] protected IProductsService ProductsService { get; set; } = default!;
```

---

## 🖼️ Componentes Blazor: La Interfaz de Usuario

Blazor me permite escribir interfaces web usando **C#** en lugar de JavaScript.

### **Estructura de un Componente**

Cada página tiene 2 archivos:

#### **1. ProductList.razor** (Vista/Markup)
```razor
@page "/products"
@using InvoicingSystem.Server.Data.Models
@rendermode @(new InteractiveWebAssemblyRenderMode(prerender: false))

<PageTitle>Productos</PageTitle>

<h1>Gestión de Productos</h1>

<RadzenButton Text="Nuevo Producto" 
              Icon="add" 
              Click="@GoToAdd" 
              ButtonStyle="ButtonStyle.Primary" />

<RadzenDataGrid @ref="grid" 
                Data="@products" 
                Count="@count" 
                TItem="Products" 
                LoadData="@LoadData"
                AllowFiltering="true" 
                AllowPaging="true">
    <Columns>
        <RadzenDataGridColumn TItem="Products" Property="Name" Title="Nombre" />
        <RadzenDataGridColumn TItem="Products" Property="CurrentPrice" Title="Precio" />
    </Columns>
</RadzenDataGrid>
```

#### **2. ProductList.razor.cs** (Code-behind/Lógica)
```csharp
public partial class ProductList : ComponentBase
{
    [Inject] protected IProductsService ProductsService { get; set; } = default!;
    [Inject] protected NotificationService NotificationService { get; set; } = default!;
    
    protected RadzenDataGrid<Products> grid = default!;
    protected IEnumerable<Products>? products;
    protected int count;
    protected bool isLoading = false;
    
    protected async Task LoadData(LoadDataArgs args)
    {
        isLoading = true;
        try
        {
            var query = new Query 
            { 
                Filter = args.Filter, 
                OrderBy = args.OrderBy, 
                Skip = args.Skip, 
                Top = args.Top 
            };
            
            var result = await ProductsService.GetProducts(query);
            
            if (result != null)
            {
                products = result.Value;  // Los datos
                count = result.Count;      // Total de registros
            }
        }
        finally
        {
            isLoading = false;
        }
    }
    
    protected void GoToAdd()
    {
        // Lógica para abrir formulario de nuevo producto
    }
}
```

---

## 📋 Patrón Maestro-Detalle: Facturas con Líneas

He implementado un sistema completo para gestionar facturas con sus líneas de detalle.

### **Arquitectura de 3 Capas**

#### **Capa 1: Lista de Facturas (SalesInvoiceHeaderList)**
Muestra el grid principal:
```csharp
public partial class SalesInvoiceHeaderList : ComponentBase
{
    protected async Task GoToAdd()
    {
        // Abro diálogo modal para crear nueva factura
        var result = await DialogService.OpenAsync<SalesInvoiceHeaderEdit>(
            "Nueva Factura",
            new Dictionary<string, object?> { { "IsNew", true } },
            new DialogOptions { Width = "900px", Height = "90vh" }
        );
        
        if (result == true)
        {
            await grid.Reload();  // Recargo la tabla
        }
    }
    
    protected async Task OnRowDoubleClick(DataGridRowMouseEventArgs<SalesInvoiceHeaders> args)
    {
        // Abro diálogo modal para editar factura existente
        var result = await DialogService.OpenAsync<SalesInvoiceHeaderEdit>(
            $"Editar Factura: {args.Data.SalesInvoiceHeaderId}",
            new Dictionary<string, object?> 
            { 
                { "IsNew", false },
                { "InvoiceId", args.Data.SalesInvoiceHeaderId }
            }
        );
        
        if (result == true)
        {
            await grid.Reload();
        }
    }
}
```

#### **Capa 2: Formulario de Factura (SalesInvoiceHeaderEdit)**
Gestiona la cabecera y las líneas:

```csharp
public partial class SalesInvoiceHeaderEdit : ComponentBase
{
    [Parameter] public string? InvoiceId { get; set; }
    [Parameter] public bool IsNew { get; set; }
    
    private SalesInvoiceHeaders? invoice;
    private List<SalesInvoiceLines> invoiceLines = new();
    
    protected override async Task OnInitializedAsync()
    {
        if (IsNew)
        {
            // Nueva factura vacía
            invoice = new SalesInvoiceHeaders
            {
                SalesInvoiceHeaderId = "",
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30)
            };
            invoiceLines = new();
        }
        else
        {
            // Cargo factura existente con OData $expand
            invoice = await SalesInvoiceHeadersService
                .GetSalesInvoiceHeaderById(InvoiceId!);
            
            // Las líneas vienen automáticamente con $expand=Lines
            invoiceLines = invoice.Lines?.ToList() ?? new();
        }
    }
    
    private async Task AddLine()
    {
        // Abro diálogo anidado para añadir línea
        var line = await DialogService.OpenAsync<SalesInvoiceLineEdit>(
            "Nueva Línea",
            new Dictionary<string, object?> 
            { 
                { "IsNew", true },
                { "Products", products },
                { "TaxRates", taxRates }
            }
        );
        
        if (line != null)
        {
            invoiceLines.Add(line);
            StateHasChanged();  // Refresco UI
        }
    }
    
    private async Task OnSubmit()
    {
        // Mapeo a DTO con líneas
        var dto = new SalesInvoiceHeadersDTO
        {
            SalesInvoiceHeaderId = invoice.SalesInvoiceHeaderId,
            InvoiceDate = invoice.InvoiceDate,
            Lines = invoiceLines.Select(l => new SalesInvoiceLinesDTO
            {
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                // ...
            }).ToList()
        };
        
        if (IsNew)
        {
            await SalesInvoiceHeadersService.CreateSalesInvoiceHeaders(dto);
        }
        else
        {
            await SalesInvoiceHeadersService.ReplaceSalesInvoiceHeaders(
                invoice.SalesInvoiceHeaderId, 
                dto
            );
        }
        
        DialogService.Close(true);  // Cierro diálogo y devuelvo éxito
    }
}
```

#### **Capa 3: Formulario de Línea (SalesInvoiceLineEdit)**
Diálogo anidado para editar una línea:

```csharp
public partial class SalesInvoiceLineEdit : ComponentBase
{
    [Parameter] public bool IsNew { get; set; }
    [Parameter] public SalesInvoiceLines? Line { get; set; }
    [Parameter] public IEnumerable<Products>? Products { get; set; }
    [Parameter] public IEnumerable<TaxRates>? TaxRates { get; set; }
    
    private SalesInvoiceLines line = new();
    
    protected override void OnInitialized()
    {
        if (!IsNew && Line != null)
        {
            // Clono para editar sin mutar el original
            line = new SalesInvoiceLines { /* copiar propiedades */ };
        }
        else
        {
            // Nueva línea
            line = new SalesInvoiceLines
            {
                SalesInvoiceLineId = Guid.NewGuid(),
                Quantity = 1,
                UnitPrice = 0
            };
        }
    }
    
    private void OnProductChanged(object productId)
    {
        // Cuando selecciono producto, copio su precio actual
        if (productId is Guid guid && Products != null)
        {
            var product = Products.FirstOrDefault(p => p.ProductId == guid);
            if (product != null)
            {
                line.UnitPrice = product.CurrentPrice;
            }
        }
    }
    
    private void Save()
    {
        // Valido
        if (line.ProductId == Guid.Empty)
        {
            NotificationService.Notify(/* error */);
            return;
        }
        
        // Cierro y devuelvo la línea editada
        DialogService.Close(line);
    }
}
```

---

## 🔍 OData $expand: Carga Eager de Relaciones

Para optimizar el rendimiento, uso **$expand** para cargar relaciones en una sola query.

### **En el Servicio del Cliente**
```csharp
public async Task<SalesInvoiceHeaders?> GetSalesInvoiceHeaderById(string id)
{
    var uri = new Uri(baseUri, $"SalesInvoiceHeaders('{id}')?$expand=Lines");
    var response = await httpClient.GetAsync(uri);
    return await response.ReadAsync<SalesInvoiceHeaders>();
}
```

### **En el Controlador del Servidor**
```csharp
[HttpGet("/odata/InvoicingSystem/SalesInvoiceHeaders({key})")]
[EnableQuery(MaxExpansionDepth = 10)]
public SingleResult<SalesInvoiceHeaders> Get(string key)
{
    // EF Core hace JOIN automático gracias a $expand
    var items = context.SalesInvoiceHeaders
        .Include(h => h.Lines)  // Opcional: también con .Include()
        .AsNoTracking()
        .Where(h => h.SalesInvoiceHeaderId == key);
    
    return SingleResult.Create(items);
}
```

Esto genera SQL optimizado:
```sql
SELECT h.*, l.*
FROM SalesInvoiceHeaders h
LEFT JOIN SalesInvoiceLines l ON h.SalesInvoiceHeaderId = l.SalesInvoiceHeaderId
WHERE h.SalesInvoiceHeaderId = 'FA2024-001'
```

---

## 📄 Generación de PDFs con QuestPDF

He integrado **QuestPDF** para generar facturas en formato PDF profesional.

### **Instalación**
```bash
dotnet add Server/InvoicingSystem.Server.csproj package QuestPDF
```

### **Servicio InvoiceDocument**
Creo una clase que implementa `IDocument`:

```csharp
public class InvoiceDocument : IDocument
{
    public SalesInvoiceHeaders Invoice { get; }
    private readonly string _logoPath;
    
    public InvoiceDocument(SalesInvoiceHeaders invoice, string webRootPath)
    {
        Invoice = invoice;
        _logoPath = Path.Combine(webRootPath, "images", "logo.png");
    }
    
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }
    
    void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                // Logo
                row.RelativeItem().Image(_logoPath);
                
                // Datos factura
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text($"Factura {Invoice.SalesInvoiceHeaderId}").Bold();
                    column.Item().Text($"Fecha: {Invoice.InvoiceDate:dd/MM/yyyy}");
                });
            });
        });
    }
    
    void ComposeContent(IContainer container)
    {
        // Tabla de líneas
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3);  // Descripción
                columns.ConstantColumn(70); // Precio
                columns.ConstantColumn(50); // Cantidad
                columns.ConstantColumn(80); // Total
            });
            
            table.Header(header =>
            {
                header.Cell().Text("Descripción");
                header.Cell().Text("Precio");
                header.Cell().Text("Cant.");
                header.Cell().Text("Total");
            });
            
            foreach (var line in Invoice.Lines ?? Enumerable.Empty<SalesInvoiceLines>())
            {
                table.Cell().Text(line.Product?.Name ?? "");
                table.Cell().Text($"{line.UnitPrice:N2}");
                table.Cell().Text(line.Quantity.ToString());
                table.Cell().Text($"{line.TotalLine:N2}");
            }
        });
    }
}
```

### **Controlador PDF**
```csharp
[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly InvoicingSystemDbContext _context;
    private readonly IWebHostEnvironment _environment;
    
    public PdfController(InvoicingSystemDbContext context, IWebHostEnvironment env)
    {
        _context = context;
        _environment = env;
    }
    
    [HttpGet("invoice/{invoiceId}")]
    public async Task<IActionResult> GenerateInvoicePdf(string invoiceId)
    {
        // Cargo factura completa con todas las relaciones
        var invoice = await _context.SalesInvoiceHeaders
            .Include(h => h.Customer)
            .Include(h => h.PaymentTerms)
            .Include(h => h.Lines)
                .ThenInclude(l => l.Product)
            .Include(h => h.Lines)
                .ThenInclude(l => l.TaxRate)
            .FirstOrDefaultAsync(h => h.SalesInvoiceHeaderId == invoiceId);
        
        if (invoice == null) return NotFound();
        
        // Configuro licencia Community
        QuestPDF.Settings.License = LicenseType.Community;
        
        // Genero PDF
        var document = new InvoiceDocument(invoice, _environment.WebRootPath);
        byte[] pdfBytes = document.GeneratePdf();
        
        // Devuelvo para descarga
        return File(pdfBytes, "application/pdf", $"Factura_{invoiceId}.pdf");
    }
}
```

### **Descarga desde el Cliente**
```csharp
protected void DownloadPdf(string invoiceId)
{
    var pdfUrl = $"{NavigationManager.BaseUri}api/pdf/invoice/{invoiceId}";
    NavigationManager.NavigateTo(pdfUrl, forceLoad: true);
}
```

---

## 🔒 Validaciones y Reglas de Negocio

Implemento validaciones en múltiples niveles:

### **1. Validaciones del Modelo (Data Annotations)**
```csharp
public class ProductsDTO
{
    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
    public required string Name { get; set; }
    
    [Required]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser positivo")]
    public decimal CurrentPrice { get; set; }
}
```

### **2. Validaciones del Cliente (Blazor)**
```razor
<RadzenTemplateForm Data="@product" Submit="@SaveProduct">
    <RadzenLabel Text="Nombre *" />
    <RadzenTextBox @bind-Value="product.Name" Name="Name" />
    <RadzenRequiredValidator Component="Name" Text="El nombre es obligatorio" />
    
    <RadzenLabel Text="Precio *" />
    <RadzenNumeric @bind-Value="product.CurrentPrice" Name="Price" />
    <RadzenRequiredValidator Component="Price" Text="El precio es obligatorio" />
    
    <RadzenButton ButtonType="ButtonType.Submit" Text="Guardar" />
</RadzenTemplateForm>
```

### **3. Validaciones del Servidor (Controlador)**
```csharp
[HttpPost]
public IActionResult Post([FromBody] ProductsDTO dto)
{
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);  // Devuelve errores de validación
    }
    
    // Validaciones personalizadas
    if (dto.CurrentPrice < 0)
    {
        return BadRequest(new { message = "El precio no puede ser negativo" });
    }
    
    // Continuar con la lógica...
}
```

### **4. Restricciones de Integridad (Base de Datos)**
```csharp
[HttpDelete("/odata/InvoicingSystem/Products({key})")]
public IActionResult Delete(Guid key)
{
    try
    {
        context.Products.Remove(product);
        context.SaveChanges();
        return NoContent();
    }
    catch (DbUpdateException ex) when (IsForeignKeyViolation(ex))
    {
        // SQL Server devolvió error 547 (FK constraint)
        return Conflict(new 
        { 
            message = "No se puede eliminar: producto facturado" 
        });
    }
}
```

---

## 🎨 Radzen Blazor Components

Uso la biblioteca **Radzen** para componentes UI profesionales:

### **DataGrid Avanzado**
```razor
<RadzenDataGrid @ref="grid" 
                Data="@products" 
                Count="@count" 
                TItem="Products" 
                LoadData="@LoadData"
                AllowFiltering="true"      <!-- Filtros por columna -->
                AllowPaging="true"          <!-- Paginación -->
                AllowSorting="true"         <!-- Ordenación -->
                PageSize="10"
                IsLoading="@isLoading"
                RowDoubleClick="@OnRowDoubleClick">
    <Columns>
        <RadzenDataGridColumn TItem="Products" 
                              Property="Name" 
                              Title="Nombre" 
                              Filterable="true" 
                              Sortable="true" />
        
        <RadzenDataGridColumn TItem="Products" 
                              Property="CurrentPrice" 
                              Title="Precio">
            <Template Context="product">
                @product.CurrentPrice.ToString("C2")
            </Template>
        </RadzenDataGridColumn>
        
        <!-- Columna de acciones -->
        <RadzenDataGridColumn TItem="Products" 
                              Filterable="false" 
                              Sortable="false">
            <Template Context="product">
                <RadzenButton Icon="delete" 
                              ButtonStyle="ButtonStyle.Danger" 
                              Click="@(() => Delete(product.ProductId))" />
            </Template>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>
```

### **Diálogos Modales**
```csharp
// Abrir diálogo
var result = await DialogService.OpenAsync<ProductEdit>(
    "Editar Producto",
    new Dictionary<string, object?> 
    { 
        { "ProductId", productId } 
    },
    new DialogOptions 
    { 
        Width = "600px", 
        Height = "auto",
        Resizable = true,
        Draggable = true
    }
);

// En ProductEdit.razor.cs
public partial class ProductEdit : ComponentBase
{
    [Parameter] public Guid ProductId { get; set; }
    
    private void Save()
    {
        DialogService.Close(true);  // Cierro y devuelvo resultado
    }
    
    private void Cancel()
    {
        DialogService.Close(false);
    }
}
```

### **Notificaciones**
```csharp
NotificationService.Notify(new NotificationMessage
{
    Severity = NotificationSeverity.Success,
    Summary = "Éxito",
    Detail = "Producto guardado correctamente",
    Duration = 4000  // 4 segundos
});
```

---

## 🧠 LINQ: Consultas Integradas en C#

**LINQ (Language Integrated Query)** me permite escribir consultas sobre colecciones usando sintaxis C#.

### **Ejemplos Prácticos**

#### **Filtrar productos caros**
```csharp
var expensiveProducts = products.Where(p => p.CurrentPrice > 100);
```

#### **Ordenar y limitar**
```csharp
var top5 = products
    .OrderByDescending(p => p.CurrentPrice)
    .Take(5);
```

#### **Proyectar (transformar)**
```csharp
var productNames = products.Select(p => p.Name);
```

#### **Agrupar**
```csharp
var groupedByPrice = products
    .GroupBy(p => p.CurrentPrice > 50 ? "Caro" : "Barato")
    .Select(g => new 
    { 
        Category = g.Key, 
        Count = g.Count() 
    });
```

#### **Joins**
```csharp
var invoiceDetails = from invoice in invoices
                     join customer in customers on invoice.CustomerId equals customer.CustomerId
                     select new 
                     {
                         InvoiceId = invoice.SalesInvoiceHeaderId,
                         CustomerName = customer.Name
                     };
```

#### **Calcular totales**
```csharp
var totalBase = Invoice.Lines?.Sum(l => l.TotalLine) ?? 0;
var totalIva = totalBase * 0.21m;
var totalFinal = totalBase + totalIva;
```

### **LINQ con Entity Framework**
Cuando uso LINQ sobre `DbSet<T>`, EF Core lo traduce a SQL:

```csharp
var invoices = context.SalesInvoiceHeaders
    .Where(h => h.InvoiceDate >= DateTime.Now.AddMonths(-1))
    .Include(h => h.Customer)
    .Include(h => h.Lines)
    .OrderByDescending(h => h.InvoiceDate)
    .ToListAsync();
```

Esto genera SQL eficiente:
```sql
SELECT h.*, c.*, l.*
FROM SalesInvoiceHeaders h
LEFT JOIN Customers c ON h.CustomerId = c.CustomerId
LEFT JOIN SalesInvoiceLines l ON h.SalesInvoiceHeaderId = l.SalesInvoiceHeaderId
WHERE h.InvoiceDate >= @p0
ORDER BY h.InvoiceDate DESC
```

---

## 🔄 Ciclo de Vida de una Petición

Voy a explicar paso a paso qué sucede cuando un usuario hace clic en "Guardar Producto":

### **1. Usuario hace clic → Evento Click**
```razor
<RadzenButton Text="Guardar" Click="@SaveProduct" />
```

### **2. Componente ejecuta método SaveProduct**
```csharp
protected async Task SaveProduct()
{
    var dto = new ProductsDTO
    {
        ProductId = product.ProductId,
        Name = product.Name,
        CurrentPrice = product.CurrentPrice
    };
    
    var result = await ProductsService.CreateProducts(dto);
}
```

### **3. Servicio hace petición HTTP**
```csharp
public async Task<Products?> CreateProducts(ProductsDTO dto)
{
    var uri = new Uri(baseUri, "Products");
    var response = await httpClient.PostAsJsonAsync(uri, dto);
    return await response.ReadAsync<Products>();
}
```
- Serializa `dto` a JSON
- Envía `POST /odata/InvoicingSystem/Products` con JSON en el body

### **4. Servidor recibe y deserializa**
ASP.NET Core automáticamente:
- Deserializa JSON → `ProductsDTO`
- Valida `ModelState`
- Llama al controlador

### **5. Controlador ejecuta lógica**
```csharp
[HttpPost]
public IActionResult Post([FromBody] ProductsDTO dto)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);
    
    var newProduct = new Products { /* mapeo */ };
    
    context.Products.Add(newProduct);
    context.SaveChanges();  // EF genera INSERT INTO...
    
    return Created(newProduct);
}
```

### **6. Base de Datos ejecuta INSERT**
```sql
INSERT INTO Products (ProductId, Name, Description, CurrentPrice)
VALUES ('...', 'Producto X', '...', 99.99)
```

### **7. Controlador devuelve respuesta**
```http
HTTP/1.1 201 Created
Content-Type: application/json

{
  "productId": "guid...",
  "name": "Producto X",
  "currentPrice": 99.99
}
```

### **8. Servicio devuelve resultado al componente**
```csharp
var result = await ProductsService.CreateProducts(dto);
// result contiene el producto creado
```

### **9. Componente actualiza UI**
```csharp
if (result != null)
{
    NotificationService.Notify(/* éxito */);
    await grid.Reload();  // Recarga la tabla
}
```

### **10. Blazor renderiza cambios**
- El DataGrid se actualiza
- El usuario ve el nuevo producto en la lista

---

## 🎯 Patrones y Mejores Prácticas que Aplico

### **1. Repository Pattern (implícito con EF Core)**
No creo repositorios explícitos; uso `DbContext` como repositorio.

### **2. Service Layer Pattern**
Los servicios del cliente encapsulan toda la comunicación HTTP.

### **3. DTO Pattern**
Separo modelos de negocio de modelos de transferencia.

### **4. Dependency Injection**
Todo se inyecta: servicios, DbContext, HttpClient.

### **5. Async/Await**
Todas las operaciones I/O son asíncronas para mejor rendimiento.

### **6. Separation of Concerns**
- **Cliente**: UI + lógica de presentación
- **Servidor**: API + lógica de negocio + acceso a datos
- **Base de Datos**: Almacenamiento

### **7. Single Responsibility Principle**
Cada clase tiene una única responsabilidad:
- `ProductsController` → gestiona endpoints de productos
- `ProductsService` → consume API de productos
- `ProductList` → renderiza lista de productos

---

## 🚀 Flujo Completo: Crear una Factura

Voy a explicar todo el proceso de creación de una factura con líneas:

### **Paso 1: Usuario hace clic en "Nueva Factura"**
```csharp
// En SalesInvoiceHeaderList.razor.cs
protected async Task GoToAdd()
{
    var result = await DialogService.OpenAsync<SalesInvoiceHeaderEdit>(
        "Nueva Factura",
        new Dictionary<string, object?> { { "IsNew", true } }
    );
}
```

### **Paso 2: Se abre SalesInvoiceHeaderEdit**
```csharp
protected override async Task OnInitializedAsync()
{
    // Cargo datos maestros (clientes, condiciones pago, productos, IVAs)
    await LoadMasterData();
    
    // Creo factura vacía
    invoice = new SalesInvoiceHeaders
    {
        SalesInvoiceHeaderId = "",
        InvoiceDate = DateTime.Now,
        DueDate = DateTime.Now.AddDays(30)
    };
    
    invoiceLines = new List<SalesInvoiceLines>();
}
```

### **Paso 3: Usuario rellena datos y añade líneas**
```csharp
private async Task AddLine()
{
    // Abro sub-diálogo para nueva línea
    var line = await DialogService.OpenAsync<SalesInvoiceLineEdit>(
        "Nueva Línea",
        new Dictionary<string, object?> 
        { 
            { "IsNew", true },
            { "Products", products },
            { "TaxRates", taxRates }
        }
    );
    
    if (line != null)
    {
        invoiceLines.Add(line);  // Añado a la lista en memoria
    }
}
```

### **Paso 4: Usuario hace clic en "Guardar"**
```csharp
private async Task OnSubmit()
{
    // Mapeo cabecera + líneas a DTO
    var dto = new SalesInvoiceHeadersDTO
    {
        SalesInvoiceHeaderId = invoice.SalesInvoiceHeaderId,
        CustomerReference = invoice.CustomerReference,
        InvoiceDate = invoice.InvoiceDate,
        DueDate = invoice.DueDate,
        CustomerId = invoice.CustomerId,
        PaymentTermsId = invoice.PaymentTermsId,
        Lines = invoiceLines.Select(l => new SalesInvoiceLinesDTO
        {
            SalesInvoiceLineId = l.SalesInvoiceLineId,
            ProductId = l.ProductId,
            TaxRateId = l.TaxRateId,
            UnitPrice = l.UnitPrice,
            Quantity = l.Quantity,
            CustomDescription = l.CustomDescription
        }).ToList()
    };
    
    // Envío al servidor
    await SalesInvoiceHeadersService.CreateSalesInvoiceHeaders(dto);
}
```

### **Paso 5: Controlador crea cabecera + líneas**
```csharp
[HttpPost]
public IActionResult Post([FromBody] SalesInvoiceHeadersDTO dto)
{
    // Creo cabecera
    var newInvoice = new SalesInvoiceHeaders
    {
        SalesInvoiceHeaderId = dto.SalesInvoiceHeaderId,
        CustomerReference = dto.CustomerReference,
        // ... resto de campos
        Lines = new List<SalesInvoiceLines>()
    };
    
    // Añado líneas
    foreach (var lineDto in dto.Lines ?? Enumerable.Empty<SalesInvoiceLinesDTO>())
    {
        newInvoice.Lines.Add(new SalesInvoiceLines
        {
            SalesInvoiceLineId = lineDto.SalesInvoiceLineId,
            ProductId = lineDto.ProductId,
            TaxRateId = lineDto.TaxRateId,
            UnitPrice = lineDto.UnitPrice,
            Quantity = lineDto.Quantity,
            CustomDescription = lineDto.CustomDescription
        });
    }
    
    // EF Core guarda cabecera + líneas en una transacción
    context.SalesInvoiceHeaders.Add(newInvoice);
    context.SaveChanges();
    
    return Created(newInvoice);
}
```

### **Paso 6: SQL generado por EF Core**
```sql
BEGIN TRANSACTION

INSERT INTO SalesInvoiceHeaders (...)
VALUES (...)

INSERT INTO SalesInvoiceLines (...)
VALUES (...), (...), (...)  -- Múltiples líneas

COMMIT
```

### **Paso 7: Respuesta al cliente**
```csharp
// En SalesInvoiceHeaderEdit
DialogService.Close(true);  // Cierro diálogo

// En SalesInvoiceHeaderList
if (result == true)
{
    await grid.Reload();  // Recargo tabla con nueva factura
}
```

---

## 📊 Tecnologías y Librerías Utilizadas

### **Backend (Server)**
- **ASP.NET Core 9**: Framework web
- **Entity Framework Core 9**: ORM para SQL Server
- **OData v8**: Protocolo de consultas avanzadas
- **QuestPDF**: Generación de PDFs
- **SQL Server**: Base de datos relacional

### **Frontend (Client)**
- **Blazor WebAssembly**: Framework SPA con C#
- **Radzen Blazor**: Biblioteca de componentes UI
- **HttpClient**: Comunicación HTTP con el servidor

### **Herramientas de Desarrollo**
- **.NET CLI**: `dotnet build`, `dotnet run`, `dotnet ef`
- **Visual Studio 2022**: IDE principal
- **Git**: Control de versiones

---

## 🎓 Conceptos Clave que Domino

### **Entity Framework Core**
- Migrations para evolución del esquema
- Relaciones (1-N, N-M) con propiedades de navegación
- Lazy Loading vs Eager Loading (`.Include()`)
- Tracking vs No-Tracking (`.AsNoTracking()`)
- Manejo de transacciones

### **OData**
- Consultas del lado del cliente (`$filter`, `$orderby`, `$expand`)
- EDM (Entity Data Model)
- Controladores OData con `[EnableQuery]`

### **Blazor**
- Ciclo de vida de componentes
- Inyección de dependencias con `[Inject]`
- Parámetros de componente con `[Parameter]`
- `StateHasChanged()` para forzar re-renderizado
- Interactividad WebAssembly vs Server

### **Arquitectura**
- Separación cliente-servidor
- DTOs para transferencia de datos
- Servicios para encapsular lógica
- Controladores como endpoints REST

### **C# Avanzado**
- Async/Await para operaciones asíncronas
- LINQ para consultas sobre colecciones
- Nullable Reference Types (`?`)
- Required members (`required`)
- Records y pattern matching

---

## 📝 Conclusión

Este proyecto me ha permitido implementar un sistema de facturación empresarial completo usando tecnologías modernas de .NET. He aprendido a:

✅ Diseñar modelos de datos normalizados con EF Core  
✅ Crear APIs REST potentes con OData  
✅ Desarrollar interfaces interactivas con Blazor WebAssembly  
✅ Gestionar relaciones maestro-detalle (facturas con líneas)  
✅ Generar documentos PDF profesionales  
✅ Implementar validaciones multi-capa  
✅ Aplicar patrones de diseño (DI, DTO, Service Layer)  
✅ Optimizar consultas con LINQ y $expand  

El resultado es una aplicación escalable, mantenible y con una excelente experiencia de usuario. 🚀

---

**Autor**: Juan Carlos Alonso  
**Tecnologías**: .NET 9, Blazor WebAssembly, Entity Framework Core, OData, QuestPDF  
**Repositorio**: https://github.com/JKFullDev/InvoicingSystem
