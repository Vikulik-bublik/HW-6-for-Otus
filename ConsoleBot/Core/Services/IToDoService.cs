using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleBot.Core.Entities;

namespace ConsoleBot.Core.Services
{
    public interface IToDoService
    {
        IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId);
        IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix);
        ToDoItem Add(ToDoUser user, string name);
        void MarkCompleted(Guid id, Guid userId);
        void Delete(Guid id);
        IReadOnlyList<ToDoItem> GetAllTasks(Guid userId);
    }
}
