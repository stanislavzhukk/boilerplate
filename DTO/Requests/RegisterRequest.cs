using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO.Requests
{
    public class RegisterRequest
    {
        [Required]
        public required string Email { get; set; }
        [Required]
        public required string Name { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
