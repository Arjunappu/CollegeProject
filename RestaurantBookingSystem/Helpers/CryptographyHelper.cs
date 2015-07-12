using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace RestaurantBookingSystem.Helpers
{
    public static class CryptographyHelper
    {
        /// <summary>
        /// Gets a MD5 hash of the specified plaintext
        /// </summary>
        /// <param name="plaintext">A string that has to be hashed</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns A string representation of the hashed string</returns>
        public static string GetMD5Hash(string plaintext)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");

            var hashalgo = (HashAlgorithm)MD5.Create();
            var computedhash = hashalgo.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            return BitConverter.ToString(computedhash).Replace("-", "");
        }

        /// <summary>
        /// Gets a MD5 hash of the specified Byte Array
        /// </summary>
        /// <param name="buffer">A Byte Array that has to be hashed</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns A string representation of the hashed string</returns>
        public static string GetMD5Hash(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", "Value cannot be null.");

            var hashalgo = (HashAlgorithm)MD5.Create();
            var computedhash = hashalgo.ComputeHash(buffer);
            return BitConverter.ToString(computedhash).Replace("-", "");
        }

        /// <summary>
        /// Matches a specified plaintext with its specified MD5 hash
        /// </summary>
        /// <param name="plaintext">A string that needs to be checked with its hash</param>
        /// <param name="hash">A string representation of the plaintext's hash</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if hash of plaintext matches the given hash, otherwise returns false</returns>
        public static bool MatchMD5Hash(string plaintext, string hash)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException("hash", "Value cannot be null or Empty.");

            var hashalgo = (HashAlgorithm)MD5.Create();
            var computedhash = hashalgo.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            var hashed = BitConverter.ToString(computedhash).Replace("-", "");
            return String.Equals(hashed, hash);
        }

        /// <summary>
        /// Matches a specified Byte Array with its specified MD5 hash
        /// </summary>
        /// <param name="plaintext">A Byte Array that needs to be checked with its hash</param>
        /// <param name="hash">A string representation of the hash</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if hash of Byte Array matches the given hash, otherwise returns false</returns>
        public static bool MatchMD5Hash(byte[] plaintext, string hash)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException("hash", "Value cannot be null or Empty.");

            var hashalgo = (HashAlgorithm)MD5.Create();
            var computedhash = hashalgo.ComputeHash(plaintext);
            var hashed = BitConverter.ToString(computedhash).Replace("-", "");
            return String.Equals(hashed, hash);
        }

        /// <summary>
        /// Gets a SHA1 hash of the specified plaintext
        /// </summary>
        /// <param name="plaintext">A string that has to be hashed</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns A string representation of the hash</returns>
        public static string GetSHA1Hash(string plaintext)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");

            var hashalgo = (HashAlgorithm)SHA1.Create();
            var computedhash = hashalgo.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            return BitConverter.ToString(computedhash).Replace("-", "");
        }

        /// <summary>
        /// Gets a SHA1 hash of the specified Byte Array
        /// </summary>
        /// <param name="buffer">A Byte Array that has to be hashed</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns A string representation of the hash</returns>
        public static string GetSHA1Hash(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", "Value cannot be null.");

            var hashalgo = (HashAlgorithm)SHA1.Create();
            var computedhash = hashalgo.ComputeHash(buffer);
            return BitConverter.ToString(computedhash).Replace("-", "");
        }

        /// <summary>
        /// Matches a specified plaintext with its specified SHA1 hash
        /// </summary>
        /// <param name="plaintext">A string that needs to be checked with its hash</param>
        /// <param name="hash">A string representation of the plaintext's hash</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if hash of plaintext matches the given hash, otherwise returns false</returns>
        public static bool MatchSHA1Hash(string plaintext, string hash)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException("hash", "Value cannot be null or Empty.");

            var hashalgo = (HashAlgorithm)SHA1.Create();
            var computedhash = hashalgo.ComputeHash(Encoding.UTF8.GetBytes(plaintext));
            var hashed = BitConverter.ToString(computedhash).Replace("-", "");
            return String.Equals(hashed, hash);
        }

        /// <summary>
        /// Matches a specified Byte Array with its specified SHA1 hash
        /// </summary>
        /// <param name="buffer">A Byte Array that needs to be checked with its hash</param>
        /// <param name="hash">A string representation of the hash</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if hash of Byte Array matches the given hash, otherwise returns false</returns>
        public static bool MatchSHA1Hash(byte[] buffer, string hash)
        {
            if (buffer == null) throw new ArgumentNullException("buffer", "Value cannot be null.");
            if (string.IsNullOrEmpty(hash)) throw new ArgumentNullException("hash", "Value cannot be null or Empty.");

            var hashalgo = (HashAlgorithm)SHA1.Create();
            var computedhash = hashalgo.ComputeHash(buffer);
            var hashed = BitConverter.ToString(computedhash).Replace("-", "");
            return String.Equals(hashed, hash);
        }

        /// <summary>
        /// Gets a Globally Unique One Time hash of a specified plaintext
        /// </summary>
        /// <param name="plaintext">A plaintext string</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns a Globally Unique One time hash</returns>
        public static string GetOneTimeHash(string plaintext)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");

            Func<string, string> md5Hasher = GetMD5Hash;
            Func<string, string> sha1Hasher = GetSHA1Hash;
            Func<string, string, string> concat = String.Concat;
            var salt = md5Hasher(Guid.NewGuid().ToString());
            var temphash = md5Hasher(plaintext);
            temphash = sha1Hasher(concat(temphash, salt));
            return concat(md5Hasher(concat(temphash, salt)), salt);
        }

        /// <summary>
        /// Matches a plaintext with a Globally Unique one time hash
        /// </summary>
        /// <param name="plaintext">A plaintext string</param>
        /// <param name="hash">A string representation claimed to be Globally Unique One Time hash of the plaintext</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns>Returns true if the Globally Unique hash specified for the plaintext matches with the claimed hash</returns>
        public static bool MatchOneTimeHash(string plaintext, string hash)
        {
            if (plaintext == null) throw new ArgumentNullException("plaintext", "Value cannot be null.");
            if (String.IsNullOrEmpty(hash)) throw new ArgumentNullException("hash", "Value cannot be null or Empty.");

            if (hash.Trim().Length != 64) return false;

            Func<string, string> md5Hasher = GetMD5Hash;
            Func<string, string> sha1Hasher = GetSHA1Hash;
            Func<string, string, string> concat = String.Concat;
            var salt = hash.Substring(32);
            var temphash = md5Hasher(plaintext);
            temphash = sha1Hasher(concat(temphash, salt));
            return String.Equals(concat(md5Hasher(concat(temphash, salt)), salt),hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}