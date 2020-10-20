using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Models.ViewModels
{
    public class OrderDetalisCart
    {
        public List<ShoppingCart> ListCart { get; set; }
        public OrderHeader orderHeader { get; set; }
    }
}
