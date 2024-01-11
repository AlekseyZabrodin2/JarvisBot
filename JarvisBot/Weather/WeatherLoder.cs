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
            string responseData;
            WeatherBaseModel weatherBaseModel = new();

            string apiKey = TelegramBotConfiguration.LoadYandexApiKeyConfiguration();
            string apiUrl = TelegramBotConfiguration.LoadYandexWeatherConfiguration();

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
                        weatherBaseModel = apiResponse;
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

                return weatherBaseModel;
            }
        }

    }
}
