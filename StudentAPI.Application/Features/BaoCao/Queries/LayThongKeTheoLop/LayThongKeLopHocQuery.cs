using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;

namespace StudentAPI.Application.Features.BaoCao.Queries.LayThongKeTheoLop;

public record LayThongKeLopHocQuery(int? BoMonId = null, int? LopHocId = null) : IQuery<IEnumerable<ThongKeLopHocDto>>;
