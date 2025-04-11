using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;


namespace ConsoleBot
{
    public class ToDoService : IToDoService
    {
        private readonly List<ToDoItem> _items = new();
        public static int MaxTaskCount;
        public static int MaxLengthCount;

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _items.Where(item => item.User.UserId == userId && item.State == ToDoItemState.Active).ToList();
        }

        public ToDoItem Add(ToDoUser user, string name)
        { 
            ValidateString(name);
            if (name.Length > MaxLengthCount)
                throw new TaskLengthLimitException(MaxLengthCount);
            if (_items.Count >= MaxTaskCount)
                throw new TaskCountLimitException(MaxTaskCount);
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

        public List<ToDoItem> GetAllTasks()
        {
            return _items;
        }

        public static void SetMaxTaskCount(ITelegramBotClient botClient, string input, Update update)
        {
            try
            {
                MaxTaskCount = ParseAndValidateInt(input, min: 1, max: 100);
                botClient.SendMessage(update.Message.Chat, $"Максимальное число задач установлено: {MaxTaskCount}");
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
        }

        public static void SetMaxLengthCount(ITelegramBotClient botClient, string input, Update update)
        {
            try
            {
                MaxLengthCount = ParseAndValidateInt(input, min: 1, max: 100);
                botClient.SendMessage(update.Message.Chat, $"Максимальная длина задач установлена на количество символов: {MaxLengthCount}.");
            }
            catch (ArgumentException ex)
            {
                throw ex;
            }
        }

        public static void ValidateString(string? str)
        {
            if (string.IsNullOrWhiteSpace(str))
            {
                throw new ArgumentException("Ввод не должен быть пустым или содержать только пробелы.");
            }
        }

        public static int ParseAndValidateInt(string? str, int min, int max)
        {
            ValidateString(str);

            if (!int.TryParse(str, out int result))
            {
                throw new ArgumentException("Ввод должен быть целым числом.");
            }
            if (result < min || result > max)
            {
                throw new ArgumentException($"Значение должно быть в диапазоне от {min} до {max}.");
            }
            return result;
        }
    }
}
