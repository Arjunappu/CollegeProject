using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Infrastructure.Repositories;
using RestaurantBookingSystem.Models;
// ReSharper disable PossibleMultipleEnumeration
namespace RestaurantBookingSystem.Helpers
{
    public static class BookingHelper
    {
        internal static BookingBill GetBookingBill(IEnumerable<RestaurantMenuItem> restaurantMenuItems, IEnumerable<RestaurantTable> restaurantTables, SeasonalOffer restaurantOffer, Coupon restaurantcoupon, int bookedslots)
        {
            var discountamt = 0M;
            decimal grossamount = restaurantTables.Sum(restaurantTable => restaurantTable.Price * bookedslots);
            grossamount = restaurantMenuItems.Aggregate(grossamount, (current, restaurantMenuItem) => (int)(current + restaurantMenuItem.Price));

            var offer = (restaurantcoupon as OfferBase) ?? (restaurantOffer);

            var netamount = grossamount;
            if (offer != null)
            {
                switch (offer.Type)
                {
                    case OfferBase.OfferType.DiscountAmount:
                        {
                            discountamt = offer.Value;
                            netamount = grossamount - discountamt;
                            if (netamount < 0) netamount = 0;
                            break;
                        }
                    case OfferBase.OfferType.DiscountPercent:
                        {
                            discountamt = (grossamount * offer.Value) / 100M;
                            netamount = grossamount - discountamt;
                            break;
                        }
                    case OfferBase.OfferType.FreeServing:
                        {
                            discountamt = new RestaurantMenuItemRepository().Find(offer.Value).Price;
                            netamount = grossamount - discountamt;
                            break;
                        }
                }
            }

            if (netamount < 0)
                throw new InvalidOperationException("Discount amount cannot be more than the total amount of the Bill");
            return new BookingBill
                       {
                           DiscountAmount = discountamt,
                           GrossAmount = grossamount,
                           NetAmount = netamount
                       };
        }

        [NonAction]
        internal static void ValidateModel(Controller controller, BookingViewModel model, int? offerid, out IEnumerable<RestaurantMenuItem> restaurantMenuItems,
                                         out IEnumerable<RestaurantTable> restaurantTables, out SeasonalOffer restaurantOffer)
        {
            if (model.BookedFor.ToUniversalTime() < DateTime.UtcNow.Floor((long)AppConfigHelper.BookingSlotMinutes, DateTimeHelper.DateTimePrecisionLevel.Minutes))
                controller.ModelState.AddModelError("addstatus", "The date and time for booking should always be a future date and time");
            if (model.BookedSlots < 1)
                controller.ModelState.AddModelError("addstatus",
                                                    "You need to make booking for atleast " + AppConfigHelper.BookingSlotMinutes +
                                                    " Minutes");
            if (model.BookedTables.IsNullOrEmpty() || model.BookedTables.Trim(',', ' ').Split(',').All(t => !t.IsNumeric()))
                controller.ModelState.AddModelError("addstatus", "Invalid or no tables selected, please try again");
            if (!model.PrefferedMenuItems.IsNullOrEmpty() &&
                model.BookedTables.Trim(',', ' ').Split(',').All(t => !t.IsNumeric()))
                controller.ModelState.AddModelError("addstatus", "Invalid Menu Items selected, please try again");
            restaurantOffer = offerid != null
                                  ? new OfferBaseRepository().Find(offerid.Value) as SeasonalOffer
                                  : null;
            var modelTables = model.BookedTables.Trim(',', ' ').Split(',').Select(Int32.Parse);
            restaurantTables =
                new RestaurantTableRepository().SelectAll().Where(table => modelTables.Any(t => t == table.TableId));
            var modelMenuItems = model.PrefferedMenuItems.IsNullOrEmpty() ? new List<int>() : model.PrefferedMenuItems.Trim(',', ' ').Split(',').Select(Int32.Parse);
            restaurantMenuItems =
                new RestaurantMenuItemRepository().SelectAll().Where(item => modelMenuItems.Any(m => m == item.ItemId));
            if (restaurantOffer!= null && restaurantOffer.Type == OfferBase.OfferType.FreeServing)
            {
                var freeitem = new RestaurantMenuItemRepository().Find(restaurantOffer.Value);
                if (freeitem != null)
                {
                    var newitemlist = new List<RestaurantMenuItem> {freeitem};
                    newitemlist.AddRange(restaurantMenuItems);
                    restaurantMenuItems = newitemlist;
                }
            }

            if (restaurantTables.Count() < 1)
                controller.ModelState.AddModelError("addstatus", "Selected Tables does not exist, please try again");
            if (modelMenuItems.Count() > 0 && restaurantMenuItems.Count() < 1)
                controller.ModelState.AddModelError("addstatus", "Selected Menu Items does not exist, please try again");
        }

        [NonAction]
        internal static void ValidateModel(Controller controller, BookingViewModel model, int? offerid, string couponcode, out IEnumerable<RestaurantMenuItem> restaurantMenuItems,
                                         out IEnumerable<RestaurantTable> restaurantTables, out SeasonalOffer restaurantOffer, out Coupon restaurantCoupon)
        {
            ValidateModel(controller, model, offerid, out restaurantMenuItems, out restaurantTables, out restaurantOffer);
            restaurantCoupon = null;
            if (couponcode.IsNullOrEmpty()) return;
            var result = new OfferBaseRepository().FindCouponByCode(couponcode.Trim());
            if (result != null)
            {
                if (result.ValidTill.ToUniversalTime() > DateTime.UtcNow)
                {
                    restaurantCoupon = result;
                    return;
                }
                controller.ModelState.AddModelError("addstatus", "Coupon Code has expired !");
                return;
            }
            controller.ModelState.AddModelError("addstatus", "Invalid Coupon code provided.");
        }
    }

    [Serializable]
    internal class TempDataConfirmBooking
    {
        public BookingViewModel Model { get; set; }
        public int OfferId { get; set; }
    }
}
// ReSharper restore PossibleMultipleEnumeration