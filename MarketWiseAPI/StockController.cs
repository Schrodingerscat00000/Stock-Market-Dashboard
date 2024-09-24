using Microsoft.AspNetCore.Mvc;
using RestSharp;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace StockAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockController : ControllerBase
    {
        private readonly StockDbContext _context;

        // Constructor to inject DbContext
        public StockController(StockDbContext context)
        {
            _context = context;
        }

        // Get stock data and calculate SMA
        [HttpGet("{ticker}")]
        public async Task<IActionResult> GetStockData(string ticker)
        {
            var apiKey = "8SDDTCNMZUHB0PTF"; 
            var url = $"https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol={ticker}&apikey={apiKey}";

            // Create RestClient and request
            var client = new RestClient();
            var request = new RestRequest(url, Method.Get);

            // Execute the request and get the response
            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var json = JObject.Parse(response.Content);

                var timeSeries = json["Time Series (Daily)"] as JObject;
                if (timeSeries == null)
                {
                    return BadRequest("No stock data available for the given ticker.");
                }

                var stockEntityList = new List<StockEntity>();  
                var closePrices = new List<double>();
                var openPrices = new List<double>();
                var highPrices = new List<double>();
                var lowPrices = new List<double>();
                var volumes = new List<double>();

                
                foreach (KeyValuePair<string, JToken> item in timeSeries)
                {
                    var date = item.Key;  
                    var stockInfo = item.Value;  

                    var close = stockInfo["4. close"]?.ToString();
                    var open = stockInfo["1. open"]?.ToString();
                    var high = stockInfo["2. high"]?.ToString();
                    var low = stockInfo["3. low"]?.ToString();
                    var volume = stockInfo["5. volume"]?.ToString();

                    
                    var stockEntity = new StockEntity
                    {
                        Ticker = ticker,
                        Date = date,
                        Open = open,
                        Close = close,
                        High = high,
                        Low = low,
                        Volume = volume
                    };

                    stockEntityList.Add(stockEntity);

                    
                    if (double.TryParse(close, out double closePrice))
                    {
                        closePrices.Add(closePrice);
                    }

                    if (double.TryParse(open, out double openPrice))
                    {
                        openPrices.Add(openPrice);
                    }

                    if (double.TryParse(high, out double highPrice))
                    {
                        highPrices.Add(highPrice);
                    }

                    if (double.TryParse(low, out double lowPrice))
                    {
                        lowPrices.Add(lowPrice);
                    }

                    if (double.TryParse(volume, out double volumeValue))
                    {
                        volumes.Add(volumeValue);
                    }
                }

                
                var stockDataList = stockEntityList.Select(e => new StockData
                {
                    Ticker = e.Ticker,
                    Date = DateTime.Parse(e.Date), 
                    Open = decimal.Parse(e.Open),   
                    Close = decimal.Parse(e.Close), 
                    High = decimal.Parse(e.High),   
                    Low = decimal.Parse(e.Low),     
                    Volume = long.Parse(e.Volume)   
                }).ToList();

                
                await _context.StockData.AddRangeAsync(stockDataList);
                await _context.SaveChangesAsync();

                
                var movingAverages = CalculateMovingAverages(closePrices, 5);

                
                return Ok(new
                {
                    StockData = stockDataList,
                    MovingAverages = movingAverages,
                    OpenPrices = openPrices,
                    HighPrices = highPrices,
                    LowPrices = lowPrices,
                    Volumes = volumes
                });
            }

            return BadRequest("Error fetching stock data.");
        }

        
        private List<double?> CalculateMovingAverages(List<double> prices, int period)
        {
            var movingAverages = new List<double?>();

            for (int i = 0; i < prices.Count; i++)
            {
                if (i < period - 1)
                {
                    movingAverages.Add(null);  
                }
                else
                {
                    var average = prices.Skip(i - period + 1).Take(period).Average();
                    movingAverages.Add(average);
                }
            }

            return movingAverages;
        }
    }
}
