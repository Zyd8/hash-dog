using System;
using System.IO;
using System.Security.Cryptography;

namespace HashDog.Models
{
    public class HashUtils
    {
        public static string GetFileHash(string path, HashType hashType)
        {
            using (var stream = File.OpenRead(path))
            {
                using (var hashAlgorithm = GetCryptographicHashAlgorithm(hashType))
                {
                    byte[] hashBytes = hashAlgorithm.ComputeHash(stream);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }

        private static HashAlgorithm GetCryptographicHashAlgorithm(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.MD5:
                    return MD5.Create();
                case HashType.SHA1:
                    return SHA1.Create();
                case HashType.SHA256:
                    return SHA256.Create();
                case HashType.SHA512:
                    return SHA512.Create();
                default:
                    throw new Exception();
            }
        }

        public static string ParseHashComparisonToString(ComparisonResult comparisonResult)
        {
            switch (comparisonResult)
            {
                case ComparisonResult.FirstRun:
                    return "FirstRun";
                case ComparisonResult.Mismatch:
                    return "Mismatch";
                case ComparisonResult.Match:
                    return "Match";
                default:
                    throw new ArgumentException("Invalid ComparisonResult value.");
            }
        }


        public static ComparisonResult HashCompare(string string1, string string2)
        {
            if (string1 == null || string2 == null)
            {
                return ComparisonResult.FirstRun;
            }
            else if (string1 != string2)
            {
                return ComparisonResult.Mismatch;
            }
            else if (string1 == string2)
            {
                return ComparisonResult.Match;
            }
            else
            {
                throw new ArgumentException("Invalid ComparisonResult value."); 
            }    
        }

        public static string ParseHashTypeToString(HashType hashType)
        {
            switch (hashType)
            {
                case HashType.MD5:
                    return "MD5";
                case HashType.SHA1:
                    return "SHA1";
                case HashType.SHA256:
                    return "SHA256";
                case HashType.SHA512:
                    return "SHA512";
                default:
                    throw new ArgumentException($"Unsupported hash type: {hashType}");
            }
        }

        public static HashType ParseHashTypeFromString(string hashTypeString)
        {
            switch (hashTypeString.ToUpper()) // Ensure case insensitivity
            {
                case "MD5":
                    return HashType.MD5;
                case "SHA1":
                    return HashType.SHA1;
                case "SHA256":
                    return HashType.SHA256;
                case "SHA512":
                    return HashType.SHA512;
                default:
                    throw new ArgumentException($"Unsupported hash type string: {hashTypeString}");
            }
        }

    }
}
