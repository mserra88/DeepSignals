using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace DeepSignals.Components.Events
{
    public partial class ShowPublicMessage : ComponentBase
    {
        [CascadingParameter] 
        private AppStateProvider? AppStateProvider { get; set; }
    }
}