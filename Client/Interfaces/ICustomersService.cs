using System.Net.Http;
using System.Threading.Tasks;
using Radzen;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;

namespace InvoicingSystem.Client.Interfaces
{
    public interface ICustomersService
    {
        // GET (Listado) - Query puede ser null
        Task<ODataServiceResult<Customers>?> GetCustomers(Query? query);

        // POST (Crear enviando DTO)
        Task<Customers?> CreateCustomers(CustomersDTO customerDto);

        // PATCH (Actualizar enviando Entidad)
        Task<HttpResponseMessage> UpdateCustomers(string CustomerId, Customers customer);

        // PUT (Reemplazar enviando DTO) 
        Task<HttpResponseMessage> ReplaceCustomers(string CustomerId, CustomersDTO customerDto);

        // DELETE (Borrar)
        Task<HttpResponseMessage> DeleteCustomers(string CustomerId);
    }
}
