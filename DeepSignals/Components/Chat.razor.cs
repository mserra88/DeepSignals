using Microsoft.AspNetCore.Components;

namespace DeepSignals.Components
{
    public partial class Chat : ComponentBase
    {
        [CascadingParameter] 
        private AppStateProvider? AppStateProvider { get; set; }
    }
}