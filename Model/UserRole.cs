using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Model
{
    [Table("user_role")]
    public class UserRole
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public int user_id { get; set; }
        public int role_id { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public DateTime? deleted_at { get; set; }

        [ForeignKey("user_id")]
        public virtual User User { get; set; }
        
        [ForeignKey("role_id")]
        public virtual Role Role { get; set; }
    }
}
