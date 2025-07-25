﻿using JarvisBot.Background;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherLoder
    {
        private readonly JarvisClientSettings _clientSettings;


        public WeatherLoder(JarvisClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }


        public async Task<string> WeatherResponse()
        {
            string weatherResponse = string.Empty;
            string factWeather = string.Empty;
            string forecastPartOne = string.Empty;
            string forecastPartTwo = string.Empty;

            try
            {
                var apiResponse = await LoadWeather();

                var forecastPartsAreaOne = apiResponse.Forecast.Parts[0];
                var forecastPartsAreaTwo = apiResponse.Forecast.Parts[1];

                factWeather = FactWeatherPartTwo(apiResponse);
                forecastPartOne = WeatherForecastPartTwo(apiResponse, forecastPartsAreaOne);
                forecastPartTwo = WeatherForecastPartTwo(apiResponse, forecastPartsAreaTwo);

                weatherResponse = DisplayWeather(factWeather, forecastPartOne, forecastPartTwo);

                return weatherResponse;
            }
            catch (WebException ex)
            {
                Console.WriteLine("Возникло исключение");
                throw;
            }
        }

        private string FactWeatherPartTwo(WeatherBaseModel apiResponse)
        {
            var forecastWeather = $"   Минск  " +
                                $"\r\n {apiResponse.Fact.Temp}°C" +
                                $"\r\n {DisplayWeatherInfo.DisplayFactInformation(apiResponse.Fact.Condition)}" +
                                $"\r\n Ощущается как: {apiResponse.Fact.FeelsLike}°C" +
                                $"\r\n Ветер {DisplayWeatherInfo.DetermineWindStrengthCategory(apiResponse.Fact.WindSpeed)}: {apiResponse.Fact.WindSpeed} м/с, {DisplayWeatherInfo.DisplayFactInformation(apiResponse.Fact.WindDir)}";

            return forecastWeather;
        }

        private string WeatherForecastPartTwo(WeatherBaseModel apiResponse, WeatherPart forecastPartsArea)
        {
            var forecastWeather = $" {DisplayWeatherInfo.DisplayFactInformation(forecastPartsArea.PartName)} " +
                                $"\r\n {forecastPartsArea.TempMin}°C ... {forecastPartsArea.TempMax}°C" +
                                $"\r\n {DisplayWeatherInfo.DisplayFactInformation(forecastPartsArea.Condition)}" +
                                $"\r\n Ветер {DisplayWeatherInfo.DetermineWindStrengthCategory(forecastPartsArea.WindSpeed)}: {forecastPartsArea.WindSpeed} м/с, {DisplayWeatherInfo.DisplayFactInformation(apiResponse.Fact.WindDir)}" +
                                $"\r\n Ощущается как: {forecastPartsArea.FeelsLike}°C";

            return forecastWeather;
        }

        private string DisplayWeather(string factWeather, string forecastPartOne, string forecastPartTwo)
        {
            var displayWeather = factWeather +
                                $"\r\n-----------------------------------------------------" +
                                $"\r\n{forecastPartOne}" +
                                $"\r\n-----------------------------------------------------" +
                                $"\r\n{forecastPartTwo}";

            return displayWeather;
        }

        public async Task<WeatherBaseModel> LoadWeather()
        {
            string responseData;
            WeatherBaseModel weatherBaseModel = new();

            string apiKey = _clientSettings.YandexKey;
            string apiUrl = _clientSettings.YandexWeather;

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
