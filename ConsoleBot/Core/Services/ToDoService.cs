using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using ConsoleBot.Core.Entities;
using ConsoleBot.Core.Exceptions;
using ConsoleBot.Core.DataAccess;

namespace ConsoleBot.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly IToDoRepository _toDoRepository;

        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoRepository.GetActiveByUserId(userId);
        }
        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            return _toDoRepository.Find(user.UserId,
                item => item.Name.StartsWith(namePrefix, StringComparison.OrdinalIgnoreCase));
        }

        public ToDoItem Add(ToDoUser user, string name)
        {
            Helper.ValidateString(name);
            if (name.Length > Helper.MaxLengthCount)
                throw new TaskLengthLimitException(Helper.MaxLengthCount);
            if (_toDoRepository.CountActive(user.UserId) >= Helper.MaxTaskCount)
                throw new TaskCountLimitException(Helper.MaxTaskCount);
            if (_toDoRepository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            var item = new ToDoItem
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                State = ToDoItemState.Active
            };
            _toDoRepository.Add(item);
            return item;
        }

        public void MarkCompleted(Guid id, Guid userId)
        {
            var item = _toDoRepository.GetAllByUserId(userId).FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.UtcNow;
                _toDoRepository.Update(item);
            }
        }

        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        public IReadOnlyList<ToDoItem> GetAllTasks(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }
    }
}
