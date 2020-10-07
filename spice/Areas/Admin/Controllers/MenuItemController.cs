using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using spice.Data;
using spice.Extension;
using spice.Models;
using spice.Models.ViewModels;
using spice.Utiles;

namespace spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class MenuItemController : Controller
    {
        public readonly ApplicationDbContext db;
        public readonly IWebHostEnvironment webHostEnvironment;
        [BindProperty]
        public MenuItemViewModel menuItemViewModel { get; set; }
        public MenuItemController(ApplicationDbContext _db, IWebHostEnvironment _webHostEnvironment)
        {
            db = _db;
            webHostEnvironment = _webHostEnvironment;
            menuItemViewModel = new MenuItemViewModel()
            {
                SubCategory = db.SubCategory,
                Category = db.Category,
                Menuitem = new MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItem = await db.MenuItem.Include(s => s.SubCategory.Category).Include(s => s.SubCategory).ToListAsync();
            return View(menuItem);
        }

        public IActionResult Create()
        {

            ViewBag.category = new SelectList(menuItemViewModel.Category, "Id", "Name");
            return View(menuItemViewModel);
        }

        [RequestSizeLimit(1_000_000)]
        [HttpPost]
        [ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(IFormCollection formValues)
        {
            try
            {
                var id = menuItemViewModel.Menuitem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategory"]);

                //menuItemViewModel.Menuitem.Description = Request.Form["Description"].ToString();
                var sub = db.SubCategory.Find(id);

                menuItemViewModel.Menuitem.SubCategory = sub;

                db.MenuItem.Add(menuItemViewModel.Menuitem);
                await db.SaveChangesAsync();


                var temp = formValues["Description"];
                menuItemViewModel.Menuitem.Description = temp;
                string webrootpath = webHostEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;

                var menuitemfromdb = await db.MenuItem.FindAsync(menuItemViewModel.Menuitem.Id);

                if (files.Count() > 0)
                {
                    var uploads = Path.Combine(webrootpath, "images");
                    var extension = Path.GetExtension(files[0].FileName);
                    using (var filesStream = new FileStream(Path.Combine(uploads, menuItemViewModel.Menuitem.Id + extension), FileMode.Create))
                    {
                        files[0].CopyTo(filesStream);
                    }

                    menuitemfromdb.Image = @"\images\" + menuItemViewModel.Menuitem.Id + extension;
                }
                else
                {
                    var uploads = Path.Combine(webrootpath, @"images\" + SD.DefaultFoodImage);
                    System.IO.File.Copy(uploads, webrootpath + @"\images\" + menuItemViewModel.Menuitem.Id + ".png");
                    menuitemfromdb.Image = @"\images\" + menuItemViewModel.Menuitem.Id + ".png";
                }
                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Ok();
            }
        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {

            menuItemViewModel.Menuitem = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).SingleOrDefaultAsync(x => x.Id == id);


            return View(menuItemViewModel);
        }
        [RequestSizeLimit(1_000_000)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IFormCollection formValues)
        {
            //menuItemViewModel.Menuitem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategory"]);
            var temp = formValues["Description"];
            var menuitemfromdb = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).SingleOrDefaultAsync(x => x.Id == id);
            // menuItemViewModel.Menuitem.Description = temp;
            string webrootpath = webHostEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

            if (files.Count() > 0)
            {
                var uploads = Path.Combine(webrootpath, "images");
                var extension = Path.GetExtension(files[0].FileName);

                var imagepath = Path.Combine(webrootpath, menuitemfromdb.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }

                using (var filesStream = new FileStream(Path.Combine(uploads, menuItemViewModel.Menuitem.Id + extension), FileMode.Create))
                {
                    files[0].CopyTo(filesStream);
                }

                menuitemfromdb.Image = @"\images\" + menuItemViewModel.Menuitem.Id + extension;
            }

            menuitemfromdb.Name = menuItemViewModel.Menuitem.Name;
            menuitemfromdb.Description = menuItemViewModel.Menuitem.Description;
            menuitemfromdb.price = menuItemViewModel.Menuitem.price;
            menuitemfromdb.spicyness = menuItemViewModel.Menuitem.spicyness;
            menuitemfromdb.SubCategoryId = menuItemViewModel.Menuitem.SubCategoryId;
            menuitemfromdb.SubCategory.CategoryId = menuItemViewModel.Menuitem.SubCategory.CategoryId;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public async Task<ActionResult> Detalis(int id)
        {

            menuItemViewModel.Menuitem = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).SingleOrDefaultAsync(x => x.Id == id);
            return View(menuItemViewModel);
        }

        public async Task<ActionResult> Delete(int id)
        {

            menuItemViewModel.Menuitem = await db.MenuItem.Include(x => x.SubCategory).Include(x => x.SubCategory.Category).SingleOrDefaultAsync(x => x.Id == id);
            string webRootPath = webHostEnvironment.WebRootPath;

            var imagePath = Path.Combine(webRootPath, menuItemViewModel.Menuitem.Image.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }
            db.MenuItem.Remove(menuItemViewModel.Menuitem);
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }

}

