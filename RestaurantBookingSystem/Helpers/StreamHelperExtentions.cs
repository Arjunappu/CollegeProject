using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RestaurantBookingSystem.Helpers
{
    public static class StreamHelperExtentions
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream", "Value cannot be null");
            if (!stream.CanRead)
                throw new IOException("Stream dosen't support Reading");
            
            if(stream.Length > Int32.MaxValue)
                throw new IOException("Data is too big, greater than 2GB");

            var length = (Int32) stream.Length;
            var buffer = new byte[length];
            var bytesread = 0;
            while (true)
            {
                bytesread = stream.Read(buffer, bytesread, length);
                if (bytesread < 1)
                    break;
            }
            return buffer;
        }
    }
}