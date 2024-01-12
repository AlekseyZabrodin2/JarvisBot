using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using JarvisBot.Data;
using Telegram.Bot.Types;
using static System.Net.Mime.MediaTypeNames;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    public class ExchangeRateLoder
    {

        private static double _oldUsdRateBuy;
        private static double _oldUsdRateSell;
        private static double _oldEurRateBuy;
        private static double _oldEurRateSell;
        private static double _oldRubRateBuy;
        private static double _oldRubRateSell;


        public static string RatesResponse(string message)
        {
            List<ExchRateRecord> rateRecords = new List<ExchRateRecord>();
            string rateResponse = null;

            try
            {
                var apiResponse = LoadAlfabankRate();
                foreach (var rate in apiResponse.Filial.Rates.ExchRate.ExchRateRecord)
                {
                    rateRecords.Add(rate);
                }

                if (rateRecords == null || rateRecords.Count == 0)
                {
                    throw new Exception("Нет данных о курсах валют");
                }

                string dateInput = apiResponse.Filial.Rates.ExchRate.Time;
                var parsedDate = DateTime.Parse(dateInput);

                if (message == "USD")
                {
                    rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
                    $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
                    $"\r\n-----------------------------------------------------" +
                    $"\r\n  1 Доллар США ({rateRecords[0].Mnem})" +
                    $"\r\n  покупка     -    {rateRecords[0].RateBuy}" +
                    $"\r\n  продажа    -    {rateRecords[0].RateSell}";
                }

                if (message == "EUR")
                {
                    rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
                    $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
                    $"\r\n-----------------------------------------------------" +
                    $"\r\n  1 Евро ({rateRecords[1].Mnem})" +
                    $"\r\n  покупка     -    {rateRecords[1].RateBuy}" +
                    $"\r\n  продажа    -    {rateRecords[1].RateSell}";
                }

                if (message == "RUB")
                {
                    rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
                    $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
                    $"\r\n-----------------------------------------------------" +
                    $"\r\n  100 Российских рублей ({rateRecords[2].Mnem})" +
                    $"\r\n  покупка     -    {rateRecords[2].RateBuy}" +
                    $"\r\n  продажа    -    {rateRecords[2].RateSell}";
                }

                return rateResponse;
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Возникло исключение: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникло исключение: {ex.Message}");
                throw;
            }
        }

        public static Filials LoadAlfabankRate()
        {
            string url = TelegramBotConfiguration.LoadAlfabankRateConfiguration();
            string xmlString;
            Filials filials = new();

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            using (HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest?.GetResponse())
            {
                if (httpWebResponse == null || httpWebResponse.StatusCode != HttpStatusCode.OK)
                {
                    Console.WriteLine("Failed to get a valid HTTP response.");
                    return null;
                }

                using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    xmlString = streamReader.ReadToEnd();
                    Console.WriteLine($"Rate response - {xmlString}");
                }

                XmlSerializer serializer = new XmlSerializer(typeof(Filials));

                using (XmlReader reader = XmlReader.Create(new StringReader(xmlString)))
                {
                    filials = (Filials)serializer.Deserialize(reader);

                    Console.WriteLine($"Example value: {filials.Filial.Name}");
                }
            }

            return filials;
        }

        /// <summary>
        /// !!! TODO !!! Продумать варианты хранения курсов с целью их дальнейшего сравнения
        /// </summary>

        private static string CheckUsdRateBuy(string newRateBuy)
        {
            string rateAfterCheck = null;

            double.TryParse(newRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newUsdRate);
            if (newUsdRate != null)
            {
                if (newUsdRate == _oldUsdRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy}";
                }
                if (newUsdRate > _oldUsdRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy} ↑";
                }
                if (newUsdRate < _oldUsdRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy} ↓";
                }

                _oldUsdRateBuy = newUsdRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты USD.");
            }

            return rateAfterCheck;
        }

        private static string CheckUsdRateSell(string newRateSell)
        {
            string rateAfterCheck = null;

            double.TryParse(newRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newUsdRate);
            if (newUsdRate != null)
            {
                if (newUsdRate == _oldUsdRateSell)
                {
                    rateAfterCheck = $"{newRateSell}";
                }
                if (newUsdRate > _oldUsdRateSell)
                {
                    rateAfterCheck = $"{newRateSell} ↑";
                }
                if (newUsdRate < _oldUsdRateSell)
                {
                    rateAfterCheck = $"{newRateSell} ↓";
                }

                _oldUsdRateSell = newUsdRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты USD.");
            }

            return rateAfterCheck;
        }

        private static string CheckEurRateBuy(string newRateBuy)
        {
            string rateAfterCheck = null;

            double.TryParse(newRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newEurRate);
            if (newEurRate != null)
            {
                if (newEurRate == _oldEurRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy}";
                }
                if (newEurRate > _oldEurRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy} ↑";
                }
                if (newEurRate < _oldEurRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy} ↓";
                }

                _oldEurRateBuy = newEurRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты USD.");
            }

            return rateAfterCheck;
        }

        private static string CheckEurRateSell(string newRateSell)
        {
            string rateAfterCheck = null;

            double.TryParse(newRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newEurRate);
            if (newEurRate != null)
            {
                if (newEurRate == _oldEurRateSell)
                {
                    rateAfterCheck = $"{newRateSell}";
                }
                if (newEurRate > _oldEurRateSell)
                {
                    rateAfterCheck = $"{newRateSell} ↑";
                }
                if (newEurRate < _oldEurRateSell)
                {
                    rateAfterCheck = $"{newRateSell} ↓";
                }

                _oldEurRateSell = newEurRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты EUR.");
            }

            return rateAfterCheck;
        }

        private static string CheckRubRateBuy(string newRubRateBuy)
        {
            string rateAfterCheck = null;

            double.TryParse(newRubRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newRubRate);
            if (newRubRate != null)
            {
                if (newRubRate == _oldRubRateBuy)
                {
                    rateAfterCheck = $"{newRubRateBuy}";
                }
                if (newRubRate > _oldRubRateBuy)
                {
                    rateAfterCheck = $"{newRubRateBuy} ↑";
                }
                if (newRubRate < _oldRubRateBuy)
                {
                    rateAfterCheck = $"{newRubRateBuy} ↓";
                }

                _oldRubRateBuy = newRubRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты RUB.");
            }

            return rateAfterCheck;
        }

        private static string CheckRubRateSell(string newRubRateSell)
        {
            string rateAfterCheck = null;

            double.TryParse(newRubRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newRubRate);
            if (newRubRate != null)
            {
                if (newRubRate == _oldRubRateSell)
                {
                    rateAfterCheck = $"{newRubRateSell}";
                }
                if (newRubRate > _oldRubRateSell)
                {
                    rateAfterCheck = $"{newRubRateSell} ↑";
                }
                if (newRubRate < _oldRubRateSell)
                {
                    rateAfterCheck = $"{newRubRateSell} ↓";
                }

                _oldRubRateSell = newRubRate;
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты RUB.");
            }

            return rateAfterCheck;
        }

    }
}
