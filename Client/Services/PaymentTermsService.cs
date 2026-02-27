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
    public partial class PaymentTermsService : InvoicingSystemBaseService, IPaymentTermsService
    {
        // El constructor le pasa las dependencias a la clase base (el principal)
        public PaymentTermsService(NavigationManager navigationManager, HttpClient httpClient)
            : base(navigationManager, httpClient)
        {
        }

        #region GET
        public async Task<Radzen.ODataServiceResult<PaymentTerms>?> GetPaymentTerms(Query query)
        {
            var uri = new Uri(baseUri, "PaymentTerms");
            uri = Radzen.ODataExtensions.GetODataUri(uri: uri, filter: $"{query.Filter}", top: query.Top, skip: query.Skip, orderby: $"{query.OrderBy}", count: query.Top != null && query.Skip != null);

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
            var response = await httpClient.SendAsync(httpRequestMessage);

            return await Radzen.HttpResponseMessageExtensions.ReadAsync<Radzen.ODataServiceResult<PaymentTerms>>(response);
        }
        #endregion

        #region POST
        public async Task<PaymentTerms?> CreatePaymentTerms(PaymentTermsDTO paymentTermsDto)
        {
            var uri = new Uri(baseUri, "PaymentTerms");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(paymentTermsDto), Encoding.UTF8, "application/json")
            };

            var response = await httpClient.SendAsync(httpRequestMessage);
            return await Radzen.HttpResponseMessageExtensions.ReadAsync<PaymentTerms>(response);
        }
        #endregion

        #region PATCH
        public async Task<HttpResponseMessage> UpdatePaymentTerms(Guid PaymentTermsId, PaymentTerms customer)
        {
            var uri = new Uri(baseUri, $"PaymentTerms({PaymentTermsId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(customer), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region PUT
        public async Task<HttpResponseMessage> ReplacePaymentTerms(Guid PaymentTermsId, PaymentTermsDTO paymentTermsDto)
        {
            var uri = new Uri(baseUri, $"PaymentTerms({PaymentTermsId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Put, uri)
            {
                Content = new StringContent(Radzen.ODataJsonSerializer.Serialize(paymentTermsDto), Encoding.UTF8, "application/json")
            };

            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion

        #region DELETE
        public async Task<HttpResponseMessage> DeletePaymentTerms(Guid PaymentTermsId)
        {
            var uri = new Uri(baseUri, $"PaymentTerms({PaymentTermsId})");
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            return await httpClient.SendAsync(httpRequestMessage);
        }
        #endregion
    }
}