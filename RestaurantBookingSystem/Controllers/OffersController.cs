using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Providers;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class OffersController : Controller
    {
        private readonly OfferBaseRepository Repository;
        private readonly int PageSize = AppConfigHelper.MediumPageSize;

        // Initialize A custom cookie based temp data provider for the controller
        protected override ITempDataProvider CreateTempDataProvider()
        {
            return new CookieBasedTempDataProvider();
        }

        //
        // GET: /Offers/

        public ActionResult Index(int? id, int? page)
        {
            var alloffers = Repository.SelectAll();
            if (id.HasValue && id > 1)
                alloffers = alloffers.Where(offer => offer.OfferId == id);
            var paginatedlist = new PaginatedList<OfferBase>(alloffers, page ?? 1, page.HasValue ? PageSize : alloffers.Count());
            return View(paginatedlist);
        }

        //
        // GET: /Offers/Add
        [OutputCache(Duration = 3600)]
        public ActionResult Add()
        {
            return View(new AddOfferModel{ Type = (OfferBase.OfferType)(-1), PublishedBy = (AddOfferModel.PublishedVia)(-1)});
        } 

        //
        // POST: /Offers/Add

        [HttpPost]
        public ActionResult Add(AddOfferModel model, HttpPostedFileBase offerimage)
        {
            try
            {
                //Explicitly Validate Model for OfferType, PublishVia and offerimage
                if ((model.Type != OfferBase.OfferType.DiscountAmount 
                    && model.Type != OfferBase.OfferType.DiscountPercent 
                    && model.Type != OfferBase.OfferType.FreeServing) 
                    || 
                    (model.PublishedBy != AddOfferModel.PublishedVia.Advertisement 
                    && model.PublishedBy != AddOfferModel.PublishedVia.Code))
                    ModelState.AddModelError("addstatus", "An Offer needs to be one of the Offer Types and should have one of the Publish Medium");
                if (model.PublishedBy == AddOfferModel.PublishedVia.Advertisement 
                    && (offerimage == null || offerimage.ContentLength < 1
                    || offerimage.ContentType != "image/jpeg" && offerimage.ContentType != "image/png"))
                    ModelState.AddModelError("addstatus", "An Offer with Advertisement needs to have a valid Advertisement Image, Only JPEG and PNG images are supported");
                if (model.PublishedBy == AddOfferModel.PublishedVia.Code
                    && (model.CouponCode == null || model.CouponCode.Trim().Length < 1))
                    ModelState.AddModelError("addstatus", "An Offer with Code needs to have a valid Coupon Code");
                if (ModelState.IsValid)
                {
                    // Attempt to add the offer
                    var restaurantoffer = 
                        model.PublishedBy == AddOfferModel.PublishedVia.Advertisement
                        ? new SeasonalOffer
                                {
                                    Type = model.Type,
                                    Description = model.Description,
                                    Title = model.Title,
                                    ValidFrom = model.ValidFrom,
                                    ValidTill = model.ValidTill,
                                    Value = model.Value,
                                    PictureFileName =
                                        new ImagesController().PutImage(offerimage, null).
                                        ToString("n")
                                }
                        : new Coupon(model.CouponCode)
                                {
                                    Type = model.Type,
                                    Description = model.Description,
                                    Title = model.Title,
                                    ValidFrom = model.ValidFrom,
                                    ValidTill = model.ValidTill,
                                    Value = model.Value
                                } as OfferBase;

                    var offerid = Repository.Add(restaurantoffer);
                    if (offerid > 0)
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                        {
                            Message = String.Format("Offer Id:{0}, {1} was successfully Added as a new {2}", offerid, model.Title, model.PublishedBy == AddOfferModel.PublishedVia.Code ? "Coupon" : "Seasonal Offer"),
                            Result = true,
                            State = ActionResultNotification.MessageState.Information
                        };
                        return RedirectToAction("Index");
                    }
                }
                // If we got this far, something failed, redisplay form
                TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                {
                    Message = ModelState.ContainsKey("addstatus") ? ModelState["addstatus"].Errors[0].ErrorMessage : "There was an Error in adding the new Offer, please try again",
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
        // GET: /Offers/Edit/5
 
        public ActionResult Edit(int id)
        {
            var offer = Repository.Find(id);

            if (offer != null)
            {
                if (offer is SeasonalOffer) ViewBag.PictureFileName = (offer as SeasonalOffer).PictureFileName;
                return View(new UpdateOfferModel(offer));
            }
            TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
            {
                Message = String.Format("No such offer Exists !"),
                Result = false,
                State = ActionResultNotification.MessageState.Error
            };
            return RedirectToAction("Index");
        }

        //
        // POST: /Offers/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, UpdateOfferModel model, HttpPostedFileBase offerimage)
        {
            try
            {
                // Get previous details
                var existingoffer = Repository.Find(id);
                if (existingoffer == null) throw new InvalidOperationException("Sorry the update cannot be done as no such offer exist");

                if (existingoffer is SeasonalOffer && model.PublishedBy == UpdateOfferModel.PublishedVia.Advertisement
                    && offerimage != null && (offerimage.ContentLength < 1
                    || (offerimage.ContentType != "image/jpeg" && offerimage.ContentType != "image/png")))
                {
                    ModelState.AddModelError("updatestatus",
                                             "An Seasonal Offer Advertisement needs to have a valid Advertisement Image, Only JPEG and PNG images are supported");
                    ViewBag.PictureFileName = (existingoffer as SeasonalOffer).PictureFileName;
                }

                if (ModelState.IsValid)
                {

                    // Attempt to Update the offer
                    if (existingoffer is SeasonalOffer && offerimage != null && offerimage.ContentLength > 0)
                    {
                        var previd = (existingoffer as SeasonalOffer).PictureFileName;
                        var imagecontroller = new ImagesController();
                        (existingoffer as SeasonalOffer).PictureFileName =
                            imagecontroller.PutImage(offerimage, null).ToString("n");
                        // if both image ids are not same then delete the prev id.
                        if (!String.Equals(previd, (existingoffer as SeasonalOffer).PictureFileName)) imagecontroller.DeleteImage(previd);
                    }

                    existingoffer.Description = model.Description;
                    existingoffer.Title = model.Title;
                    existingoffer.ValidTill = model.ValidTill;

                    //And push the changed details
                    if (Repository.Update(existingoffer))
                    {
                        TempData[TempDataStringResuorce.ActionResultNotification] = new ActionResultNotification
                            {
                                Message = String.Format("Details for {0} was successfully Updated", existingoffer.GetType().Name),
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
        // POST: /Offers/Delete/5

        //Only Ajax Request is processed, a normal post request is redirected to Index action method
        [HttpPost]
        public ActionResult Delete(int id)
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var offer = Repository.Find(id);
                    var result = false;
                    if (offer != null)
                        result = Repository.Delete(id);
                    var message = result ? String.Format("The {0} for '{1}' was successfully deleted", offer.GetType().Name, offer.Title) : String.Format("Failed to delete the Offer, please try again");
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

        public OffersController()
        {
            Repository = new OfferBaseRepository();
        }
    }
}
