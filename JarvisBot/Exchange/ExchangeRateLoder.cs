using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JarvisBot.Exchange
{
    public class ExchangeRateLoder
    {



        public static string RatesResponse()
        {
            string rateResponse = null;
            try
            {
                var apiResponse = LoadRate();
                foreach (var rate in apiResponse.Rates)
                {
                    rateResponse = $"Курс на дату - {rate.Date}\r\n{rate.Name} - {rate.Rate}";
                }
                return rateResponse;
            }
            catch (WebException ex)
            {
                Console.WriteLine("Возникло исключение");
                throw;
            }
        }

        public static AlfaExchangeRates LoadRate()
        {
            string url = "https://developerhub.alfabank.by:8273/partner/1.0.1/public/nationalRates?currencyCode=840";
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse httpWebResponse = (HttpWebResponse)httpWebRequest?.GetResponse();
            string response;

            using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream()))
            {
                response = streamReader.ReadToEnd();
                Console.WriteLine($"Rate response - {response}");
            }

            AlfaExchangeRates apiResponse = JsonSerializer.Deserialize<AlfaExchangeRates>(response);

            return apiResponse;
        }

    }
}
