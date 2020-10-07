using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace spice.Models
{
    public class MenuItem
    {
        public int Id { get; set; }
        [Required]
        public String Name { get; set; }

        [AllowHtml]
        public string Description { get; set; }
        public string spicyness { get; set; }
        public enum Espicy { Na = 0, NotSpicy, spice = 2, veryspice = 3 }
         //[AllowExtensions(Extensions = "png,jpg", ErrorMessage = "Please select only Supported Files .png | .jpg")]
         
        public string Image { get; set; }
      
        [Display(Name = "SubCategory")]
        public int SubCategoryId { get; set; }
        [ForeignKey("SubCategoryId")]
        public virtual SubCategory SubCategory { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = " Price should be grater than ${1}")]
        public double price { get; set; }
    }
}
