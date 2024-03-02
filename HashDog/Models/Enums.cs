namespace HashDog;

public enum HashType
{
    MD5,
    SHA1,
    SHA256,
    SHA512,
}


public enum HashCompareResult
{
    firstRun,
    match,
    mismatch,
}


public enum RunFrequency
{
    Hourly,
    Daily,
    Monthly,
    Weekly,
    Yearly,
}