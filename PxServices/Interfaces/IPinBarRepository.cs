using PxServices.Models;

namespace PxServices.Interfaces
{
    public interface IPhaseSeriesRepository
    {
        IList<PhaseSeriesConfig> GetConfigs();
        void SaveConfig(PhaseSeriesConfig config);
        void DeleteConfig(int configId);
    }
}
