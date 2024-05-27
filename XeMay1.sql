create database XeMay1;
CREATE TABLE DanhSachXeMay1 (
    So_Khung VARCHAR(50) PRIMARY KEY,
	So_May VARCHAR(50),
	Mau VARCHAR(50),
	Dung_Tich_Xi_Lanh INT,
    Hang_Xe VARCHAR(255),
    Ten_Xe VARCHAR(255),
	Anh VARBINARY(MAX)
);



USE XeMay1;

INSERT INTO DanhSachXeMay1 (So_Khung, So_May, Mau, Dung_Tich_Xi_Lanh, Hang_Xe, Ten_Xe, Anh)
VALUES 
    
    ('WINX987654321', '654321', 'Xanh', 160, 'Honda', 'Winner X', (SELECT * FROM OPENROWSET(BULK N'E:\C#HocLai\xe2.jpg', SINGLE_BLOB) AS Image)),
    ('SIR987654321', '987654', 'Trắng', 115, 'Yamaha', 'Sirius', (SELECT * FROM OPENROWSET(BULK N'E:\C#HocLai\xe3.jpg', SINGLE_BLOB) AS Image)),
    ('XYZ123456789', '987654', 'Đen', 125, 'Honda', 'Future', (SELECT * FROM OPENROWSET(BULK N'E:\C#HocLai\xe4.jpg', SINGLE_BLOB) AS Image)),
    ('ABC987654321', '654321', 'Vàng', 135, 'Yamaha', 'Exciter 135', (SELECT * FROM OPENROWSET(BULK N'E:\C#HocLai\xe5.jpg', SINGLE_BLOB) AS Image));


INSERT INTO DanhSachXeMay1 (So_Khung, So_May, Mau, Dung_Tich_Xi_Lanh, Hang_Xe, Ten_Xe, Anh)
VALUES 
    ('EXC123456789', '123456', 'Đỏ', 150, 'Yamaha', 'Exciter 150', (SELECT * FROM OPENROWSET(BULK N'E:\C#HocLai\xe1.jpg', SINGLE_BLOB) AS Image));

	select * from DanhSachXeMay1