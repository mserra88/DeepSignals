using Microsoft.AspNetCore.Components;
namespace DeepSignals.Components
{
    public partial class LineChart : ComponentBase
    {
        [CascadingParameter]
        private AppStateProvider? AppStateProvider { get; set; }


        [Parameter]
        public List<Point> DataPoints { get; set; }

        public string DataPointsString
        {
            get
            {
                var points = new List<string>();
                foreach (var point in DataPoints)
                {
                    points.Add($"{point.X},{point.Y}");
                }
                return string.Join(" ", points);
            }
        }

        public class Point
        {
            public int X { get; set; }
            public int Y { get; set; }
        }
    }
}