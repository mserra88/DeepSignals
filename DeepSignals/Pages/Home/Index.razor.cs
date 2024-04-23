using DeepSignals.Components;
using DeepSignals.Models;
using Microsoft.AspNetCore.Components;

namespace DeepSignals.Pages.Home
{
   public partial class Index
    {
        [CascadingParameter] private AppStateProvider AppStateProvider { get; set; }
    }
}
