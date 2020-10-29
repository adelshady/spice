using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spice.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace spice.ViewComponenets
{
    public class UserNameViewComponenet : ViewComponent
    {
        private readonly ApplicationDbContext db;
        public UserNameViewComponenet(ApplicationDbContext _db)
        {
            db = _db;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var userfromdb = await db.ApplicationUser.FirstOrDefaultAsync(x => x.Id == claim.Value);

            return View(userfromdb);
        }
    }
}
