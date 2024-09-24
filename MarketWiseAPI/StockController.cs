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
            var apiKey = "8SDDTCNMZUHB0PTF"; // Add your Alpha Vantage API key
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

                var stockEntityList = new List<StockEntity>();  // Changed to StockEntity (your model class)
                var closePrices = new List<double>();
                var openPrices = new List<double>();
                var highPrices = new List<double>();
                var lowPrices = new List<double>();
                var volumes = new List<double>();

                // Iterate over the timeSeries as KeyValuePair to get both the date (key) and stock data (value)
                foreach (KeyValuePair<string, JToken> item in timeSeries)
                {
                    var date = item.Key;  // Extract the date
                    var stockInfo = item.Value;  // Extract the stock data for the date

                    var close = stockInfo["4. close"]?.ToString();
                    var open = stockInfo["1. open"]?.ToString();
                    var high = stockInfo["2. high"]?.ToString();
                    var low = stockInfo["3. low"]?.ToString();
                    var volume = stockInfo["5. volume"]?.ToString();

                    // Store in StockEntity for database storage
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

                    // Collect data for calculations
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

                // Convert stockEntityList to List<StockData>
                var stockDataList = stockEntityList.Select(e => new StockData
                {
                    Ticker = e.Ticker,
                    Date = DateTime.Parse(e.Date),  // Convert string to DateTime
                    Open = decimal.Parse(e.Open),   // Convert string to decimal
                    Close = decimal.Parse(e.Close), // Convert string to decimal
                    High = decimal.Parse(e.High),   // Convert string to decimal
                    Low = decimal.Parse(e.Low),     // Convert string to decimal
                    Volume = long.Parse(e.Volume)   // Convert string to long
                }).ToList();

                // Save stock data to the database
                await _context.StockData.AddRangeAsync(stockDataList);
                await _context.SaveChangesAsync();

                // Calculate the 5-day simple moving average (SMA)
                var movingAverages = CalculateMovingAverages(closePrices, 5);

                // Return stock data, moving averages, and additional information (open, high, low, volume)
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

        // Method to calculate the SMA (Simple Moving Average)
        private List<double?> CalculateMovingAverages(List<double> prices, int period)
        {
            var movingAverages = new List<double?>();

            for (int i = 0; i < prices.Count; i++)
            {
                if (i < period - 1)
                {
                    movingAverages.Add(null);  // Not enough data points for this moving average
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
