using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Model
{
    [Table("role")]
    public class Role
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public DateTime? deleted_at { get; set; }
        
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
