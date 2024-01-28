using JarvisBot.Data;
using JarvisBot.Weather;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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


        public void ReadJsonFile()
        {

            if (!File.Exists(TelegramBotConfiguration.OldExchangeRatesPath))
            {
                File.WriteAllText(TelegramBotConfiguration.OldExchangeRatesPath, "{}");
            }

            string jsonContent = File.ReadAllText(TelegramBotConfiguration.OldExchangeRatesPath);
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
            File.WriteAllText(TelegramBotConfiguration.OldExchangeRatesPath, jsonString);
        }

        

    }
}
