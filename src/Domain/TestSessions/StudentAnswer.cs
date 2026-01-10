namespace Domain.TestSessions;

public class StudentAnswer
{
    public Guid QuestionId { get; set; }
    
    // Для Single/Multiple choice
    public List<Guid> SelectedOptionIds { get; set; } = new();
    
    // Для Short Answer / Essay
    public string? TextAnswer { get; set; }
    
    // Скільки балів отримано (розраховується автоматично або вручну)
    public int PointsAwarded { get; set; }
    
    public bool IsMarkedManually { get; set; }
}