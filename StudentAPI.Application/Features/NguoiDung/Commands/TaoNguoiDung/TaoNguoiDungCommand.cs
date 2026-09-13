using StudentAPI.Application.Common.Interfaces;

namespace StudentAPI.Application.Features.NguoiDung.Commands.TaoNguoiDung;

public record TaoNguoiDungCommand(
    string Email,
    string Password,
    string HoTen,
    string Role
) : ICommand<int>;