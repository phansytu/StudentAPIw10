# Hệ thống Quản lý Sinh viên - API Documentation

Hệ thống backend quản lý Sinh viên, Lớp học, Bộ môn với xác thực JWT (Access Token + Refresh Token) và phân quyền theo Role (Admin, GiangVien, SinhVien).

## Mục lục

- [Danh sách API Endpoints](#danh-sách-api-endpoints)
  - [1. Authentication](#1-authentication--apiauth)
  - [2. Quản lý Người dùng](#2-quản-lý-người-dùng--apinguoidung)
  - [3. Lớp học](#3-lớp-học--apilophoc)
  - [4. Sinh viên](#4-sinh-viên--apisinhvien)
  - [5. Bộ môn](#5-bộ-môn--apibomon)
  - [6. Báo cáo & Dashboard](#6-báo-cáo--dashboard--apibaocao)
- [Database Objects](#-database-objects)
- [Test Plan](#-test-plan--checklist-swagger--postman)

---
- dữ liệu test
 - role Admin
   email: admin@studentapi.com
   pass: Admin@123456
 - role: SinhVien
   email: sinhvien@studentapi.com
   pass: sinhvien
 - role: GiangVien
   email: giangvien@studentapi.com
   pass: giangvien
## Danh sách API Endpoints

### 1. Authentication — `/api/Auth`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| POST | `/api/Auth/login` | Đăng nhập hệ thống, lấy Access & Refresh Token | Công khai (AllowAnonymous) |
| POST | `/api/Auth/refresh-token` | Đổi Access Token mới bằng Refresh Token | Công khai (AllowAnonymous) |

### 2. Quản lý Người dùng — `/api/NguoiDung`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| POST | `/api/NguoiDung` | Tạo tài khoản người dùng mới (Admin, GiangVien, SinhVien) | Admin |

### 3. Lớp học — `/api/LopHoc`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| GET | `/api/LopHoc` | Lấy danh sách lớp (phân trang) | Admin, GiangVien, SinhVien |
| GET | `/api/LopHoc/{id}` | Lấy chi tiết 1 lớp theo ID | Admin, GiangVien, SinhVien |
| POST | `/api/LopHoc` | Tạo lớp mới | Admin |
| PUT | `/api/LopHoc/{id}` | Cập nhật lớp | Admin, GiangVien |
| DELETE | `/api/LopHoc/{id}` | Xoá lớp (chặn nếu lớp còn sinh viên) | Admin |

### 4. Sinh viên — `/api/SinhVien`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| GET | `/api/SinhVien` | Lấy danh sách (tìm kiếm, lọc, sắp xếp, phân trang) | Admin, GiangVien |
| GET | `/api/SinhVien/paged-advanced` | Lấy danh sách nâng cao qua Stored Procedure | Admin, GiangVien |
| GET | `/api/SinhVien/{id}` | Lấy chi tiết 1 sinh viên | Admin, GiangVien, SinhVien |
| POST | `/api/SinhVien` | Tạo sinh viên mới | Admin |
| PUT | `/api/SinhVien/{id}` | Cập nhật sinh viên | Admin, GiangVien |
| DELETE | `/api/SinhVien/{id}` | Xoá sinh viên | Admin |

### 5. Bộ môn — `/api/BoMon`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| GET | `/api/BoMon` | Lấy danh sách bộ môn (phân trang) | Admin, GiangVien, SinhVien |
| GET | `/api/BoMon/{id}` | Lấy chi tiết 1 bộ môn | Admin, GiangVien, SinhVien |
| POST | `/api/BoMon` | Tạo bộ môn mới | Admin |
| PUT | `/api/BoMon/{id}` | Cập nhật bộ môn | Admin |
| DELETE | `/api/BoMon/{id}` | Xoá bộ môn | Admin |

### 6. Báo cáo & Dashboard — `/api/BaoCao`

| Method | Route | Mô tả | Quyền (Role) |
|--------|-------|-------|--------------|
| GET | `/api/BaoCao/summary` | Thống kê tổng quan (tổng SV, tổng lớp, tổng bộ môn, điểm TB) | Admin, GiangVien |
| GET | `/api/BaoCao/thong-ke-theo-lop` | Thống kê theo lớp (sĩ số, tỉ lệ nam/nữ, điểm TB/max/min) | Admin, GiangVien |

---

## 🗄 Database Objects

### Tables & Entities

- **NguoiDung**: Quản lý tài khoản, mật khẩu băm, Role (Admin, GiangVien, SinhVien).
- **RefreshToken**: Lưu chuỗi Refresh Token, ngày hết hạn và trạng thái thu hồi (`IsRevoked`).
- **SinhVien, LopHoc, BoMon**: Các bảng nghiệp vụ cốt lõi.

### Stored Procedures & Views

- `sp_SinhVien_GetPagedAdvanced`: Lọc và phân trang sinh viên nâng cao theo từ khoá, lớp, bộ môn, khoảng điểm.
- `vw_BaoCao_ChiTietSinhVien`: View báo cáo chi tiết sinh viên (xếp loại, lớp, bộ môn).
- `vw_BaoCao_ThongKeTheoLop`: View báo cáo thống kê sĩ số và điểm số theo lớp.

### Indexes

- `IX_LopHoc_boMonId`: Tối ưu JOIN LopHoc và BoMon.
- `IX_SinhVien_lopHocId`: Tối ưu JOIN SinhVien và LopHoc.
- `IX_NguoiDung_Email` (Unique): Tối ưu truy vấn kiểm tra đăng nhập/email.

---

## 🧪 Test Plan — Checklist (Swagger / Postman)

### 1. Authentication & Security Test Cases

| Case | Request / Thao tác | Mong đợi | Thực tế |
|------|--------------------|----------|:-------:|
| TC-AUTH-01 | `POST /api/Auth/login` đúng email/password Admin | 200 OK, trả về `accessToken`, `refreshToken` và `accessTokenExpiresAt` | ☐ |
| TC-AUTH-02 | `POST /api/Auth/login` sai mật khẩu | 401 Unauthorized / 400 Bad Request | ☐ |
| TC-AUTH-03 | Gọi API `[Authorize]` không gửi Token Header | 401 Unauthorized | ☐ |
| TC-AUTH-04 | Dùng tài khoản SinhVien gọi `DELETE /api/SinhVien/1` | 403 Forbidden (Không đủ quyền Admin) | ☐ |
| TC-AUTH-05 | `POST /api/Auth/refresh-token` với Refresh Token hợp lệ | 200 OK, trả về cặp Token mới, Refresh Token cũ bị đánh dấu `IsRevoked = true` | ☐ |

### 2. Sinh viên — Create / Update / Delete / Get

| Case | Input | Mong đợi | Thực tế |
|------|-------|----------|:-------:|
| TC-SV-01 | Email đúng định dạng, đầy đủ field hợp lệ | 201 Created / 200 OK, trả về ID mới | ☐ |
| TC-SV-02 | Email sai định dạng (vd "abc") | 400 Bad Request, ProblemDetails chứa lỗi Email | ☐ |
| TC-SV-03 | Email đã tồn tại trong hệ thống | 400 Bad Request, thông báo trùng email | ☐ |
| TC-SV-04 | HoTen rỗng | 400 Bad Request, lỗi field HoTen | ☐ |
| TC-SV-05 | DiemTB = 15 (ngoài khoảng 0-10) | 400 Bad Request, lỗi field DiemTB | ☐ |
| TC-SV-06 | `PUT /api/SinhVien/{id}` cập nhật email của chính nó | 200 OK (Không báo lỗi trùng email) | ☐ |
| TC-SV-07 | `PUT /api/SinhVien/{id}` cập nhật email đã bị dùng bởi SV khác | 400 Bad Request | ☐ |
| TC-SV-08 | `DELETE /api/SinhVien/{id}` thành công | 200 OK / 204 No Content | ☐ |

### 3. Lớp học & Exception Handling

| Case | Input | Mong đợi | Thực tế |
|------|-------|----------|:-------:|
| TC-LH-01 | `DELETE /api/LopHoc/{id}` khi lớp đang có sinh viên | 400 Bad Request — Chặn xóa lớp có sinh viên | ☐ |
| TC-LH-02 | `DELETE /api/LopHoc/{id}` khi lớp không có sinh viên | 200 OK / 204 No Content | ☐ |
| TC-EX-01 | Lỗi Validate FluentValidation | 400 Bad Request, body cấu trúc `ValidationProblemDetails` | ☐ |
| TC-EX-02 | Lỗi `NotFoundException` | 404 Not Found, `ProblemDetails.Title = "Không tìm thấy tài nguyên"` | ☐ |
