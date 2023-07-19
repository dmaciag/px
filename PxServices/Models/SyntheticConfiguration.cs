using PxServices.Enums;

namespace PxServices.Models
{
    public class Config
    {
        public decimal StartAmount { get; private set; }
        public int StartYear { get; }
        public int StartMonth { get; }

        public decimal LastTopTickerPrice { get; }
        public decimal LasBotTickerPrice { get; }
        
        public DistributionType DistributionType { get; }

        public Config(decimal startAmount, int startYear, int startMonth, decimal lastTopTickerPrice, decimal lastBotTickerPrice, DistributionType distributionType)
        {
            StartAmount = startAmount;
            StartYear = startYear;
            StartMonth = startMonth;
            LastTopTickerPrice = lastTopTickerPrice;
            LasBotTickerPrice = lastBotTickerPrice;
            DistributionType = distributionType;
        }

        public void TrySetAmount(decimal startAmount)
        {
            if (StartAmount <= 0)
                StartAmount = startAmount;
        }

        public bool HasLastPredictedPrice => LastTopTickerPrice > 0 && LasBotTickerPrice > 0;
        public override string ToString() => $"StartAmount: {StartAmount}, StartYear: {StartYear}, StartMonth: {StartMonth}.";
    }
}