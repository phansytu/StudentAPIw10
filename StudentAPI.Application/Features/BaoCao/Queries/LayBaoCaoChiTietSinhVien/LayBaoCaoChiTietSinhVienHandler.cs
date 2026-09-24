using Dapper;
using MediatR;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;

namespace StudentAPI.Application.Features.BaoCao.Queries.LayBaoCaoChiTietSinhVien;

public class LayBaoCaoChiTietSinhVienHandler
    : IRequestHandler<LayBaoCaoChiTietSinhVienQuery, IEnumerable<BaoCaoChiTietSinhVienDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICacheService _cache;

    private static string ListPrefix = "baocao:chitiet:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);


    public LayBaoCaoChiTietSinhVienHandler(ISqlConnectionFactory connectionFactory, ICacheService cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<IEnumerable<BaoCaoChiTietSinhVienDto>> Handle(
        LayBaoCaoChiTietSinhVienQuery request,
        CancellationToken cancellationToken)
    {
        var key = $"{ListPrefix}sv_{request.SinhVienId}_lop_{request.LopHocId}_bm_{request.BoMonId}_kw_{request.Keyword?.Trim().ToLower()}";
        var result = await _cache.GetOrCreateAsync(
            key,
            factory: async () =>
            {

                using var connection = _connectionFactory.CreateConnection();

                var sql = @"
            SELECT 
                SinhVienId,
                MaSinhVien,
                HoTen,
                GioiTinh,
                GioiTinhText,
                NgaySinh,
                Tuoi,
                Email,
                DiemTB,
                XepLoai,
                LopHocId,
                MaLop,
                TenLop,
                ChuyenNganh,
                BoMonId,
                TenBoMon
            FROM dbo.vw_BaoCao_ChiTietSinhVien
            WHERE (@SinhVienId IS NULL OR SinhVienId = @SinhVienId)
              AND (@LopHocId IS NULL OR LopHocId = @LopHocId)
              AND (@BoMonId IS NULL OR BoMonId = @BoMonId)
              AND (@Keyword IS NULL OR HoTen LIKE N'%' + @Keyword + '%' OR MaSinhVien LIKE '%' + @Keyword + '%')";

                return await connection.QueryAsync<BaoCaoChiTietSinhVienDto>(
                    sql,
                    new
                    {
                        request.SinhVienId,
                        request.LopHocId,
                        request.BoMonId,
                        request.Keyword
                    }

                );
            },
            Ttl, cancellationToken
        )!;

        return result ?? Enumerable.Empty<BaoCaoChiTietSinhVienDto>();
    }
}