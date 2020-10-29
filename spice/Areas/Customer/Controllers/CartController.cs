using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using spice.Data;
using spice.Models;
using spice.Models.ViewModels;
using spice.Utiles;
using Stripe;


namespace spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        public readonly ApplicationDbContext db;
        [BindProperty]
        public OrderDetalisCart detalisCart { get; set; }
        public CartController(ApplicationDbContext _db)
        {
            db = _db;
        }
        public async Task<IActionResult> Index()
        {

            detalisCart = new OrderDetalisCart()
            {
                orderHeader = new Models.OrderHeader()
            };
            // detalisCart.orderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            var cart = db.shoppingCart.Where(x => x.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detalisCart.ListCart = cart.ToList();
            }
            foreach (var list in detalisCart.ListCart)
            {
                list.MenuItem = await db.MenuItem.Where(x => x.Id == list.MenuItemId).FirstOrDefaultAsync();
                detalisCart.orderHeader.OrderTotal = detalisCart.orderHeader.OrderTotal + (list.MenuItem.price * list.Count);
                //list.MenuItem.Description = SD.ConvertToRawHtml(list.MenuItem.Description);
                //if (list.MenuItem.Description.Length > 100)
                //{
                //    list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                //}
            }

            detalisCart.orderHeader.OrderTotalOrginal = detalisCart.orderHeader.OrderTotal;
            if (HttpContext.Session.GetString("ssCouponCode") != null)
            {
                detalisCart.orderHeader.CouponCode = HttpContext.Session.GetString("ssCouponCode");
                var couponfromDb = await db.Coupon.Where(x => x.Name.ToLower() == detalisCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detalisCart.orderHeader.OrderTotal = DiscountPrice(couponfromDb, detalisCart.orderHeader.OrderTotalOrginal);
            }

            return View(detalisCart);
        }

        public async Task<IActionResult> Summary()
        {

            detalisCart = new OrderDetalisCart()
            {
                orderHeader = new Models.OrderHeader()
            };
            // detalisCart.orderHeader.OrderTotal = 0;
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser applicationUser = await db.ApplicationUser.Where(x => x.Id == claim.Value).FirstOrDefaultAsync();

            var cart = db.shoppingCart.Where(x => x.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detalisCart.ListCart = cart.ToList();
            }
            foreach (var list in detalisCart.ListCart)
            {
                list.MenuItem = await db.MenuItem.Where(x => x.Id == list.MenuItemId).FirstOrDefaultAsync();
                detalisCart.orderHeader.OrderTotal = detalisCart.orderHeader.OrderTotal + (list.MenuItem.price * list.Count);
            }

            detalisCart.orderHeader.OrderTotalOrginal = detalisCart.orderHeader.OrderTotal;
            detalisCart.orderHeader.PickName = applicationUser.Name;
            detalisCart.orderHeader.PhoneNumber = applicationUser.PhoneNumber;
            detalisCart.orderHeader.PickUpTime = DateTime.Now;
            if (HttpContext.Session.GetString("ssCouponCode") != null)
            {
                detalisCart.orderHeader.CouponCode = HttpContext.Session.GetString("ssCouponCode");
                var couponfromDb = await db.Coupon.Where(x => x.Name.ToLower() == detalisCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detalisCart.orderHeader.OrderTotal = DiscountPrice(couponfromDb, detalisCart.orderHeader.OrderTotalOrginal);
            }

            return View(detalisCart);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(String stripeToken)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            detalisCart.ListCart = await db.shoppingCart.Where(x => x.ApplicationUserId == claim.Value).ToListAsync();
            detalisCart.orderHeader.paymentstatus = SD.PaymentStatusPending;
            detalisCart.orderHeader.PickUpTime = Convert.ToDateTime(detalisCart.orderHeader.PickUpDate.ToShortDateString() + " " + detalisCart.orderHeader.PickUpTime.ToShortTimeString());
            detalisCart.orderHeader.status = SD.PaymentStatusPending;
            detalisCart.orderHeader.UserId = claim.Value;
            detalisCart.orderHeader.OrderDate = DateTime.Now;

            db.OrderHeader.Add(detalisCart.orderHeader);
            await db.SaveChangesAsync();

            foreach (var item in detalisCart.ListCart)
            {
                item.MenuItem = await db.MenuItem.Where(x => x.Id == item.MenuItemId).FirstOrDefaultAsync();
                OrderDetalis orderDetalis = new OrderDetalis()
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = detalisCart.orderHeader.Id,
                    Name = item.MenuItem.Name,
                    Description = item.MenuItem.Description,
                    Price = item.MenuItem.price,
                    Count = item.Count
                };
                detalisCart.orderHeader.OrderTotalOrginal += orderDetalis.Price * orderDetalis.Count;
                db.OrderDetalis.Add(orderDetalis);

            }

            if (HttpContext.Session.GetString("ssCouponCode") != null)
            {
                detalisCart.orderHeader.CouponCode = HttpContext.Session.GetString("ssCouponCode");
                var couponfromDb = await db.Coupon.Where(x => x.Name.ToLower() == detalisCart.orderHeader.CouponCode.ToLower()).FirstOrDefaultAsync();
                detalisCart.orderHeader.OrderTotal = DiscountPrice(couponfromDb, detalisCart.orderHeader.OrderTotalOrginal);
            }
            else
            {
                detalisCart.orderHeader.OrderTotal = detalisCart.orderHeader.OrderTotalOrginal;
            }
            detalisCart.orderHeader.CouponCodeDiscount = detalisCart.orderHeader.OrderTotalOrginal - detalisCart.orderHeader.OrderTotal;

            db.shoppingCart.RemoveRange(detalisCart.ListCart);
            HttpContext.Session.SetInt32("ssCouponCode", 0);
            await db.SaveChangesAsync();

            var Options = new ChargeCreateOptions()
            {
                Amount = Convert.ToInt32(detalisCart.orderHeader.OrderTotal * 100),
                Currency = "usd",
                Description = "Order ID" + detalisCart.orderHeader.Id,
                Source = stripeToken
            };
            var service = new ChargeService();
            Charge charge = service.Create(Options);

            if (charge.BalanceTransactionId == null)
            {
                detalisCart.orderHeader.paymentstatus = SD.PaymentStatusRejected;
            }
            else
            {
                detalisCart.orderHeader.TransactionID = charge.BalanceTransactionId;
            }
            if(charge.Status.ToLower() == "succeeded")
            {
                detalisCart.orderHeader.paymentstatus = SD.PaymentStatusApproved;
                detalisCart.orderHeader.status = SD.StatusSubmitted;
            }
            else
            {
                detalisCart.orderHeader.paymentstatus = SD.PaymentStatusRejected;
            }
            await db.SaveChangesAsync();

            return RedirectToAction("Confirm", "Order",new { id= detalisCart.orderHeader.Id});


        }


        public IActionResult AddCoupon()
        {
            if (detalisCart.orderHeader.CouponCode == null)
            {
                detalisCart.orderHeader.CouponCode = "";
            }

            HttpContext.Session.SetString("ssCouponCode", detalisCart.orderHeader.CouponCode);
            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemovoCoupon()
        {
            HttpContext.Session.SetString("ssCouponCode", string.Empty);
            return RedirectToAction(nameof(Index));
        }

        public double DiscountPrice(spice.Models.Coupon couponFromDb, double OrderTotalOrginal)
        {
            if (couponFromDb == null)
            {
                return OrderTotalOrginal;
            }
            else
            {
                if (couponFromDb.MinimumAmount > OrderTotalOrginal)
                {
                    return OrderTotalOrginal;
                }
                else
                {
                    if (Convert.ToInt32(couponFromDb.CouponType) == (int)spice.Models.Coupon.ECouponType.dollar)
                    {
                        return Math.Round(OrderTotalOrginal - couponFromDb.Discount, 2);
                    }
                    else
                    {
                        if (Convert.ToInt32(couponFromDb.CouponType) == (int)spice.Models.Coupon.ECouponType.percent)
                        {
                            return Math.Round(OrderTotalOrginal - (OrderTotalOrginal * couponFromDb.Discount / 100), 2);
                        }

                    }
                }
            }
            return OrderTotalOrginal;
        }


        public async Task<IActionResult> plus(int cartId)
        {
            var cart = db.shoppingCart.Where(x => x.Id == cartId).FirstOrDefault();
            cart.Count += 1;
            await db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> minus(int cartId)
        {
            var cart = db.shoppingCart.Where(x => x.Id == cartId).FirstOrDefault();
            if (cart.Count == 1)
            {
                db.shoppingCart.Remove(cart);
                await db.SaveChangesAsync();

                var cnt = db.shoppingCart.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("ssCartCount", cnt);
            }
            else
            {
                cart.Count -= 1;
                await db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> remove(int cartId)
        {
            var cart = db.shoppingCart.Where(x => x.Id == cartId).FirstOrDefault();

            db.shoppingCart.Remove(cart);
            await db.SaveChangesAsync();

            var cnt = db.shoppingCart.Where(x => x.ApplicationUserId == cart.ApplicationUserId).ToList().Count();
            HttpContext.Session.SetInt32("ssCartCount", cnt);

            return RedirectToAction(nameof(Index));
        }
    }

}
