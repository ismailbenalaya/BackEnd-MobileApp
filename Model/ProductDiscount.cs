using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Model{

    [Table("discount")]
    public class ProductDiscount
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public DateTime? deleted_at { get; set; }
    }

}