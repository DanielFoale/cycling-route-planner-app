using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CyclingRoutePlannerApp
{
    internal class SHA
    {
        public string SHA256_hashing(string text)
        {
            SHA256 mySHA256 = SHA256.Create();
            byte[] hashValue = mySHA256.ComputeHash(Encoding.ASCII.GetBytes(text));
            return Convert.ToHexString(hashValue);
        }
    }
}
