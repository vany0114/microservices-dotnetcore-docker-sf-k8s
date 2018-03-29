using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Duber.Domain.User.Model;
using Duber.Infrastructure.Repository.Abstractions;

namespace Duber.Domain.User.Repository
{
    public interface IUserRepository : IRepository<Model.User>
    {
        Task<IList<Model.User>> GetUsersAsync();

        void Update(Model.User user);
    }
}
