namespace PxServices.Models
{
    public class PinBarAlgoConfig
    {
        public string Ticker { get; set; }
        public int Tolerance { get; set; }
        public int WindowSize { get; set; }
        public int KeyDiffZeros { get; set; }
    }
}
