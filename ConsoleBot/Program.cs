using ConsoleBot.Core.DataAccess;
using ConsoleBot.Core.Services;
using ConsoleBot.Infrastructure.DataAccess;
using ConsoleBot.TelegramBot;
using Otus.ToDoList.ConsoleBot;

namespace ConsoleBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var userRepository = new InMemoryUserRepository();
            var toDoRepository = new InMemoryToDoRepository();
            var reportService = new ToDoReportService(toDoRepository);
            var userService = new UserService(userRepository);
            var toDoService = new ToDoService(toDoRepository);
            var handler = new UpdateHandler(userService, toDoService, reportService);
            var botClient = new ConsoleBotClient();

            try
            {
                botClient.StartReceiving(handler);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}