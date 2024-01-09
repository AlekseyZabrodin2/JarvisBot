using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherFact
    {
        [JsonProperty("obs_time")]
        public long ObsTime { get; set; }

        [JsonProperty("temp")]
        public long Temp { get; set; }

        [JsonProperty("feels_like")]
        public long FeelsLike { get; set; }

        [JsonProperty("icon")]
        public string Icon { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }

        [JsonProperty("wind_speed")]
        public double WindSpeed { get; set; }

        [JsonProperty("wind_dir")]
        public string WindDir { get; set; }

        [JsonProperty("pressure_mm")]
        public long PressureMm { get; set; }

        [JsonProperty("pressure_pa")]
        public long PressurePa { get; set; }

        [JsonProperty("humidity")]
        public long Humidity { get; set; }

        [JsonProperty("daytime")]
        public string Daytime { get; set; }

        [JsonProperty("polar")]
        public bool Polar { get; set; }

        [JsonProperty("season")]
        public string Season { get; set; }

        [JsonProperty("wind_gust")]
        public double WindGust { get; set; }



    }
}
