using System.Text.Json.Serialization;

namespace JarvisBot.Weather
{
    public class WeatherInfo
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lon")]
        public double Lon { get; set; }

    }
}
