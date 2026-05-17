IF COL_LENGTH('dbo.YeuCauHuy', 'TenNganHang') IS NULL
BEGIN
    ALTER TABLE dbo.YeuCauHuy ADD TenNganHang NVARCHAR(100) NULL;
END

IF COL_LENGTH('dbo.YeuCauHuy', 'SoTaiKhoanHoanTien') IS NULL
BEGIN
    ALTER TABLE dbo.YeuCauHuy ADD SoTaiKhoanHoanTien NVARCHAR(50) NULL;
END

IF COL_LENGTH('dbo.YeuCauHuy', 'TenChuTaiKhoan') IS NULL
BEGIN
    ALTER TABLE dbo.YeuCauHuy ADD TenChuTaiKhoan NVARCHAR(150) NULL;
END
