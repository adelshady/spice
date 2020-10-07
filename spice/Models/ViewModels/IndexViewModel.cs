using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace spice.Models.ViewModels
{
    public class IndexViewModel
    {
        public IEnumerable<MenuItem> MenuItem { get; set; }
        public IEnumerable<Category> category{ get; set; }
        public IEnumerable<Coupon> coupon{ get; set; }
    }

}
