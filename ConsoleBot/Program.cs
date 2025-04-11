using Otus.ToDoList.ConsoleBot;

namespace ConsoleBot
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var userService = new UserService();
            var toDoService = new ToDoService();
            var handler = new UpdateHandler(userService, toDoService);
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