using System.ComponentModel.DataAnnotations;

namespace vuez.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Používateľské meno je povinné.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Heslo je povinné.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
