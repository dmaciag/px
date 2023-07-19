using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PricedX.Models
{
    public class EngineIndex
    {
        public int StartIndex { get; private set; }

        public EngineIndex(int startIndex)
        {
            Set(startIndex);
        }

        public void Set(int startIndex)
        {
            StartIndex = startIndex;
        }
    }
}