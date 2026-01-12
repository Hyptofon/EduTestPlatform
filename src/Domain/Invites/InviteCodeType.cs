namespace Domain.Invites;

public enum InviteCodeType
{
    Master = 0,      // Для Organization Admin
    Staff = 1,       // Для Teachers
    Student = 2      // Для Students
}