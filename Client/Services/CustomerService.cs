using InvoicingSystem.Client.Interfaces;
using InvoicingSystem.Server.Data.Models;
using InvoicingSystem.Server.Data.Models.DTOs;
using Microsoft.AspNetCore.Components;
using Radzen;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace InvoicingSystem.Client.Services
{
    public partial class CustomersService : InvoicingSystemBaseService, ICustomersService
    {
        // El constructor le pasa las dependencias a la clase base (el principal)
        public CustomersService(NavigationManager navigationManager, HttpClient httpClient)
            : base(navigationManager, httpClient)
        {
        }

        #region GET
        public async Task<Radzen.ODataServiceResult<Customers>> GetCustomers(Query query)
        {
            var uri = new Uri(baseUri, "Customers");
            uri = Radzen.ODataExtensions.GetODataUri(uri: uri, filter: $"{query.Filter}", top: query.Top, skip: query.Skip, orderby: $"{query.OrderBy}", count: query.Top != null && query.Skip != null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<Customers>>(response);
        }
        #endregion

        #region POST
        public async Task<Customers> CreateCustomers(CustomersDTO customerDto)
        {
            var uri = new Uri(baseUri, "Customers");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(customerDto), Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(httpRequestMessage);
            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Customers>(response);
        }
        #endregion

        #region PATCH
        public async Task<HttpResponseMessage> UpdateCustomers(string CustomerId, Customers customer)
        {
            var uri = new Uri(baseUri, $"Customers('{CustomerId}')");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(customer), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region PUT
        // Si en algún momento usáis PUT en vez de PATCH, este va con el DTO
        public async Task<HttpResponseMessage> ReplaceCustomers(string CustomerId, CustomersDTO customerDto)
        {
            var uri = new Uri(baseUri, $"Customers('{CustomerId}')");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(customerDto), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region DELETE
        public async Task<HttpResponseMessage> DeleteCustomers(string CustomerId)
        {
            var uri = new Uri(baseUri, $"Customers('{CustomerId}')");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion
    }
}