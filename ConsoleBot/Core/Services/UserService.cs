using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Entities;
using Otus.ToDoList.ConsoleBot.Types;

namespace ConsoleBot.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.UtcNow
            };
            _userRepository.Add(user);
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _userRepository.GetUserByTelegramUserId(telegramUserId);
        }
    }
}
