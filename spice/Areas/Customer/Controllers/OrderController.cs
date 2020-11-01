using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender emailSender;
        public OrderController(ApplicationDbContext _db , IEmailSender _emailSender)
        {
            emailSender = _emailSender;
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

        [HttpGet]
        public async Task<IActionResult> Invoice(int? id)
        {
          
          
                OrderDetalisViewModel orderDetalisVM = new OrderDetalisViewModel()
                {
                    orderHeader = await db.OrderHeader.Include(x => x.ApplicationUser).Where(x => x.Id == id).FirstOrDefaultAsync(),
                    orderDetalis = await db.OrderDetalis.Where(x => x.OrderId == id).ToListAsync()
                };
               
            return View(orderDetalisVM);
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
            await emailSender.SendEmailAsync(db.Users.Where(x => x.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - order Created " + orderHeader.Id.ToString(), "order has been Preaper successfuly");

            return RedirectToAction(nameof(MangeOrder));
        }
        [Authorize(Roles = SD.MangerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> OrderReady(int id)
        {
            OrderHeader orderHeader = await db.OrderHeader.FindAsync(id);
            orderHeader.status = SD.StatusReady;
            await db.SaveChangesAsync();
            //  email logic to notify user that order is read for pickup
            await emailSender.SendEmailAsync(db.Users.Where(x => x.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - order Created " + orderHeader.Id.ToString(), "order has been Ready successfuly");

            return RedirectToAction(nameof(MangeOrder));
        }
        [Authorize(Roles = SD.MangerUser + "," + SD.KitchenUser)]
        public async Task<IActionResult> OrderCancel(int id)
        {
            OrderHeader orderHeader = await db.OrderHeader.FindAsync(id);
            orderHeader.status = SD.StatusCancelled;
            await db.SaveChangesAsync();
            await emailSender.SendEmailAsync(db.Users.Where(x => x.Id == orderHeader.UserId).FirstOrDefault().Email, "Spice - order Created " + orderHeader.Id.ToString(), "order has been Canceled successfuly");

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
        // Order Details action link
        [Authorize]
        public async Task<IActionResult> OrderPickup(string searchName = null, string searchPhone = null, string searchEmail = null)
        {

            StringBuilder pram = new StringBuilder();
            pram.Append("/Customer/Order/OrderPickup?");
            pram.Append("&searchName");
            if (searchName != null)
            {
                pram.Append(searchName);
            }
            pram.Append("&srearchPhone");
            if (searchPhone != null)
            {
                pram.Append(searchPhone);
            }
            pram.Append("&srearchEmail");
            if (searchEmail != null)
            {
                pram.Append(searchEmail);
            }
            List<OrderDetalisViewModel> orderList = new List<OrderDetalisViewModel>();
            List<OrderHeader> orderHeader = new List<OrderHeader>();
            if (searchPhone != null || searchName != null || searchEmail != null)
            {

                var user = new ApplicationUser();

                if (searchName != null)
                {
                    orderHeader = await db.OrderHeader.Where(o => o.PickName.ToLower().Contains(searchName.ToLower()) && o.status == SD.StatusReady)
                        .OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                {
                    if (searchPhone != null)
                    {
                        orderHeader = await db.OrderHeader.Where(o => o.PickName.Contains(searchName) && o.status == SD.StatusReady)
                            .OrderByDescending(o => o.OrderDate).ToListAsync();
                    }
                    else
                    {
                        user = await db.ApplicationUser.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                        if (searchEmail != null)
                        {
                            orderHeader = await db.OrderHeader.Include(o => o.ApplicationUser)
                                .Where(o => o.UserId == user.Id && o.status == SD.StatusReady).OrderByDescending(o => o.OrderDate).ToListAsync();
                        }
                    }
                }
            }
            else
            {
                orderHeader = await db.OrderHeader.Include(o => o.ApplicationUser).Where(x => x.status == SD.StatusReady).ToListAsync();

            }
            foreach (OrderHeader item in orderHeader)
            {
                OrderDetalisViewModel orderDetalisViewModel = new OrderDetalisViewModel()
                {
                    orderHeader = item,
                    orderDetalis = await db.OrderDetalis.Where(o => o.OrderId == item.Id).ToListAsync()
                };
                orderList.Add(orderDetalisViewModel);
            }
            return View(orderList);
        }
        
    }

}

