using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Test3.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        public decimal Price { get; set; }
        public bool status { get; set; }

        public string? userName { get; set; }

        public virtual ICollection<OrderItems>? OrderItems { get; set; }

        public virtual IdentityUser? User { get; set; }

    }
}
