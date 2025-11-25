using System.Text.Json.Serialization;

namespace JarvisBot.Weather
{
    public class WeatherPart
    {
        [JsonPropertyName("part_name")]
        public string PartName { get; set; }

        [JsonPropertyName("temp_min")]
        public double TempMin { get; set; }

        [JsonPropertyName("temp_avg")]
        public double TempAvg { get; set; }

        [JsonPropertyName("temp_max")]
        public double TempMax { get; set; }

        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_gust")]
        public double WindGust { get; set; }

        [JsonPropertyName("wind_dir")]
        public string WindDir { get; set; }

        [JsonPropertyName("pressure_mm")]
        public double PressureMm { get; set; }

        [JsonPropertyName("pressure_pa")]
        public double PressurePa { get; set; }

        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }

        [JsonPropertyName("prec_mm")]
        public double PrecMm { get; set; }

        [JsonPropertyName("prec_prob")]
        public double PrecProb { get; set; }

        [JsonPropertyName("prec_period")]
        public double PrecPeriod { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("feels_like")]
        public double FeelsLike { get; set; }

        [JsonPropertyName("daytime")]
        public string Daytime { get; set; }

        [JsonPropertyName("polar")]
        public bool Polar { get; set; }
    }
}
