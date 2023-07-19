using PricedX.Models;
using PricedX.Utils;
using PxServices.Interfaces;
using PxServices.Models;
using System.Linq;
using System.Reflection;
using System.Text;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace PxServices.CoreAlgos
{
    public class PinBarAlgo : IPinBarAlgo
    {
        public IDataRetrievalService _dataRetrievalService { get; set; }
        public IHistoricRatioAlgo _historicRatioAlgo { get; set; }
        public IEngine _engine { get; set; }
        public IDateGetter _dateGetter { get; set; }
        public int _roundCount { get; set; }


        public PinBarAlgo(IDataRetrievalService dataRetrievalService, IEngine engine, IHistoricRatioAlgo historicRatioAlgo)
        {
            _historicRatioAlgo = historicRatioAlgo;
            _dataRetrievalService = dataRetrievalService;
            _engine = engine;
            _dateGetter = new OuterDateGetter();
            _roundCount = 10;
        }

        public void StartAlgo(PinBarAlgoConfig config)
        {
            if (string.IsNullOrEmpty(config.Ticker))
                return;

            var bars = _dataRetrievalService.GetPinBars(config.Ticker);
            if (bars.Count < config.WindowSize)
                return;


            var targetBars = bars.TakeLast(config.WindowSize).ToList();
            RunCore(bars, targetBars, config);

            return;
            for (int i = config.WindowSize -1; i < bars.Count; i++)
            {
                //var targetBars = bars.Take(i+1).TakeLast(config.WindowSize).ToList();
                //RunCore(bars, targetBars, config);
            }
        }

        private void RunCore(IList<PinBar> bars, IList<PinBar> targetBars, PinBarAlgoConfig config)
        {
            var targetDifferential = GetPinbarDeltaMap(targetBars, config);
            var matches = GetMatches(bars, targetDifferential, config, out int matchCount);
            if (matchCount == 0)
                return;

            decimal? avgMatchDiff = CalcAverageDiff(matches);
            
            if(Math.Abs(avgMatchDiff.Value) > 0.25m && matchCount > 10)
            {
                string x = "hi";
            }

            return;
        }

        private decimal CalcAverageDiff(IList<Tuple<PinBar, PinBar>> matches)
        {
            var avgMatchDiff = 0.0m;
            for (int i = 0; i < matches.Count; i++)
            {
                var match = matches[i];

                var beforeBar = match.Item1;
                var afterBar = match.Item2;

                var closeDiffPct = (afterBar.Close / beforeBar.Close - 1) * 100.0m;

                if (i == 0)
                {
                    avgMatchDiff = closeDiffPct;
                    continue;
                }

                avgMatchDiff = (avgMatchDiff * i + closeDiffPct) / (i + 1);
            }

            return avgMatchDiff;
        }

        private IList<Tuple<PinBar, PinBar>> GetMatches(IList<PinBar> bars, Dictionary<int, PinBarDelta> targetDifferential, PinBarAlgoConfig config, out int matchCount)
        {
            var queue = new Queue<PinBar>(config.WindowSize);
            var matches = new List<Tuple<PinBar, PinBar>>();
            matchCount = 0;
            for (int i = 0; i < bars.Count; i++)
            {
                var lastBar = i == 0 ? null : bars[i - 1];
                var upcomingBar = bars[i];
                if (queue.Count < config.WindowSize)
                {
                    queue.Enqueue(bars[0]);
                    continue;
                }

                var historicDifferential = GetPinbarDeltaMap(queue.ToList(), config);
                if (IsWithinTolerance(targetDifferential, historicDifferential, config) && lastBar != null)
                {
                    matchCount++;
                    matches.Add(new Tuple<PinBar, PinBar>(lastBar, upcomingBar));
                }

                queue.Dequeue();
                queue.Enqueue(upcomingBar);
            }

            return matches;
        }

        private bool IsWithinTolerance(Dictionary<int, PinBarDelta> targetDeltaMap, Dictionary<int, PinBarDelta> historicDeltaMap, PinBarAlgoConfig config)
        {
            var tolerancePct = (decimal)config.Tolerance;
            foreach(var kv in targetDeltaMap)
            {
                if (!historicDeltaMap.ContainsKey(kv.Key))
                    return false;

                var targetPinBarDelta = kv.Value;
                var historicPinBarDelta = historicDeltaMap[kv.Key];

                var pctMultiplier = 100.0m;

                var deltaDiffOpen = (targetPinBarDelta.DiffOpen - historicPinBarDelta.DiffOpen) * pctMultiplier;
                var deltaDiffClose = (targetPinBarDelta.DiffClose - historicPinBarDelta.DiffClose) * pctMultiplier;

                if (Math.Abs(deltaDiffOpen) > tolerancePct || Math.Abs(deltaDiffClose) > tolerancePct)
                    return false;
            }

            return true;
        }

        private Dictionary<int, PinBarDelta> GetPinbarDeltaMap(IList<PinBar> pinBars, PinBarAlgoConfig config)
        {
            var deltaMap = new Dictionary<int, PinBarDelta>();
            for (int i = 1; i< pinBars.Count; i++)
            {
                var sourceBar = pinBars[i - 1];
                var destBar = pinBars[i];

                deltaMap.Add(i, new PinBarDelta()
                {
                    DiffOpen = PctDiff(sourceBar.Open, destBar.Open, config),
                    DiffClose = PctDiff(sourceBar.Close, destBar.Close, config)
                });
            }

            return deltaMap;
        }

        private decimal PctDiff(decimal source, decimal destination, PinBarAlgoConfig config)
        {
            var k = config.KeyDiffZeros + 1;
            var multipler = Math.Pow(10, k - 1);
            var multipledPct = (int)((destination / source - 1) * 100.0m * (decimal)multipler);
            return ((decimal)multipledPct / (decimal)Math.Pow(10,k-1));
        }
    }
}