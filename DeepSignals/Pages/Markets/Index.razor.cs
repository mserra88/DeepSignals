using DeepSignals.Components;
using DeepSignals.Models;
using Microsoft.AspNetCore.Components;
using System.Net.NetworkInformation;

namespace DeepSignals.Pages.Markets
{
    public partial class Index
    {
        [CascadingParameter]
        private AppStateProvider? AppStateProvider { get; set; }

        [Parameter]
        public string? route { get; set; }

        private async Task ReadTickerSelected(string _tickerSelected)
        {
            if (_tickerSelected == "All")
            {
                AppStateProvider.NavigationManager.NavigateTo($"markets/");

            }
            else {
                AppStateProvider.NavigationManager.NavigateTo($"markets/{_tickerSelected}");

            }

        }

        private async Task ReadTickerSelected2(string _tickerSelected)
        {
            if (_tickerSelected == "All")
            {
                AppStateProvider.NavigationManager.NavigateTo($"markets/" + AppStateProvider.firstParameter);//

            }
            else
            {
                AppStateProvider.NavigationManager.NavigateTo($"markets/{AppStateProvider.firstParameter}/{_tickerSelected}"); //

            }

        }
    }
}
