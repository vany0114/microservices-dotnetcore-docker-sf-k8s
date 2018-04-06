using System.Collections.Generic;
using System.Threading.Tasks;
using Duber.Infrastructure.Repository.Abstractions;

namespace Duber.Domain.User.Repository
{
    public interface IUserRepository : IRepository<Model.User>
    {
        Task<IList<Model.User>> GetUsersAsync();

        void Update(Model.User user);
    }
}
