namespace bancoSol.Constants
{
    public static class TransactionType
    {
        public const string Deposit = "Deposit";
        public const string Withdrawal = "Withdrawal";
        public const string TransferIn = "TransferIn";
        public const string TransferOut = "TransferOut";
    }

    public static class AccountStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
        public const string Blocked = "Blocked";
    }

    public static class Currency
    {
        public const string BOB = "BOB";
        public const string USD = "USD";
    }

    public static class TransferStatus
    {
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}