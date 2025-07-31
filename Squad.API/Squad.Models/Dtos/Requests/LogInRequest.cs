using System.ComponentModel.DataAnnotations;

namespace Squad.Models.Dtos.Requests
{
    public class LogInRequest
    {
        [Required(ErrorMessage = "Username is required.")]
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; }
    }
}
