using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface IPaymentTermsService
    {
        // GET (Listado)
        Task<ODataServiceResult<PaymentTerms>?> GetPaymentTerms(Query query);

        // POST (Crear enviando DTO)
        Task<PaymentTerms?> CreatePaymentTerms(PaymentTermsDTO paymentTermsDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdatePaymentTerms(Guid PaymentTermsId, PaymentTerms paymentTerms);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplacePaymentTerms(Guid PaymentTermsId, PaymentTermsDTO paymentTermsDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeletePaymentTerms(Guid PaymentTermsId);
    }
}