using JarvisBot.Background;
using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    public class OldExchangeRates
    {
        
        [JsonPropertyName("old_Usd_Rate_Buy")]
        public double OldUsdRateBuy { get; set; }

        [JsonPropertyName("old_Usd_Rate_Sell")]
        public double OldUsdRateSell { get; set; }

        [JsonPropertyName("old_Eur_Rate_Buy")]
        public double OldEurRateBuy { get; set; }

        [JsonPropertyName("old_Eur_Rate_Sell")]
        public double OldEurRateSell { get; set; }

        [JsonPropertyName("old_Rub_Rate_Buy")]
        public double OldRubRateBuy { get; set; }

        [JsonPropertyName("old_Rub_Rate_Sell")]
        public double OldRubRateSell { get; set; }

        private readonly JarvisClientSettings _clientSettings;

        public OldExchangeRates()
        {
            
        }

        public OldExchangeRates(JarvisClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
        }


        public void ReadJsonFile()
        {
            var directory = Path.GetDirectoryName(_clientSettings.OldExchangeRatesPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {                
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(_clientSettings.OldExchangeRatesPath))
            {
                File.WriteAllText(_clientSettings.OldExchangeRatesPath, "{}");
            }

            string jsonContent = File.ReadAllText(_clientSettings.OldExchangeRatesPath);
            var loadeRate = JsonSerializer.Deserialize<OldExchangeRates>(jsonContent);
            if (loadeRate != null)
            {
                OldUsdRateBuy = loadeRate.OldUsdRateBuy;
                OldUsdRateSell = loadeRate.OldUsdRateSell;
                OldEurRateBuy = loadeRate.OldEurRateBuy;
                OldEurRateSell = loadeRate.OldEurRateSell;
                OldRubRateBuy = loadeRate.OldRubRateBuy;
                OldRubRateSell = loadeRate.OldRubRateSell;
            }
            else
            {
                Console.WriteLine("Ошибка чтения файла со старыми курсами валют");
            }
        }

        public void WriteJsonFile()
        {
            string jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(_clientSettings.OldExchangeRatesPath, jsonString);
        }

    }
}
