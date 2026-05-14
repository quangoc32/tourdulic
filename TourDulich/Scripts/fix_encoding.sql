UPDATE ChinhSachHuy SET MoTa = N'Hủy trước 15 ngày trở lên: hoàn 100%' WHERE SoNgayTuHuy = 15;
UPDATE ChinhSachHuy SET MoTa = N'Hủy trước 7–14 ngày: hoàn 70%' WHERE SoNgayTuHuy = 7;
UPDATE ChinhSachHuy SET MoTa = N'Hủy trước 3–6 ngày: hoàn 30%' WHERE SoNgayTuHuy = 3;
UPDATE ChinhSachHuy SET MoTa = N'Hủy trong vòng 3 ngày: không hoàn tiền' WHERE SoNgayTuHuy = 0;
