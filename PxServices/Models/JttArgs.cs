using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PxServices.Models
{
    public class JttArgs
    {
        public string TickerA { get; set; }
        public string TickerB { get; set; }
        public decimal PctA { get; set;} 
        public decimal PctB { get; set; }  
        public decimal M1 { get; set; } 
        public decimal M2 { get; set; } 
        public decimal B1 { get; set; } 
        public decimal B2 { get; set; }
        public int StartYear { get; set;} 
        public int StartMonth { get; set;} 
        public decimal StartAmount { get; set; }
        public decimal LastTickerAPrice { get; set;} 
        public decimal LastTickerBPrice { get; set; }
        public string JttBoundTopOneDt { get; set;} 
        public string JttBoundTopTwoDt { get; set; }
        public string JttBoundBottomOneDt { get; set;} 
        public string JttBoundBottomTwoDt { get; set; }
        public decimal JttBoundTopValueOne { get; set;} 
        public decimal JttBoundTopValueTwo { get; set; }
        public decimal JttBoundBottomValueOne { get; set;} 
        public decimal JttBoundBottomValueTwo { get; set; }
    }
}