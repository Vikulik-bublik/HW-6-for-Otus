using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;
using System.Xml.Linq;

namespace ConsoleBot
{
    public class UpdateHandler : IUpdateHandler
    {
        private readonly IUserService _userService;
        private readonly IToDoService _toDoService;

        public UpdateHandler(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            var telegramUserId = update.Message.From.Id;
            var telegramUserName = update.Message.From.Username;
            var command = update.Message.Text.Split(' ')[0];

            try
            {
                var user = _userService.GetUser(telegramUserId);
                //условия для задания максимального количества задач и ее максимальной длины
                if (user != null && user.WaitingForMaxTaskCount)
                {
                    HandleMaxTaskCountInput(botClient, user, update);
                    return;
                }

                if (user != null && user.WaitingForMaxLengthCount)
                {
                    HandleMaxLengthCountInput(botClient, user, update);
                    return;
                }
                //условие обработки команды start
                if (command == "/start")
                {
                    Start(botClient, user, update);
                    return;
                }

                if (user != null)
                {
                    HandleRegisteredUserCommands(botClient, command, user, update);
                }
                else
                {
                    HandleUnregisteredUserCommands(botClient, command, update);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex, botClient, update);
            }
        }
        //метод для задания максимального количества задач при условии состояния
        private void HandleMaxTaskCountInput(ITelegramBotClient botClient, ToDoUser user, Update update)
        {
            var input = update.Message.Text.Trim(); 
            try
            {
                Helper.SetMaxTaskCount(botClient, input, update);
                user.WaitingForMaxTaskCount = false;
                botClient.SendMessage(update.Message.Chat, "Теперь введите максимальную длину задачи (от 1 до 100):");
                user.WaitingForMaxLengthCount = true;
            }
            catch (ArgumentException ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);
            }
        }
        //метод для задания максимальной длины задачи при условии состояния
        private void HandleMaxLengthCountInput(ITelegramBotClient botClient, ToDoUser user, Update update)
        {
            var input = update.Message.Text.Trim();
            try
            {
                Helper.SetMaxLengthCount(botClient, input, update);
                user.WaitingForMaxLengthCount = false;
                botClient.SendMessage(update.Message.Chat, "Настройки успешно сохранены!");
            }
            catch (ArgumentException ex)
            {
                botClient.SendMessage(update.Message.Chat, ex.Message);
            }
        }
        //метод обработки команды start
        private void Start(ITelegramBotClient botClient, ToDoUser? user, Update update)
        {
            if (user == null)
            {
                user = _userService.RegisterUser(update.Message.From.Id, update.Message.From.Username);
                botClient.SendMessage(update.Message.Chat, $"Добро пожаловать, Вы зарегистрированы как {user.TelegramUserName}!");
                botClient.SendMessage(update.Message.Chat, "Введите максимальное количество задач (от 1 до 100):");
                user.WaitingForMaxTaskCount = true;
            }
            else if (user.WaitingForMaxTaskCount)
            {
                var input = update.Message.Text;
                Helper.ValidateString(input);
                try
                {
                    Helper.SetMaxTaskCount(botClient, input, update);
                    user.WaitingForMaxTaskCount = false;
                    botClient.SendMessage(update.Message.Chat, "Теперь введите максимальную длину задачи (от 1 до 100):");
                    user.WaitingForMaxLengthCount = true;
                }
                catch (ArgumentException ex)
                {
                    botClient.SendMessage(update.Message.Chat, ex.Message);
                }
            }
            else if (user.WaitingForMaxLengthCount)
            {
                var input = update.Message.Text;
                Helper.ValidateString(input);
                try
                {
                    Helper.SetMaxLengthCount(botClient, input, update);
                    user.WaitingForMaxLengthCount = false;
                    botClient.SendMessage(update.Message.Chat, "Настройки успешно сохранены!");
                }
                catch (ArgumentException ex)
                {
                    botClient.SendMessage(update.Message.Chat, ex.Message);
                }
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, $"Мы уже знакомы, {user.TelegramUserName}!");
            }
        }

        //кейс обработки команд зарегистрированных пользователей
        private void HandleRegisteredUserCommands(ITelegramBotClient botClient, string command, ToDoUser? user, Update update)
        {
            switch (command)
            {
                case "/info":
                    botClient.SendMessage(update.Message.Chat, "Вот информация о боте. \nДата создания: 23.02.2025. Версия: 2.0.1 от 18.04.2025.");
                    break;
                case "/help":
                    Help(botClient, update);
                    break;
                case "/addtask":
                    AddTask(botClient, user, update);
                    break;
                case "/removetask":
                    RemoveTask(botClient, update);
                    break;
                case "/showtasks":
                    ShowTasks(botClient, user.UserId, update);
                    break;
                case "/completetask":
                    CompleteTask(botClient, update);
                    break;
                case "/showalltasks":
                    ShowAllTasks(botClient, update);
                    break;
                default:
                    botClient.SendMessage(update.Message.Chat, "Введена неверная команда. Пожалуйста, попробуйте снова.");
                    break;
            }
        }

        //кейс обработки команд незарегистрированных пользователей
        private void HandleUnregisteredUserCommands(ITelegramBotClient botClient, string command, Update update)
        {
            if (command == "/info" || command == "/help")
            {
                if (command == "/info")
                    botClient.SendMessage(update.Message.Chat, "Вот информация о боте. \nДата создания: 23.02.2025. Версия: 2.0.1 от 18.04.2025.");

                if (command == "/help")
                    Help(botClient, update);
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, "Вы не зарегистрированы или введена неверная команда. \nНезарегистрированным пользователям доступны команды только /help и /info. \nПожалуйста, используйте команду /start для регистрации.");
            }
        }

        //общий Хелп для всех категорий пользователей
        private void Help(ITelegramBotClient botClient, Update update)
        {
            var helpMessage = "Доступные команды: /start, /help, /info. Для зарегистрированных пользователей:" +
                              "\n/addtask <имя задачи> - добавление в список новой задачи;" +
                              "\n/removetask <номер задачи> - удаление задачи из списка по ее порядковому номеру;" +
                              "\n/completetask <ID задачи> - установить задачу как выполненную по ее ID;" +
                              "\n/showtasks - показать все активные задачи;" +
                              "\n/showalltasks - показать весь список задач.";
            botClient.SendMessage(update.Message.Chat, helpMessage);
        }

        //добавляем задачу по имени
        private void AddTask(ITelegramBotClient botClient, ToDoUser? user, Update update)
        {
            var taskName = update.Message.Text.Substring(8).Trim();
            if (!string.IsNullOrWhiteSpace(taskName))
            {
                _toDoService.Add(user, taskName);
                botClient.SendMessage(update.Message.Chat, $"Задача '{taskName}' добавлена.");
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, "Пожалуйста , укажите имя задачи.");
            }
        }

        //удаляем задачу по ее порядковому номеру
        private void RemoveTask(ITelegramBotClient botClient, Update update)
        {
            var input = update.Message.Text.Substring(12).Trim();
            if (int.TryParse(input, out int taskIndex))
            {
                var tasks = _toDoService.GetAllTasks();
                if (taskIndex >= 1 && taskIndex <= tasks.Count)
                {
                    var taskToRemove = tasks[taskIndex - 1];
                    _toDoService.Delete(taskToRemove.Id);
                    botClient.SendMessage(update.Message.Chat, $"Задача '{taskToRemove.Name}' удалена.");
                }
                else
                {
                    botClient.SendMessage(update.Message.Chat, $"Задача с порядковым номером {taskIndex} не найдена. Пожалуйста, введите номер от 1 до {tasks.Count}.");
                }
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, "Некорректный номер задачи.");
            }
        }

        //показываем активные задачи
        private void ShowTasks(ITelegramBotClient botClient, Guid userId, Update update)
        {
            var tasks = _toDoService.GetActiveByUserId(userId);
            var taskList = string.Join(Environment.NewLine,
                tasks.Select(t => $"\nЗадача: {t.Name} - Время создания задачи: {t.CreatedAt} - ID задачи: {t.Id}"));

            botClient.SendMessage(update.Message.Chat, string.IsNullOrEmpty(taskList) ? "Нет активных задач." : taskList);
        }

        //меняем состаяние задачи из активного в неактивное по ID
        private void CompleteTask(ITelegramBotClient botClient, Update update)
        {
            if (Guid.TryParse(update.Message.Text.Substring(14), out var taskIdToComplete))
            {
                _toDoService.MarkCompleted(taskIdToComplete);
                botClient.SendMessage(update.Message.Chat, $"Задача с ID '{taskIdToComplete}' помечена как выполненная.");
            }
            else
            {
                botClient.SendMessage(update.Message.Chat, "Некорректный ID задачи.");
            }
        }

        //показываем все активные и неактивные задачи
        private void ShowAllTasks(ITelegramBotClient botClient, Update update)
        {
            var allTasks = _toDoService.GetAllTasks();
            var allTaskList = string.Join(Environment.NewLine,
                allTasks.Select(t => $"\nСтатус задачи: ({t.State}) Задача: {t.Name} - Время создания задачи: {t.CreatedAt} - ID задачи: {t.Id}"));
            botClient.SendMessage(update.Message.Chat, string.IsNullOrEmpty(allTaskList) ? "Нет задач." : allTaskList);
        }

        //кейсы исключений
        private void HandleException(Exception ex, ITelegramBotClient botClient, Update update)
        {
            switch (ex)
            {
                case ArgumentException argEx:
                    botClient.SendMessage(update.Message.Chat, $"Ошибка аргумента: {argEx.Message}");
                    botClient.SendMessage(update.Message.Chat, "Произошла ошибка. Пожалуйста , проверьте введенные данные.");
                    break;

                case TaskCountLimitException taskCountLimit:
                    botClient.SendMessage(update.Message.Chat, $"Превышен лимит: {taskCountLimit.Message}");
                    break;

                case TaskLengthLimitException taskLengthLimit:
                    botClient.SendMessage(update.Message.Chat, $"Превышен лимит: {taskLengthLimit.Message}");
                    break;

                case DuplicateTaskException taskDouble:
                    botClient.SendMessage(update.Message.Chat, $"Дубликат задачи: {taskDouble.Message}");
                    break;

                default:
                    botClient.SendMessage(update.Message.Chat, $"Неизвестная ошибка: {ex.Message}");
                    botClient.SendMessage(update.Message.Chat, "Произошла неизвестная ошибка. Пожалуйста , попробуйте позже.");
                    break;
            }
        }
    }
}

