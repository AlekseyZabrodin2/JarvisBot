using JarvisBot.Background;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace JarvisBot.Exchange.AlfaBankInSyncRates
{
    public class ExchangeRateLoder
    {
        private readonly JarvisClientSettings _clientSettings;
        private static OldExchangeRates _oldExchangeRates;
        private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
        private bool _rateIsUpdate = false;

        public ExchangeRateLoder(JarvisClientSettings clientSettings, OldExchangeRates oldExchangeRates)
        {
            _clientSettings = clientSettings;
            _oldExchangeRates = oldExchangeRates;
        }


        public async Task<string> RatesResponse(string message, CancellationToken cancellationToken)
        {
            List<ExchRateRecord> rateRecords = new List<ExchRateRecord>();
            string? rateResponse = null;

            try
            {
                var apiResponse = await LoadAlfabankRateAsync();
                foreach (var rate in apiResponse.Filial.Rates.ExchRate.ExchRateRecord)
                {
                    rateRecords.Add(rate);
                }

                if (rateRecords == null || rateRecords.Count == 0)
                {
                    throw new Exception("Нет данных о курсах валют");
                }

                _oldExchangeRates.ReadJsonFile();

                string dateInput = apiResponse.Filial.Rates.ExchRate.Time;
                var parsedDate = DateTime.Parse(dateInput);

                if (message == "💵 USD")
                {
                    rateResponse = GetUsdRates(rateRecords, parsedDate);
                }

                if (message == "💶 EUR")
                {
                    rateResponse = GetEurRates(rateRecords, parsedDate);
                }

                if (message == "💷 RUB")
                {
                    rateResponse = GetRubRates(rateRecords, parsedDate);
                }

                return rateResponse;
            }
            catch (WebException ex)
            {
                Console.WriteLine($"Возникло WebException: {ex.Message}");
                _logger.Error($"Возникло WebException: {ex}");
                //return ex.ToString();
                return "Возникло исключение при попытке загрузить курс валют";
                //throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Возникло Exception: {ex.Message}");
                _logger.Error($"Возникло Exception: {ex}");
                //return ex.ToString();
                return "Возникло исключение при попытке загрузить курс валют";
                //throw;
            }
        }       

        public async Task<Filials> LoadAlfabankRateAsync()
        {
            string url = _clientSettings.AlfabankRate;
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            client.DefaultRequestHeaders.Add("Accept", "application/xml,text/html,application/xhtml+xml");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");

            // первый запрос (чтобы сервер отдал cookie)
            var response1 = await client.GetAsync(url);
            var html = await response1.Content.ReadAsStringAsync();

            if (html.Contains("hg-security"))
            {
                Console.WriteLine("Anti-bot detected. Trying second request...");
                await Task.Delay(1000); // подождём немного
                var response2 = await client.GetAsync(url);
                html = await response2.Content.ReadAsStringAsync();
            }

            if (html.StartsWith("<html", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Still received HTML, access blocked.");
                _logger.Error("Alfabank rate endpoint returned HTML — bot protection.");
                return null;
            }

            // теперь парсим XML
            var serializer = new XmlSerializer(typeof(Filials));
            using var stringReader = new StringReader(html);
            using var reader = XmlReader.Create(stringReader);
            var filials = (Filials)serializer.Deserialize(reader);

            return filials;
        }

        private string GetUsdRates(List<ExchRateRecord> rateRecords, DateTime parsedDate)
        {
            string? rateResponse;
            var rateUsdBuy = CheckUsdRateBuy(rateRecords[0].RateBuy);
            var rateUsdSell = CheckUsdRateSell(rateRecords[0].RateSell);

            rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
            $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
            $"\r\n-----------------------------------------------------" +
            $"\r\n  1 Доллар США ({rateRecords[0].Mnem}) - 💵" +
            $"\r\n  покупка     -    {rateUsdBuy}" +
            $"\r\n  продажа    -    {rateUsdSell}";
            return rateResponse;
        }

        private string CheckUsdRateBuy(string newRateBuy)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newUsdRate);
            if (newUsdRate != null)
            {                
                double rateDifference = newUsdRate - _oldExchangeRates.OldUsdRateBuy;

                if (newUsdRate == _oldExchangeRates.OldUsdRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy}";
                }
                else if (newUsdRate > _oldExchangeRates.OldUsdRateBuy)
                {
                    rateAfterCheck = $"↑ {newRateBuy} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true ;
                    _oldExchangeRates.OldUsdRateBuy = newUsdRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if (newUsdRate < _oldExchangeRates.OldUsdRateBuy)
                {
                    rateAfterCheck = $"↓ {newRateBuy} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldUsdRateBuy = newUsdRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты USD Buy.");
                _logger.Error("Ошибка преобразования курса валюты USD Buy.");
            }

            return rateAfterCheck;
        }

        private string CheckUsdRateSell(string newRateSell)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newUsdRate);
            if (newUsdRate != null)
            {
                double rateDifference = newUsdRate - _oldExchangeRates.OldUsdRateSell;

                if (newUsdRate == _oldExchangeRates.OldUsdRateSell)
                {
                    rateAfterCheck = $"{newRateSell}";
                }
                else if (newUsdRate > _oldExchangeRates.OldUsdRateSell)
                {
                    rateAfterCheck = $"↑ {newRateSell} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldUsdRateSell = newUsdRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if (newUsdRate < _oldExchangeRates.OldUsdRateSell)
                {
                    rateAfterCheck = $"↓ {newRateSell} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldUsdRateSell = newUsdRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты USD Sell.");
                _logger.Error("Ошибка преобразования курса валюты USD Sell.");
            }

            return rateAfterCheck;
        }

        private string GetEurRates(List<ExchRateRecord> rateRecords, DateTime parsedDate)
        {
            string? rateResponse;
            var rateEurBuy = CheckEurRateBuy(rateRecords[1].RateBuy);
            var rateEurSell = CheckEurRateSell(rateRecords[1].RateSell);

            rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
            $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
            $"\r\n-----------------------------------------------------" +
            $"\r\n  1 Евро ({rateRecords[1].Mnem}) - 💶" +
            $"\r\n  покупка     -    {rateEurBuy}" +
            $"\r\n  продажа    -    {rateEurSell}";
            return rateResponse;
        }

        private string CheckEurRateBuy(string newRateBuy)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newEurRate);
            if (newEurRate != null)
            {
                double rateDifference = newEurRate - _oldExchangeRates.OldEurRateBuy;

                if (newEurRate == _oldExchangeRates.OldEurRateBuy)
                {
                    rateAfterCheck = $"{newRateBuy}";
                }
                else if (newEurRate > _oldExchangeRates.OldEurRateBuy)
                {
                    rateAfterCheck = $"↑ {newRateBuy} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldEurRateBuy = newEurRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if (newEurRate < _oldExchangeRates.OldEurRateBuy)
                {
                    rateAfterCheck = $"↓ {newRateBuy} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldEurRateBuy = newEurRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты EUR Buy.");
                _logger.Error("Ошибка преобразования курса валюты EUR Buy.");
            }

            return rateAfterCheck;
        }

        private string CheckEurRateSell(string newRateSell)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newEurRate);
            if (newEurRate != null)
            {
                double rateDifference = newEurRate - _oldExchangeRates.OldEurRateSell;

                if (newEurRate == _oldExchangeRates.OldEurRateSell)
                {
                    rateAfterCheck = $"{newRateSell}";
                }
                else if (newEurRate > _oldExchangeRates.OldEurRateSell)
                {
                    rateAfterCheck = $"↑ {newRateSell} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldEurRateSell = newEurRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if (newEurRate < _oldExchangeRates.OldEurRateSell)
                {
                    rateAfterCheck = $"↓ {newRateSell} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldEurRateSell = newEurRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты EUR Sell.");
                _logger.Error("Ошибка преобразования курса валюты EUR Sell.");
            }

            return rateAfterCheck;
        }

        private string GetRubRates(List<ExchRateRecord> rateRecords, DateTime parsedDate)
        {
            string? rateResponse;
            var rateRubBuy = CheckRubRateBuy(rateRecords[2].RateBuy);
            var rateRubSell = CheckRubRateSell(rateRecords[2].RateSell);

            rateResponse = $"Курс InSync на дату - {parsedDate.ToString("dd/MM/yyyy")}" +
            $"\r\n                        на - {parsedDate.ToString("HH/mm")}" +
            $"\r\n-----------------------------------------------------" +
            $"\r\n  100 Российских рублей ({rateRecords[2].Mnem}) - 💷" +
            $"\r\n  покупка     -    {rateRubBuy}" +
            $"\r\n  продажа    -    {rateRubSell}";
            return rateResponse;
        }

        private string CheckRubRateBuy(string newRubRateBuy)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRubRateBuy, NumberStyles.Float, CultureInfo.InvariantCulture, out double newRubRate);
            if (newRubRate != null)
            {
                double rateDifference = newRubRate - _oldExchangeRates.OldRubRateBuy;

                if (newRubRate == _oldExchangeRates.OldRubRateBuy)
                {
                    rateAfterCheck = $"{newRubRateBuy}";
                }
                else if (newRubRate > _oldExchangeRates.OldRubRateBuy)
                {
                    rateAfterCheck = $"↑ {newRubRateBuy} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldRubRateBuy = newRubRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if(newRubRate < _oldExchangeRates.OldRubRateBuy)
                {
                    rateAfterCheck = $"↓ {newRubRateBuy} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldRubRateBuy = newRubRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты RUB Buy.");
                _logger.Error("Ошибка преобразования курса валюты RUB Buy.");
            }

            return rateAfterCheck;
        }

        private string CheckRubRateSell(string newRubRateSell)
        {
            string? rateAfterCheck = null;

            double.TryParse(newRubRateSell, NumberStyles.Float, CultureInfo.InvariantCulture, out double newRubRate);
            if (newRubRate != null)
            {
                double rateDifference = newRubRate - _oldExchangeRates.OldRubRateSell;

                if (newRubRate == _oldExchangeRates.OldRubRateSell)
                {
                    rateAfterCheck = $"{newRubRateSell}";
                }
                else if (newRubRate > _oldExchangeRates.OldRubRateSell)
                {
                    rateAfterCheck = $"↑ {newRubRateSell} ( +{rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldRubRateSell = newRubRate;
                    _oldExchangeRates.WriteJsonFile();
                }
                else if (newRubRate < _oldExchangeRates.OldRubRateSell)
                {
                    rateAfterCheck = $"↓ {newRubRateSell} ( {rateDifference:0.####} )";
                    _rateIsUpdate = true;
                    _oldExchangeRates.OldRubRateSell = newRubRate;
                    _oldExchangeRates.WriteJsonFile();
                }
            }
            else
            {
                // В случае ошибки преобразования возвращаем пустую строку
                Console.WriteLine("Ошибка преобразования курса валюты RUB Sell.");
                _logger.Error("Ошибка преобразования курса валюты RUB Sell.");
            }

            return rateAfterCheck;
        }

        public async Task<string> EqualityCurrencyExchangeRate(string rate, CancellationToken cancellationToken)
        {
            string? newRate = null;
            if (!cancellationToken.IsCancellationRequested)
            {
                var updateRate = await RatesResponse(rate, cancellationToken);
                if (_rateIsUpdate)
                {
                    newRate = updateRate;
                    _rateIsUpdate = false;
                }
            }
            return newRate;
        }

    }
}
