using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Model{

    [Table("user")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public int? Telephone { get; set; }
        public DateTime created_at{ get; set; }
        public DateTime? modified_at { get; set; }
        public DateTime? deleted_at { get; set; }
       public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
