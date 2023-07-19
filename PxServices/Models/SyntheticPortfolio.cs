using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class Portfolio
    {
        private IList<Instrument> Instruments;

        public Portfolio()
        {
            Instruments = new List<Instrument>();
        }

        public void Add(Instrument stock)
        {
            Instruments.Add(stock);
        }

        public IList<Instrument> GetInstruments()
        {
            return Instruments;
        }

        public bool IsValid()
        {
            if (Instruments.Count <= 1)
                return false;

            foreach(var instrument in Instruments)
            {
                if (instrument.DataPoints == null || instrument.DataPoints.Count <= 1)
                    return false;

                if (instrument.DataPoints[0].Date == null)
                    return false;
            }

            return true;
        }
    }
}