namespace Duber.Domain.ACL
{
    public static class ThirdPartyServices
    {
        public static class Payment
        {
            public static string PerformPayment()
            {
                return "/api/payment/performpayment?userId={0}&reference={1}";
            }
        }
    }
}
