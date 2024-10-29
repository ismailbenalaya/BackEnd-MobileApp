using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackEnd.Model{

    [Table("product_inventory")]
    public class ProductInventory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int Quantity { get; set; }
        public DateTime created_at { get; set; }
        public DateTime modified_at { get; set; }
        public DateTime? delete_at { get; set; }

    }
}
