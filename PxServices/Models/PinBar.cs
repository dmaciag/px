namespace PxServices.Models
{
    public class PinBar
    {
        public decimal Open { get; set; }
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public int Volume { get; set; }
        public DateTime Date { get; set; }
    }
}
