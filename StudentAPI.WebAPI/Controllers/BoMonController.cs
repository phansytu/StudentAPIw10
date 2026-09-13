using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StudentAPI.Application.Common.Exceptions;
using StudentAPI.Application.Common.Models;
using StudentAPI.Application.Features.BoMon.Commands.CapNhatBoMon;
using StudentAPI.Application.Features.BoMon.Commands.TaoBoMon;
using StudentAPI.Application.Features.BoMon.Commands.XoaBoMon;
using StudentAPI.Application.Features.BoMon.Common;
using StudentAPI.Application.Features.BoMon.Queries.LayBoMonTheoId;
using StudentAPI.Application.Features.BoMon.Queries.LayDanhSachBoMon;

namespace StudentAPI.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BoMonController : ControllerBase
{
    private readonly ISender _mediator;

    public BoMonController(ISender mediator) => _mediator = mediator;

    [HttpGet]
    [Authorize(Roles = "Admin,GiangVien,SinhVien")]
    public async Task<IActionResult> GetList([FromQuery] LayDanhSachBoMonQuery query)
    {
        var pageResult = await _mediator.Send(query);
        var response = ApiResponse<PageResponse<BoMonDto>>.SuccessResult(
            pageResult,
            "Lấy danh sách bộ môn thành công"
        );
        return Ok(response);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,GiangVien,SinhVien")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _mediator.Send(new LayBoMonTheoIdQuery(id));
        return Ok(ApiResponse<BoMonDto>.SuccessResult(result, "Lấy thông tin bộ môn thành công"));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] TaoBoMonCommand command)
    {
        var newId = await _mediator.Send(command);
        return Ok(ApiResponse<int>.SuccessResult(newId, "Tạo mới bộ môn thành công"));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] CapNhatBoMonCommand command)
    {
        if (id != command.Id)
        {
            throw new BadRequestException("ID trên tham số URL không khớp với ID trong dữ liệu gửi lên.");
        }

        var result = await _mediator.Send(command);
        return Ok(ApiResponse<bool>.SuccessResult(result, "Cập nhật thông tin bộ môn thành công"));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new XoaBoMonCommand(id));
        return Ok(ApiResponse<bool>.SuccessResult(result, "Xóa bộ môn thành công"));
    }
}