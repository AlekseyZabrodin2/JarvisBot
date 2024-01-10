using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherPart
    {
        [JsonPropertyName("part_name")]
        public string PartName { get; set; }

        [JsonPropertyName("temp_min")]
        public long TempMin { get; set; }

        [JsonPropertyName("temp_avg")]
        public long TempAvg { get; set; }

        [JsonPropertyName("temp_max")]
        public long TempMax { get; set; }

        [JsonPropertyName("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("wind_gust")]
        public double WindGust { get; set; }

        [JsonPropertyName("wind_dir")]
        public string WindDir { get; set; }

        [JsonPropertyName("pressure_mm")]
        public long PressureMm { get; set; }

        [JsonPropertyName("pressure_pa")]
        public long PressurePa { get; set; }

        [JsonPropertyName("humidity")]
        public long Humidity { get; set; }

        [JsonPropertyName("prec_mm")]
        public float PrecMm { get; set; }

        [JsonPropertyName("prec_prob")]
        public long PrecProb { get; set; }

        [JsonPropertyName("prec_period")]
        public long PrecPeriod { get; set; }

        [JsonPropertyName("icon")]
        public string Icon { get; set; }

        [JsonPropertyName("condition")]
        public string Condition { get; set; }

        [JsonPropertyName("feels_like")]
        public int FeelsLike { get; set; }

        [JsonPropertyName("daytime")]
        public string Daytime { get; set; }

        [JsonPropertyName("polar")]
        public bool Polar { get; set; }
    }
}
