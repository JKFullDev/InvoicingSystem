using Radzen;
using InvoicingSystem.Server.Components;
using Microsoft.EntityFrameworkCore;
using InvoicingSystem.Server.Data;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.Edm;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Client.Services;
using InvoicingSystem.Client.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRAR EL DBCONTEXT (Faltaba esto para que el Controller pueda usar la DB)
builder.Services.AddDbContext<InvoicingSystemDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. CONFIGURAR ODATA (Faltaba toda esta parte para que las rutas /odata funcionen)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Ignoro referencias circulares para evitar errores de serialización
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    })
    .AddOData(options => options
        .Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
        .AddRouteComponents("odata/InvoicingSystem", GetEdmModel()));

builder.Services.AddRazorComponents()
      .AddInteractiveWebAssemblyComponents();
builder.Services.AddScoped<ICustomersService, CustomersService>();
builder.Services.AddScoped<IPaymentTermsService, PaymentTermsService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ITaxRatesService, TaxRatesService>();
builder.Services.AddScoped<ISalesInvoiceLinesService, SalesInvoiceLinesService>();
builder.Services.AddScoped<ISalesInvoiceHeadersService, SalesInvoiceHeadersService>();
builder.Services.AddScoped<ICartService, CartService>(); // Para evitar error de prerendering
builder.Services.AddSingleton<SidebarStateService>();  // Servicio para controlar el sidebar principal
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "InvoicingSystemTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddHttpClient();

var app = builder.Build();

// Configuración del pipeline
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

// 3. MAPEAR LOS CONTROLADORES
app.MapControllers();

app.MapRazorComponents<App>()
   .AddInteractiveWebAssemblyRenderMode()
   .AddAdditionalAssemblies(typeof(InvoicingSystem.Client._Imports).Assembly);

app.Run();

// 4. DEFINIR EL MODELO EDM (Obligatorio para OData)
static IEdmModel GetEdmModel()
{
    var builder = new ODataConventionModelBuilder();

    // Configuración de EntitySets
    builder.EntitySet<Customers>("Customers");
    builder.EntitySet<PaymentTerms>("PaymentTerms");
    builder.EntitySet<Products>("Products");
    builder.EntitySet<TaxRates>("TaxRates");
    builder.EntitySet<SalesInvoiceLines>("SalesInvoiceLines");
    builder.EntitySet<SalesInvoiceHeaders>("SalesInvoiceHeaders");

    // Configuración de navegaciones para evitar auto-expand
    var salesInvoiceHeaders = builder.EntityType<SalesInvoiceHeaders>();

    // Las navegaciones inversas NO deben auto-expandirse
    salesInvoiceHeaders.HasMany(h => h.Lines).AutoExpand = false;
    salesInvoiceHeaders.HasOptional(h => h.Customer).AutoExpand = false;
    salesInvoiceHeaders.HasOptional(h => h.PaymentTerms).AutoExpand = false;

    var salesInvoiceLines = builder.EntityType<SalesInvoiceLines>();
    salesInvoiceLines.HasOptional(l => l.Product).AutoExpand = false;
    salesInvoiceLines.HasOptional(l => l.TaxRate).AutoExpand = false;

    return builder.GetEdmModel();
}