using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class MenuController : Controller
    {
        private readonly RestaurantMenuItemRepository Repository;
        private readonly int PageSize = AppConfigHelper.MediumPageSize;

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Menu/

        public ActionResult Index(int? id, int? page)
        {
            var allmenu = Repository.SelectAll();
            if (id.HasValue && id > 1)
                allmenu = allmenu.Where(offer => offer.ItemId == id);
            var paginatedlist = new PaginatedList<RestaurantMenuItem>(allmenu, page ?? 1, page.HasValue ? PageSize : allmenu.Count());
            return View(paginatedlist);
        }

        //
        // GET: /Menu/Today
        [Authorize(Roles = "Employee")]
        public ActionResult Today()
        {
            ViewBag.Title = ViewBag.Title ?? "Today's Servings";
            return ServingsOn(DateTime.UtcNow.Date);
        }

        //
        // GET: /Menu/ServingsOn
        [Authorize(Roles = "Employee")]
        public ActionResult ServingsOn()
        {
            ViewBag.Title = ViewBag.Title ?? "Servings On";
            return Today();
        }

        //
        // POST: /Menu/ServingsOn
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public ActionResult ServingsOn(DateTime fordate)
        {
            //reduce the date precision to Date only by omiting the time part
            fordate = fordate.Floor(DateTimeHelper.DateTimePrecisionLevel.Days);
            var result = Repository.SelectBookedMenuItems(fordate);
            if (Request.IsAjaxRequest())
            {
                //If request is Ajax then send a JSON representation of the Dictionary, Also send the Date as a string so it can be easily parsed by client
                return
                    Json(
                        result
                        .ToDictionary(item => fordate.ToISODateTimeString(), item => result)
                        .ToLookup(kvp => new { kvp.Key, kvp.Value }));
            }
            return View("ServingsOn", result);
        }
        
        //
        // GET: /Menu/Add

        public ActionResult Add()
        {
            return View(new RestaurantMenuViewModel());
        } 

        //
        // POST: /Menu/Add

        [HttpPost]
        public ActionResult Add(RestaurantMenuViewModel model, HttpPostedFileBase menuimage)
        {
            try
            {
                //Explicitly Validate Model for menu image
                if (menuimage == null || menuimage.ContentLength < 1 || (menuimage.ContentType != "image/jpeg" && menuimage.ContentType != "image/png"))
                    ModelState.AddModelError("addstatus", "A Menu Item needs to have a valid Image, Only JPEG and PNG images are supported");
                if (ModelState.IsValid)
                {
                    // Attempt to add the offer
                    var restauranttable = new RestaurantMenuItem
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Price = model.Price,
                        PictureFile = new ImagesController().PutImage(menuimage, null).ToString("n")
                    };

                    var itemid = Repository.Add(restauranttable);
                    if (itemid > 0)
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Message = String.Format("Menu Item \"{0}\" with Id:{1} was successfully Added", model.Name, itemid),
                            Result = true,
                            State = ActionResultNotification.MessageState.Information
                        };
                        return RedirectToAction("Index");
                    }
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in adding the new Menu item, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Warning
                };
                return View(model);
            }
            catch (Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = e.Message,
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
        }
        
        //
        // GET: /Menu/Edit/5
 
        public ActionResult Edit(int id)
        {
            var menuitem = Repository.Find(id);

            if (menuitem != null)
            {
                return View(new RestaurantMenuViewModel(menuitem));
            }
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
            {
                Message = String.Format("No such Menu item Exists !"),
                Result = false,
                State = ActionResultNotification.MessageState.Error
            };
            return RedirectToAction("Index");
        }

        //
        // POST: /Menu/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, RestaurantMenuViewModel model, HttpPostedFileBase menuimage)
        {
            try
            {
                // Get previous details
                var existingmenuitem = Repository.Find(id);
                if (existingmenuitem == null) throw new InvalidOperationException("Sorry the update cannot be done as no such menu Item exist");

                if (menuimage != null && (menuimage.ContentLength < 1
                    || (menuimage.ContentType != "image/jpeg" && menuimage.ContentType != "image/png")))
                {
                    ModelState.AddModelError("updatestatus",
                                             "A Menu Item needs to have a valid Image, Only JPEG and PNG images are supported");
                    model.PictureFileName = existingmenuitem.PictureFile;
                }

                if (ModelState.IsValid)
                {

                    // Attempt to Update the offer
                    if (menuimage != null && menuimage.ContentLength > 0)
                    {
                        var previd = existingmenuitem.PictureFile;
                        var imagecontroller = new ImagesController();
                        existingmenuitem.PictureFile =
                            imagecontroller.PutImage(menuimage, null).ToString("n");
                        // if both image ids are not same then delete the prev id.
                        if (!String.Equals(previd, existingmenuitem.PictureFile)) imagecontroller.DeleteImage(previd);
                    }

                    existingmenuitem.Description = model.Description;
                    existingmenuitem.Name = model.Name;
                    existingmenuitem.Price = model.Price;

                    //And push the changed details
                    if (Repository.Update(existingmenuitem))
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Message = String.Format("Details for {0} was successfully Updated", existingmenuitem.Name),
                            Result = true,
                            State = ActionResultNotification.MessageState.Information
                        };
                        return RedirectToAction("Index");
                    }
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("updatestatus") ? ModelState["updatestatus"].Errors[0].ErrorMessage : "Cannot Update the details of the offer with given details, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Warning
                };
                return View(model);
            }
            catch (Exception e)
            {
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = e.Message,
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
                };
                return View(model);
            }
        }

        //
        // POST: /Menu/Delete/5

        //Only Ajax Request is processed, a normal post request is redirected to Index action method
        [HttpPost]
        //[Authorize(Roles = "Admin, Employee")]
        public ActionResult Delete(int id)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var table = Repository.Find(id);
                    var result = false;
                    if (table != null)
                        result = Repository.Delete(id);
                    var message = result ? String.Format("The Menu Item {0} with Item Id of {1} was succcessfully deleted", table.Name, table.ItemId) : String.Format("Failed to delete the Table, please try again");
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = result,
                                Message = message
                            });
                }
                catch (Exception e)
                {
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = false,
                                Message = e.Message
                            });
                }
            }
            return RedirectToAction("Index");
        }

        public MenuController()
        {
            Repository = new RestaurantMenuItemRepository();
        }
    }
}
