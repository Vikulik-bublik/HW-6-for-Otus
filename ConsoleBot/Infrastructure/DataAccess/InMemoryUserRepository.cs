using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        private readonly List<ToDoUser> _users = new();

        public ToDoUser? GetUser(Guid userId)
        {
            return _users.FirstOrDefault(user => user.UserId == userId);
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);
        }

        public void Add(ToDoUser user)
        {
            _users.Add(user);
        }
    }
}
