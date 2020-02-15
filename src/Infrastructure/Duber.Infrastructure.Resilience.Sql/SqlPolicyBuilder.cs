using Duber.Infrastructure.Resilience.Sql.Internals;

namespace Duber.Infrastructure.Resilience.Sql
{
    public class SqlPolicyBuilder
    {
        public ISqlAsyncPolicyBuilder UseAsyncExecutor()
        {
            return new SqlAsyncPolicyBuilder();
        }

        public ISqlAsyncPolicyBuilder UseAsyncExecutorWithSharedPolicies()
        {
            return new SqlAsyncPolicyBuilder(true);
        }

        public ISqlSyncPolicyBuilder UseSyncExecutor()
        {
            return new SqlSyncPolicyBuilder();
        }

        public ISqlSyncPolicyBuilder UseSyncExecutorWithSharedPolicies()
        {
            return new SqlSyncPolicyBuilder(true);
        }
    }
}
