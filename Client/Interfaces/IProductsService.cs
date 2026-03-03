using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface IProductsService
    {
        // GET (Listado) - Query puede ser null
        Task<ODataServiceResult<Products>?> GetProducts(Query? query);

        // POST (Crear enviando DTO)
        Task<Products?> CreateProducts(ProductsDTO paymentTermsDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdateProducts(Guid ProductsId, Products paymentTerms);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplaceProducts(Guid ProductsId, ProductsDTO paymentTermsDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeleteProducts(Guid ProductsId);

        // Verifico si un producto está en facturas
        Task<(bool isInvoiced, int count)> IsProductInvoiced(Guid productId);
    }
}
