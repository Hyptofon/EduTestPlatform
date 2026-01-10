namespace Domain.TestSessions;

public enum TestSessionStatus
{
    InProgress = 0,
    Completed = 1,    // Здав сам
    Abandoned = 2,    // Вийшов час
    Evaluated = 3     // Перевірено (оцінка фінальна)
}