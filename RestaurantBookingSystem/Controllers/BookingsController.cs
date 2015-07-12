using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;
using RestaurantBookingSystem.Helpers;

namespace RestaurantBookingSystem.Controllers
{
    public class BookingsController : Controller
    {
        // ReSharper disable InconsistentNaming
        private readonly RestaurantBookingRepository Repository;
        private readonly int PageSize = AppConfigHelper.MediumPageSize;
        // ReSharper restore InconsistentNaming

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        // ReSharper disable PossibleMultipleEnumeration
        // Lists all the booking of that day and if there is any parameter for id then it simply redirects to details action for users
        //
        // GET: /Bookings/

        [Authorize(Roles = "Guest, Customer")]
        public ActionResult Index(int? id, int? page)
        {
            var allbookings = Repository.SelectAll();
            var user = User.Identity as RestaurantUserIdentity;
            if (user != null)
                allbookings = allbookings.Where(b => b.BookingCustomer.UserId == user.UserId);
            if (id.HasValue && id > 0) 
                allbookings = allbookings.Where(b => b.BookingId == id);
            var paginatedlist = new PaginatedList<RestaurantBooking>(allbookings, page ?? 1, page.HasValue ? PageSize : allbookings.Count());
            return View(paginatedlist);
        }

        [Authorize(Roles = "Employee, Admin")]
        public ActionResult Today()
        {
            return List(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddHours(24), null);
        }

        [Authorize(Roles = "Employee, Admin")]
        public ActionResult List(DateTime from, DateTime till, int? page)
        {
            from = from.ToUniversalTime();
            till = till.ToUniversalTime();
            var matchedbookings = Repository.SelectAll().Where(booking => booking.BookedFor.ToUniversalTime() >= from && booking.BookedTill.ToUniversalTime() <= till);
            var paginatedlist = new PaginatedList<RestaurantBooking>(matchedbookings, page ?? 0, page.HasValue ? PageSize : matchedbookings.Count());
            return View("List", paginatedlist);
        }

        //
        // GET: /Bookings/Detail/5

        [Authorize]
        public ActionResult Detail(int? id)
        {
            return View("Detail", Repository.Find(id ?? 0));
        }

        //
        // GET: /Bookings/New

        public ActionResult New()
        {
            //Delete any previous booking detail
            TempData.Remove(TempDataStringResuorce.NewBookingData);
            return View(new BookingViewModel());
        }

        //
        // POST: /Bookings/New/5

        [HttpPost]
        public ActionResult New(BookingViewModel model, int? offerid)
        {
            try
            {
                IEnumerable<RestaurantMenuItem> restaurantMenuItems;
                IEnumerable<RestaurantTable> restaurantTables;
                SeasonalOffer restaurantOffer;
                BookingHelper.ValidateModel(this, model, offerid, out restaurantMenuItems, out restaurantTables, out restaurantOffer);
                if (ModelState.IsValid)
                {
                    TempData[TempDataStringResuorce.NewBookingData] = new TempDataConfirmBooking { Model = model, OfferId = offerid ?? -1};
                    return Request.IsAuthenticated ? RedirectToAction("Confirm", "Bookings") : RedirectToAction("RegisterGuest", "Account", new {returnurl = Url.Action("Confirm")});
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in making you booking, please try again !",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
            catch (Exception e)
            {
                var result = new ActionResultNotification
                                 {
                                     Result = false,
                                     Message = e.Message,
                                     State = ActionResultNotification.MessageState.Error
                                 };
                if (Request.IsAjaxRequest())
                    return Json(result);
                TempData[TempDataStringResuorce.ActionResultNotification] = result;
                return View();
            }
        }

        //
        // GET: /Bookings/Confirm
        [Authorize]
        public ActionResult Confirm()
        {
            //Only peek for data as it will be required in post action
            var tmpdata = TempData.Peek(TempDataStringResuorce.NewBookingData) as TempDataConfirmBooking;
            try
            {
                if (tmpdata == null)
                {
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                            {
                                Result = false,
                                Message = "Invalid Booking Attempt, please try again !",
                                State = ActionResultNotification.MessageState.Error
                            };
                    return RedirectToAction("New");
                }

                var model = tmpdata.Model;
                if (model == null)
                {
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Result = false,
                        Message = "Invalid Booking Request, please try again !",
                        State = ActionResultNotification.MessageState.Error
                    };
                    return RedirectToAction("New");
                }
                var offerid = tmpdata.OfferId;
                IEnumerable<RestaurantMenuItem> restaurantMenuItems;
                IEnumerable<RestaurantTable> restaurantTables;
                SeasonalOffer restaurantOffer;
                BookingHelper.ValidateModel(this, model, offerid, out restaurantMenuItems, out restaurantTables, out restaurantOffer);
                if (ModelState.IsValid)
                    return View(new ConfirmBookingViewModel
                                    {
                                        Bill = BookingHelper.GetBookingBill(restaurantMenuItems, restaurantTables, restaurantOffer, null, model.BookedSlots),
                                        BookedFor = model.BookedFor,
                                        BookedSlots = model.BookedSlots,
                                        BookedTables = model.BookedTables,
                                        MenuItems = restaurantMenuItems.ToList(),
                                        Offer = restaurantOffer,
                                        PrefferedMenuItems = model.PrefferedMenuItems,
                                        Tables = restaurantTables.ToList()
                                    });

                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in making you booking, please try again !",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return RedirectToAction("New");
            }
            catch(Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Result = false,
                            Message = e.Message,
                            State = ActionResultNotification.MessageState.Warning
                        };
                return RedirectToAction("Index", "Home");
            }
        }

        //
        // POST: /Bookings/Confirm

        [HttpPost]
        [Authorize]
        public ActionResult Confirm(string couponcode)
        {
            var tmpdata = TempData.Peek(TempDataStringResuorce.NewBookingData) as TempDataConfirmBooking;
            try
            {
                if (tmpdata == null)
                {
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                            {
                                Result = false,
                                Message = "Invalid Booking Request, please try again !",
                                State = ActionResultNotification.MessageState.Error
                            };
                    return RedirectToAction("New");
                }

                var model = tmpdata.Model;
                if (model == null)
                {
                    TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                    {
                        Result = false,
                        Message = "Invalid Booking Request, please try again !",
                        State = ActionResultNotification.MessageState.Error
                    };
                    return RedirectToAction("New");
                }
                var offerid = tmpdata.OfferId;
                IEnumerable<RestaurantMenuItem> restaurantMenuItems;
                IEnumerable<RestaurantTable> restaurantTables;
                SeasonalOffer restaurantOffer;
                Coupon restaurantcoupon;
                BookingHelper.ValidateModel(this, model, offerid, couponcode, out restaurantMenuItems, out restaurantTables, out restaurantOffer, out restaurantcoupon);
                if (restaurantOffer != null && restaurantcoupon != null)
                {
                    ModelState.AddModelError("addstatus", "Two or more offers cannot be clubbed together, either use a Coupon Code or a Seasonal Offer");
                }
                if (ModelState.IsValid)
                {
                    var booking = new RestaurantBooking
                                      {
                                          BookedFor = model.BookedFor.ToUniversalTime(),
                                          BookedOn = DateTime.UtcNow,
                                          BookedTill = model.BookedFor.ToUniversalTime().AddMinutes(AppConfigHelper.BookingSlotMinutes * model.BookedSlots),
                                          BookedTables = restaurantTables.ToList(),
                                          PrefferedMenuItems = restaurantMenuItems.ToList(),
                                          BookingCustomer = new AccountMembershipService().GetUser(((RestaurantUserIdentity)User.Identity).UserGuid, true)
                                      };
                    booking.Bills.Add(BookingHelper.GetBookingBill(restaurantMenuItems, restaurantTables, restaurantOffer,
                                                     restaurantcoupon, model.BookedSlots));

                    //Status of the booking is automaticaly set by repository according to availability
                    var bookingresult = Repository.Add(booking);
                    if (bookingresult > 0) TempData.Remove(TempDataStringResuorce.NewBookingData);
                    var message = bookingresult > 0
                                      ? String.Format("The Booking of Booking ID:{0} was successful", bookingresult)
                                      : String.Format("The Booking was Unsuccessful! Please try again");
                    var result = new ActionResultNotification
                    {
                        Result = bookingresult > 0,
                        Message = message,
                        State = bookingresult > 0 ? ActionResultNotification.MessageState.Information : ActionResultNotification.MessageState.Error
                    };
                    if (Request.IsAjaxRequest())
                        return Json(result);

                    TempData[TempDataStringResuorce.ActionResultNotification] = result;
                    return RedirectToAction("Index");
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in making you booking, please try again !",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
            catch (Exception e)
            {
                var result = new ActionResultNotification
                {
                    Result = false,
                    Message = e.Message,
                    State = ActionResultNotification.MessageState.Error
                };
                if (Request.IsAjaxRequest())
                    return Json(result);
                TempData[TempDataStringResuorce.ActionResultNotification] = result;
                if (tmpdata != null)
                TempData[TempDataStringResuorce.NewBookingData] =
                    new TempDataConfirmBooking { Model = tmpdata.Model, OfferId = tmpdata.OfferId };
                return Confirm();
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult ValidateCoupon(string couponcode)
        {
            if (Request.IsAjaxRequest())
            {
                if (couponcode.IsNullOrEmpty())
                    return Json(new
                    {
                        Notification = new ActionResultNotification
                            {
                                Result = false,
                                Message = "Please enter a Coupon Code",
                                State = ActionResultNotification.MessageState.Error
                            }
                    });
                var result = new OfferBaseRepository().FindCouponByCode(couponcode.Trim());
                if (result != null)
                {
                    if (result.ValidTill.ToUniversalTime() > DateTime.UtcNow)
                    {
                        var MenuItem = result.Type == OfferBase.OfferType.FreeServing
                                           ? new RestaurantMenuItemRepository().Find(result.Value)
                                           : null;
                        return Json(new
                        {
                            Notification = new ActionResultNotification
                                {
                                    Result = true,
                                    Message = "Coupon is Valid.",
                                    State = ActionResultNotification.MessageState.Information
                                },
                            CouponDetail = new
                                {
                                    Value = MenuItem == null ? result.Value : MenuItem.Price,
                                    result.Type,
                                    MenuItemName = MenuItem == null ? "" : MenuItem.Name
                                }
                        });
                    }
                    return Json(new
                    {
                        Notification = new ActionResultNotification
                        {
                            Result = false,
                            Message = "Coupon Code has expired !",
                            State = ActionResultNotification.MessageState.Warning
                        }
                    });
                }
                return Json(new
                {
                    Notification = new ActionResultNotification
                        {
                            Result = false,
                            Message = "Invalid Coupon code provided.",
                            State = ActionResultNotification.MessageState.Error
                        }
                });

            }
            return RedirectToAction("New");
        }

        //
        // GET: /Bookings/Edit/5

        [Authorize]
        public ActionResult Edit(int id)
        {
            return View(Repository.Find(id));
        }

        //
        // POST: /Bookings/Edit/5

        [HttpPost]
        [Authorize]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }
        }


        //
        // POST: /Bookings/Cancel/5

        [HttpPost]
        [Authorize]
        public ActionResult Cancel(int id)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var booking = Repository.Find(id);
                    var result = false;
                    if (booking != null)
                        result = Repository.Cancel(id);
                    var message = result ? String.Format("Booking with id {0} of {1} Tables for {2} was succcessfully Cancelled", booking.BookingId, booking.BookedTables.Count, booking.BookedFor.TryMakingLocalToClient().ToString("dddd, dd mmm, h:mm tt")) : String.Format("Failed to cancel the booking, please try again");
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = result,
                                Message = message,
                                State = result ? ActionResultNotification.MessageState.Information : ActionResultNotification.MessageState.Warning
                            });
                }
                catch (Exception e)
                {
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = false,
                                Message = e.Message,
                                State = ActionResultNotification.MessageState.Error
                            });
                }
            }
            return RedirectToAction("Index");
        }

        public BookingsController()
            : this(new RestaurantBookingRepository())
        {
        }

        public BookingsController(RestaurantBookingRepository viewModel)
        {
            Repository = viewModel;
        }
        // ReSharper restore PossibleMultipleEnumeration
    }
}
