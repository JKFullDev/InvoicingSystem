using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Services
{
    public partial class SalesInvoiceHeadersService : InvoicingSystemBaseService, ISalesInvoiceHeadersService
    {
        // El constructor le pasa las dependencias a la clase base (el principal)
        public SalesInvoiceHeadersService(NavigationManager navigationManager, HttpClient httpClient)
            : base(navigationManager, httpClient)
        {
        }

        #region GET
        public async Task<Radzen.ODataServiceResult<SalesInvoiceHeaders>?> GetSalesInvoiceHeaders(Query query)
        {
            var uri = new Uri(baseUri, "SalesInvoiceHeaders");

            // Añado $expand=Lines para cargar las líneas automáticamente
            var expand = "Lines";

            uri = Radzen.ODataExtensions.GetODataUri(
                uri: uri, 
                filter: $"{query.Filter}", 
                top: query.Top, 
                skip: query.Skip, 
                orderby: $"{query.OrderBy}", 
                expand: expand,
                count: query.Top != null && query.Skip != null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<SalesInvoiceHeaders>>(response);
        }
        #endregion

        #region GET BY CUSTOMER ID
        public async Task<Radzen.ODataServiceResult<SalesInvoiceHeaders>?> GetSalesInvoiceHeadersByCustomerId(string customerId)
        {
            var uri = new Uri(baseUri, "SalesInvoiceHeaders");

            // Construyo la query OData filtrando por el campo CustomerId
            var filter = $"CustomerId eq '{customerId}'";
            
            uri = Radzen.ODataExtensions.GetODataUri(
                uri: uri,
                filter: filter,
                expand: "Lines"
            );

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<SalesInvoiceHeaders>>(response);
        }
        #endregion

        #region GET BY ID
        // Obtengo una factura por ID con sus líneas expandidas usando OData $expand
        public async Task<SalesInvoiceHeaders?> GetSalesInvoiceHeaderById(string salesInvoiceHeaderId)
        {
            var uri = new Uri(baseUri, $"SalesInvoiceHeaders('{salesInvoiceHeaderId}')?$expand=Lines");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode)
            {
                return await Radzen.HttpResponseMessageExtensions.ReadAsync<SalesInvoiceHeaders>(response);
            }

            return null;
        }
        #endregion

        #region POST
        public async Task<SalesInvoiceHeaders?> CreateSalesInvoiceHeaders(SalesInvoiceHeadersDTO productDto)
        {
            var uri = new Uri(baseUri, "SalesInvoiceHeaders");

            // Uso PostAsJsonAsync que serializa correctamente todas las propiedades
            var response = await httpClient.PostAsJsonAsync(uri.ToString(), productDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<SalesInvoiceHeaders>();
            }

            return null;
        }
        #endregion

        #region PATCH
        public async Task<HttpResponseMessage> UpdateSalesInvoiceHeaders(string salesInvoiceHeaderId, SalesInvoiceHeaders salesInvoiceHeaders)
        {
            var uri = new Uri(baseUri, $"SalesInvoiceHeaders('{salesInvoiceHeaderId}')");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(salesInvoiceHeaders), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region PUT
        public async Task<HttpResponseMessage> ReplaceSalesInvoiceHeaders(string salesInvoiceHeaderId, SalesInvoiceHeadersDTO salesInvoiceHeadersDto)
        {
            var uri = new Uri(baseUri, $"SalesInvoiceHeaders('{salesInvoiceHeaderId}')");

            // Uso PutAsJsonAsync que serializa correctamente todas las propiedades
            return await httpClient.PutAsJsonAsync(uri.ToString(), salesInvoiceHeadersDto);
        }
        #endregion

        #region DELETE
        public async Task<HttpResponseMessage> DeleteSalesInvoiceHeaders(string salesInvoiceHeaderId)
        {
            var uri = new Uri(baseUri, $"SalesInvoiceHeaders('{salesInvoiceHeaderId}')");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion
    }
}