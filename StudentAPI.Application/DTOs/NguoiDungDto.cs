namespace StudentAPI.Application.DTOs;

public record NguoiDungDto(
    int Id,
    string Email,
    string HoTen,
    string Role,
    bool IsActive,
    DateTime CreatedAt
);