using System;
using System.Collections.Generic;
using System.Text;

namespace WalletAPI.Services.Models
{
    public class Root
    {
        public bool Success { get; set; }
        public int Timestamp { get; set; }
        public string Base { get; set; }
        public string Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
