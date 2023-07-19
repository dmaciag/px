using PxServices.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class BoundSet
    {
        private static readonly log4net.ILog Logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Line UpperBoundLine { get; private set; }
        public Line LowerBoundLine { get; private set; }

        public BoundSet(CoordinateSet upperCoordinateSet, CoordinateSet bottomCoordinateSet)
        {
            UpperBoundLine = new Line(upperCoordinateSet);
            LowerBoundLine = new Line(bottomCoordinateSet);
        }
    }
}