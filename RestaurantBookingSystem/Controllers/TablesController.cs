using System;
using System.Drawing;
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
    public class TablesController : Controller
    {
        private readonly RestaurantTableRepository Repository;
        private readonly int PageSize = AppConfigHelper.LargePageSize;

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Tables/
        [Authorize(Roles = "Admin")]
        public ActionResult Index(int? id, int? page)
        {
            var alltables = Repository.SelectAll();
            if (id.HasValue)
                alltables = alltables.Where(table => table.TableId == id);
            var paginatedlist = new PaginatedList<RestaurantTable>(alltables, page ?? 1, page.HasValue ? PageSize : alltables.Count());
            return View(paginatedlist);
        }

        //
        // GET: /Tables/CurrentStatus
        [Authorize(Roles = "Admin, Employee")]
        public ActionResult CurrentStatus()
        {
            ViewBag.Title = ViewBag.Title ?? "Current Status";
            return
                StatusOn(DateTime.UtcNow.Floor((long) AppConfigHelper.BookingSlotMinutes,
                                            DateTimeHelper.DateTimePrecisionLevel.Minutes), null);
        }

        // GET: /Tables/StatusOn
        [Authorize(Roles = "Admin, Employee")]
        public ActionResult StatusOn()
        {
            ViewBag.Title = ViewBag.Title ?? "Status On";
            return CurrentStatus();
        }

        //
        // POST: /Tables/StatusOn
        [HttpPost]
        public ActionResult StatusOn(DateTime fromdatetime, DateTime? todatetime)
        {
            fromdatetime = fromdatetime.ToUniversalTime();
            todatetime = todatetime != null ? todatetime.Value.ToUniversalTime() : fromdatetime.AddMinutes(AppConfigHelper.BookingSlotMinutes);
            // make sure we do not show status before shop opening time
            //fromdatetime = new DateTime(Math.Max(DateTime.UtcNow.TryMakingLocalToClient().Date.AddHours(7).Ticks, fromdatetime.TryMakingLocalToClient().Ticks), DateTimeKind.Unspecified).TryMakingUnivarsalFromClient();
            //todatetime = new DateTime(Math.Max(DateTime.UtcNow.TryMakingLocalToClient().Date.AddHours(7).AddMinutes(AppConfigHelper.BookingSlotMinutes).Ticks, (todatetime.Value.TryMakingLocalToClient()).Ticks), DateTimeKind.Unspecified).TryMakingUnivarsalFromClient();
            var result = Repository.SelectAll(fromdatetime, todatetime.Value);
            if (Request.IsAjaxRequest())
            {
                //If request is Ajax then send a JSON representation of the Dictionary, Also send the Date as a string so it can be easily parsed by client
                return
                    Json(
                        result
                        .ToDictionary(kvp => kvp.Key.ToISODateTimeString(), kvp => kvp.Value)
                        .ToLookup(kvp => new { kvp.Key, kvp.Value }));
            }
            return View("StatusOn",result);
        }

        //
        // GET: /Tables/Add
        [Authorize(Roles = "Admin")]
        public ActionResult Add()
        {
            return View(new RestaurantTableModel());
        } 

        //
        // POST: /Tables/Add

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Add(RestaurantTableModel model, HttpPostedFileBase floorplan)
        {
            try
            {
                //Explicitly Validate Model for floorplan image or previous image guid
                var floorplanfileguid = Guid.Empty;
                if (floorplan == null && !Guid.TryParse(model.FloorPlanFileName, out floorplanfileguid))
                    ModelState.AddModelError("addstatus", "One of the Given Floor Plans need to be selected");
                if (floorplan != null && (floorplan.ContentLength < 1 || (floorplan.ContentType != "image/jpeg" && floorplan.ContentType != "image/png")))
                    ModelState.AddModelError("addstatus", "A Floor Plan needs to have a valid Floor Plan Image, Only JPEG and PNG images are supported");
                if (ModelState.IsValid)
                {
                    // Attempt to add the offer
                    var restauranttable = new RestaurantTable
                                              {
                                                  TableType = model.TableType,
                                                  Alignment = model.Alignment,
                                                  Position = new Point(model.PositionX, model.PositionY),
                                                  Price = model.Price,
                                                  FloorPlanFileName = floorplanfileguid == Guid.Empty ? new ImagesController().PutImage(floorplan, null).ToString("n") : floorplanfileguid.ToString("n")
                                              };

                    var tableid = Repository.Add(restauranttable);
                    if (tableid > 0)
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Message = String.Format("Table Id:{0}, with seating capacity of {1} was successfully Added", tableid, (int)model.TableType),
                            Result = true,
                            State = ActionResultNotification.MessageState.Information
                        };
                        return RedirectToAction("Index");
                    }
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in adding the new Table, please try again",
                    Result = false,
                    State = ActionResultNotification.MessageState.Error
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
        // POST: /Tables/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id, int price)
        {
            if (Request.IsAjaxRequest())
                try
                {
                    var existingTable = Repository.Find(id);
                    if (existingTable == null) throw new Exception(String.Format("Sorry update cannot be done as Table with Id: {0} does not exist", id));
                    existingTable.Price = price;

                    //Push the changed details
                    if (Repository.Update(existingTable))
                    {
                        return Json(
                                    new ActionResultNotification
                                    {
                                        Message = String.Format("Details for Table Id:{0} was successfully Updated", existingTable.TableId),
                                        Result = true
                                    });

                    }
                    // If we got this far, something failed, redisplay form
                    return Json( 
                                new ActionResultNotification
                                {
                                    Message = "Cannot Update the details of the table with given details, please try again",
                                    Result = false
                                }); 
                }
                catch(Exception e)
                {
                    return
                        Json(
                            new ActionResultNotification
                            {
                                Result = false,
                                Message = e.Message
                            });
                }
            return RedirectToAction("Index");
        }

        //
        // POST: /Tables/Delete/5

        //Only Ajax Request is processed, a normal post request is redirected to Index action method
        [HttpPost]
        [Authorize(Roles = "Admin")]
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
                    var message = result ? String.Format("The Table {0} with seating capicity of {1} was succcessfully deleted", table.TableId, (int)table.TableType) : String.Format("Failed to delete the Table, please try again");
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

        public TablesController()
        {
            Repository = new RestaurantTableRepository();
        }
    }
}