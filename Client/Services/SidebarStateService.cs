namespace InvoicingSystem.Client.Services
{
    public class SidebarStateService
    {
        private bool _isExpanded = true;
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                NotifyStateChanged();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
