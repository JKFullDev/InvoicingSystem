using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface ISalesInvoiceLinesService
    {
        // GET (Listado)
        Task<ODataServiceResult<SalesInvoiceLines>?> GetSalesInvoiceLines(Query query);

        // POST (Crear enviando DTO)
        Task<SalesInvoiceLines?> CreateSalesInvoiceLines(SalesInvoiceLinesDTO paymentTermsDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdateSalesInvoiceLines(Guid SalesInvoiceLinesId, SalesInvoiceLines paymentTerms);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplaceSalesInvoiceLines(Guid SalesInvoiceLinesId, SalesInvoiceLinesDTO paymentTermsDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeleteSalesInvoiceLines(Guid SalesInvoiceLinesId);

        // Verifico si está en facturas
        Task<(bool isInvoiced, int count)> IsSalesInvoiceLineInvoiced(Guid salesInvoiceLinesId);
    }
}