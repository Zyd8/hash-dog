using System;

namespace HashDog;

public class Parser
{
    public static HashType ParseStringToHashType(string value)
    {
        if (string.Equals(value, "md5", StringComparison.OrdinalIgnoreCase))
        {
            return HashType.MD5;
        }
        else if (string.Equals(value, "sha1", StringComparison.OrdinalIgnoreCase))
        {
            return HashType.SHA1;
        }
        else if (string.Equals(value, "sha256", StringComparison.OrdinalIgnoreCase))
        {
            return HashType.SHA256;
        }
        else if (string.Equals(value, "sha512", StringComparison.OrdinalIgnoreCase))
        {
            return HashType.SHA512;
        }
        else
        {
            throw new ArgumentException("Invalid hash algorithm");
        }
    }

    public static string ParseHashTypeToString(HashType value)
    {
        if (value == HashType.MD5)
        {
            return "md5";
        }
        else if (value == HashType.SHA1) 
        {
            return "sha1";
        }
        else if (value == HashType.SHA256) 
        {
            return "sha256";
        }
        else if (value == HashType.SHA512) 
        {
            return "sha512";
        }
        else
        {
            throw new ArgumentException("Invalid hash algorithm");
        }
    }

    public static string ParseHashCompareResultToString(HashCompareResult value)
    {
        if (value == HashCompareResult.firstRun)
        {
            return "First Run";
        }
        else if (value == HashCompareResult.match)
        {
            return "Match";
        }
        else if (value == HashCompareResult.mismatch)
        {
            return "Mismatch";
        }
        else
        {
            throw new ArgumentException("Invalid hash comparison result type");
        }
    }

    public static HashCompareResult ParseStringToHashCompareResult(string value)
    {
        if (string.Equals(value, "First Run", StringComparison.OrdinalIgnoreCase))
        {
            return HashCompareResult.firstRun;
        }
        else if (string.Equals(value, "Match", StringComparison.OrdinalIgnoreCase))
        {
            return HashCompareResult.match;
        }
        else if (string.Equals(value, "Mismatch", StringComparison.OrdinalIgnoreCase))
        {
            return HashCompareResult.mismatch;
        }
        else
        {
            throw new ArgumentException("Invalid hash comparison result string");
        }
    }
}