using MediatR;
using Microsoft.AspNetCore.Authorization; // 👈 Thêm namespace này
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Application.Common.Exceptions;
using StudentAPI.Application.Common.Models;
using StudentAPI.Application.Features.LopHoc.Commands.CapNhatLopHoc;
using StudentAPI.Application.Features.LopHoc.Commands.TaoLopHoc;
using StudentAPI.Application.Features.LopHoc.Commands.XoaLopHoc;
using StudentAPI.Application.Features.LopHoc.Common;
using StudentAPI.Application.Features.LopHoc.Queries.LayDanhSachLopHoc;
using StudentAPI.Application.Features.LopHoc.Queries.LayLopHocTheoId;

namespace StudentAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LopHocController : ControllerBase
{
    private readonly ISender _mediator;

    public LopHocController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Admin,GiangVien,SinhVien")]
    public async Task<IActionResult> GetList([FromQuery] LayDanhSachLopHocQuery query)
    {
        var pageResult = await _mediator.Send(query);
        var response = ApiResponse<PageResponse<LopHocDto>>.SuccessResult(
            pageResult,
            "Lấy danh sách lớp học thành công"
        );
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,GiangVien,SinhVien")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new LayLopHocTheoIdQuery(id));
        return Ok(ApiResponse<LopHocDto>.SuccessResult(result, "Lấy thông tin lớp học thành công"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TaoLopHocCommand command)
    {
        var newId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResult(newId, "Tạo mới lớp học thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,GiangVien")]
    public async Task<IActionResult> Update(int id, [FromBody] CapNhatLopHocCommand command)
    {
        if (id != command.id)
        {
            throw new BadRequestException("ID trên tham số URL không khớp với ID trong dữ liệu gửi lên.");
        }

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Cập nhật thông tin lớp học thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new XoaLopHocCommand(id));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Xóa lớp học thành công"));
    }
}