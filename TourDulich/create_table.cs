using System;
using System.Data.SqlClient;

class Program
{
    static void Main()
    {
        string connStr = "data source=QUANG\\QUANGDEVWEB;initial catalog=tourdulich;integrated security=True;trustservercertificate=True;";
        string sql = @"
            IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='LienHe' and xtype='U')
            BEGIN
                CREATE TABLE LienHe (
                    ID_LienHe INT IDENTITY(1,1) PRIMARY KEY,
                    HoTen NVARCHAR(100),
                    Email NVARCHAR(100),
                    TieuDe NVARCHAR(200),
                    NoiDung NVARCHAR(MAX),
                    NgayGui DATETIME,
                    TrangThai NVARCHAR(50) DEFAULT N'Chua x? lý'
                )
            END
        ";
        using (SqlConnection conn = new SqlConnection(connStr))
        {
            conn.Open();
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.ExecuteNonQuery();
                Console.WriteLine("Table LienHe created successfully!");
            }
        }
    }
}
