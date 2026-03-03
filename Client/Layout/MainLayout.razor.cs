using System.Net.Http;
using InvoicingSystem.Client.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;
using Radzen;
using Radzen.Blazor;

namespace InvoicingSystem.Client.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject]
        protected IJSRuntime JSRuntime { get; set; }

        [Inject]
        protected NavigationManager NavigationManager { get; set; }

        [Inject]
        protected DialogService DialogService { get; set; }

        [Inject]
        protected TooltipService TooltipService { get; set; }

        [Inject]
        protected ContextMenuService ContextMenuService { get; set; }

        [Inject]
        protected NotificationService NotificationService { get; set; }

        [Inject]
        protected SidebarStateService SidebarStateService { get; set; }

        private bool sidebarExpanded = true;

        protected override void OnInitialized()
        {
            // Sincronizo con el servicio
            sidebarExpanded = SidebarStateService.IsExpanded;
            SidebarStateService.OnChange += OnSidebarStateChanged;
        }

        private void OnSidebarStateChanged()
        {
            sidebarExpanded = SidebarStateService.IsExpanded;
            StateHasChanged();
        }

        void SidebarToggleClick()
        {
            sidebarExpanded = !sidebarExpanded;
            SidebarStateService.IsExpanded = sidebarExpanded;
        }

        public void Dispose()
        {
            SidebarStateService.OnChange -= OnSidebarStateChanged;
        }
    }
}
