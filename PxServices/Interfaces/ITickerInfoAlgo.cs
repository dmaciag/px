using PxServices.Models;
namespace PxServices.Interfaces
{
    public interface ITickerInfoAlgo
    {
        TickerDataResultSet GetTickerInfos(TickerDataSetArgs args);
    }
}
