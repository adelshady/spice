using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using spice.Data;
using spice.Models;
using spice.Models.ViewModels;
using spice.Utiles;

namespace spice.Areas.Admin.Controllers
{
    [Authorize(Roles = SD.MangerUser)]
    [Area("Admin")]
    public class SubCategoryController : Controller
    {

        private readonly ApplicationDbContext db;
        [TempData]
        public string StutasMessage { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            this.db = db;
        }
        public async Task<IActionResult> Index()
        {
            var SubCtegory = await db.SubCategory.Include(x=>x.Category).ToListAsync();
            return View(SubCtegory);
        }
        public async Task<IActionResult> Create()
        {
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(p => p.Name).Distinct().ToListAsync(),
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ExitedSubCategory =  db.SubCategory.Include(x => x.Category).Where(x => x.Name == model.SubCategory.Name && x.Category.Id == model.SubCategory.CategoryId);

                if (ExitedSubCategory.Count() > 0)
                {
                    StutasMessage = "Error: SubCategory exited under " + ExitedSubCategory.First().Category.Name +" "+ " category  please use another name";
                }
                else
                {
                    db.SubCategory.Add(model.SubCategory);
                    await db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
             }
            SubCategoryAndCategoryViewModel VmModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                StatusMassage = StutasMessage
            };
            return View(VmModel);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategory = await db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);
            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = subcategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(p => p.Name).Distinct().ToListAsync(),
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,SubCategoryAndCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var ExitedSubCategory = db.SubCategory.Include(x => x.Category).Where(x => x.Name == model.SubCategory.Name && x.Category.Id == model.SubCategory.CategoryId);

                if (ExitedSubCategory.Count() > 0)
                {
                    StutasMessage = "Error: SubCategory exited under " + ExitedSubCategory.First().Category.Name + " " + " category  please use another name";
                }
                else
                {
                    var subcategory = await db.SubCategory.FindAsync(id);
                    subcategory.Name = model.SubCategory.Name;
                   
                    await db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            SubCategoryAndCategoryViewModel VmModel = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = model.SubCategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(x => x.Name).ToListAsync(),
                StatusMassage = StutasMessage
            };
            return View(VmModel);
        }

        [HttpGet]
        public async Task<IActionResult> Detalis(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subcategory = await db.SubCategory.FindAsync(id);

            SubCategoryAndCategoryViewModel model = new SubCategoryAndCategoryViewModel()
            {
                CategoryList = await db.Category.ToListAsync(),
                SubCategory = subcategory,
                SubCategoryList = await db.SubCategory.OrderBy(x => x.Name).Select(p => p.Name).Distinct().ToListAsync(),
            };
            return View(model);
        }

        public async Task<IActionResult> Delete (int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var Deletesubcategory = await db.SubCategory.FindAsync(id);
            db.SubCategory.Remove(Deletesubcategory);
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }

        [ActionName("GetSubCategory")]
        public async Task<IActionResult> GetSubCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await(from subcategory in db.SubCategory
                             where subcategory.CategoryId == id
                             select subcategory).ToListAsync();
            return Json(new SelectList(subCategories, "Id", "Name"));
        }
    }   
}
