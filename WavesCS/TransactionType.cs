namespace WavesCS
{    
    public enum TransactionType : byte
    {
        Issue = 3,
        Transfer = 4,
        Reissue = 5,
        Burn = 6,
        Lease = 8,
        LeaseCancel = 9,
        Alias = 10,
        MassTransfer = 11,
        DataTx = 12,    
    }
}
