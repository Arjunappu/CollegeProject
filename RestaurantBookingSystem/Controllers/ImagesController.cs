using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using RestaurantBookingSystem.Helpers;
using System.Web.Mvc;

namespace RestaurantBookingSystem.Controllers
{
    public class ImagesController : Controller
    {
        [OutputCache(Duration = 3600, VaryByHeader = "Etag")]
        public FileStreamResult GetImage(string id)
        {
            Guid guid;
            if (Guid.TryParse(id,out guid) && guid != Guid.Empty)
            {
                using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
                using (var cmd = new SqlCommand("[dbo].[GetBlob]", cn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier).Value = guid;

                    try
                    {
                        cn.Open();
                        using (var rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess))
                        {
                            if (rdr.HasRows && rdr.Read())
                            {
                                var result = new
                                                 {
                                                     ID = rdr.TryGetDataAsGuid(0),
                                                     MimeType = rdr.TryGetDataAsString(1),
                                                     ImageBlobStream = rdr.GetSqlBytes(2).Stream,
                                                     Sha1Hash = rdr.GetString(3)
                                                 };
                                if (Response != null)
                                    Response.Cache.SetETag(result.Sha1Hash);
                                return File(result.ImageBlobStream, result.MimeType);
                            }
                        }
                    }
                    catch (SqlException)
                    {
                        return null;
                    }
                }
            }
            //Set etag even for invalid image response so that they dont come back again asking for it
            Response.Cache.SetETag(CryptographyHelper.GetSHA1Hash(guid.ToString("n")));
            return null;
        }

        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Put)]
        [Authorize]
        public Guid PutImage(HttpPostedFileBase image, Guid? id)
        {
            var result = Guid.Empty;
            if (image == null || image.ContentLength < 1)
                return result;
            
            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            using (var cmd = new SqlCommand("[dbo].[AddBlob]", cn))
            {
                var guid = id ?? Guid.NewGuid();
                var buffer = image.InputStream.ReadToEnd();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier).Value = guid;
                cmd.Parameters.Add("@MIMETYPE", SqlDbType.Char, 20).Value = image.ContentType;
                cmd.Parameters.Add("@BLOB", SqlDbType.VarBinary, image.ContentLength).Value = buffer;
                cmd.Parameters.Add("@SHA1HASH", SqlDbType.Char, 40).Value =
                    CryptographyHelper.GetSHA1Hash(buffer);

                try
                {
                    cn.Open();
                    return Guid.TryParse(cmd.ExecuteScalar().ToString(), out result) ? result : Guid.Empty;
                }
                catch (SqlException)
                {
                    return result;
                }
                
            }
        }

        [AcceptVerbs(HttpVerbs.Post | HttpVerbs.Delete)]
        [Authorize(Roles = "Admin, Employee")]
        [OutputCache(Duration = 3600, VaryByParam = "id")]
        public bool DeleteImage(string id)
        {
            var result = false;
            Guid guid;
            if (id == null || !Guid.TryParse(id, out guid) || guid == Guid.Empty)
                return false;

            using (var cn = new SqlConnection(DatabaseConnection.ConnectionStringToDb))
            using (var cmd = new SqlCommand("[dbo].[DeleteBlob]", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@UID", SqlDbType.UniqueIdentifier).Value = guid;

                try
                {
                    cn.Open();
                    return result = cmd.ExecuteNonQuery() == 1;
                }
                catch (SqlException)
                {
                    return result;
                }
            }
        }
    }
}
