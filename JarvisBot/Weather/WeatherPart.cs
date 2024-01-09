using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherPart
    {
        [JsonProperty("part_name")]
        public string PartName { get; set; }

        [JsonProperty("temp_min")]
        public long TempMin { get; set; }

        [JsonProperty("temp_avg")]
        public long TempAvg { get; set; }

        [JsonProperty("temp_max")]
        public long TempMax { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_gust")]
        public double WindGust { get; set; }

        [JsonProperty("wind_dir")]
        public string WindDir { get; set; }

        [JsonProperty("pressure_mm")]
        public long PressureMm { get; set; }

        [JsonProperty("pressure_pa")]
        public long PressurePa { get; set; }

        [JsonProperty("humidity")]
        public long Humidity { get; set; }

        [JsonProperty("prec_mm")]
        public long PrecMm { get; set; }

        [JsonProperty("prec_prob")]
        public long PrecProb { get; set; }

        [JsonProperty("prec_period")]
        public long PrecPeriod { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("feels_like")]
        public long FeelsLike { get; set; }

        [JsonProperty("daytime")]
        public string Daytime { get; set; }

        [JsonProperty("polar")]
        public bool Polar { get; set; }
    }
}
