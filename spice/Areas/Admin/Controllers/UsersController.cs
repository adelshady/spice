using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spice.Data;
using spice.Utiles;

namespace spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.MangerUser)]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext db;
        public UsersController(ApplicationDbContext _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {
            var calaimidentity = (ClaimsIdentity)this.User.Identity;
            var claim = calaimidentity.FindFirst(ClaimTypes.NameIdentifier);
            return View(await db.ApplicationUser.Where(x => x.Id != claim.Value).ToListAsync());
        }

        public async Task<IActionResult> Lock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var applicationUser = await db.ApplicationUser.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.LockoutEnd = DateTime.Now.AddYears(1000);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UnLock(string id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var applicationUser = await db.ApplicationUser.Where(x => x.Id == id).FirstOrDefaultAsync();
            if (applicationUser == null)
            {
                return NotFound();
            }

            applicationUser.LockoutEnd = DateTime.Now;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
