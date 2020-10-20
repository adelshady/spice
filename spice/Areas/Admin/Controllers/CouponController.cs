using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spice.Data;
using spice.Models;
using spice.Utiles;

namespace spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.MangerUser)]
    [Area("Admin")]
    public class CouponController : Controller
    {
        private readonly ApplicationDbContext db;
        public CouponController(ApplicationDbContext _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {
            return View(await db.Coupon.ToListAsync());
        }

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coupon.Picture = p1;
                }
                db.Coupon.Add(coupon);
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(coupon);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var couponid = db.Coupon.Find(id);
            return View(couponid);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Coupon coupon)
        {
            var coup = db.Coupon.Where(x => x.Id == id).SingleOrDefault();
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {

                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    coup.Picture = p1;
                }
                else
                {
                    coupon.Picture = coup.Picture;
                  
                }
            }
            coup.Name = coupon.Name;
            coup.Discount = coupon.Discount;
            coup.MinimumAmount = coupon.MinimumAmount;
            coup.IsActive = coupon.IsActive;
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }

        
        public async Task<IActionResult> Detalis(int id)
        {
            var coupon = await db.Coupon.Where(x => x.Id == id).SingleOrDefaultAsync();
            return View(coupon);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var coupon = await db.Coupon.Where(x => x.Id == id).SingleOrDefaultAsync();
            db.Coupon.Remove(coupon);
          await  db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}