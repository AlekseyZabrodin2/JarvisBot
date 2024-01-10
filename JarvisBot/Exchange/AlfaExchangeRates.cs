using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace JarvisBot.Exchange
{
    public class AlfaExchangeRates
    {
        [JsonPropertyName("rates")]
        public List<AlfaInformationsRate> Rates { get; set; }
    }
}
