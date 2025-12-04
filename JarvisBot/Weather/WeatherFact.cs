using System.Text.Json.Serialization;

namespace JarvisBot.Weather
{
    public class WeatherFact
    {
        [JsonPropertyName("obs_time")]
        public long ObsTime { get; set; }

        [JsonPropertyName("temp")]
        public long Temp { get; set; }

        [JsonPropertyName("feels_like")]
        public long FeelsLike { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_dir")]
        public string WindDir { get; set; }

        [JsonPropertyName("pressure_mm")]
        public long PressureMm { get; set; }

        [JsonPropertyName("pressure_pa")]
        public long PressurePa { get; set; }

        [JsonPropertyName("humidity")]
        public long Humidity { get; set; }

        [JsonPropertyName("daytime")]
        public string Daytime { get; set; }

        [JsonPropertyName("polar")]
        public bool Polar { get; set; }

        [JsonPropertyName("season")]
        public string Season { get; set; }

        [JsonPropertyName("wind_gust")]
        public double WindGust { get; set; }



    }
}
