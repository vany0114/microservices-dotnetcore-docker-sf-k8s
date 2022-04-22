namespace Duber.Infrastructure.Resilience.Sql.Policies
{
    internal static class PolicyKeys
    {
        public const string SqlFallbackAsyncPolicy = "A.SqlFallbackAsyncPolicy";
        public const string SqlFallbackSyncPolicy = "A.SqlFallbackSyncPolicy";
        public const string SqlOverallTimeoutSyncPolicy = "B.SqlOverallTimeoutSyncPolicy";
        public const string SqlOverallTimeoutAsyncPolicy = "B.SqlOverallTimeoutAsyncPolicy";
        public const string SqlCommonTransientErrorsSyncPolicy = "C.SqlCommonTransientErrorsSyncPolicy";
        public const string SqlCommonTransientErrorsAsyncPolicy = "C.SqlCommonTransientErrorsAsyncPolicy";
        public const string SqlTransactionAsyncPolicy = "D.SqlTransactionAsyncPolicy";
        public const string SqlTransactionSyncPolicy = "D.SqlTransactionSyncPolicy";
        public const string SqlTimeoutPerRetrySyncPolicy = "E.SqlTimeoutPerRetrySyncPolicy";
        public const string SqlTimeoutPerRetryAsyncPolicy = "E.SqlTimeoutPerRetryAsyncPolicy";
        public const string SqlCircuitBreakerAsyncPolicy = "SqlCircuitBreakerAsyncPolicy";
        public const string SqlCircuitBreakerSyncPolicy = "SqlCircuitBreakerSyncPolicy";
    }
}
