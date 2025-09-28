using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hipot.Data.Core.Models
{
    public class Project
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
