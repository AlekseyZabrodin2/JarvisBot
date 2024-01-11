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


    }
}
