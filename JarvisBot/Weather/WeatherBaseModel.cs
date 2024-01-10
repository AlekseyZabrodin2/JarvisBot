using JarvisBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
