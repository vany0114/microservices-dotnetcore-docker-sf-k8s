using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Duber.Domain.User.Persistence;
using Duber.Infrastructure.Repository.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace Duber.Domain.User.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly UserContext _context;

        public UserRepository(UserContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IUnitOfWork UnitOfWork => _context;

        public async Task<IList<Model.User>> GetUsersAsync()
        {
            return await _context.Users
                .Include(x => x.PaymentMethod)
                .ToListAsync();
        }

        public async Task<Model.User> GetUserAsync(int userId)
        {
            return await _context.Users.SingleOrDefaultAsync(x => x.Id == userId);
        }

        public Model.User GetUser(int userId)
        {
            return _context.Users.SingleOrDefault(x => x.Id == userId);
        }

        public void Update(Model.User user)
        {
            _context.Entry(user).State = EntityState.Modified;
        }
    }
}