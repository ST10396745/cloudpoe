using System.ComponentModel.DataAnnotations;

namespace Test3.Models
{
    public class OrderItems
    {
        [Key]
        public int OrderItemID { get; set; }

        public int OrderID { get; set; }

        public int ProductID { get; set; }

        public double Price { get; set; }

        public virtual Order? Order { get; set; }

        public virtual Product? Product { get; set; }
    }

}
