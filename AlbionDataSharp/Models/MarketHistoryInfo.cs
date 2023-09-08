using AlbionData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlbionDataSharp.Models
{
    public class MarketHistoryInfo
    {
        public uint AlbionId { get; set; }
        public Timescale Timescale { get; set; }
        public ushort Quality { get; set; }
        public string LocationID { get; set; }
    }
}
