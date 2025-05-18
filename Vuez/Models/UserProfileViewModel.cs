using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace vuez.Models.ViewModels
{
    public class UserProfileViewModel
    {
        [Required(ErrorMessage = "Meno je povinné")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Priezvisko je povinné")]
        public string Surname { get; set; }

        [EmailAddress(ErrorMessage = "Neplatný e-mail")]
        public string Email { get; set; }

        public IFormFile? SignatureFile { get; set; }  // ? pre možnosť null
        public string? SignatureImagePath { get; set; }
    }
}
