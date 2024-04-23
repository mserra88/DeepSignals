using DeepSignals.Models;
using DeepSignals.Services;
using DeepSignals.Settings.Helpers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.JSInterop;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using Google.Protobuf;
using static DeepSignals.Workers.HubMainWorker;

using System;
using System;
using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using Microsoft.ML.Data; // Agregar esta directiva para resolver el error CS0246
using Microsoft.AspNetCore.Components.Routing;
using System.Linq;

namespace DeepSignals.Components
{
    public partial class AppStateProvider : ComponentBase, IDisposable
    {

        [Inject]
        public IJSRuntime IJSRuntime { get; set; }

        [Inject]
        public IMemoryCache IMemoryCache { get; set; }

        [Inject]
        public NavigationManager NavigationManager { get; set; }

        [Inject]
        public SignalRService SignalRService { get; set; }

        [Inject]
        public SQLService SQLService { get; set; }

        [Inject]
        public ProtectedStorageService ProtectedStorageService { get; set; }

        [Parameter]
        public Session Session { get; set; }

        [Parameter]
        public RenderFragment? ChildContent { get; set; }

        [Parameter]
        public string AppName { get; set; }

        public bool IsLoaded { get; private set; }

        public IEnumerable<Tickers> filteredTickerList { get; set; }

        public IEnumerable<Tickers> UNfilteredTickerList => SQLService.Tickers;

        public string Title { get; set; }


        public string firstParameter { get; set; }
        public string secondParameter { get; set; }

        public void Navigate(ChangeEventArgs e) => NavigationManager.NavigateTo($"markets/{(string)e.Value}"); //Pasar la url COMPLETA.

        private bool redirectToNotFound = false;



        public void ProcessCurrentLocation(bool init = false)
        {
            if (init)
            {
               NavigationManager.LocationChanged += HandleLocationChanged;
            }

            Title = null; //??


            filteredTickerList = UNfilteredTickerList;


            var page = MappedDictionary.StateToInfo;

            string[] segments = NavigationManager.Uri.Split('/');
            string root = segments.ElementAtOrDefault(3);
            firstParameter = segments.ElementAtOrDefault(4);
            secondParameter = segments.ElementAtOrDefault(5);

            _state = LocationState.NotFound;



            if (string.IsNullOrEmpty(root))
            {
                _state = LocationState.Home;
            }
            else 
            {
                if (root.ToLower() == "markets" && string.IsNullOrEmpty(firstParameter) && string.IsNullOrEmpty(secondParameter))
                {
                    _state = LocationState.Markets;
                }
                else if (!string.IsNullOrEmpty(firstParameter))
                {
                    bool hasType = Enum.TryParse(typeof(Models.Type), firstParameter, out object type);
                    if (hasType && (!string.IsNullOrEmpty(secondParameter) && secondParameter != "popular"))
                    {
                        object sector = null;
                        object exchange = null;

                        if (Enum.TryParse(typeof(Stocks), secondParameter, out sector)
                            || Enum.TryParse(typeof(Cryptocurrencies), secondParameter, out sector))
                        {
                            /*
                            var id = (LocationState)Enum.Parse(typeof(LocationState), secondParameter);

                            LocationStateMapperConfig.StateToInfo.TryGetValue(id, out var stateInfo);

                            //var sec = (Enum)Enum.Parse(typeof(TickerType), firstParameter);

                            */
                            _state = LocationState.Sector;

                            Title = page[(Enum)type].Sector[(Enum)sector];


                            filteredTickerList = filteredTickerList.Where(item => item.type == type.ToString() && item.sector == sector.ToString());//.ToList();

                            //NO//filteredTickerList = filteredTickerList.Where(group => group.Any(item => item.type == (int)type && item.sector == (int)sector));

                            //SI////SI//filteredTickerList = filteredTickerList.SelectMany(group => group.Where(item => item.type == (int)type && item.sector == (int)sector).Select(item => new { GroupKey = group.Key, Ticker = item })).GroupBy(item => item.GroupKey, item => item.Ticker);

                            //SI//filteredTickerList = filteredTickerList.SelectMany(group => group).Where(item => item.type == (int)type && item.sector == (int)sector).GroupBy(item => item.type, item => item);
                        }
                        else if (Enum.TryParse(typeof(Exchanges), secondParameter, out exchange)
                            || Enum.TryParse(typeof(ExchangesCripto), secondParameter, out exchange))
                        {
                            _state = LocationState.Exchange;//añadir location state market/exchange.

                            Title = page[(Enum)type].Exchange[(Enum)exchange];

                            filteredTickerList = filteredTickerList.Where(item => item.type == type.ToString() && item.exchange == exchange.ToString());//.ToList();

                            //NO//filteredTickerList = filteredTickerList.Where(group => group.Any(item => item.type == (int)type && item.exchange == (int)exchange));

                            //SI////SI//filteredTickerList = filteredTickerList.SelectMany(group => group.Where(item => item.type == (int)type && item.exchange == (int)exchange).Select(item => new { GroupKey = group.Key, Ticker = item })).GroupBy(item => item.GroupKey, item => item.Ticker);

                            //SI//filteredTickerList = filteredTickerList.SelectMany(group => group).Where(item => item.type == (int)type && item.exchange == (int)exchange).GroupBy(item => item.type, item => item);
                        }
                    }
                    else if (hasType && (string.IsNullOrEmpty(secondParameter) || secondParameter == "popular"))
                    {
                        Title = page[(Enum)type].Type;

                        /*
                        var sector = page[(Enum)type].Sector;
                        if (sector != null)
                        {
                            filteredSectorList = filteredTickerList.SelectMany(group => group.Where(item => item.type == (int)type).Select(item => new { GroupKey = group.Key, Ticker = item })).GroupBy(item => item.GroupKey, item => item.Ticker);
                        }
                        */



                        filteredTickerList = filteredTickerList.Where(item => item.type == type.ToString());//.ToList();
                        //filteredTickerList = filteredTickerList.Where(group => group.Any(item => item.type == (int)type));/////////NOSEYO.

                        _state = LocationState.Type;

                        if (secondParameter == "popular")
                        {
                            Title = Title + secondParameter;

                            filteredTickerList = filteredTickerList.Where(item => item.popular == secondParameter);
                            //filteredTickerList = filteredTickerList.Where(item => item.popular == true);


                            //SI//filteredTickerList = filteredTickerList.SelectMany(group => group.Where(item => item.popular == true).Select(item => new { GroupKey = group.Key, Ticker = item })).GroupBy(item => item.GroupKey, item => item.Ticker);                            // Filtrar nuevamente para incluir solo elementos populares

                            _state = LocationState.Popular;
                        }
                    }
                    else if (!hasType && filteredTickerList.FirstOrDefault(item => item.name == firstParameter.ReplaceSpecialCharacters())?.name != null) //cambiar lo de null 
                    {
                        //filteredTickerList.Any(group => group.Any(item => item.name == firstParameter.ReplaceSpecialCharacters()))
                        //VALIDO COMPLETAMENTE//!string.IsNullOrEmpty(filteredTickerList.SelectMany(group => group).FirstOrDefault(item => item.name == firstParameter.ReplaceSpecialCharacters())?.name)

                        //no //

                        _state = LocationState.Ticker;

                        Title = firstParameter;
                    }
                    else if (!hasType && firstParameter.ToLower() == "movers")
                    {
                        //obtener los que mayor % cambio porcentual tengan a lo largo y durante el dia

                        //select los que mas %tengan
                        _state = LocationState.Movers;

                    }
                }
            }

            if (IsNotFound() && !redirectToNotFound)
            {
                redirectToNotFound = true; // Establece el flag
                NavigationManager.NavigateTo("NotFound");
            }

            if (string.IsNullOrEmpty(Title))
            {
                Title = page[_state].Type;
            }

         // StateHasChanged();//MUAJAJJAJA
        }



        /*
    
        public string GetTickerTypeName(int key)
        {
            return Enum.GetName(typeof(Models.Type), key);
        }

        public string GetSectorName(int type, int sector)
        {
            if (Enum.IsDefined(typeof(TickerType), type) && TickerSectorMapperConfig.typeToSectorEnum.TryGetValue((TickerType)type, out var sectorEnumType))
            {
                if (Enum.IsDefined(sectorEnumType, sector))
                {
                    return Enum.GetName(sectorEnumType, sector);
                }
            }

            return null;
        }
        */

        LocationState _state { get; set; }

        private bool IsLocation(LocationState stateToCheck) => _state == stateToCheck;

        public bool IsHome() => IsLocation(LocationState.Home);

        public bool IsNotFound() => IsLocation(LocationState.NotFound);
        public bool IsTicker() => IsLocation(LocationState.Ticker);

        public bool IsMovers() => IsLocation(LocationState.Movers);
        public bool IsMarkets() => IsLocation(LocationState.Markets);
        public bool IsType() => IsLocation(LocationState.Type);
        public bool IsSector() => IsLocation(LocationState.Sector);
        public bool IsExchange() => IsLocation(LocationState.Exchange);
        public bool IsPopular() => IsLocation(LocationState.Popular);

        private void HandleLocationChanged(object sender, LocationChangedEventArgs e)
        {

            /*
            var parameters = e.Location.GetParameterValues();
            if (parameters.TryGetValue("parameterName", out var parameterName))
            {
                AppState.OnParametersChanged(parameterName, AnotherParameterName);
            }
            if (parameters.TryGetValue("AnotherParameterName", out var anotherParameterName))
            {
                // Update AnotherParameterName here
            }
            */

            ProcessCurrentLocation();

        }


        protected override async Task OnInitializedAsync()
        {
            try
            {
                //NavigationManager.LocationChanged += HandleLocationChanged;
                //NavigationManager.LocationChanged += HandleLocationChanged;

                ProcessCurrentLocation(true);


                CounterCurrentCount = await GetCounterCurrentCount();

                OnlineClients = SignalRService._clientManagerService._ConnectedUsers;

                SignalRService.HubConnection?.On(Strings.Events.StateHasChanged, async () => {


                    Console.WriteLine("1" );

                    await InvokeAsync(StateHasChanged);
                
                
                });

                SignalRService.HubConnection?.On<string, string>(Strings.Events.ShowPublicMessage, async (user, message) =>
                {
                    var encodedMsg = $"{user}: {message}";
                    PublicMessage.Add(encodedMsg);
                    await InvokeAsync(StateHasChanged);
                });

                await SignalRService.StartConnection(ProtectedStorageService, Session);

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                //HubConnectionStateMessages.Add("There was an error while starting the connection at AppStateProvider.OnInitializedAsync: " + ex.ToString());
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
                // No olvides desuscribirte del evento cuando el componente se desmonta
                NavigationManager.LocationChanged -= HandleLocationChanged;
                await SignalRService.DisposeAsync(Session);
            }
        }

        #region COMPONENTS

        #region Chat
        #region SendPublicMessage
        public string? userInput { get; set; }

        public string? messageInput { get; set; } = "Write a message";


        public async Task SendPublicMessage()
        {
            if (!string.IsNullOrEmpty(messageInput))
            {
                await SignalRService.HubConnection.SendAsync(Strings.Events.SendPublicMessage, userInput, messageInput);
                messageInput = string.Empty;
            }
        }
        #endregion

        #region ShowPublicMessage
        public List<string> PublicMessage { get; set; } = new List<string>();
        #endregion ShowPublicMessage
        #endregion

        #region  Counter
        public int CounterCurrentCount { get; set; }

        private async Task<int> GetCounterCurrentCount()
        {
            var result = await ProtectedStorageService.GetCount();
            return result.Success ? result.Value : 0;
        }

        public async Task Increment()
        {
            CounterCurrentCount++;
            await ProtectedStorageService.SetCount(CounterCurrentCount);
            StateHasChanged();
        }
        #endregion

        public ConcurrentDictionary<string, Dictionary<string, (string, List<(string, string)>)>> OnlineClients { get; set; }
        
        #endregion
    }
}
























/*
 * 


        // Definir la clase de datos de entrada
        // Definir la clase de datos de entrada
        public class SentimentData
        {
            [LoadColumn(0)] public bool Sentiment { get; set; } // true para positivo, false para negativo
            [LoadColumn(1)] public string Text { get; set; }
        }
        // Definir la clase de predicción
        public class SentimentPrediction
        {
            [ColumnName("PredictedLabel")]
            public bool Prediction { get; set; }
        }





var data = new[]
 {
new SentimentData { Sentiment = true, Text = "La empresa XYZ reporta ganancias récord este trimestre." },
new SentimentData { Sentiment = false, Text = "La demanda de petróleo ha disminuido significativamente." },
new SentimentData { Sentiment = true, Text = "El índice de acciones alcanza un nuevo máximo histórico." },
new SentimentData { Sentiment = false, Text = "La empresa ABC anuncia despidos masivos debido a pérdidas financieras." },
new SentimentData { Sentiment = true, Text = "La empresa XYZ lanza un innovador producto al mercado." },
new SentimentData { Sentiment = false, Text = "El precio del oro cae debido a la incertidumbre económica." },
new SentimentData { Sentiment = true, Text = "El mercado inmobiliario experimenta un crecimiento sostenido." },
new SentimentData { Sentiment = false, Text = "La compañía de telecomunicaciones reporta una caída en sus ingresos." },
new SentimentData { Sentiment = true, Text = "La bolsa de valores registra un día positivo para los inversores." },
new SentimentData { Sentiment = false, Text = "El mercado de criptomonedas sufre una fuerte corrección." },
new SentimentData { Sentiment = true, Text = "La empresa ABC adquiere una startup de tecnología." },
new SentimentData { Sentiment = false, Text = "El sector automotriz enfrenta desafíos debido a la escasez de chips." },
new SentimentData { Sentiment = true, Text = "La economía del país muestra signos de recuperación." },
new SentimentData { Sentiment = false, Text = "La compañía de alimentos retira un producto por problemas de calidad." },
new SentimentData { Sentiment = true, Text = "Las exportaciones del país aumentan en el último trimestre." },
new SentimentData { Sentiment = false, Text = "La empresa XYZ enfrenta demandas por prácticas anticompetitivas." },
new SentimentData { Sentiment = true, Text = "La industria del turismo se recupera con la apertura de fronteras." },
new SentimentData { Sentiment = false, Text = "La compañía de tecnología recorta puestos de trabajo." },
new SentimentData { Sentiment = true, Text = "El mercado de bienes raíces muestra signos de estabilidad." },
new SentimentData { Sentiment = false, Text = "El sector minorista enfrenta una desaceleración en las ventas." },
new SentimentData { Sentiment = true, Text = "La empresa ABC lanza una campaña de responsabilidad social corporativa." },
new SentimentData { Sentiment = false, Text = "El precio de la gasolina alcanza un nuevo máximo histórico." },
new SentimentData { Sentiment = true, Text = "El sector financiero experimenta un período de crecimiento." },
new SentimentData { Sentiment = false, Text = "El fabricante de dispositivos electrónicos cierra una planta de producción." },
new SentimentData { Sentiment = true, Text = "El país alcanza un superávit en su balanza comercial." },
new SentimentData { Sentiment = false, Text = "La empresa de software anuncia una disminución en sus ganancias." }
};
// Crear un contexto ML.NET
var mlContext = new MLContext();

// Convertir los datos en un IDataView
var dataView = mlContext.Data.LoadFromEnumerable(data);

// Renombrar la columna de etiquetas a "Label"
// Renombrar la columna de etiquetas a "Label"
var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(SentimentData.Sentiment))
    .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text)))
    .Append(mlContext.Transforms.NormalizeMinMax("Features"));

          //  .Append(mlContext.BinaryClassification.Trainers.SdcaLogisticRegression());

// Configurar el pipeline de transformación y entrenamiento del modelo
/*var pipeline = mlContext.Transforms.Conversion.MapValueToKey("Label", nameof(SentimentData.Sentiment))
    .Append(mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))
        .Append(mlContext.Transforms.Text.TokenizeIntoWords("Tokens", "Features"))
        .Append(mlContext.Transforms.Text.RemoveDefaultStopWords("Tokens", "Tokens")));

*/
/*
                // Entrenar el modelo
                var model = pipeline.Fit(dataView);

                // Clasificación de nuevas noticias
                var predictionEngine = mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);
                var new_text = new SentimentData { Text = "Las acciones de la empresa XYZ suben después del anuncio de una investigación regulatoria." };
                var prediction = predictionEngine.Predict(new_text);

                Console.WriteLine("Sentimiento de la noticia: " + (prediction.Prediction ? "Positivo" : "Negativo"));

                Console.ReadLine();

*/