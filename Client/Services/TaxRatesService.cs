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
    public partial class TaxRatesService : InvoicingSystemBaseService, ITaxRatesService
    {
        // El constructor le pasa las dependencias a la clase base (el principal)
        public TaxRatesService(NavigationManager navigationManager, HttpClient httpClient)
            : base(navigationManager, httpClient)
        {
        }

        #region GET
        public async Task<Radzen.ODataServiceResult<TaxRates>?> GetTaxRates(Query query)
        {
            var uri = new Uri(baseUri, "TaxRates");
            uri = Radzen.ODataExtensions.GetODataUri(uri: uri, filter: $"{query.Filter}", top: query.Top, skip: query.Skip, orderby: $"{query.OrderBy}", count: query.Top != null && query.Skip != null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<TaxRates>>(response);
        }
        #endregion

        #region POST
        public async Task<TaxRates?> CreateTaxRates(TaxRatesDTO productDto)
        {
            var uri = new Uri(baseUri, "TaxRates");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(productDto), Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(httpRequestMessage);
            return await Radzen.HttpResponseMessageExtensions.ReadAsync<TaxRates>(response);
        }
        #endregion

        #region PATCH
        public async Task<HttpResponseMessage> UpdateTaxRates(Guid TaxRateId, TaxRates product)
        {
            var uri = new Uri(baseUri, $"TaxRates({TaxRateId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(product), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region PUT
        public async Task<HttpResponseMessage> ReplaceTaxRates(Guid TaxRateId, TaxRatesDTO productDto)
        {
            var uri = new Uri(baseUri, $"TaxRates({TaxRateId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(productDto), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region DELETE
        public async Task<HttpResponseMessage> DeleteTaxRates(Guid TaxRateId)
        {
            var uri = new Uri(baseUri, $"TaxRates({TaxRateId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region VERIFICACIÓN
        // Consulto si un producto está en facturas
        public async Task<(bool isInvoiced, int count)> IsTaxRateInvoiced(Guid taxRateId)
        {
            var uri = new Uri(baseUri, $"TaxRates({taxRateId})/IsInvoiced");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                var result = System.Text.Json.JsonSerializer.Deserialize<InvoicedStatus>(jsonString, 
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (result?.isInvoiced ?? false, result?.invoiceCount ?? 0);
            }

            return (false, 0);
        }

        // Clase auxiliar para deserializar la respuesta
        private class InvoicedStatus
        {
            public bool isInvoiced { get; set; }
            public int invoiceCount { get; set; }
        }
        #endregion
    }
}