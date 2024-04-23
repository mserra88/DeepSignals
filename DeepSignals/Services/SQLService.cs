using DeepSignals.Models;
using System.Data.SqlClient;
using System.Net;
using System.Reflection;
using DeepSignals.Settings.Helpers;
namespace DeepSignals.Services
{
    public class SQLService
    {
        public IEnumerable<Tickers> Tickers { get; set; }

        public SQLService() 
        {
             Tickers = GetTable<Tickers>().OrderBy(ticker => ticker.type).ThenBy(ticker => ticker.sector).ThenBy(ticker => ticker.name);
        }

        //public IEnumerable<IGrouping<int, Tickers>> Types { get; set; }

        public List<HistoricalModel> GetHistorical(string ticker) 
        {           
            System.Type type = typeof(HistoricalModel);

            var dataList = new List<HistoricalModel>();

            try
            {
                var id = Tickers.FirstOrDefault(item => item.name == ticker).id;
                //var id = Types.SelectMany(group => group).FirstOrDefault(item => item.name == ticker)?.id;


                string query = $"SELECT * FROM Historicals WHERE id = {id}  ORDER BY _Date";

                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        PropertyInfo[] properties = type.GetProperties();

                        while (reader.Read())
                        {
                            HistoricalModel obj = new HistoricalModel();


      
                                foreach (PropertyInfo property in properties)
                                {
                                    if (property.Name != "name")
                                    {
                                        object value = reader[property.Name];//columnName

                                        if (value != DBNull.Value)
                                        {
                                            property.SetValue(obj, value);
                                        }
                                    }

                                }

                                dataList.Add(obj);



                        }
                    }
                }


            }
            catch (Exception ex)
            {
                var t = ex.ToString();
            }

            // dataList = dataList.OrderBy(item => item._Date).ToList();
            // dataList.RemoveAll(item => item._Close == null);

            return dataList;// TaLib.GetHistoricalCalculated(dataList);
        
        
        }
       /*
        public static List<BaseClass> getTickers(string table) =>
            JsonConvert.DeserializeObject<List<BaseClass>>(JsonConvert.SerializeObject(table switch
            {
                "TimeFrame" => Models.TimeFrame.PreloadedData,
                "Libraries" => Models.Libraries.PreloadedData,
                "DataSource" => GetTable<DataSource>(),
                _ => GetTable<Tickers>()
            }));
 */
        private const string CONNECTION_STRING_SERVER = "Data Source=" + DATA_SOURCE + ";" +
"Initial Catalog=" + DATABASE + ";" +
"Database=" + DATABASE + ";" +
"User ID=" + USER_ID + ";" +
"Password=" + PASSWORD + ";" +
"MultipleActiveResultSets=" + MARS + ";" +
"Persist Security Info=" + PSI + ";" +
"Integrated Security=" + IS + ";";

        private const string CONNECTION_STRING_LOCAL = "Data Source=DESKTOP-CATEEQS;Initial Catalog=signals;Database=signals;User ID=test;Password=sa;";


        private static string GetConnectionString()
        {
            string host = Dns.GetHostName().ToString();
            string ConnectionString = "";

            if (host == "DESKTOP-CATEEQS")
            {
                ConnectionString = CONNECTION_STRING_LOCAL;

            }
            else
            {
                ConnectionString = CONNECTION_STRING_SERVER;
            }

            return ConnectionString;
        }



        private void InsertCalculatedDataIntoDatabase(string ticker,  List<HistoricalModel> calculatedDataList)
        {
            int i = 0;
            try
            {
                using (SqlConnection connection = new SqlConnection(GetConnectionString()))
                {
                    connection.Open();

                    string insertQuery = "INSERT INTO [dbo].[Historicals] (id, _Date, _Open, _High, _Low, _Close, _AdjClose, _Volume, _PriceChange, _VolumeChange, _VolumeIndicator, _MA_0, _MA_1, _MA_2, _TRIX, _RSI) VALUES (@id, @Date, @Open, @High, @Low, @Close, @AdjClose, @Volume, @PriceChange, @VolumeChange, @VolumeIndicator, @MA_0, @MA_1, @MA_2, @TRIX, @RSI)";
            
                    foreach (var calculatedData in calculatedDataList)
                    {
                        using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                        {


                           // Console.WriteLine(calculatedData._VolumeIndicator);

                        
                            // Agregar parámetros para la inserción usando los valores calculados
                            insertCommand.Parameters.AddWithValue("@id", calculatedData.id);
                            insertCommand.Parameters.AddWithValue("@Date", calculatedData._Date);


                            insertCommand.Parameters.AddWithValue("@Open", calculatedData._Open != 0 ? (object)calculatedData._Open : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@High", calculatedData._High != 0 ? (object)calculatedData._High : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Low", calculatedData._Low != 0 ? (object)calculatedData._Low : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Close", calculatedData._Close != 0 ? (object)calculatedData._Close : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@AdjClose", calculatedData._AdjClose != 0 ? (object)calculatedData._AdjClose : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@Volume", calculatedData._Volume != 0 ? (object)calculatedData._Volume : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@PriceChange", calculatedData._PriceChange != 0 ? (object)calculatedData._PriceChange : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@VolumeChange", calculatedData._VolumeChange != 0 ? (object)calculatedData._VolumeChange : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@VolumeIndicator", calculatedData._VolumeIndicator != 0 ? (object)calculatedData._VolumeIndicator : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@MA_0", calculatedData._MA_0 != 0 ? (object)calculatedData._MA_0 : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@MA_1", calculatedData._MA_1 != 0 ? (object)calculatedData._MA_1 : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@MA_2", calculatedData._MA_2 != 0 ? (object)calculatedData._MA_2 : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@TRIX", calculatedData._TRIX != 0 ? (object)calculatedData._TRIX : DBNull.Value);
                            insertCommand.Parameters.AddWithValue("@RSI", calculatedData._RSI != 0 ? (object)calculatedData._RSI : DBNull.Value);


                            insertCommand.ExecuteNonQuery();

                            i++;
                        }
                    }
                }

                Console.WriteLine("Los datos han sido almacenados en la base de datos correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: en registro "  + ticker + " de "  + i +  " ........ " + ex.Message);
            }
        }

        private List<HistoricalModel> LoadDataFromCSV(int id , string ticker)
        {
            List<HistoricalModel> dataList = new List<HistoricalModel>();

            // Ruta del archivo .csv
            string csvFilePath = "C:\\Users\\Usuario\\Desktop\\dev\\DeepSignals\\DeepSignals\\DeepSignals\\wwwroot\\" + ticker + ".csv";

            try
            {
                using (StreamReader reader = new StreamReader(csvFilePath))
                {
        
                    bool firstLineSkipped = false;

                    while (!reader.EndOfStream)
                    {
                        string[] rows = reader.ReadLine().Split(',');

                        // Saltar la primera línea
                        if (!firstLineSkipped)
                        {
                            firstLineSkipped = true;
                            continue;
                        }

                       
                            // Agregar los parámetros a la consulta


                            var date = Convert.ToDateTime(rows[0]);

                            double open, high, low, close, adjClose, volume;

                            double.TryParse(rows[1], out open);
                            double.TryParse(rows[2], out high);
                            double.TryParse(rows[3], out low);
                            double.TryParse(rows[4], out close);
                            double.TryParse(rows[5], out adjClose);
                            double.TryParse(rows[6], out volume);

                            HistoricalModel historicalData = new HistoricalModel
                            {
                                id= id,
                                _Date = date,
                                _Open = open,
                                _High = high,
                                _Low = low,
                                _Close = close,
                                _AdjClose = adjClose,
                                _Volume = volume
                            };

                            dataList.Add(historicalData);
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error loading data from CSV: " + ex.Message);
            }

            return dataList;
        }

        public void INSERT()
        {
            foreach (var tickerOb in Tickers)
            {
                //var tickerOb = tickerGroup.FirstOrDefault();  // Obtener el primer elemento del grupo

                var data = LoadDataFromCSV(tickerOb.id, tickerOb.name);

                // Realizar cálculos en los datos para este ticker
                data = TaLib.GetHistoricalCalculated(tickerOb.type, data);

                // Insertar los datos calculados en la base de datos
                InsertCalculatedDataIntoDatabase(tickerOb.name, data);
            }
        }

        public static (string, string) GetType(Models.Type tickerType)
        {
            if (Models.MappedDictionary.StateToInfo[tickerType].Type != null)
            {
                return (Models.MappedDictionary.StateToInfo[tickerType].Type, tickerType.ToString());
            }
            return ("", "");
        }

        public static (string, string) GetSector(Models.Type tickerType, string sectorValue)
        {
            if (Models.MappedDictionary.StateToInfo[tickerType].Sector != null)
            {
                var sectorEnumType = tickerType == Models.Type.Stocks ? typeof(Models.Stocks) : typeof(Models.Cryptocurrencies);
                if (Enum.TryParse(sectorEnumType, sectorValue, out var sectorEnum)) //.ToString("D1")
                {
                    return (Models.MappedDictionary.StateToInfo[tickerType].Sector[(Enum)sectorEnum], sectorEnum.ToString());
                }
            }
            return ("", "");
        }

        public static  (string, string) GetExchange(Models.Type tickerType, string exchangeValue)
        {
            if (Models.MappedDictionary.StateToInfo[tickerType].Exchange != null)
            {
                var exchangeEnumType = tickerType == Models.Type.Stocks ? typeof(Models.Exchanges) : typeof(Models.ExchangesCripto);
                if (Enum.TryParse(exchangeEnumType, exchangeValue, out var exchangeEnum))
                {
                    return (Models.MappedDictionary.StateToInfo[tickerType].Exchange[(Enum)exchangeEnum], exchangeEnum.ToString()); ;
                }
            }
            return ("", "");
        }

        public static List<T> GetTable<T>(string? parameterName = null)
        {
            System.Type type = typeof(T);

            var dataList = new List<T>();

            string query = $"SELECT * FROM {type.Name}" + (!string.IsNullOrWhiteSpace(parameterName) ? $" WHERE name = '{parameterName}'" : "");

            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    PropertyInfo[] properties = type.GetProperties();

                    while (reader.Read())
                    {
                        T obj = Activator.CreateInstance<T>();

                        string typeValue = null; // Variable temporal para almacenar el valor de "type"


                        foreach (PropertyInfo property in properties)
                        {
                            object value = reader[property.Name];

                            if (value != DBNull.Value)
                            {
                                if (property.Name == "popular")
                                {
                                    typeValue = "All";
                                    if ((bool)value == true)
                                    {
                                        typeValue = property.Name;
                                    }

                                    property.SetValue(obj, typeValue);

                                }
                                else if(property.Name == "type") 
                                {
                                    typeValue = ((Models.Type)value).ToString(); 

                                    property.SetValue(obj, typeValue);

                                }
                                else if (property.Name == "sector") 
                                {
                                    if(Enum.TryParse<Models.Type>(typeValue, out var t))
                                    {

                                        object sectorStocks = null;

                                        object sectorCurrencies = null;

                                        var isStock = Enum.TryParse(typeof(Stocks), value.ToString(), out sectorStocks);
                                        var isCC = Enum.TryParse(typeof(Cryptocurrencies), value.ToString(), out sectorCurrencies);

                                        if (isStock || isCC)
                                        {
                                            if (MappedDictionary.StateToInfo[(Enum)t].Sector.ContainsKey((Enum)sectorStocks))
                                            {
                                                var x = MappedDictionary.StateToInfo[(Enum)t].Sector[(Enum)sectorStocks];                                    

                                                property.SetValue(obj, sectorStocks.ToString());//x                                 REPLICAR ESTO

                                            }

                                            if (MappedDictionary.StateToInfo[(Enum)t].Sector.ContainsKey((Enum)sectorCurrencies))
                                            {
                                                var x = MappedDictionary.StateToInfo[(Enum)t].Sector[(Enum)sectorCurrencies];

                                                property.SetValue(obj, sectorCurrencies.ToString());//x                                 REPLICAR ESTO

                                            }
                                        }
                                    }

                                }
                                else if (property.Name == "exchange")
                                {
                                    if (Enum.TryParse<Models.Type>(typeValue, out var t))
                                    {
                                        object exchanges = null;
                                        object exchangesCripto = null;

                                        var isexchanges = Enum.TryParse(typeof(Exchanges), value.ToString(), out exchanges);
                                        var isCC = Enum.TryParse(typeof(ExchangesCripto), value.ToString(), out exchangesCripto);

                                        if (isexchanges || isCC)
                                        {
                                            if (MappedDictionary.StateToInfo[(Enum)t].Exchange.ContainsKey((Enum)exchanges))
                                            {
                                                var x = MappedDictionary.StateToInfo[(Enum)t].Exchange[(Enum)exchanges];

                                                property.SetValue(obj, exchanges.ToString());//x                                 REPLICAR ESTO

                                            }

                                            if (MappedDictionary.StateToInfo[(Enum)t].Exchange.ContainsKey((Enum)exchangesCripto))
                                            {
                                                var x = MappedDictionary.StateToInfo[(Enum)t].Exchange[(Enum)exchangesCripto];

                                                property.SetValue(obj, exchangesCripto.ToString());//x                                 REPLICAR ESTO

                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    property.SetValue(obj, value);
                                }




                            }
                        }

                        dataList.Add(obj);
                    }
                }
            }

            return dataList;
        }


        //_config.GetConnectionString("IONOS")
        //Data Source=;Initial Catalog=;Database=;User ID=;Password=;MultipleActiveResultSets=True;Persist Security Info=False;Integrated Security=False;      
        //server=;Initial Catalog=;User ID=;Password=;MultipleActiveResultSets=True;
        //    "MyDatabase": "Data Source=myserver;Initial Catalog=mydatabase;User ID=myuser;Password=mypassword"
        private const string DATA_SOURCE = "db956791730.hosting-data.io";
        private const string DATABASE = "db956791730";
        private const string USER_ID = "dbo956791730";
        private const string PASSWORD = "67Dyfz7hVw7QMJ2";
        private const string MARS = "True";
        private const string PSI = "False";
        private const string IS = "False";


    }
}




/*


foreach (var tickerOb in Tickers)
{
    var id = tickerOb.id;
    var ticker = tickerOb.name;

    // Ruta del archivo .csv
    string csvFilePath = "C:\\Users\\Usuario\\Desktop\\dev\\DeepSignals\\DeepSignals\\DeepSignals\\wwwroot\\" + ticker + ".csv";

    // Cadena de conexión a la base de datos

    try
    {
        // Leer los datos del archivo .csv y realizar inserción fila por fila
        using (StreamReader reader = new StreamReader(csvFilePath))
        {
            using (SqlConnection connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();

                string tableName = "Historicals"; // Reemplaza "nombre_tabla" con el nombre de tu tabla en la base de datos
                string insertQuery = "INSERT INTO [dbo].[Historicals] (id, _Date, _Open, _High, _Low, _Close, _AdjClose, _Volume) VALUES (@id, @Date, @Open, @High, @Low, @Close, @AdjClose, @Volume)";
                // Variable booleana para saltar la primera línea (cabecera)
                bool firstLineSkipped = false;

                while (!reader.EndOfStream)
                {
                    string[] rows = reader.ReadLine().Split(',');

                    // Saltar la primera línea
                    if (!firstLineSkipped)
                    {
                        firstLineSkipped = true;
                        continue;
                    }

                    // Resto del procesamiento como antes
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        // Agregar los parámetros a la consulta
                        command.Parameters.AddWithValue("@id", id);
                        command.Parameters.AddWithValue("@Date", Convert.ToDateTime(rows[0]));

                        double open, high, low, close, adjClose, volume;

                        double.TryParse(rows[1], out open);
                        double.TryParse(rows[2], out high);
                        double.TryParse(rows[3], out low);
                        double.TryParse(rows[4], out close);
                        double.TryParse(rows[5], out adjClose);
                        double.TryParse(rows[6], out volume);

                        command.Parameters.AddWithValue("@Open", open != 0 ? (object)open : DBNull.Value);
                        command.Parameters.AddWithValue("@High", high != 0 ? (object)high : DBNull.Value);
                        command.Parameters.AddWithValue("@Low", low != 0 ? (object)low : DBNull.Value);
                        command.Parameters.AddWithValue("@Close", close != 0 ? (object)close : DBNull.Value);
                        command.Parameters.AddWithValue("@AdjClose", adjClose != 0 ? (object)adjClose : DBNull.Value);
                        command.Parameters.AddWithValue("@Volume", volume != 0 ? (object)volume : DBNull.Value);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        Console.WriteLine("Los datos han sido almacenados en la base de datos correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error: " + ex.Message);
    }

}
 */