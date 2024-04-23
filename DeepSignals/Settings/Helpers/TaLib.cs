using static TicTacTec.TA.Library.Core;
using TicTacTec.TA.Library;
using DeepSignals.Models;
using System.Data.SqlClient;

namespace DeepSignals.Settings.Helpers
{
    public class TaLib //static
    {
        private static readonly int[] _MA = new int[3];

        public static double[] CalculateVolumeIndicator(double[] data)
        {
            int period = 14;
            double[] volumeIndicator = new double[data.Length];
            double sum = 0;
            int count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] != 0)
                {
                    sum += data[i];
                    count++;
                }
                if (i >= period)
                {
                    sum -= data[i - period];
                    count--;
                }
                if (count != 0)
                {
                    volumeIndicator[i] = data[i] / (sum / count);
                }
            }
            return volumeIndicator;
        }

        public static double[] Change(double[] data)
        {
            int period = 1;

            /*
            double[] dataChange = new double[data.Length];

            for (int i = 1; i < data.Length; i++)
            {
                if (i < period)
                {
                    dataChange[i] = (data[i] - data[0]) / data[0] * 100;
                }
                else
                {
                    dataChange[i] = (data[i] - data[i - period]) / data[i - period] * 100;
                }
            }
            return dataChange;
            */

            //TA LiB
            int length = data.Length;
            int begIdx, outIdx;
            double[] ch = new double[length];

            // Calcular el ROC
            RetCode retCode = RocP(0, length - 1, data, 1, out begIdx, out outIdx, ch);

            return arrangeResult(ch, length, begIdx);
        }

        public static double[] MovingAverage(double[] data, int level)
        {
            List<string> _Terms = new List<string> { "Short Term", "Mid Term", "Long Term" };

            string TS = _Terms[0];
            string TM = _Terms[1];
            string TL = _Terms[2];

            string _Term = _Terms[2];

            int period = 0;
            if (level == 1)
            {
                if (_Term == TS)
                {
                    period = 5;
                }
                if (_Term == TM)
                {
                    period = 20;
                }
                if (_Term == TL)
                {
                    period = 50;
                }
                _MA[0] = period;
            }
            else if (level == 2)
            {
                if (_Term == TS)
                {
                    period = 10;
                }
                if (_Term == TM)
                {
                    period = 50;
                }
                if (_Term == TL)
                {
                    period = 100;
                }

                _MA[1] = period;
            }
            else if (level == 3)
            {
                if (_Term == TS)
                {
                    period = 20;
                }
                if (_Term == TM)
                {
                    period = 100;
                }
                if (_Term == TL)
                {
                    period = 200;
                }

                _MA[2] = period;
            }

            //TA LiB
            int length = data.Length;
            int begIdx, outIdx;
            double[] ma = new double[length];



            RetCode retCode = Core.MovingAverage(0, length - 1, data, period, MAType.Sma, out begIdx, out outIdx, ma);

            return arrangeResult(ma, length, begIdx);
        }

        public static double[] TRIX(double[] data)
        {
            int period = 14;
            int length = data.Length;
            int begIdx, outIdx;
            double[] trix = new double[data.Length];
            RetCode retCode = Trix(0, length - 1, data, period, out begIdx, out outIdx, trix);
            return arrangeResult(trix, length, begIdx);
        }

        public static double[] RSI(double[] data)
        {
            int period = 14;
            int length = data.Length;
            int begIdx, outIdx;
            double[] rsi = new double[length];
            RetCode retCode = Rsi(0, length - 1, data, period, out begIdx, out outIdx, rsi);
            return arrangeResult(rsi, length, begIdx);
        }

        public static double[] CalculateBbands(double[] data)
        {
            int period = 14;
            int begIdx, outIdx;
            double[] upperBand = new double[data.Length];
            double[] middleBand = new double[data.Length];
            double[] lowerBand = new double[data.Length];
            RetCode retCode = Bbands(0, data.Length - 1, data, period, 2, 2, MAType.Sma, out begIdx, out outIdx, upperBand, middleBand, lowerBand);
            double[] result = new double[data.Length];
            return result;
        }

        public static double[] arrangeResult(double[] data, int length, int begIdx)
        {
            double[] result = new double[length];
            foreach (int i in Enumerable.Range(0, length))
            {
                if (i > begIdx)
                {
                    result[i] = data[i - begIdx];
                }
            }
            return result;
        }
        public static List<HistoricalModel> GetHistoricalCalculated(string type, List<HistoricalModel> DataList)
        {
            
            try
            {
                // Obtener las listas de fechas y cierres de DataList
                var dataDates = DataList.Select(item => item._Date).ToArray();
                var dataCloses = DataList.Select(item => item._Close).ToArray();
                var dataVolumes = DataList.Select(item => item._Volume).ToArray();


                foreach (int j in Enumerable.Range(0, dataDates.Length))
                {
                    // Actualizar los valores directamente en DataList en lugar de agregar un nuevo objeto a resultList
                    DataList[j]._MA_0 = MovingAverage(dataCloses, 1)[j];
                    DataList[j]._MA_1 = MovingAverage(dataCloses, 2)[j];
                    DataList[j]._MA_2 = MovingAverage(dataCloses, 3)[j];



                    //DataList[j]._PriceChange = Change(dataCloses)[j];
                    //DataList[j]._VolumeChange = Change(dataVolumes)[j];


                    if (type != Models.Type.Currencies.ToString())
                    {
                        DataList[j]._VolumeIndicator = CalculateVolumeIndicator(dataVolumes)[j];

                    }


                    DataList[j]._TRIX = TRIX(dataCloses)[j];
                    DataList[j]._RSI = RSI(dataCloses)[j];
                }

                Console.WriteLine("Los datos han sido calculados correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
            


            
            return DataList; // Ordenar por fecha si es necesario: resultList.OrderByDescending(x => x._Date).ToList();
        }



        
        public static List<HistoricalModel> GetHistoricalCalculated2(string ticker, List<HistoricalModel> DataList)
        {




            ticker = ticker.ToUpper();

            List<List<HistoricalModel>> DatalistList = new List<List<HistoricalModel>>();
            string[] dataDates = { };
            double[] dataCloses = { };
            double[] dataVolumes = { };
            List<string> _Sources = new List<string> { "YAHOO", "GOOGLE", "BING" };
            int[] sourcesIndex = { 0 };
            //string tickerSS = new List<string> { "NSDQ", "SPX500", "DJ30" }[0]; //OJO EL 000000000000000000000000000000000000000000000000
            int[] _SourceIndex = { 0 };

           // List<HistoricalModel> DataList = new List<HistoricalModel>();
            /*
            var test = new List<string> { "NSDQ", "SPX500", "DJ30" }.ToArray().Contains(ticker);
            if (test != true)
            {
                return DataList;
            }
            */


            //Source source = new Source();

            foreach (string currentSource in _Sources) //foreach (int count in sources) + if (sources.Contains(count)) //foreach (int count in Enumerable.Range(0, _Sources.Count))
            {
                if (sourcesIndex.Contains(Array.IndexOf(_Sources.ToArray(), currentSource)))
                {
                    DatalistList.Add(DataList);
                    /*
                    DatalistList.Add(new List<HistoricalModel>());
                    
                    string field = getArrayName(ticker, currentSource, "_Date");

                    dataDates = (string[])typeof(Source).GetField(field).GetValue(source);//(string[])GetType().GetField(getArrayName(ticker, source, "_Date")).GetValue(this);         




                    dataCloses = getArray(ticker, currentSource, "_Close", source);
                    dataVolumes = getArray(ticker, currentSource, "_Volume", source);

                    foreach (int j in Enumerable.Range(0, dataDates.Length))
                    {
                        DatalistList[DatalistList.Count - 1].Add(new HistoricalModel()
                        {
                            _Date = dataDates[j],
                            //_Open = getArrayValue(ticker, source, "_Open", j),
                            //_High = getArrayValue(ticker, source, "_High", j),
                            //_Low = getArrayValue(ticker, source, "_Low", j),
                            _Close = getArrayValue(ticker, currentSource, "_Close", source, j),
                            //_CloseAdj = getArrayValue(ticker, source, "_CloseAdj", j),
                            _Volume = getArrayValue(ticker, currentSource, "_Volume", source, j),

                            _MA_0 = MovingAverage(dataCloses, 1)[j],
                            _MA_1 = MovingAverage(dataCloses, 2)[j],
                            _MA_2 = MovingAverage(dataCloses, 3)[j],
                            _PriceChange = Change(dataCloses)[j],
                            _VolumeChange = Change(dataVolumes)[j],
                            _TRIX = TRIX(dataCloses)[j],
                            _RSI = RSI(dataCloses)[j],
                        });
                    }
                    


                    */

                    DataList = DatalistList.First();

                    if (sourcesIndex.Length > 1)
                    {
                        for (int i = 0; i < DataList.Count; i++)
                        {
                            DataList[i]._Open = DatalistList.Select(x => x[i]._Open).Average();
                            //DataList[i]._High = DatalistList.Select(x => x[i]._High).Average();
                            //DataList[i]._Low = DatalistList.Select(x => x[i]._Low).Average();
                            DataList[i]._Close = DatalistList.Select(x => x[i]._Close).Average();
                            // DataList[i]._CloseAdj = DatalistList.Select(x => x[i]._CloseAdj).Average();
                            DataList[i]._Volume = DatalistList.Select(x => x[i]._Volume).Average();

                            DataList[i]._MA_0 = DatalistList.Select(x => x[i]._MA_0).Average();
                            DataList[i]._MA_1 = DatalistList.Select(x => x[i]._MA_1).Average();
                            DataList[i]._MA_2 = DatalistList.Select(x => x[i]._MA_2).Average();
                            DataList[i]._PriceChange = DatalistList.Select(x => x[i]._PriceChange).Average();
                            DataList[i]._VolumeChange = DatalistList.Select(x => x[i]._VolumeChange).Average();
                            DataList[i]._VolumeIndicator = DatalistList.Select(x => x[i]._VolumeIndicator).Average();
                            DataList[i]._TRIX = DatalistList.Select(x => x[i]._TRIX).Average();
                            DataList[i]._RSI = DatalistList.Select(x => x[i]._TRIX).Average();
                        }
                    }
                }
            }

            return DataList; //return DataList.OrderByDescending(x => x._Date).ToList();
        }




        /*

        public static string getArrayName(string ticker, string source, string data)
        {
            return ticker + "_" + source + data;
        }

        public static double getArrayValue(string ticker, string source, string data, object model, int position)
        {
            return getArray(ticker, source, data, model)[position];
        }

        public static double[] getArray(string ticker, string source, string data, object model)
        {
            return (double[])typeof(Source).GetField(getArrayName(ticker, source, data)).GetValue(model);
        }

        */























    }
}