using Application.Tests.Commands;
using MediatR;
using System.Text;

namespace Application.Tests.Queries;

/// <summary>
/// Query для отримання шаблону імпорту питань.
/// </summary>
public record GetImportTemplateQuery : IRequest<ImportTemplateResult>
{
    public string Format { get; init; } = "csv";
}

public record ImportTemplateResult
{
    public byte[] Content { get; init; } = [];
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class GetImportTemplateQueryHandler
    : IRequestHandler<GetImportTemplateQuery, ImportTemplateResult>
{
    public Task<ImportTemplateResult> Handle(
        GetImportTemplateQuery request,
        CancellationToken cancellationToken)
    {
        var csv = GenerateTemplateCsv();
        var content = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv)).ToArray();

        return Task.FromResult(new ImportTemplateResult
        {
            Content = content,
            FileName = "question_import_template.csv",
            ContentType = "text/csv"
        });
    }

    private string GenerateTemplateCsv()
    {
        var sb = new StringBuilder();

        // Легенда (коментарі)
        sb.AppendLine("# EduTestPlatform - Question Import Template");
        sb.AppendLine("# ");
        sb.AppendLine("# Формат колонок:");
        sb.AppendLine("# Type - тип питання: SingleChoice, MultipleChoice, ShortAnswer, OpenEssay");
        sb.AppendLine("# Question - текст питання");
        sb.AppendLine("# Points - кількість балів за правильну відповідь");
        sb.AppendLine("# Answer1..4 - варіанти відповідей");
        sb.AppendLine("# IsCorrect1..4 - чи правильна відповідь (true/false або 1/0)");
        sb.AppendLine("# ");
        sb.AppendLine("# Для ShortAnswer: Answer1 = правильна відповідь, IsCorrect1 = true");
        sb.AppendLine("# Для OpenEssay: відповіді не потрібні (перевірка вручну)");
        sb.AppendLine("# ");

        // Заголовок
        sb.AppendLine("Type,Question,Points,Answer1,IsCorrect1,Answer2,IsCorrect2,Answer3,IsCorrect3,Answer4,IsCorrect4");

        // Приклади
        sb.AppendLine("SingleChoice,\"Яка столиця України?\",1,\"Київ\",true,\"Львів\",false,\"Одеса\",false,\"Харків\",false");
        sb.AppendLine("MultipleChoice,\"Які числа є простими?\",2,\"2\",true,\"3\",true,\"4\",false,\"5\",true");
        sb.AppendLine("ShortAnswer,\"Скільки буде 2+2?\",1,\"4\",true,,,,,");
        sb.AppendLine("OpenEssay,\"Опишіть свою улюблену книгу\",5,,,,,,,");

        return sb.ToString();
    }
}
