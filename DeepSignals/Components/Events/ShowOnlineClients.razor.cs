using Microsoft.AspNetCore.Components;

namespace DeepSignals.Components.Events
{
    public partial class ShowOnlineClients : ComponentBase
    {
        string className = "";

        [CascadingParameter] 
        private AppStateProvider? AppStateProvider { get; set; }
    }
}