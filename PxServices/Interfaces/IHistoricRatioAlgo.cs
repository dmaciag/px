using PxServices.Models;
using System;
using System.Collections.Generic;

namespace PxServices.Interfaces
{
    public interface IHistoricRatioAlgo
    {
        HistoricRatioSeries GetHistoricRatioSeries(HistoricRatioArgs historicRatioArgs);

        void SetRoundCount(int roundCount);
    }
}
