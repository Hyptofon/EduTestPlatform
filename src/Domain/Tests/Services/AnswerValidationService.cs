using System.Text.RegularExpressions;

namespace Domain.Tests.Services;

/// <summary>
/// Domain service для валідації відповідей студентів.
/// Забезпечує автоматичну перевірку для SingleChoice, MultipleChoice та ShortAnswer.
/// OpenEssay потребує ручної перевірки викладачем.
/// </summary>
public static class AnswerValidationService
{
    /// <summary>
    /// Валідує відповідь студента та повертає кількість зароблених балів.
    /// </summary>
    public static int ValidateAnswer(Question question, StudentAnswer answer)
    {
        return question.Type switch
        {
            QuestionType.SingleChoice => ValidateSingleChoice(question, answer),
            QuestionType.MultipleChoice => ValidateMultipleChoice(question, answer),
            QuestionType.ShortAnswer => ValidateShortAnswer(question, answer),
            QuestionType.OpenEssay => 0, // Потребує ручної перевірки викладачем
            _ => 0
        };
    }

    /// <summary>
    /// Перевіряє відповідь з одним правильним варіантом.
    /// </summary>
    private static int ValidateSingleChoice(Question question, StudentAnswer answer)
    {
        if (answer.SelectedAnswerIds == null || answer.SelectedAnswerIds.Count != 1)
            return 0;

        var selectedId = answer.SelectedAnswerIds.First();
        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);

        if (correctAnswer == null)
            return 0;

        return correctAnswer.Id == selectedId ? question.Points : 0;
    }

    /// <summary>
    /// Перевіряє відповідь з кількома правильними варіантами.
    /// Повні бали тільки якщо вибрані всі правильні і жодного неправильного.
    /// </summary>
    private static int ValidateMultipleChoice(Question question, StudentAnswer answer)
    {
        if (answer.SelectedAnswerIds == null || answer.SelectedAnswerIds.Count == 0)
            return 0;

        var correctAnswerIds = question.Answers
            .Where(a => a.IsCorrect)
            .Select(a => a.Id)
            .ToHashSet();

        var selectedIds = answer.SelectedAnswerIds.ToHashSet();

        // Повні бали тільки при точному співпадінні
        if (correctAnswerIds.SetEquals(selectedIds))
            return question.Points;

        // Часткові бали: підраховуємо правильно вибрані мінус помилково вибрані
        var correctSelected = selectedIds.Intersect(correctAnswerIds).Count();
        var incorrectSelected = selectedIds.Except(correctAnswerIds).Count();

        // Формула часткового оцінювання: (правильні - помилкові) / всього правильних * бали
        var partialScore = (correctSelected - incorrectSelected) / (double)correctAnswerIds.Count * question.Points;

        return Math.Max(0, (int)Math.Round(partialScore));
    }

    /// <summary>
    /// Перевіряє текстову відповідь з нормалізацією.
    /// Ігнорує регістр, зайві пробіли, trim на початку/кінці.
    /// </summary>
    private static int ValidateShortAnswer(Question question, StudentAnswer answer)
    {
        if (string.IsNullOrWhiteSpace(answer.TextAnswer))
            return 0;

        var correctAnswer = question.Answers.FirstOrDefault(a => a.IsCorrect);
        if (correctAnswer == null)
            return 0;

        var normalizedStudentAnswer = NormalizeText(answer.TextAnswer);
        var normalizedCorrectAnswer = NormalizeText(correctAnswer.Text);

        // Порівняння без урахування регістру
        return normalizedStudentAnswer.Equals(normalizedCorrectAnswer, StringComparison.OrdinalIgnoreCase)
            ? question.Points
            : 0;
    }

    /// <summary>
    /// Нормалізує текст: trim, видаляє зайві пробіли, приводить до нижнього регістру.
    /// </summary>
    private static string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Видаляємо пробіли на початку і в кінці
        var trimmed = text.Trim();

        // Замінюємо множинні пробіли на один
        var normalized = Regex.Replace(trimmed, @"\s+", " ");

        return normalized.ToLowerInvariant();
    }
}
