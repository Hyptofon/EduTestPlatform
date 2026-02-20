using Application.Common.Interfaces;
using Application.Common.Interfaces.Repositories;
using Application.Tests.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Application.Tests.Commands;

/// <summary>
/// Command для імпорту питань з Excel/CSV файлу в існуючий тест.
/// Підтримує формати: .xlsx, .csv
/// </summary>
public record ImportTestQuestionsCommand : IRequest<Either<TestException, ImportResult>>
{
    public required Guid TestId { get; init; }
    public required IFormFile File { get; init; }
    public bool ReplaceExisting { get; init; } = false;
}

public record ImportResult
{
    public int ImportedCount { get; init; }
    public int SkippedCount { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = [];
    public IReadOnlyList<string> Warnings { get; init; } = [];
}

public class ImportTestQuestionsCommandHandler(
    ITestRepository testRepository,
    IApplicationDbContext dbContext)
    : IRequestHandler<ImportTestQuestionsCommand, Either<TestException, ImportResult>>
{
    public async Task<Either<TestException, ImportResult>> Handle(
        ImportTestQuestionsCommand request,
        CancellationToken cancellationToken)
    {
        var testId = new TestId(request.TestId);
        var testOption = await testRepository.GetByIdAsync(testId, cancellationToken);

        if (testOption.IsNone)
        {
            return new TestNotFoundException(testId);
        }

        var test = testOption.IfNone(() => throw new InvalidOperationException());

        try
        {
            var extension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
            
            List<QuestionImportDto> importedQuestions;
            var errors = new List<string>();
            var warnings = new List<string>();

            using var stream = request.File.OpenReadStream();

            if (extension == ".csv")
            {
                importedQuestions = ParseCsv(stream, errors, warnings);
            }
            else if (extension == ".xlsx")
            {
                importedQuestions = ParseExcel(stream, errors, warnings);
            }
            else
            {
                return new InvalidImportFormatException(extension);
            }

            if (importedQuestions.Count == 0 && errors.Count > 0)
            {
                return new ImportFailedException(errors);
            }

            // Конвертуємо в доменні моделі
            var questions = importedQuestions.Select(ConvertToQuestion).ToList();

            // Отримуємо існуючий контент
            var existingContent = JsonSerializer.Deserialize<TestContentDto>(test.ContentJson) 
                ?? new TestContentDto();

            if (request.ReplaceExisting)
            {
                existingContent.Questions = questions;
            }
            else
            {
                existingContent.Questions ??= [];
                existingContent.Questions.AddRange(questions);
            }

            // Оновлюємо тест
            var newContentJson = JsonSerializer.Serialize(existingContent);
            test.UpdateContent(newContentJson);
            
            testRepository.Update(test);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new ImportResult
            {
                ImportedCount = importedQuestions.Count,
                SkippedCount = 0,
                Errors = errors,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            return new UnhandledTestException(testId, ex);
        }
    }

    private List<QuestionImportDto> ParseCsv(Stream stream, List<string> errors, List<string> warnings)
    {
        var questions = new List<QuestionImportDto>();
        using var reader = new StreamReader(stream);
        
        // Пропускаємо заголовок
        var header = reader.ReadLine();
        if (string.IsNullOrEmpty(header))
        {
            errors.Add("CSV file is empty");
            return questions;
        }

        var lineNumber = 1;
        while (!reader.EndOfStream)
        {
            lineNumber++;
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) continue;

            try
            {
                var parts = ParseCsvLine(line);
                if (parts.Length < 4)
                {
                    warnings.Add($"Line {lineNumber}: Not enough columns, skipped");
                    continue;
                }

                // Формат: Type, Question, Points, Answer1, IsCorrect1, Answer2, IsCorrect2, ...
                var question = new QuestionImportDto
                {
                    Type = parts[0].Trim(),
                    Text = parts[1].Trim(),
                    Points = int.TryParse(parts[2].Trim(), out var pts) ? pts : 1,
                    Answers = []
                };

                // Парсимо варіанти відповідей
                for (var i = 3; i < parts.Length - 1; i += 2)
                {
                    if (string.IsNullOrWhiteSpace(parts[i])) continue;
                    
                    var isCorrect = i + 1 < parts.Length && 
                        (parts[i + 1].Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ||
                         parts[i + 1].Trim() == "1");

                    question.Answers.Add(new AnswerImportDto
                    {
                        Text = parts[i].Trim(),
                        IsCorrect = isCorrect
                    });
                }

                questions.Add(question);
            }
            catch (Exception ex)
            {
                warnings.Add($"Line {lineNumber}: {ex.Message}");
            }
        }

        return questions;
    }

    private string[] ParseCsvLine(string line)
    {
        var result = new List<string>();
        var current = "";
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current);
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current);
        return result.ToArray();
    }

    private List<QuestionImportDto> ParseExcel(Stream stream, List<string> errors, List<string> warnings)
    {
        // Примітка: для повної підтримки Excel потрібен пакет ClosedXML
        // Поки що повертаємо помилку з інструкцією
        errors.Add("Excel import requires ClosedXML package. Please use CSV format or install ClosedXML.");
        return [];
    }

    private QuestionDto ConvertToQuestion(QuestionImportDto import)
    {
        return new QuestionDto
        {
            Id = Guid.NewGuid(),
            Type = import.Type,
            Text = import.Text,
            Points = import.Points,
            Answers = import.Answers.Select(a => new AnswerDto
            {
                Id = Guid.NewGuid(),
                Text = a.Text,
                IsCorrect = a.IsCorrect
            }).ToList()
        };
    }
}

// DTOs для імпорту
public record QuestionImportDto
{
    public string Type { get; init; } = "SingleChoice";
    public string Text { get; init; } = string.Empty;
    public int Points { get; init; } = 1;
    public List<AnswerImportDto> Answers { get; init; } = [];
}

public record AnswerImportDto
{
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}

// DTOs для контенту тесту
public record TestContentDto
{
    public List<QuestionDto> Questions { get; set; } = [];
}

public record QuestionDto
{
    public Guid Id { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public int Points { get; init; }
    public List<AnswerDto> Answers { get; init; } = [];
}

public record AnswerDto
{
    public Guid Id { get; init; }
    public string Text { get; init; } = string.Empty;
    public bool IsCorrect { get; init; }
}

// Exceptions
public class InvalidImportFormatException(string format)
    : TestException(TestId.Empty(), $"Unsupported import format: {format}. Supported formats: .csv, .xlsx");

public class ImportFailedException(IReadOnlyList<string> errors)
    : TestException(TestId.Empty(), $"Import failed: {string.Join("; ", errors)}");
