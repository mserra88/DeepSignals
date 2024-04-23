using DeepSignals.Models;
using DeepSignals.Services;
using DeepSignals.Settings.Helpers;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using static DeepSignals.Workers.HubMainWorker;
using static Strings;

namespace DeepSignals.Components.Events
{
    public partial class ShowHistorical : ComponentBase, IDisposable
    {
        string max = "max";
        string y25 = "y25";
        string y10 = "y10";
        string y3 = "y3";
        string y = "y";
        string m6 = "m6";
        string d = "d";
        string m = "m";
        string w = "w";




        int currentPage = 1;
        int itemsPerPage = 14;
        bool IsFirstPage => currentPage == 1;
        bool IsLastPage => currentPage == TotalPages;

        int TotalPages => (int)Math.Ceiling((double)model.Count / itemsPerPage);


        async Task ChangePage(int pageChange)
        {
            currentPage += pageChange;

            await InvokeAsync(StateHasChanged);

        }


        async Task GoToFirstPage()
        {
            currentPage = 1;
            await InvokeAsync(StateHasChanged);
        }

        async Task GoToLastPage()
        {
            currentPage = TotalPages;
            await InvokeAsync(StateHasChanged);
        }

        string timeframe { get; set; } = "d";
        bool isCalculated = false;

        string showtimeframe { get; set; }

        private async Task Show(string value)
        {
            if (AppStateProvider is not null)
            {

                showtimeframe = value;


                if (showtimeframe == "w") // Semanal
                {
                    if (timeframe == "d") // Diario
                    {
                        // Encontrar la fecha de inicio del último mes (aproximadamente 30 días atrás).
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());
                        DateTime startDate = lastDate.AddDays(-7);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio del último mes.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes al último mes.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                    else if (timeframe == "w") // Semanal
                    {
                        // Encontrar la fecha de inicio de la última semana (aproximadamente 7 días atrás).
                        string lastWeekData = weeklyData[weeklyData.Count - 1]._Date.ToString();
                        string[] dateRange = lastWeekData.Split('#');
                        DateTime startDate = DateTime.Parse(dateRange[0].Trim());
                        DateTime endDate = DateTime.Parse(dateRange[1].Trim());

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de la última semana.
                        int startIndex = weeklyData.FindIndex(data => DateTime.Parse(data._Date.ToString().Split('#')[0].Trim()) >= startDate);

                        // Obtener los datos correspondientes a la última semana.
                        model = weeklyData.GetRange(startIndex, weeklyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == "m") // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Encontrar la fecha de inicio del último mes (aproximadamente 30 días atrás).
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());
                        DateTime startDate = lastDate.AddMonths(-1);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio del último mes.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes al último mes.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                    else if (timeframe == "w") // Semanal
                    {
                        // Encontrar la fecha de inicio del último mes (aproximadamente 30 días atrás).
                        DateTime lastDate = DateTime.Parse(weeklyData[weeklyData.Count - 1]._Date.ToString().Split('#')[1].Trim());
                        DateTime startDate = lastDate.AddMonths(-1); // Obtener la fecha de inicio del último mes.

                        // Filtrar los datos semanales para obtener solo las semanas del último mes.
                        var weeksInLastMonth = weeklyData.Where(data =>
                        {
                            DateTime weekStartDate = DateTime.Parse(data._Date.ToString().Split('#')[0].Trim());
                            DateTime weekEndDate = DateTime.Parse(data._Date.ToString().Split('#')[1].Trim());

                            return weekStartDate >= startDate && weekEndDate <= lastDate;
                        }).ToList();

                        model = weeksInLastMonth;

                        // La variable 'weeksInLastMonth' contiene las semanas del último mes.
                        // Puedes usarla según tus necesidades.
                    }
                }
                else if (showtimeframe == m6) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Obtener la última fecha disponible
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());

                        // Calcular la fecha correspondiente a 6 meses atrás
                        DateTime startDate = lastDate.AddMonths(-6);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de los últimos 6 meses.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes a los últimos 6 meses.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == y) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Obtener la última fecha disponible
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());

                        // Calcular la fecha correspondiente a 6 meses atrás
                        DateTime startDate = lastDate.AddYears(-1);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de los últimos 6 meses.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes a los últimos 6 meses.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == y3) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Obtener la última fecha disponible
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());

                        // Calcular la fecha correspondiente a 6 meses atrás
                        DateTime startDate = lastDate.AddYears(-3);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de los últimos 6 meses.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes a los últimos 6 meses.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == y10) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Obtener la última fecha disponible
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());

                        // Calcular la fecha correspondiente a 6 meses atrás
                        DateTime startDate = lastDate.AddYears(-10);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de los últimos 6 meses.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes a los últimos 6 meses.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == y25) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        // Obtener la última fecha disponible
                        DateTime lastDate = DateTime.Parse(dailyData[dailyData.Count - 1]._Date.ToString());

                        // Calcular la fecha correspondiente a 6 meses atrás
                        DateTime startDate = lastDate.AddYears(-25);

                        // Buscar la posición en la lista donde se encuentra la fecha de inicio de los últimos 6 meses.
                        int startIndex = dailyData.FindIndex(data => DateTime.Parse(data._Date.ToString()) >= startDate);

                        // Obtener los datos correspondientes a los últimos 6 meses.
                        model = dailyData.GetRange(startIndex, dailyData.Count - startIndex);
                    }
                }
                else if (showtimeframe == max) // Mensual
                {
                    if (timeframe == "d") // Diario
                    {
                        model = dailyData;
                    }
                }


                /*
                          int totalCount = monthlyData.Count;
                          int monthlyCount = totalCount / 2;
                          monthlyData = monthlyData.GetRange(totalCount - monthlyCount, monthlyCount);

                           totalCount = weeklyData.Count;
                          int weeklyCount = totalCount / 4;
                          weeklyData = weeklyData.GetRange(totalCount - weeklyCount, weeklyCount);

                           totalCount = dailyData.Count;
                          int dailyCount = totalCount / 8;
                      dailyData = dailyData.GetRange(totalCount - dailyCount, dailyCount);
                      */




                //    AppStateProvider.Increment(); 


            }
        }

        private async Task Group(string value)
        {
            if (AppStateProvider is not null) 
            {

                timeframe = value;


                if (timeframe == w) // Semanal
                {

                    model = weeklyData;


                }
                else if (timeframe == m) // Mensual
                {
                    model = monthlyData;



                }
                else
                {
                    model = dailyData;

                }





                //    AppStateProvider.Increment(); 


            }
        }
        public void Dispose()
        {
            DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                //await AppStateProvider.SignalRService.RemoveSuscriber(AppStateProvider.Session, parametername, Type);
            }
        }

        [CascadingParameter]
        private AppStateProvider? AppStateProvider { get; set; }

        [Parameter]
        public string parametername { get; set; }

        [Parameter]
        public string Type { get; set; } = Strings.ShowHistorical.Type.Historical;

        [Parameter]
        public bool Mini { get; set; }

        [Parameter]
        public bool ParametersSetAsync { get; set; }

        private string previousParameterName;

        public string recievedParameterName { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            if (parametername != previousParameterName)
            {
                update = true;
                await LoadModel();
            }

            previousParameterName = parametername;
        }
        List<HistoricalModel> model { get; set; }
        List<HistoricalModel> tempMODEL { get; set; }

        private List<string> xSource = new List<string>();
        private List<int> ySource = new List<int>();
        private List<object> source = new List<object>();
        private string DivID { get; set; }

        private string? err { get; set; }
        public string? DerivedException { get; set; }


        private bool showChart;
        private bool showDetail;
        private string currentTime { get; set; }
        private double rand { get; set; }
        public bool update { get; set; } = true;


//private List<LineChart.Point> ChartData { get; set; }// analiza linea
  
        PriceData PD = new PriceData();
        List<Candlestick> _candles = new List<Candlestick>();

        Candlestick lastCandle = new Candlestick();
        DateTime _up;
        TickerData tickerData { get { return AppStateProvider.SignalRService._tickerDataService.GetTickerData(parametername); } }


        

        protected override async Task OnInitializedAsync()
        {
            /*
                      DivID = "CHART_test_";
                      if (_static)
                      {
                          DivID += "_" + "_" + Guid.NewGuid().ToString();
                      }
          */




            DivID = "CHART";
            if (!ParametersSetAsync)
            {
                DivID += "_" + Guid.NewGuid().ToString();
            }



/*
            ChartData = new List<LineChart.Point>
        {
            new LineChart.Point { X = 50, Y = 200 },
            new LineChart.Point { X = 100, Y = 150 },
            new LineChart.Point { X = 150, Y = 100 },
            new LineChart.Point { X = 200, Y = 120 },
            new LineChart.Point { X = 250, Y = 180 },
            new LineChart.Point { X = 300, Y = 250 },
            new LineChart.Point { X = 350, Y = 220 },
            new LineChart.Point { X = 400, Y = 180 },
            new LineChart.Point { X = 450, Y = 200 },
        };
*/



                // To retrieve the data for a specific ticker, you can use the GetTickerData method
              




            //AppStateProvider.clientSignalRService.HubConnection.SendAsync("GetHistorical", parametername);
            // Evento de actualización de datos
            //AppStateProvider.SignalRService.HubConnection.On(Strings.Events.ShowHistorical, async (string recievedParameter, PriceData value, Candlestick currentCandle, DateTime updatetime) =>
            //{



                /*

                if (parametername == recievedParameter)
                {





                    // Utilizar el gráfico SVG como desees, por ejemplo, asignarlo a una propiedad y mostrarlo en tu interfaz de usuario
                    // ...
                    /*
                     * 
                    source.Add(ySource);
                    source.Add(xSource);

                    source.Add(DivID);

                    await AppStateProvider.IJSRuntime.InvokeAsync<object>("marketLineChart", source.ToArray());

                    xSource.Clear();
                    ySource.Clear();*/
                /*   }

                    */
            //});

        }

        private string GetCachedSvgChart(string parameterName)
        {
            string cacheKey = "SvgChart"+ parameterName; // Una clave única para el gráfico SVG en la caché

            // Intentar obtener el gráfico SVG de la caché
            if (AppStateProvider.IMemoryCache.TryGetValue(cacheKey, out string cachedSvgChart))
            {
                return cachedSvgChart; // Devolver el gráfico SVG desde la caché si está presente
            }
            else
            {
                // Si el gráfico SVG no está en caché, generarlo y agregarlo a la caché
                string svgChart = GenerateSvgChart();

                var cacheEntryOptions = new MemoryCacheEntryOptions();
                cacheEntryOptions.SetAbsoluteExpiration(TimeSpan.FromMinutes(30));
                cacheEntryOptions.Size = 1;

                cacheEntryOptions.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);
                cacheEntryOptions.Priority = CacheItemPriority.High;
                cacheEntryOptions.Size = 1;

                AppStateProvider.IMemoryCache.Set(cacheKey, svgChart, cacheEntryOptions); // Cachear por 30 minutos (ajusta según tus necesidades)
                return svgChart;
            }
        }

        string SvgChart { get; set; }


        // Método para generar el gráfico SVG
        private string GenerateSvgChart()
        {


            StringBuilder svgChart = new StringBuilder();

            // Configurar atributos del SVG
            svgChart.AppendLine("<svg width=\"500\" height=\"300\">");
            svgChart.AppendLine("<polyline points=\"");

            // Generar los puntos de datos
            for (int i = 0; i < xSource.Count; i++)
            {
                // Obtener las coordenadas x e y del punto de datos
                string x = xSource[i];
                int y = ySource[i];

                // Convertir la coordenada x a un valor de posición en el gráfico
                int xPosition = ConvertXToPosition(x); /* Lógica para convertir la coordenada x a una posición en el gráfico */

                // Convertir la coordenada y a un valor de posición en el gráfico
                int yPosition = ConvertYToPosition(y);/* Lógica para convertir la coordenada y a una posición en el gráfico */

                // Agregar el punto al gráfico SVG
                svgChart.AppendLine($"{xPosition},{yPosition} ");
            }

            // Cerrar el gráfico SVG
            svgChart.AppendLine("\" fill=\"none\" stroke=\"green\" />");
            svgChart.AppendLine("</svg>");

            return svgChart.ToString();
        }

        // Variables para los límites y escala del gráfico
        string minX => xSource.Min();
        string maxX => xSource.Max();
        int minY => ySource.Min();
        int maxY => ySource.Max();

        int chartWidth = 500; // Ancho del gráfico SVG
        int chartHeight = 300; // Alto del gráfico SVG

        // Función para convertir la coordenada x a un valor de posición en el gráfico
        int ConvertXToPosition(string x)
        {
            int xIndex = xSource.IndexOf(x);
            double xRange = xSource.Count - 1;
            double xPosition = xIndex / xRange * chartWidth;
            return (int)xPosition;
        }


        // Función para convertir la coordenada y a un valor de posición en el gráfico
        int ConvertYToPosition(int y)
        {
            double yRange = maxY - minY;
            double yPosition = chartHeight - (y - minY) / yRange * chartHeight;
            return (int)yPosition;
        }

        [Parameter]
        public List<double> Values { get; set; } = new();

        [Parameter]
        public double Max { get; set; }

        [Parameter]
        public double Min { get; set; }

        private int? _selectedPoint;

        private const double GraphHeight = 300.0;
        private const double GraphWidth = 500.0;

        private double GraphRange => Math.Abs(Max) + Math.Abs(Min);
        private double ZeroY => Math.Abs(Max) / GraphRange * GraphHeight;

        private double Xpos(int x)
        {
            return 50.0 + x * (GraphWidth / (Values.Count - 1));
        }

        private double Ypos(double y)
        {
            return ZeroY - y * (GraphHeight / GraphRange);
        }

        // Convert Values to a list of coordinates for the polyline SVG element
        private string PointList =>
            string.Join(" ",
                Values.Select(
                    (y, index) => $"{Xpos(index)},{Ypos(y)}"
                    )
                );

        private string SelectedPointInfo => $"x={_selectedPoint}, y={Values[_selectedPoint.Value]:0.000}";
        string svgCode = "";

        private string GenerateSVGGraph()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 " + (GraphWidth + 100) + " " + GraphHeight + "\">");
            sb.AppendLine("<g stroke=\"gray\" text-anchor=\"end\">");
            sb.AppendLine("<text x=\"40\" y=\"" + ZeroY + "\">0</text>");
            sb.AppendLine("<text x=\"40\" y=\"15\">" + Max + "</text>");
            sb.AppendLine("<text x=\"40\" y=\"" + GraphHeight + "\" stroke=\"green\">" + Min + "</text>");
            sb.AppendLine("<line x1=\"50\" x2=\"50\" y1=\"0\" y2=\"" + GraphHeight + "\"/>");
            sb.AppendLine("<line x1=\"50\" x2=\"" + (GraphWidth + 50) + "\" y1=\"" + ZeroY + "\" y2=\"" + ZeroY + "\"/>");
            sb.AppendLine("</g>");
            sb.AppendLine("<g>");
            sb.AppendLine("<polyline stroke=\"blue\" fill=\"none\" points=\"" + PointList + "\"/>");
            sb.AppendLine("</g>");

            // ... Continúa generando el código SVG para los puntos y la información adicional según sea necesario

            sb.AppendLine("</svg>");

            return sb.ToString();
        }

        public string ToggleDetailButtonText => toggleDetail ? "Show Detail" : "Hide Detail";
        private bool toggleDetail = true;
        private string? DetailCssClass => toggleDetail ? "collapse" : null;

        private async Task ToggleDetail()
        {
            toggleDetail = !toggleDetail;
            await Task.CompletedTask;
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
            }
        }
        private bool addSucriber;
        List<HistoricalModel> weeklyData = new List<HistoricalModel>();
        List<HistoricalModel> monthlyData = new List<HistoricalModel>();
        List<HistoricalModel> dailyData = new List<HistoricalModel>();

        private async Task LoadModel() 
        {







/*
            if (AppStateProvider.SignalRService.IsConnected)
            {
                await AppStateProvider.SignalRService.AddSuscriber(AppStateProvider.Session, parametername, DivID);

            }
            else
            {
                while (!AppStateProvider.SignalRService.IsConnected)
                {
                    // Pausamos la ejecución para evitar un consumo excesivo de recursos.
                    // Puedes ajustar el tiempo de espera según tus necesidades.
                    await Task.Delay(13); // Pausa de 1 segundo (1000 milisegundos).
                                          // También puedes usar Thread.Sleep(1000) si no estás trabajando en un contexto asincrónico.
                }

                // En este punto, AppStateProvider.isLoaded es true y AppStateProvider.IsConnected también es true, lo que significa que la conexión se ha establecido.

                // Agregamos el suscriptor una vez que la conexión está establecida.
                await AppStateProvider.SignalRService.AddSuscriber(AppStateProvider.Session, parametername, DivID);


            }
*/















            /*
            if (AppStateProvider.clientSignalRService.IsConnected)
            {
                await AppStateProvider.clientSignalRService.AddSuscriber(AppStateProvider.clientSessionService, parametername);
            }
            else {
                await AppStateProvider.clientSignalRService.WaitForConnectionAsync();
                /*
                AppStateProvider.clientSignalRService.ConnectionEstablished += async (sender, e) =>
                {                    
                    //await ((ClientSignalRService)sender).WaitForConnectionAsync();
                    //AppStateProvider.AddSuscriber(parametername);
                };
*/
            /*
                            await AppStateProvider.clientSignalRService.AddSuscriber(AppStateProvider.clientSessionService, parametername);
                        }
            */


            // model = AppStateProvider.SQLService.GetHistorical(parametername);



            model = await CacheHelper.TryGetValueOrCreateAsync(AppStateProvider.IMemoryCache, GetHistorical, "HISTORICAL4_" + parametername, parametername, TimeSpan.FromHours(1));

            // Semanal

            List<HistoricalModel> currentWeekPrices = new List<HistoricalModel>();
                                int currentWeekNumber = -1;

                                foreach (var data in model)
                                {
                                    DateTime date = DateTime.Parse(data._Date.ToString());
                                    int weekNumber = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                                    if (currentWeekNumber == -1)
                                    {
                                        currentWeekNumber = weekNumber;
                                    }

                                    if (weekNumber == currentWeekNumber)
                                    {
                                        currentWeekPrices.Add(data);
                                    }
                                    else
                                    {
                                        HistoricalModel weeklySummary = new HistoricalModel
                                        {
                                            //_Date = currentWeekPrices.First()._Date + " # " + currentWeekPrices.Last()._Date,
                                            _Open = currentWeekPrices.First()._Open,
                                            _High = currentWeekPrices.Max(d => d._Close),
                                            _Low = currentWeekPrices.Min(d => d._Close),
                                            _Close = currentWeekPrices.Last()._Close,
                                            _AdjClose = currentWeekPrices.Last()._AdjClose,
                                            _Volume = currentWeekPrices.Sum(d => d._Volume),
                                            // Resto de los campos no mencionados, si necesitas almacenarlos.
                                        };

                                        weeklyData.Add(weeklySummary);
                                        currentWeekPrices.Clear();
                                        currentWeekNumber = weekNumber;
                                        currentWeekPrices.Add(data);
                                    }
                                }

                                // Almacenar el último intervalo
                                if (currentWeekPrices.Count > 0)
                                {
                                    HistoricalModel weeklySummary = new HistoricalModel
                                    {
                                        //_Date = $"{currentWeekPrices.First()._Date} # {currentWeekPrices.Last()._Date}",
                                        _Open = currentWeekPrices.First()._Open,
                                        _High = currentWeekPrices.Max(d => d._Close),
                                        _Low = currentWeekPrices.Min(d => d._Close),
                                        _Close = currentWeekPrices.Last()._Close,
                                        _AdjClose = currentWeekPrices.Last()._AdjClose,
                                        _Volume = currentWeekPrices.Sum(d => d._Volume),
                                        // Resto de los campos no mencionados, si necesitas almacenarlos.
                                    };

                                    weeklyData.Add(weeklySummary);
                                }

                                // En este punto, la lista 'weeklyData' contendrá los precios aglutinados para intervalos semanales.



                        // Mensual
                      
                                List<HistoricalModel> currentMonthPrices = new List<HistoricalModel>();
                                int currentMonthNumber = -1;

                                foreach (var data in model)
                                {
                                    DateTime date = DateTime.Parse(data._Date.ToString());
                                    int monthNumber = date.Month;

                                    if (currentMonthNumber == -1)
                                    {
                                        currentMonthNumber = monthNumber;
                                    }

                                    if (monthNumber == currentMonthNumber)
                                    {
                                        currentMonthPrices.Add(data);
                                    }
                                    else
                                    {
                                        HistoricalModel monthlySummary = new HistoricalModel
                                        {
                                            //_Date = $"{currentMonthPrices.First()._Date} # {currentMonthPrices.Last()._Date}",
                                            _Open = currentMonthPrices.First()._Open,
                                            _High = currentMonthPrices.Max(d => d._Close),
                                            _Low = currentMonthPrices.Min(d => d._Close),
                                            _Close = currentMonthPrices.Last()._Close,
                                            _AdjClose = currentMonthPrices.Last()._AdjClose,
                                            _Volume = currentMonthPrices.Sum(d => d._Volume),
                                            // Resto de los campos no mencionados, si necesitas almacenarlos.
                                        };

                                        monthlyData.Add(monthlySummary);
                                        currentMonthPrices.Clear();
                                        currentMonthNumber = monthNumber;
                                        currentMonthPrices.Add(data);
                                    }
                                }

                                // Almacenar el último intervalo
                                if (currentMonthPrices.Count > 0)
                                {
                                    HistoricalModel monthlySummary = new HistoricalModel
                                    {
                                        //_Date = $"{currentMonthPrices.First()._Date} # {currentMonthPrices.Last()._Date}",
                                        _Open = currentMonthPrices.First()._Open,
                                        _High = currentMonthPrices.Max(d => d._Close),
                                        _Low = currentMonthPrices.Min(d => d._Close),
                                        _Close = currentMonthPrices.Last()._Close,
                                        _AdjClose = currentMonthPrices.Last()._AdjClose,
                                        _Volume = currentMonthPrices.Sum(d => d._Volume),
                                        // Resto de los campos no mencionados, si necesitas almacenarlos.
                                    };

                                    monthlyData.Add(monthlySummary);
                                }

                                // En este punto, la lista 'monthlyData' contendrá los precios aglutinados para intervalos mensuales.



                          
                                // Manejar el caso para intervalo diario (timeframe == "d"), si es necesario.
                                dailyData = model;




            if (timeframe == "w") // Semanal
            {

                model = weeklyData;


            }
            else if (timeframe == "m") // Mensual
            {
                model = monthlyData;



            }
            else
            {

                model = dailyData;

            }











            //Task<string> GETPARAMETERFROMCLIENT();

            if (Type == Strings.ShowHistorical.Type.Chart)
                {
                showChart = true;
                }
                else if (Type == Strings.ShowHistorical.Type.Detail)
                {
                showDetail = true;
                }
                else if (Type == Strings.ShowHistorical.Type.Historical)
                {
                showChart = true;
                showDetail = true;
                }


                /*

                foreach (var historical in AppStateProvider.HistoricalList)
                {
                    if (parametername == historameter)
                    {
                        model = historical.message.GetRange(historical.message.Count - 10, 10);
                        recievedParameterName = historical.recievedParameter;
                        currentTime = historical.time;
                        rand = historical.value;


                    }ical.recievedPar
                }
                */



                if (ParametersSetAsync)
                {
                    //AppStateProvider.clientSignalRService.HubConnection.SendAsync("GetHistorical", parametername);
                }



        }



        private async Task<List<HistoricalModel>> GetHistorical(string ticker, CancellationToken cancellationToken)
        {
            // Tu lógica aquí para obtener los datos históricos usando el parámetro "ticker"
            return AppStateProvider.SQLService.GetHistorical(ticker); //parametername
        }

       
    }
}

/*
protected override bool ShouldRender()
{
    // Comprobar si el parámetro "parametername" ha cambiado
    if (!string.Equals(parametername, previousParameterName))
    {
        previousParameterName = parametername;
        return true; // Renderizar el componente nuevamente
    }

    // Comprobar si otros parámetros de entrada han cambiado
    // Si sí, retornar true para renderizar el componente nuevamente

    return false; // No renderizar el componente nuevamente
}
*/