namespace StudentAPI.Application.Features.LopHoc.Common;

public class LopHocDto
{
    public int Id { get; set; }
    public string MaLop { get; set; } = string.Empty;
    public string TenLop { get; set; } = string.Empty;
    public string ChuyenNganh { get; set; } = string.Empty;
    public int BoMonId { get; set; }

}