using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using JarvisBot.Data;
using System.IO;
using System.Net;
using Telegram.Bot.Exceptions;
using System.Text.Json.Serialization;
using System.Net.Http;
using NLog.Fluent;

namespace JarvisBot.Exchange
{
    public class ExchangeRate
    {

        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }

        [JsonPropertyName("iso")]
        public string Iso { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }



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

        public static ApiResponse LoadRate()
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

            ApiResponse apiResponse = JsonSerializer.Deserialize<ApiResponse>(response);

            return apiResponse;
        }        
    }
}
