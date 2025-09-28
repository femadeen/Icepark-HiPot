using System.ComponentModel.DataAnnotations;

namespace Hipot.Data.Core.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public Role Role { get; set; }

        public int ProjectId { get; set; }

        public Project Project { get; set; }
    }
}
