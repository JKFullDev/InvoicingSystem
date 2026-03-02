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
    // Este nombre "Customers" debe coincidir con el del Controller
    builder.EntitySet<Customers>("Customers");
    builder.EntitySet<PaymentTerms>("PaymentTerms");
    builder.EntitySet<Products>("Products");
    builder.EntitySet<TaxRates>("TaxRates");
    builder.EntitySet<SalesInvoiceLines>("SalesInvoiceLines");
    builder.EntitySet<SalesInvoiceHeaders>("SalesInvoiceHeaders");
    // Añade aquí el resto de tablas cuando crees sus controllers (Products, etc.)
    return builder.GetEdmModel();
}