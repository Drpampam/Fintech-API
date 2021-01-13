using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WalletAPI.Services.Models
{
    public class Transaction : BaseModel
    {
        [Required]
        public string Type { get; set; }
        [Required]
        public string Currency { get; set; }
        [Required]
        public double Amount { get; set; }
        [Required]
        public string Status { get; set;}

        public User Users { get; set; }
        [Required]
        public string WalletId { get; set; }
        public Wallet Wallet { get; set; }
    }
}
