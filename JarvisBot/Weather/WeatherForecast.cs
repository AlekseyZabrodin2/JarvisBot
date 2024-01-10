﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JarvisBot.Weather
{
    public class WeatherForecast
    {
        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }

        [JsonPropertyName("date_ts")]
        public long DateTs { get; set; }

        [JsonPropertyName("week")]
        public long Week { get; set; }

        [JsonPropertyName("sunrise")]
        public string Sunrise { get; set; }

        [JsonPropertyName("sunset")]
        public string Sunset { get; set; }

        [JsonPropertyName("moon_code")]
        public long MoonCode { get; set; }

        [JsonPropertyName("moon_text")]
        public string MoonText { get; set; }

        [JsonPropertyName("parts")]
        public WeatherPart[] Parts { get; set; }

    }
}
