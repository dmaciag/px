using PxServices.Models;
using System;
using System.Collections.Generic;

namespace PxServices.Interfaces
{
    public interface IJttSeriesAlgo
    {
        JttResult RunAlgo(JttArgs jttArgs);
    }
}
