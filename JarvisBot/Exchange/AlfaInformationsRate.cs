using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using JarvisBot.Data;
using System.IO;
using System.Net;
using Telegram.Bot.Exceptions;
using System.Text.Json.Serialization;
using System.Net.Http;
using NLog.Fluent;

namespace JarvisBot.Exchange
{
    public class AlfaInformationsRate
    {
        [JsonPropertyName("rate")]
        public decimal Rate { get; set; }

        [JsonPropertyName("iso")]
        public string Iso { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

    }
}
