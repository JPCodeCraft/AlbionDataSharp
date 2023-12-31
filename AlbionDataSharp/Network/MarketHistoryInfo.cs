﻿using AlbionData.Models;

namespace AlbionDataSharp.Network
{
    public class MarketHistoryInfo
    {
        public uint AlbionId { get; set; }
        public Timescale Timescale { get; set; }
        public ushort Quality { get; set; }
        public string LocationID { get; set; }
    }
}
