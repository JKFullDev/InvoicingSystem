using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface ITaxRatesService
    {
        // GET (Listado)
        Task<ODataServiceResult<TaxRates>?> GetTaxRates(Query query);

        // POST (Crear enviando DTO)
        Task<TaxRates?> CreateTaxRates(TaxRatesDTO paymentTermsDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdateTaxRates(Guid TaxRatesId, TaxRates paymentTerms);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplaceTaxRates(Guid TaxRatesId, TaxRatesDTO paymentTermsDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeleteTaxRates(Guid TaxRatesId);

        // Verifico si está en facturas
        Task<(bool isInvoiced, int count)> IsTaxRateInvoiced(Guid taxRateId);
    }
}