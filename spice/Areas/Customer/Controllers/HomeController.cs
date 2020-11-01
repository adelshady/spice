using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using spice.Data;
using spice.Models;
using spice.Models.ViewModels;
//using Microsoft.AspNetCore.Authorization;

namespace spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;

        public HomeController(ApplicationDbContext _db)
        {
            db = _db;
            var x;
        }

        public async Task<IActionResult> Index()
        {
            IndexViewModel indexVM = new IndexViewModel()
            {
                MenuItem = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).ToListAsync(),
                category = await db.Category.ToListAsync(),
                coupon = await db.Coupon.Where(x => x.IsActive == true).ToListAsync()
            };

            var ClaimIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var  cnt =  db.shoppingCart.Where(x => x.ApplicationUserId == claim.Value).ToList().Count;
                HttpContext.Session.SetInt32("ssCartCount", cnt);
            }

            return View(indexVM);
        }
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menuitemDb = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).Where(x => x.Id == id).FirstOrDefaultAsync();
            ShoppingCart shoppingCart = new ShoppingCart()
            {
                MenuItem = menuitemDb,
                MenuItemId = menuitemDb.Id
            };
            return View(shoppingCart);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart shoppingCartobj)
        {
            shoppingCartobj.Id = 0;
          
            //if (ModelState.IsValid)
            //{

                var ClaimIdentity = (ClaimsIdentity)this.User.Identity;
                var claim = ClaimIdentity.FindFirst(ClaimTypes.NameIdentifier);
                shoppingCartobj.ApplicationUserId = claim.Value;

                ShoppingCart shoppingCart = await db.shoppingCart.Where(x => x.ApplicationUserId == shoppingCartobj.ApplicationUserId && x.MenuItemId == shoppingCartobj.MenuItemId).FirstOrDefaultAsync();

                if (shoppingCart == null)
                {
                    await db.shoppingCart.AddAsync(shoppingCartobj);
                }
                else
                {
                    shoppingCart.Count += shoppingCartobj.Count;
                }
                await db.SaveChangesAsync();

                var count = db.shoppingCart.Where(x => x.ApplicationUserId == shoppingCartobj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("ssCartCount", count);
                return RedirectToAction(nameof(Index));

            //}
            //else
            //{
            //    var menuitemDb = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).Where(x => x.Id == shoppingCartobj.MenuItemId).FirstOrDefaultAsync();
            //    ShoppingCart shoppingCart = new ShoppingCart()
            //    {
            //        MenuItem = menuitemDb,
            //        MenuItemId = menuitemDb.Id
            //    };
            //    return View(shoppingCart);
            //}
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
