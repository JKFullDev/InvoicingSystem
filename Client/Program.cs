using InvoicingSystem.Client;
using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Client.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddRadzenComponents();
builder.Services.AddScoped<ICustomersService, CustomersService>();
builder.Services.AddScoped<IPaymentTermsService, PaymentTermsService>();
builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ITaxRatesService, TaxRatesService>();
builder.Services.AddScoped<ISalesInvoiceLinesService, SalesInvoiceLinesService>();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "InvoicingSystemTheme";
    options.Duration = TimeSpan.FromDays(365);
});
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var host = builder.Build();
await host.RunAsync();