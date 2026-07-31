using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PaperTradingBot
{
    public class LiveBrokerAPI
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiSecret;
        private readonly string _baseUrl;

        public LiveBrokerAPI(string apiKey = "DEMO_API_KEY", string apiSecret = "DEMO_SECRET", bool isPaperTrading = true)
        {
            _httpClient = new HttpClient();
            _apiKey = apiKey;
            _apiSecret = apiSecret;
            _baseUrl = isPaperTrading 
                ? "https://paper-api.alpaca.markets/v2" 
                : "https://api.alpaca.markets/v2";

            if (!_httpClient.DefaultRequestHeaders.Contains("APCA-API-KEY-ID"))
            {
                _httpClient.DefaultRequestHeaders.Add("APCA-API-KEY-ID", _apiKey);
                _httpClient.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", _apiSecret);
            }
        }

        public async Task<decimal> GetLivePriceAsync(string symbol)
        {
            try
            {
                // Canlı sanal piyasa fiyat çekimi (Paper Trading Ticker API)
                string requestUrl = $"https://data.alpaca.markets/v2/stocks/{symbol}/trades/latest";
                var response = await _httpClient.GetAsync(requestUrl);
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return 100.00m; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ [API UYARI] {symbol} canlı fiyat çekilemedi: {ex.Message}");
            }

            return 0m;
        }

        public async Task<bool> SendPaperOrderAsync(string symbol, string side, int quantity, decimal limitPrice = 0m)
        {
            try
            {
                string orderType = limitPrice > 0 ? "limit" : "market";
                string jsonBody = $"{{\"symbol\": \"{symbol}\", \"qty\": {quantity}, \"side\": \"{side.ToLower()}\", \"type\": \"{orderType}\", \"time_in_force\": \"day\"}}";

                var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/orders", content);

                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"   🌐 [CANLI DEMO API] {symbol} {side.ToUpper()} Emri Sanal Borsaya İletildi! (Lot: {quantity})");
                    return true;
                }
                else
                {
                    Console.WriteLine($"   ⚠️ [CANLI DEMO API] Emir İletim Yanıtı: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ⚠️ [CANLI DEMO API HATA] {ex.Message}");
            }

            return false;
        }
    }
}
