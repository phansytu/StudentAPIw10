using System.Data;
using Dapper;
using MediatR;
using StudentAPI.Application.Common.Interfaces;
using StudentAPI.Application.DTOs;

namespace StudentAPI.Application.Features.BaoCao.Queries.LayThongKeTongQuan;


public class LayThongKeTongQuanHandler : IRequestHandler<LayThongKeTongQuanQuery, ThongKeTongQuanDto>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ICacheService _cache;
    private const string ListPrefix = "thongke:tongquat:";
    private static readonly TimeSpan Ttl = TimeSpan.FromMinutes(15);
    public LayThongKeTongQuanHandler(ISqlConnectionFactory connectionFactory, ICacheService cache)
    {
        _cache = cache;
        _connectionFactory = connectionFactory;
    }

    public async Task<ThongKeTongQuanDto> Handle(
        LayThongKeTongQuanQuery request,
        CancellationToken cancellationToken)
    {
        var key = $"{ListPrefix}";
        var result = await _cache.GetOrCreateAsync(
            key,
            async () =>
            {
                using var connection = _connectionFactory.CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<ThongKeTongQuanDto>(
                        "sp_Dashboard_GetSummaryStats",
                        commandType: CommandType.StoredProcedure
    );
            },
            Ttl, cancellationToken

        )!;
        return result ?? new ThongKeTongQuanDto();
    }
}