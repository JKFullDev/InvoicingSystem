using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface ISalesInvoiceHeadersService
    {
        // GET (Listado)
        Task<ODataServiceResult<SalesInvoiceHeaders>?> GetSalesInvoiceHeaders(Query query);

        // GET (Una factura por ID con líneas expandidas)
        Task<SalesInvoiceHeaders?> GetSalesInvoiceHeaderById(string salesInvoiceHeaderId);

        // POST (Crear enviando DTO)
        Task<SalesInvoiceHeaders?> CreateSalesInvoiceHeaders(SalesInvoiceHeadersDTO salesInvoiceHeadersDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdateSalesInvoiceHeaders(string salesInvoiceHeaderId, SalesInvoiceHeaders salesInvoiceHeaders);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplaceSalesInvoiceHeaders(string salesInvoiceHeaderId, SalesInvoiceHeadersDTO salesInvoiceHeadersDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeleteSalesInvoiceHeaders(string salesInvoiceHeaderId);
    }
}