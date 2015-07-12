using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using RestaurantBookingSystem.Helpers;
using RestaurantBookingSystem.Infrastructure.DataEntities;
using RestaurantBookingSystem.Models;

namespace RestaurantBookingSystem.Infrastructure.Repositories
{
    /// <summary>
    /// A Repository Class to Provide CRUD Operations on Data
    /// </summary>
    public class OfferBaseRepository : IDataRepository<OfferBase>
    {
        #region Implementation of IDataRepository<OfferBase>

        /// <summary>
        /// Finds a offer with the specified Id
        /// </summary>
        /// <param name="id">The id of the offer</param>
        /// <returns>Resturns the offer if operation is successful otherwise returns null</returns>
        public OfferBase Find(int id)
        {
            return FindOfferById(id);
        }

        /// <summary>
        /// Selects all the offers from the Database and Returns them
        /// </summary>
        /// <returns>Returns the IEnumerable List of found offers</returns>
        public IEnumerable<OfferBase> SelectAll()
        {
            return GetAllOffers();
        }

        /// <summary>
        /// Adds a new Offer to DataBase
        /// </summary>
        /// <param name="offer">Offer to add of OfferBase Type</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns the Id of added Item if operation is successful otherwise returns -1</returns>
        public int Add(OfferBase offer)
        {
            if (offer == null) throw new ArgumentNullException("offer");
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("AddOffer", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@DESCRIPTION", SqlDbType.Text).Value = offer.Description;
                    cmd.Parameters.Add("@TITLE", SqlDbType.VarChar, 50).Value = offer.Title;
                    cmd.Parameters.Add("@TYPE", SqlDbType.TinyInt).Value = (int)offer.Type;
                    cmd.Parameters.Add("@VALIDFROM", SqlDbType.DateTime).Value = offer.ValidFrom;
                    cmd.Parameters.Add("@VALIDTILL", SqlDbType.DateTime, 50).Value = offer.ValidTill;
                    cmd.Parameters.Add("@VALUE", SqlDbType.BigInt).Value = offer.Value;
                    cmd.Parameters.Add("@PICTUREFILENAME", SqlDbType.Text, 50).
                        Value = typeof(SeasonalOffer) == offer.GetType() ? ((SeasonalOffer)offer).PictureFileName : null;
                    cmd.Parameters.Add("@CODE", SqlDbType.VarChar, 50).
                        Value = typeof(Coupon) == offer.GetType() ? ((Coupon)offer).Code : null;

                    cn.Open();
                    var res = Convert.ToInt32(cmd.ExecuteScalar());
                    return res > 0 ? res : -1;
                }
            }
        }

        /// <summary>
        /// Updates the Available Offer with new Details
        /// </summary>
        /// <param name="offer">The Modified Offer which needs to be updated</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Update(OfferBase offer)
        {
            if (offer == null)
                throw new ArgumentNullException("offer");
            if (offer.OfferId == 0)
                throw new InvalidOperationException("For Updating an Offer the Id of the Offer should be non Zero");
            if (Find(offer.OfferId) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("UpdateOffer", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@OFFERID", SqlDbType.BigInt).Value = offer.OfferId;
                        cmd.Parameters.Add("@DESCRIPTION", SqlDbType.Text).Value = offer.Description;
                        cmd.Parameters.Add("@TITLE", SqlDbType.VarChar, 50).Value = offer.Title;
                        cmd.Parameters.Add("@TYPE", SqlDbType.TinyInt).Value = (int)offer.Type;
                        cmd.Parameters.Add("@VALIDFROM", SqlDbType.DateTime).Value = offer.ValidFrom;
                        cmd.Parameters.Add("@VALIDTILL", SqlDbType.DateTime, 50).Value = offer.ValidTill;
                        cmd.Parameters.Add("@VALUE", SqlDbType.BigInt).Value = offer.Value;
                        cmd.Parameters.Add("@PICTUREFILENAME", SqlDbType.Text, 50).
                            Value = typeof(SeasonalOffer) == offer.GetType() ? ((SeasonalOffer)offer).PictureFileName : null;
                        cmd.Parameters.Add("@CODE", SqlDbType.VarChar, 50).
                            Value = typeof(Coupon) == offer.GetType() ? ((Coupon)offer).Code : null;

                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        if (res == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Deletes the Offer with specified id
        /// </summary>
        /// <param name="id">The id of the Offer that needs to be deleted</param>
        /// <exception cref="InvalidOperationException"></exception>
        /// <returns>Returns true if operation is successful otherwise returns false</returns>
        public bool Delete(int id)
        {
            if (FindOfferById(id) != null)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                {
                    using (var cmd = new SqlCommand("DeleteOffer", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@OFFERID", SqlDbType.BigInt).Value = id;
                        cn.Open();
                        var res = cmd.ExecuteNonQuery();
                        if (res == 1)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// Gets all the Offers of both type viz. SeasonalOffer and Coupon (Ordered by ValidTill Decending)
        /// </summary>
        /// <returns>Returns all Offers as IEnumerable"></returns>
        private static IEnumerable<OfferBase> GetAllOffers()
        {
            var offers = new BindingList<OfferBase>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("SelectAllOffers", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    GetAllRecord(offers, cn, cmd);
                }
            }
            return offers;
        }

        /// <summary>
        /// Finds an Offer with the given Id
        /// </summary>
        /// <param name="offerid">The Id of the offer</param>
        /// <returns>If Found Returns the first result as OfferBase otherwise returns null</returns>
        private static OfferBase FindOfferById(int offerid)
        {
            var offers = new BindingList<OfferBase>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindOfferById", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@offerid", SqlDbType.BigInt).Value = offerid;
                    GetAllRecord(offers, cn, cmd);
                }
            }
            if (offers.Count < 1)
            {
                return null;
            }
            return offers[0];
        }

        /// <summary>
        /// Gets all the SeasonalOffers (Ordered by ValidTill Decending)
        /// </summary>
        /// <returns>Returns an IEnumerable list of Seasonal Offers</returns>
        public IEnumerable<SeasonalOffer> GetAllSeasonalOffers()
        {
            var offers = new BindingList<SeasonalOffer>();
            foreach (var offer in GetAllOffers().Where(offer => typeof(SeasonalOffer) == offer.GetType()))
            {
                offers.Add(offer as SeasonalOffer);
            }
            return offers;
        }

        /// <summary>
        /// Gets all the offers of Coupon Type (Ordered by ValidTill Decending)
        /// </summary>
        /// <returns>Returns an IEnumerable List of Coupons</returns>
        public IEnumerable<Coupon> GetAllCoupons()
        {
            var offers = new BindingList<Coupon>();

            foreach (var offer in GetAllOffers().Where(offer => typeof(Coupon) == offer.GetType()))
            {
                offers.Add(offer as Coupon);
            }
            return offers;
        }

        /// <summary>
        /// Finds the Coupon with the specified Coupon Code
        /// </summary>
        /// <param name="code">The Coupon Code that needs to be searched</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns the Coupon if found otherwise returns null</returns>
        public Coupon FindCouponByCode(string code)
        {
            if (code == null) throw new ArgumentNullException("code");
            var offers = new BindingList<OfferBase>();
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            {
                using (var cmd = new SqlCommand("FindCouponByCode", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@code", SqlDbType.VarChar, 50).Value = code;
                    GetAllRecord(offers, cn, cmd);
                }
            }
            if (offers.Count < 1)
            {
                return null;
            }
            return offers[0] as Coupon;
        }

        /// <summary>
        /// A Utility method to Get all the records from given SqlConnection and SqlCommand
        /// </summary>
        /// <param name="offers">Th OfferBase Collection which will be updated with Read Rows</param>
        /// <param name="cn">The SqlConnection object to use for Database connection</param>
        /// <param name="cmd">The SqlCommand object that will be used to retrive data</param>
        private static void GetAllRecord(IList<OfferBase> offers, SqlConnection cn, SqlCommand cmd)
        {
            cn.Open();
            var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            while (rdr.Read())
            {
                OfferBase offer;
                if (!String.IsNullOrWhiteSpace(rdr.TryGetDataAsString(8)))
                {
                    var coupon = new Coupon(rdr.TryGetDataAsInt(0), rdr.TryGetDataAsString(8))
                                     {
                                         Description = rdr.TryGetDataAsString(1),
                                         Title = rdr.TryGetDataAsString(2),
                                         Type = (OfferBase.OfferType)rdr.TryGetDataAsInt(3),
                                         ValidFrom = rdr.TryGetDataAsDateTime(4),
                                         ValidTill = rdr.TryGetDataAsDateTime(5),
                                         Value = rdr.TryGetDataAsInt(6)
                                     };
                    offer = coupon;
                }
                else
                {
                    var seasonaloffer = new SeasonalOffer(rdr.TryGetDataAsInt(0))
                                            {
                                                Description = rdr.TryGetDataAsString(1),
                                                Title = rdr.TryGetDataAsString(2),
                                                Type = (OfferBase.OfferType)rdr.TryGetDataAsInt(3),
                                                ValidFrom = rdr.TryGetDataAsDateTime(4),
                                                ValidTill = rdr.TryGetDataAsDateTime(5),
                                                Value = rdr.TryGetDataAsInt(6),
                                                PictureFileName = rdr.TryGetDataAsString(7)
                                            };
                    offer = seasonaloffer;
                }

                offers.Add(offer);
            }
        }
    }
}