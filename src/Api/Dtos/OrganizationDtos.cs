namespace Api.Dtos;

public record CreateOrganizationDto(
    string Name, 
    string Type, 
    Guid? ParentId,
    bool GenerateInvite // Чи треба генерувати інвайт-код (наприклад, для кореня Університету)
);
