﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ConsoleBot
{
    public class UserService : IUserService
    {
        private readonly Dictionary<long, ToDoUser> _users = new();

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser
            {
                UserId = Guid.NewGuid(),
                TelegramUserId = telegramUserId,
                TelegramUserName = telegramUserName,
                RegisteredAt = DateTime.UtcNow
            };
            _users[telegramUserId] = user;
            return user;
        }

        public ToDoUser? GetUser(long telegramUserId)
        {
            _users.TryGetValue(telegramUserId, out var user);
            return user;
        }
    }

}
