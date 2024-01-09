using JarvisBot.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherBaseModel
    {

        [JsonProperty("now")]
        public long Now { get; set; }

        [JsonProperty("now_dt")]
        public DateTimeOffset NowDt { get; set; }

        [JsonProperty("info")]
        public WeatherInfo Info { get; set; }

        [JsonProperty("fact")]
        public WeatherFact Fact { get; set; }

        [JsonProperty("forecast")]
        public WeatherForecast Forecast { get; set; }


    }
}
