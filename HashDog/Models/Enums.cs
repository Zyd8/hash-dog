public enum HashType
{
    MD5,
    SHA1,
    SHA256,
    SHA512,
    CRC32, 
    CRC64
}

public enum TransferMode
{
    copy,
    cut
}

public enum HashTransferResult
{
    match,
    mismatch
}