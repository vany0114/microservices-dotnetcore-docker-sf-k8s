using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Duber.Infrastructure.DDD;

namespace Duber.Domain.Invoice.Persistence
{
    public interface IInvoiceContext : IDisposable
    {
        Task<int> ExecuteAsync<T>(T entity, string sql, object parameters = null, int? timeOut = null, CommandType? commandType = null)
            where T : Entity, IAggregateRoot;

        Task<IEnumerable<T>> QueryAsync<T>(string sql, object parameters = null, int? timeOut = null, CommandType? commandType = null)
            where T : Entity, IAggregateRoot;

        Task<T> QuerySingleAsync<T>(string sql, object parameters = null, int? timeOut = null, CommandType? commandType = null)
            where T : Entity, IAggregateRoot;
    }
}