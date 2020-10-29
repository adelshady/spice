using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Models.ViewModels
{
    public class OrderDetalisViewModel
    {
        public OrderHeader orderHeader { get; set; }
        public List<OrderDetalis> orderDetalis { get; set; }
    }
}
