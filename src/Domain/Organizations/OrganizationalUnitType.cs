namespace Domain.Organizations;

public enum OrganizationalUnitType
{
    Root = 0,       // Університет або Школа
    Faculty = 1,    // Факультет
    Department = 2, // Кафедра або Клас (в школі)
    Subject = 3,    // Предмет (Кінцева точка)
    Group = 4       // Академічна група (опціонально)
}