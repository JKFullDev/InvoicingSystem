using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class Index : ComponentBase
    {
        [Inject] protected ISalesInvoiceHeadersService InvoicesService { get; set; } = default!;
        [Inject] protected ICustomersService CustomersService { get; set; } = default!;
        [Inject] protected IProductsService ProductsService { get; set; } = default!;
        [Inject] protected ISalesInvoiceLinesService LinesService { get; set; } = default!;

        private bool isLoading = true;

        // Métricas
        private int totalInvoices = 0;
        private int totalCustomers = 0;
        private int totalProducts = 0;
        private int overdueInvoices = 0;  // Vencidas
        private int pendingInvoices = 0;  // Pendientes (aún en plazo)
        private decimal totalRevenue = 0;

        // Últimas facturas
        private IEnumerable<SalesInvoiceHeaders>? latestInvoices;

        protected override async Task OnInitializedAsync()
        {
            await LoadDashboardData();
        }

        private async Task LoadDashboardData()
        {
            isLoading = true;

            try
            {
                var now = DateTime.Now;

                // Traigo facturas con sus líneas para calcular el total
                var invoicesQuery = new Query
                {
                    OrderBy = "InvoiceDate desc",
                    Expand = "Lines"
                };
                var invoicesResult = await InvoicesService.GetSalesInvoiceHeaders(invoicesQuery);

                if (invoicesResult?.Value != null)
                {
                    var allInvoices = invoicesResult.Value.ToList();

                    totalInvoices = allInvoices.Count;

                    // Cuento vencidas vs pendientes
                    overdueInvoices = allInvoices.Count(i => i.DueDate < now);
                    pendingInvoices = allInvoices.Count(i => i.DueDate >= now);

                    // Sumo el total de todas las facturas
                    totalRevenue = 0;
                    foreach (var invoice in allInvoices)
                    {
                        if (invoice.Lines != null && invoice.Lines.Any())
                        {
                            totalRevenue += invoice.Lines.Sum(line => line.TotalLine);
                        }
                    }

                    // Me quedo con las 5 más recientes
                    latestInvoices = allInvoices.Take(5);
                }

                // Traigo clientes
                var customersResult = await CustomersService.GetCustomers(null);
                if (customersResult?.Value != null)
                {
                    totalCustomers = customersResult.Value.Count();
                }

                // Traigo productos
                var productsResult = await ProductsService.GetProducts(null);
                if (productsResult?.Value != null)
                {
                    totalProducts = productsResult.Value.Count();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] LoadDashboardData: {ex.Message}");
                Console.WriteLine($"[ERROR] StackTrace: {ex.StackTrace}");
            }
            finally
            {
                isLoading = false;
            }
        }
    }
}
