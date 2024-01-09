using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherForecast
    {
        [JsonProperty("date")]
        public DateTimeOffset Date { get; set; }

        [JsonProperty("date_ts")]
        public long DateTs { get; set; }

        [JsonProperty("week")]
        public long Week { get; set; }

        [JsonProperty("sunrise")]
        public string Sunrise { get; set; }

        [JsonProperty("sunset")]
        public string Sunset { get; set; }

        [JsonProperty("moon_code")]
        public long MoonCode { get; set; }

        [JsonProperty("moon_text")]
        public string MoonText { get; set; }

        [JsonProperty("parts")]
        public WeatherPart[] Parts { get; set; }

    }
}
