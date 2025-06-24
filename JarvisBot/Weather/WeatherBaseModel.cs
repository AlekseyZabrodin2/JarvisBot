using System.Text.Json.Serialization;

namespace JarvisBot.Weather
{
    public class WeatherBaseModel
    {

        [JsonPropertyName("now")]
        public int Now { get; set; }

        [JsonPropertyName("now_dt")]
        public string NowDt { get; set; }

        [JsonPropertyName("info")]
        public WeatherInfo Info { get; set; }

        [JsonPropertyName("fact")]
        public WeatherFact Fact { get; set; }

        [JsonPropertyName("forecast")]
        public WeatherForecast Forecast { get; set; }

    }
}
