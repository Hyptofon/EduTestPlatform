using Application.Common.Interfaces.Repositories;
using Application.Tests.Commands;
using Application.Tests.Exceptions;
using Domain.Tests;
using LanguageExt;
using MediatR;
using System.Text;
using System.Text.Json;

namespace Application.Tests.Queries;

/// <summary>
/// Query для експорту тесту у формат CSV.
/// </summary>
public record ExportTestQuery : IRequest<Either<TestException, ExportResult>>
{
    public required Guid TestId { get; init; }
    public string Format { get; init; } = "csv"; // csv або json
}

public record ExportResult
{
    public byte[] Content { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class ExportTestQueryHandler(
    ITestRepository testRepository)
    : IRequestHandler<ExportTestQuery, Either<TestException, ExportResult>>
{
    public async Task<Either<TestException, ExportResult>> Handle(
        ExportTestQuery request,
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
            var content = JsonSerializer.Deserialize<TestContentDto>(test.ContentJson);
            if (content?.Questions == null || content.Questions.Count == 0)
            {
                return new TestExportEmptyException(testId);
            }

            byte[] fileContent;
            string fileName;
            string contentType;

            if (request.Format.Equals("json", StringComparison.OrdinalIgnoreCase))
            {
                // Експорт у JSON
                var exportData = new TestExportDto
                {
                    Title = test.Title,
                    Description = test.Description,
                    Questions = content.Questions
                };
                
                fileContent = JsonSerializer.SerializeToUtf8Bytes(exportData, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                fileName = $"{SanitizeFileName(test.Title)}_export.json";
                contentType = "application/json";
            }
            else
            {
                // Експорт у CSV
                var csv = GenerateCsv(content.Questions);
                fileContent = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();
                fileName = $"{SanitizeFileName(test.Title)}_export.csv";
                contentType = "text/csv";
            }

            return new ExportResult
            {
                Content = fileContent,
                FileName = fileName,
                ContentType = contentType
            };
        }
        catch (Exception ex)
        {
            return new UnhandledTestException(testId, ex);
        }
    }

    private string GenerateCsv(List<QuestionDto> questions)
    {
        var sb = new StringBuilder();
        
        // Заголовок
        sb.AppendLine("Type,Question,Points,Answer1,IsCorrect1,Answer2,IsCorrect2,Answer3,IsCorrect3,Answer4,IsCorrect4");

        foreach (var question in questions)
        {
            var row = new List<string>
            {
                EscapeCsv(question.Type),
                EscapeCsv(question.Text),
                question.Points.ToString()
            };

            // Додаємо до 4 варіантів відповідей
            for (var i = 0; i < 4; i++)
            {
                if (i < question.Answers.Count)
                {
                    row.Add(EscapeCsv(question.Answers[i].Text));
                    row.Add(question.Answers[i].IsCorrect ? "true" : "false");
                }
                else
                {
                    row.Add("");
                    row.Add("");
                }
            }

            sb.AppendLine(string.Join(",", row));
        }

        return sb.ToString();
    }

    private string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }
        return value;
    }

    private string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries))
            .Replace(" ", "_")
            .ToLowerInvariant();
    }
}

public record TestExportDto
{
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public List<QuestionDto> Questions { get; init; } = [];
}

public class TestExportEmptyException(TestId testId)
    : TestException(testId, "Test has no questions to export");
