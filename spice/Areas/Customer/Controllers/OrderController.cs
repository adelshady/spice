using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using spice.Data;
using spice.Models;
using spice.Models.ViewModels;
using spice.Utiles;

namespace spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext db;
        public OrderController(ApplicationDbContext _db)
        {
            db = _db;
        }
        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var claimidentity = (ClaimsIdentity)User.Identity;
            var claim = claimidentity.FindFirst(ClaimTypes.NameIdentifier);
            //OrderDetalis order = await db.OrderDetalis.Include(x => x.OrderHeader).Include(x => x.OrderHeader.ApplicationUser)
            //    .FirstOrDefaultAsync(x => x.OrderHeader.UserId == claim.Value && x.Id == id);
           // List<OrderDetalisViewModel> test = new List<OrderDetalisViewModel>();
            //return View(order);
            OrderDetalisViewModel OrderDetalisVM = new OrderDetalisViewModel()
            {
                orderHeader = await db.OrderHeader.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.UserId == claim.Value && x.Id == id),
                orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == id).ToListAsync()
            };
            
            return View(OrderDetalisVM);
        }
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> OrderHistory()
        {
            var claimidentity = (ClaimsIdentity)User.Identity;
            var cliam = claimidentity.FindFirst(ClaimTypes.NameIdentifier);

            List<OrderDetalisViewModel> OrderDetalis = new List<OrderDetalisViewModel>();

            List<OrderHeader> orderHeaders = await db.OrderHeader.Include(x => x.ApplicationUser).Where(x => x.UserId == cliam.Value).ToListAsync();
            foreach (var item in orderHeaders)
            {
                OrderDetalisViewModel orderDetalisVM = new OrderDetalisViewModel()
                {
                    orderHeader = item,
                    orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == item.Id).ToListAsync()
                };
                OrderDetalis.Add(orderDetalisVM);
            }
            return View(OrderDetalis);
        }

        public async Task<IActionResult> TotalOrder()
        {
            

            List<OrderDetalisViewModel> OrderDetalis = new List<OrderDetalisViewModel>();
            List<OrderHeader> orderHeaders = await db.OrderHeader.Include(x=>x.ApplicationUser).ToListAsync();

       //     List<OrderHeader> orderHeaders = await db.OrderHeader.Include(x => x.ApplicationUser).Where(x => x.UserId == cliam.Value).ToListAsync();
            foreach (var item in orderHeaders)
            {
                OrderDetalisViewModel orderDetalisVM = new OrderDetalisViewModel()
                {
                    orderHeader = item,
                    orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == item.Id).ToListAsync()
                };
                OrderDetalis.Add(orderDetalisVM);
            }
            return View(OrderDetalis.OrderByDescending(x=>x.orderHeader.PickUpTime));
        }

        [Authorize(Roles = SD.MangerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> MangeOrder()
        {
            List<OrderDetalisViewModel> OrderDetalis = new List<OrderDetalisViewModel>();

            List<OrderHeader> orderHeaders = await db.OrderHeader.Where(x=>x.status.ToLower() == SD.StatusSubmitted || x.status.ToLower()== SD.StatusInProcess)
                                             .OrderByDescending(x=>x.PickUpTime)
                                             .ToListAsync();
            foreach (var item in orderHeaders)
            {
                OrderDetalisViewModel orderDetalisVM = new OrderDetalisViewModel()
                {
                    orderHeader = item,
                    orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == item.Id).ToListAsync()
                };
                OrderDetalis.Add(orderDetalisVM);
            }
            return View(OrderDetalis.OrderByDescending(x=>x.orderHeader.PickUpTime));
        }
        [Authorize(Roles= SD.MangerUser + ","+  SD.KitchenUser)]
        public async Task<IActionResult> OrderPreapare(int id)
        {
            OrderHeader orderHeader = await db.OrderHeader.FindAsync(id);
            orderHeader.status = SD.StatusInProcess;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(MangeOrder));
        }
        [Authorize(Roles = SD.MangerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> OrderReady(int id)
        {
            OrderHeader orderHeader = await db.OrderHeader.FindAsync(id);
            orderHeader.status = SD.StatusReady;
            await db.SaveChangesAsync();
            //  email logic to notify user that order is read for pickup
            return RedirectToAction(nameof(MangeOrder));
        }
        [Authorize(Roles = SD.MangerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> OrderCancel(int id)
        {
            OrderHeader orderHeader = await db.OrderHeader.FindAsync(id);
            orderHeader.status = SD.StatusCancelled;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(MangeOrder));
        }


        [Authorize]
        public async Task<IActionResult> GetOrderDetalis(int id)
        {
            OrderDetalisViewModel orderDetalisVM = new OrderDetalisViewModel()
            {
                orderHeader = await db.OrderHeader.Include(x => x.ApplicationUser).FirstOrDefaultAsync(x => x.Id == id),
                orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == id).ToListAsync()
            };

            return PartialView("_IndividualOrderDetalis", orderDetalisVM);
        }

    }
}
