using JarvisBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherLoder
    {





        public static async Task<string> LoadWeather()
        {
            string responseData = null;

            string apiKey = TelegramBotConfiguration.LoadYandexApiKeyConfiguration();

            // Координаты Минска
            string latitude = "53.90228271";
            string longitude = "27.56183052";

            // <язык ответа> (по умолчанию "ru_RU")
            string language = "ru_RU";

            string apiUrl = $"https://api.weather.yandex.ru/v2/informers?lat={latitude}&lon={longitude}&lang={language}";

            //https://yandex.by/pogoda/minsk?lat=53.902735&lon=27.555691

            using (HttpClient httpClient = new HttpClient())
            {
                //Добавление заголовка с API-ключом
                httpClient.DefaultRequestHeaders.Add("X-Yandex-API-Key", apiKey);

                try
                {
                    HttpResponseMessage response = await httpClient.GetAsync(apiUrl).ConfigureAwait(false);

                    if (response.IsSuccessStatusCode)
                    {
                        responseData = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(responseData);
                    }
                    else
                    {
                        Console.WriteLine($"Ошибка: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Возникло исключение: {ex.Message}");
                }

                return responseData;
            }
        }

    }
}
