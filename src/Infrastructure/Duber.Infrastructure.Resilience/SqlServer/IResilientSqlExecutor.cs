namespace Duber.Infrastructure.Resilience.SqlServer
{
    public interface ISqlExecutor
    {
        ResilientExecutor<ISqlExecutor> CreateResilientSqlClient();
    }
}