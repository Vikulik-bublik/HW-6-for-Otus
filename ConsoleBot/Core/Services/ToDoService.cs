using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using ConsoleBot.Core.Entities;
using ConsoleBot.Core.Exceptions;

namespace ConsoleBot.Core.Services
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _items.Where(item => item.User.UserId == userId && item.State == ToDoItemState.Active).ToList();
        }

        public ToDoItem Add(ToDoUser user, string name)
        { 
            Helper.ValidateString(name);
            if (name.Length > Helper.MaxLengthCount)
                throw new TaskLengthLimitException(Helper.MaxLengthCount);
            if (_items.Count >= Helper.MaxTaskCount)
                throw new TaskCountLimitException(Helper.MaxTaskCount);
            if (_items.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase) && t.User.UserId == user.UserId))
                throw new DuplicateTaskException(name);
            
            var item = new ToDoItem
            {
                Id = Guid.NewGuid(),
                User = user,
                Name = name,
                CreatedAt = DateTime.UtcNow,
                State = ToDoItemState.Active
            };
            _items.Add(item);
            return item;
        }

        public void MarkCompleted(Guid id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                item.State = ToDoItemState.Completed;
                item.StateChangedAt = DateTime.UtcNow;
            }
        }

        public void Delete(Guid id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
            {
                _items.Remove(item);
            }
        }

        public IReadOnlyList<ToDoItem> GetAllTasks()
        {
            return _items.AsReadOnly();
        }
    }
}
