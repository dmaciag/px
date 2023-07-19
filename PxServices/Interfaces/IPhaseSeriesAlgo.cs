using PxServices.Models;

namespace PxServices.Interfaces
{
    public interface IPhaseSeriesAlgo
    {
        PhaseSeries GetPhaseSeries(PhaseSeriesArgs phaseSeriesArgs);
    }
}
