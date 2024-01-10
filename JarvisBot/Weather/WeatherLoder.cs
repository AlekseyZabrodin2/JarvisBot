using JarvisBot.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherLoder
    {

        private static WeatherBaseModel _weatherBaseModel = new();



        public static async Task<string> WeatherResponse()
        {
            string rateResponse = null;
            try
            {
                var apiResponse = await LoadWeather();

                foreach (var item in apiResponse.Forecast.Parts)
                {
                    rateResponse = $" Текущая температура: {apiResponse.Fact.Temp}°C" +
                    $"\r\n Минимальная температура: {item.TempMin}°C" +
                    $"\r\n Максимальная температура: {item.TempMax}°C" +
                    $"\r\n Средняя температура: {item.TempAvg}°C" +
                    $"\r\n Ощущается как: {item.FeelsLike}°C" +
                    $"\r\n Скорость ветра: {apiResponse.Fact.WindSpeed} м/с";
                }

                return rateResponse;
            }
            catch (WebException ex)
            {
                Console.WriteLine("Возникло исключение");
                throw;
            }
        }

        public static async Task<WeatherBaseModel> LoadWeather()
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
                        Console.WriteLine($"Weather response - {responseData}");

                        WeatherBaseModel apiResponse = JsonSerializer.Deserialize<WeatherBaseModel>(responseData);
                        _weatherBaseModel = apiResponse;
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

                return _weatherBaseModel;
            }
        }

    }
}
