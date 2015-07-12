using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RestaurantBookingSystem.Helpers
{
    public static class StringExtentions
    {
        /// <summary>
        /// An Extention method to check wheather a string expression is Null, Empty or Filled with Whitespaces
        /// </summary>
        /// <param name="expression">The string Expression to check</param>
        /// <returns>Returns True is string contains atleast one valid character otherwise returns false</returns>
        public static bool IsNullOrEmpty(this string expression)
        {
            return expression == null || String.IsNullOrEmpty(expression.Trim());
        }

        /// <summary>
        /// A Utility method to check wheather the string expression is a Numeric character
        /// </summary>
        /// <param name="expression">The string expression to check</param>
        /// <returns>Returns true if string represents a number otherwise returns false</returns>
        public static bool IsNumeric(this string expression)
        {
            // Define variable to collect out parameter of the TryParse method. If the conversion fails, the out parameter is zero.
            double retNum;

            // The TryParse method converts a string in a specified style and culture-specific format to its double-precision floating point number equivalent.
            // The TryParse method does not generate an exception if the conversion fails. If the conversion passes, True is returned. If it does not, False is returned.
            return Double.TryParse(expression, System.Globalization.NumberStyles.Any, System.Globalization.NumberFormatInfo.InvariantInfo, out retNum);

        }

        /// <summary>
        /// Truncates the given string expression upto the given maxlength with trailing elipses if length is bigger than given maxlength
        /// </summary>
        /// <param name="s">The string expresion to truncate</param>
        /// <param name="maxLength">The integer upperlimit for the string length</param>
        /// <returns>Returns the origial string if string is smaller than maxlength otherwise a truncated string with trailing elipses</returns>
        public static string Truncate(this string s, int maxLength)
        {
            if (string.IsNullOrEmpty(s) || maxLength <= 0)
                return string.Empty;
            if (s.Length > maxLength)
                return s.Substring(0, maxLength) + "...";
            return s;
        }

        /// <summary>
        /// Concatenates the items of a list to a 'seperator' Seperated string
        /// </summary>
        /// <param name="list">A list of strings that you want to concatenate</param>
        /// <param name="seperator">A char literal to seperate each string</param>
        /// <returns>
        /// <para>Returns a Concatenated string by joining each item of the list seperated</para>
        /// <para> with a seperator charactor (Returns null if argument is null)</para>
        /// </returns>
        public static string ToString(this IList<string> list, char seperator)
        {
            if (list == null) return null;
            var builder = new StringBuilder();
            foreach (var item in list)
            {
                builder.Append(String.Format("{0}{1}", item, seperator));
            }
            return builder.ToString().Trim(seperator);
        }

        /// <summary>
        /// Converts the items of a Dictionary to a UriEncoded Query string with item's Key as query parameter and item's value as parameter value
        /// </summary>
        /// <typeparam name="TValue">Type of Dictionary item's value</typeparam>
        /// <param name="dictionary">A Dictionary of key value pairs</param>
        /// <param name="prepend">
        /// <para>A bool value indicating wheather to prepend query string with a '?' mark,</para>
        /// <para>(should be false in case passing result to a UriBuilder.Query property)</para>
        /// </param>
        /// <returns>Returns a UriEncoded string formatted as a Uri Query Parameter key-value pair, Returns null if Dictionary is null</returns>
        public static string ToUriQueryString<TValue>(this Dictionary<string, TValue> dictionary, bool prepend = true)
        {
            if (dictionary == null) return null;
            var builder = new StringBuilder();
            if (prepend)
            {
                builder.Append("?");
            }
            foreach (var item in dictionary)
            {
                builder.Append(String.Format("{0}={1}&", item.Key,  Uri.EscapeDataString(item.Value.ToString())));
            }
            return builder.ToString().TrimEnd('&');
        }
    }
}