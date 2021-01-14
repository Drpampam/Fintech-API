using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace WalletAPI.Services.Models
{
    public class Wallet : BaseModel
    {
        public double Balance { get; set; } = 0;
        [Required]
        public string Currency { get; set; }
        
        [Required]
        public string UserId { get; set; }
        public User User { get; set; }
        public ICollection<Transaction> Transactions { get; set; }
    }
}
