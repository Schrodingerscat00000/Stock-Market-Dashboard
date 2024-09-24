using System.ComponentModel.DataAnnotations;

public class StockEntity
{
    [Key]
    public int Id { get; set; }

    public string Ticker { get; set; }
    public string Date { get; set; }
    public string Open { get; set; }
    public string Close { get; set; }
    public string High { get; set; }
    public string Low { get; set; }
    public string Volume { get; set; }
}
