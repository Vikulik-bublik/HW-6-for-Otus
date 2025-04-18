using System;
using Otus.ToDoList.ConsoleBot.Types;
using Otus.ToDoList.ConsoleBot;

public class Helper
{
    public static int MaxTaskCount;
    public static int MaxLengthCount;
    public static void SetMaxTaskCount(ITelegramBotClient botClient, string input, Update update)
    {
        try
        {
            MaxTaskCount = ParseAndValidateInt(input, min: 1, max: 100);
            botClient.SendMessage(update.Message.Chat, $"Максимальное число задач установлено: {MaxTaskCount}.");
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
