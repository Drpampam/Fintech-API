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
        [Range(0, int.MaxValue, ErrorMessage = "Enter a valid integer number")]
        public double Amount { get; set; }
        [Required]
        public string Status { get; set;}

        public string UserId { get; set; }
        public User User { get; set; }
        [Required]
        public string WalletId { get; set; }
        public Wallet Wallet { get; set; }
    }
}
