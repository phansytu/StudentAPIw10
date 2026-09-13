using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Application.Common.Exceptions;
using StudentAPI.Application.Common.Models;
using StudentAPI.Application.Features.SinhVien.Commands.CapNhatSinhVien;
using StudentAPI.Application.Features.SinhVien.Commands.TaoSinhVien;
using StudentAPI.Application.Features.SinhVien.Commands.XoaSinhVien;
using StudentAPI.Application.Features.SinhVien.Common;
using StudentAPI.Application.Features.SinhVien.Queries.LayDanhSachSinhVien;
using StudentAPI.Application.Features.SinhVien.Queries.LayDanhSachSinhVienPhanTrang;
using StudentAPI.Application.Features.SinhVien.Queries.LaySinhVienTheoId;

namespace StudentAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SinhVienController : ControllerBase
{
    private readonly ISender _mediator;

    public SinhVienController(ISender mediator) => _mediator = mediator;


    [HttpGet]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetList([FromQuery] LayDanhSachSinhVienQuery query)
    {
        var pageResult = await _mediator.Send(query);

        var response = ApiResponse<PageResponse<SinhVienDto>>.SuccessResult(
            pageResult,
            "Lấy danh sách sinh viên thành công"
        );
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,GiangVien,SinhVien")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new LaySinhVienTheoIdQuery(id));
        return Ok(ApiResponse<SinhVienDto>.SuccessResult(result, "Lấy thông tin sinh viên thành công"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TaoSinhVienCommand command)
    {
        var newId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResult(newId, "Tạo mới thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> Update(int id, [FromBody] CapNhatSinhVienCommand command)
    {
        if (id != command.id)
        {
            throw new BadRequestException("ID trên tham số URL không khớp với ID trong dữ liệu gửi lên.");
        }

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Cập nhật thông tin sinh viên thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new XoaSinhVienCommand(id));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Xóa sinh viên thành công"));
    }

    [HttpGet("paged-advanced")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> GetPagedAdvanced([FromQuery] LayDanhSachSinhVienPhanTrangQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}