using System;
using System.Net.Http;
using Microsoft.AspNetCore.Components;

namespace InvoicingSystem.Client.Services
{
    // Clase principal de la que heredarán todos los demás servicios
    public class InvoicingSystemBaseService
    {
        protected readonly HttpClient httpClient;
        protected readonly Uri baseUri;
        protected readonly NavigationManager navigationManager;

        public InvoicingSystemBaseService(NavigationManager navigationManager, HttpClient httpClient)
        {
            this.httpClient = httpClient;
            this.navigationManager = navigationManager;
            this.baseUri = new Uri($"{navigationManager.BaseUri}odata/InvoicingSystem/");
        }
    }
}