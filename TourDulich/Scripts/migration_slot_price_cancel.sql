-- ============================================================
-- MIGRATION: Slot / Giá mùa / Hủy tour
-- Chạy script này trên database TourDulich
-- ============================================================

-- 1. Bảng LichKhoiHanh (slot từng ngày của tour)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LichKhoiHanh' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[LichKhoiHanh] (
        [ID_LichKhoiHanh]  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ID_Tour]          INT NOT NULL,
        [NgayKhoiHanh]     DATE NOT NULL,
        [SoLuongToiDa]     INT NOT NULL DEFAULT 20,
        [SoLuongDaDat]     INT NOT NULL DEFAULT 0,
        [TrangThai]        NVARCHAR(20) NOT NULL DEFAULT N'Mở',
        [GhiChu]           NVARCHAR(200) NULL,
        CONSTRAINT [FK_LichKhoiHanh_Tour] FOREIGN KEY ([ID_Tour]) REFERENCES [dbo].[Tour]([ID_Tour]),
        CONSTRAINT [UQ_LichKhoiHanh] UNIQUE ([ID_Tour], [NgayKhoiHanh])
    );
    PRINT N'✅ Tạo bảng LichKhoiHanh thành công';
END
ELSE
    PRINT N'⚠️  Bảng LichKhoiHanh đã tồn tại, bỏ qua';

-- 2. Bảng MuaGia (giá mùa toàn site)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='MuaGia' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[MuaGia] (
        [ID_MuaGia]     INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [TenMua]        NVARCHAR(100) NOT NULL,
        [NgayBatDau]    DATE NOT NULL,
        [NgayKetThuc]   DATE NOT NULL,
        [HeSoGia]       DECIMAL(4,2) NOT NULL DEFAULT 1.00,
        [MoTa]          NVARCHAR(300) NULL,
        [IsActive]      BIT NOT NULL DEFAULT 1,
        CONSTRAINT [CK_MuaGia_HeSo] CHECK ([HeSoGia] > 0),
        CONSTRAINT [CK_MuaGia_NgayHopLe] CHECK ([NgayKetThuc] >= [NgayBatDau])
    );
    -- Dữ liệu mẫu
    INSERT INTO [dbo].[MuaGia] ([TenMua],[NgayBatDau],[NgayKetThuc],[HeSoGia],[MoTa]) VALUES
        (N'Hè 2025',            '2025-06-01','2025-08-31', 1.25, N'Cao điểm mùa hè, tăng 25%'),
        (N'Tết Nguyên Đán 2026','2026-01-25','2026-02-05', 1.40, N'Cao điểm Tết, tăng 40%'),
        (N'Ưu đãi cuối năm 2025','2025-11-01','2025-11-30',0.85, N'Khuyến mãi tháng 11, giảm 15%');
    PRINT N'✅ Tạo bảng MuaGia thành công';
END
ELSE
    PRINT N'⚠️  Bảng MuaGia đã tồn tại, bỏ qua';

-- 3. Bảng ChinhSachHuy (% hoàn tiền theo số ngày trước khởi hành)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ChinhSachHuy' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[ChinhSachHuy] (
        [ID_ChinhSach]  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [SoNgayTuHuy]  INT NOT NULL,
        [PhanTramHoan]  INT NOT NULL DEFAULT 0,
        [MoTa]          NVARCHAR(200) NULL,
        CONSTRAINT [CK_ChinhSach_PhanTram] CHECK ([PhanTramHoan] BETWEEN 0 AND 100)
    );
    -- Chính sách mặc định
    INSERT INTO [dbo].[ChinhSachHuy] ([SoNgayTuHuy],[PhanTramHoan],[MoTa]) VALUES
        (15, 100, N'Hủy trước 15 ngày trở lên: hoàn 100%'),
        (7,  70,  N'Hủy trước 7–14 ngày: hoàn 70%'),
        (3,  30,  N'Hủy trước 3–6 ngày: hoàn 30%'),
        (0,  0,   N'Hủy trong vòng 3 ngày: không hoàn tiền');
    PRINT N'✅ Tạo bảng ChinhSachHuy thành công';
END
ELSE
    PRINT N'⚠️  Bảng ChinhSachHuy đã tồn tại, bỏ qua';

-- 4. Bảng YeuCauHuy (yêu cầu hủy do user gửi)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='YeuCauHuy' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[YeuCauHuy] (
        [ID_YeuCauHuy]  INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [ID_DatTour]    INT NOT NULL,
        [NgayGui]       DATETIME NOT NULL DEFAULT GETDATE(),
        [LyDo]          NVARCHAR(500) NULL,
        [TrangThai]     NVARCHAR(30) NOT NULL DEFAULT N'Chờ xử lý',
        [PhanTramHoan]  INT NULL,
        [TienHoan]      DECIMAL(18,2) NULL,
        [NgayXuLy]      DATETIME NULL,
        [GhiChuAdmin]   NVARCHAR(500) NULL,
        CONSTRAINT [FK_YeuCauHuy_DatTour] FOREIGN KEY ([ID_DatTour]) REFERENCES [dbo].[DatTour]([ID_DatTour])
    );
    PRINT N'✅ Tạo bảng YeuCauHuy thành công';
END
ELSE
    PRINT N'⚠️  Bảng YeuCauHuy đã tồn tại, bỏ qua';

-- 5. Thêm cột CoYeuCauHuy vào DatTour
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME='DatTour' AND COLUMN_NAME='CoYeuCauHuy'
)
BEGIN
    ALTER TABLE [dbo].[DatTour] ADD [CoYeuCauHuy] BIT NOT NULL DEFAULT 0;
    PRINT N'✅ Thêm cột CoYeuCauHuy vào DatTour thành công';
END
ELSE
    PRINT N'⚠️  Cột CoYeuCauHuy đã tồn tại, bỏ qua';

PRINT N'';
PRINT N'🎉 Migration hoàn tất!';
