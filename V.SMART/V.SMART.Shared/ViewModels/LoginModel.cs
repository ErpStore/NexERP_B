using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.ViewModels
{
    public class LoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
