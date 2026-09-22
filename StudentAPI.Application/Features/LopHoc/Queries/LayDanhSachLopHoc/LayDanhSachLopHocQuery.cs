using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.Common.Models;
using StudentAPI.Application.Features.LopHoc.Common;

namespace StudentAPI.Application.Features.LopHoc.Queries.LayDanhSachLopHoc;

public record LayDanhSachLopHocQuery(
    int PageIndex = 1,
    int PageSize = 10,
    string? SearchTerm = null,
    int? BoMonId = null
) : IQuery<PageResponse<LopHocDto>>;