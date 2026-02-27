using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using Radzen.Blazor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Pages
{
    public partial class ProductList : ComponentBase
    {
        [Inject] protected IProductsService ProductsService { get; set; } = default!;
        [Inject] protected NotificationService NotificationService { get; set; } = default!;

        // Variables de la tabla
        protected RadzenDataGrid<Products> grid = default!;
        protected IEnumerable<Products>? products;
        protected int count;
        protected bool isLoading = false;

        // Variables del formulario Maestro-Detalle
        protected bool showForm = false;
        protected bool isNew = false;
        protected Products productToEdit = new Products { Name = "", Description = "" };

        // Indico si el producto está en facturas
        protected bool isProductInvoiced = false;
        protected int invoiceCount = 0;

        protected override async Task OnInitializedAsync()
        {
            await InvokeAsync(StateHasChanged);
        }

        protected async Task LoadData(LoadDataArgs args)
        {
            isLoading = true;
            try
            {
                var query = new Query { Filter = args.Filter, OrderBy = args.OrderBy, Skip = args.Skip, Top = args.Top };
                var result = await ProductsService.GetProducts(query);

                if (result != null)
                {
                    products = result.Value;
                    count = result.Count;
                }
            }
            finally
            {
                isLoading = false;
            }
        }

        protected void GoToAdd()
        {
            isNew = true;
            productToEdit = new Products { Name = "", Description = "" };
            isProductInvoiced = false; // Reseteo la advertencia
            invoiceCount = 0;
            showForm = true;
        }

        protected async Task OnRowDoubleClick(DataGridRowMouseEventArgs<Products> args)
        {
            if (args.Data == null) return;

            isNew = false;

            // Clono el producto para editarlo
            productToEdit = new Products
            {
                ProductId = args.Data.ProductId,
                Name = args.Data.Name ?? "",
                Description = args.Data.Description ?? "",
                CurrentPrice = args.Data.CurrentPrice
            };

            // Verifico si el producto está en facturas para mostrar advertencia
            var (isInvoiced, count) = await ProductsService.IsProductInvoiced(args.Data.ProductId);
            isProductInvoiced = isInvoiced;
            invoiceCount = count;

            showForm = true;
        }

        protected void CancelEdit()
        {
            showForm = false;
        }

        protected async Task SaveProduct(Products item)
        {
            try
            {
                // Mapeo el producto a DTO para enviarlo al servidor
                var dto = new ProductsDTO
                {
                    ProductId = item.ProductId,
                    Name = item.Name,
                    Description = item.Description,
                    CurrentPrice = item.CurrentPrice,
                };

                if (isNew)
                {
                    var createdProduct = await ProductsService.CreateProducts(dto);
                    if (createdProduct != null)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Éxito",
                            Detail = "Producto creado correctamente",
                            Duration = 4000
                        });
                    }
                }
                else
                {
                    // Uso PUT para actualizar completamente el producto
                    var response = await ProductsService.ReplaceProducts(item.ProductId, dto);

                    if (response.IsSuccessStatusCode)
                    {
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Success,
                            Summary = "Éxito",
                            Detail = "Producto actualizado correctamente",
                            Duration = 4000
                        });
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        NotificationService.Notify(new NotificationMessage
                        {
                            Severity = NotificationSeverity.Error,
                            Summary = "Error al actualizar",
                            Detail = $"No se pudo actualizar el producto. Código: {response.StatusCode}",
                            Duration = 6000
                        });
                        Console.WriteLine($"Error: {errorContent}");
                        return; // No cierro el formulario si hay error
                    }
                }

                showForm = false;
                await grid.Reload(); // Recargo la tabla para mostrar los cambios
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error al guardar: {ex.Message}",
                    Duration = 6000
                });
                Console.WriteLine($"Error al guardar: {ex.Message}");
            }
        }

        protected async Task DeleteProduct(Guid productId)
        {
            try
            {
                var response = await ProductsService.DeleteProducts(productId);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Éxito",
                        Detail = "Producto eliminado correctamente",
                        Duration = 4000
                    });
                    await grid.Reload();
                    StateHasChanged();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    // Error 409 Conflict - restricción de clave foránea
                    var errorContent = await response.Content.ReadAsStringAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "No se puede eliminar",
                        Detail = "No se puede eliminar este producto porque está siendo utilizado en una o más facturas.",
                        Duration = 6000
                    });
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Warning,
                        Summary = "No encontrado",
                        Detail = "El producto no existe",
                        Duration = 4000
                    });
                }
                else
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error al eliminar el producto",
                        Duration = 4000
                    });
                }
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Error inesperado: {ex.Message}",
                    Duration = 4000
                });
            }
        }
    }
}

