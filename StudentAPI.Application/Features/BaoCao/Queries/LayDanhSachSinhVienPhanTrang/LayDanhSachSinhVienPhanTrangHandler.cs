using System.Data;
using Dapper;
using MediatR;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.Common.Models;
using StudentAPI.Application.DTOs;

namespace StudentAPI.Application.Features.SinhVien.Queries.LayDanhSachSinhVienPhanTrang;

public class LayDanhSachSinhVienPhanTrangHandler
    : IRequestHandler<LayDanhSachSinhVienPhanTrangQuery, PageResponse<SinhVienPagedDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICacheService _cache;
    private const string ListPrefix = "sinhvien:diemso:lopId:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);


    public LayDanhSachSinhVienPhanTrangHandler(ISqlConnectionFactory connectionFactory, ICacheService cache)
    {
        _connectionFactory = connectionFactory;
        _cache = cache;
    }

    public async Task<PageResponse<SinhVienPagedDto>> Handle(
        LayDanhSachSinhVienPhanTrangQuery request,
        CancellationToken cancellationToken)
    {
        var key = $"{ListPrefix}{request.LopHocId}_bmId_{request.BoMonId}_{request.Keyword}_{request.MaxDiem}_{request.MinDiem}_{request.PageIndex}_{request.PageSize}";

        var result = await _cache.GetOrCreateAsync(
            key,
            async () =>
            {


                using var connection = _connectionFactory.CreateConnection();

                var parameters = new DynamicParameters();
                parameters.Add("@PageIndex", request.PageIndex, DbType.Int32);
                parameters.Add("@PageSize", request.PageSize, DbType.Int32);
                parameters.Add("@Keyword", request.Keyword, DbType.String);
                parameters.Add("@LopHocId", request.LopHocId, DbType.Int32);
                parameters.Add("@BoMonId", request.BoMonId, DbType.Int32);
                parameters.Add("@MinDiem", request.MinDiem, DbType.Double);
                parameters.Add("@MaxDiem", request.MaxDiem, DbType.Double);

                parameters.Add("@TotalRecords", dbType: DbType.Int32, direction: ParameterDirection.Output);
                var items = await connection.QueryAsync<SinhVienPagedDto>(
                    "sp_SinhVien_GetPagedAdvanced",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                int totalRecords = parameters.Get<int>("@TotalRecords");

                return new PageResponse<SinhVienPagedDto>(
                    items,
                    totalRecords,
                    request.PageIndex,
                    request.PageSize
                );
            },
            Ttl, cancellationToken
        )!;
        return result!;

    }
}