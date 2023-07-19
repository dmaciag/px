using PxServices.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PxServices.Interfaces
{
    public interface IDataRetrievalService
    {
        IList<Point> GetPoints(string ticker); 
        IList<PinBar> GetPinBars(string ticker);
    }
}
