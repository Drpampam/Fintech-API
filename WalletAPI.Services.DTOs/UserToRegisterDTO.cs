using System;
using System.ComponentModel.DataAnnotations;

namespace WalletAPI.Services.DTOs
{
    public class UserToRegisterDTO
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MaxLength(14, ErrorMessage = "Phone number length should not be greater than 14")]
        [RegularExpression(@"^\+\d{2,3}\d{9,10}$", ErrorMessage = "The format must include a country-code e.g +234800000000")]
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        [Required]
        public string MainCurrency { get; set; }
    }
}
