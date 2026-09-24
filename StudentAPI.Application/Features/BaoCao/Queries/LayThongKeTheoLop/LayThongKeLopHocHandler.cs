using Dapper;
using MediatR;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;

namespace StudentAPI.Application.Features.BaoCao.Queries.LayThongKeTheoLop;

public class LayThongKeLopHocHandler : IRequestHandler<LayThongKeLopHocQuery, IEnumerable<ThongKeLopHocDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICacheService _cache;

    private static string ListPrefix = "lophoc:thongke:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);

    public LayThongKeLopHocHandler(ISqlConnectionFactory connectionFactory, ICacheService cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<IEnumerable<ThongKeLopHocDto>> Handle(
        LayThongKeLopHocQuery request,
        CancellationToken cancellationToken)
    {
        var key = $"{ListPrefix}{request.LopHocId}_{request.BoMonId}";
        var result = await _cache.GetOrCreateAsync(
            key,
            async () =>
            {
                using var connection = _connectionFactory.CreateConnection();

                var sql = @"
            SELECT 
                LopHocId,
                BoMonId,
                MaLop,
                TenLop,
                ChuyenNganh,
                TenBoMon,
                TongSoSinhVien,
                SoNam,
                SoNu,
                DiemTrungBinhLop,
                DiemCaoNhat,
                DiemThapNhat
            FROM dbo.vw_BaoCao_ThongKeTheoLop
            WHERE (@BoMonId IS NULL OR BoMonId = @BoMonId)";

                return await connection.QueryAsync<ThongKeLopHocDto>(
                   sql,
                   new { BoMonId = request.BoMonId }
               );
            }, Ttl, cancellationToken
        )!;



        return result!;
    }
}