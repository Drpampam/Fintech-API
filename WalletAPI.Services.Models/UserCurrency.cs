using System;
using System.Collections.Generic;
using System.Text;

namespace WalletAPI.Services.Models
{
    public class UserCurrency : BaseModel
    {
        public string MainCurrency { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }
    }
}
