using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace XeMay
{
    public partial class Form1 : Form
    {
        SqlConnection sqlConnection;
        string conn = "Data Source=DESKTOP-M84T3VE;Initial Catalog=XeMay1;User ID=sa;Password=123";

        public Form1()
        {
            InitializeComponent();
            LoadTable();
            this.dgvDanhSachXeMay.SelectionChanged += new EventHandler(dgvDanhSachXeMay_SelectionChanged);
        }

        private void LoadTable()
        {
            using (SqlConnection con = new SqlConnection(conn))
            {
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT So_Khung,So_May,Mau,Dung_Tich_Xi_Lanh ,Hang_Xe ,Ten_Xe, CAST('Xem ảnh' AS NVARCHAR(50)) AS Anh FROM dbo.DanhSachXeMay1", con))
                    {
                        SqlDataAdapter adt = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();

                        adt.Fill(dt);

                        dgvDanhSachXeMay.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void dgvDanhSachXeMay_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDanhSachXeMay.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvDanhSachXeMay.SelectedRows[0];
                string SoKhung = selectedRow.Cells["So_Khung"].Value.ToString();


                txtSoKhung.Text = selectedRow.Cells["So_Khung"].Value.ToString();
                txtSoMay.Text = selectedRow.Cells["So_May"].Value.ToString();
                txtMau.Text = selectedRow.Cells["Mau"].Value.ToString();
                cbbXiLanh.Text = selectedRow.Cells["Dung_Tich_Xi_lanh"].Value.ToString();
                txtHangXe.Text = selectedRow.Cells["Hang_Xe"].Value.ToString();

                txtTenXe.Text = selectedRow.Cells["Ten_Xe"].Value.ToString();

                if (SoKhung == "Xem ảnh")
                {
                    // Không cần hiển thị ảnh trong PictureBox khi nhấp vào "Xem ảnh"
                    pcAnh.Image = null;
                }
                else
                {
                    // Hiển thị ảnh từ cơ sở dữ liệu trong PictureBox
                    byte[] imageData = GetImageDataFromDatabase(SoKhung);

                    if (imageData != null)
                    {
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            pcAnh.Image = Image.FromStream(ms);
                        }
                    }
                    else
                    {
                        pcAnh.Image = null;
                    }
                }
            }
        }

        // Còn lại giữ nguyên


        private byte[] GetImageDataFromDatabase(string SoKhung)
        {
            byte[] imageData = null;

            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(conn))
                {
                    sqlConnection.Open();
                    string query = "SELECT Anh FROM DanhSachXeMay1 WHERE So_Khung = @SoKhung";
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@SoKhung", SoKhung);
                        var result = sqlCommand.ExecuteScalar();
                        if (result != DBNull.Value)
                        {
                            imageData = (byte[])result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Bỏ qua thông báo lỗi và không thực hiện hành động gì khác
                // Có thể ghi log lỗi vào một file hoặc hệ thống quản lý lỗi tại đây nếu cần
            }

            return imageData;
        }


        private void btnThem_Click(object sender, EventArgs e)
        {
            // Kiểm tra dữ liệu nhập vào
            if (string.IsNullOrEmpty(txtSoKhung.Text) || string.IsNullOrEmpty(txtSoMay.Text) ||
                string.IsNullOrEmpty(txtMau.Text) || string.IsNullOrEmpty(cbbXiLanh.Text) ||
                string.IsNullOrEmpty(txtHangXe.Text) || string.IsNullOrEmpty(txtTenXe.Text))
            {
                MessageBox.Show("Vui lòng nhập đủ thông tin.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra xem số khung và số máy đã tồn tại trong danh sách hay chưa
            string soKhung = txtSoKhung.Text.Trim();
            string soMay = txtSoMay.Text.Trim();
            if (XeDaTonTai(soKhung, soMay))
            {
                MessageBox.Show("Số khung hoặc số máy đã tồn tại trong danh sách.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Thêm vào danh sách
            try
            {
                using (SqlConnection con = new SqlConnection(conn))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("INSERT INTO dbo.DanhSachXeMay1 (So_Khung, So_May, Mau, Dung_Tich_Xi_Lanh, Hang_Xe, Ten_Xe) VALUES (@SoKhung, @SoMay, @Mau, @XiLanh, @HangXe, @TenXe)", con))
                    {
                        cmd.Parameters.AddWithValue("@SoKhung", soKhung);
                        cmd.Parameters.AddWithValue("@SoMay", soMay);
                        cmd.Parameters.AddWithValue("@Mau", txtMau.Text.Trim());
                        cmd.Parameters.AddWithValue("@XiLanh", cbbXiLanh.Text.Trim());
                        cmd.Parameters.AddWithValue("@HangXe", txtHangXe.Text.Trim());
                        cmd.Parameters.AddWithValue("@TenXe", txtTenXe.Text.Trim());

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Thêm xe máy thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Reload lại bảng sau khi thêm thành công
                            LoadTable();
                            // Xóa dữ liệu trên các control sau khi thêm thành công
                            ClearInputs();
                        }
                        else
                        {
                            MessageBox.Show("Thêm xe máy thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool XeDaTonTai(string soKhung, string soMay)
        {
            // Thực hiện truy vấn để kiểm tra xem số khung hoặc số máy đã tồn tại trong danh sách hay chưa
            // Trả về true nếu đã tồn tại, ngược lại trả về false
            try
            {
                using (SqlConnection con = new SqlConnection(conn))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM dbo.DanhSachXeMay1 WHERE So_Khung = @SoKhung OR So_May = @SoMay", con))
                    {
                        cmd.Parameters.AddWithValue("@SoKhung", soKhung);
                        cmd.Parameters.AddWithValue("@SoMay", soMay);

                        int count = (int)cmd.ExecuteScalar();
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return true; // Trả về true nếu có lỗi xảy ra
            }
        }

        private void ClearInputs()
        {
            txtSoKhung.Clear();
            txtSoMay.Clear();
            txtMau.Clear();
            cbbXiLanh.SelectedIndex = -1;
            txtHangXe.Clear();
            txtTenXe.Clear();
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem người dùng đã chọn dòng nào để xoá chưa
            if (dgvDanhSachXeMay.SelectedRows.Count > 0)
            {
                // Hiển thị thông báo xác nhận xoá
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn xoá xe máy này không?", "Xác nhận xoá", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                // Nếu người dùng đồng ý xoá
                if (result == DialogResult.Yes)
                {
                    // Lấy dữ liệu từ dòng được chọn
                    DataGridViewRow selectedRow = dgvDanhSachXeMay.SelectedRows[0];
                    string SoKhung = selectedRow.Cells["So_Khung"].Value.ToString();

                    // Thực hiện xoá dòng từ cơ sở dữ liệu
                    try
                    {
                        using (SqlConnection con = new SqlConnection(conn))
                        {
                            con.Open();
                            using (SqlCommand cmd = new SqlCommand("DELETE FROM dbo.DanhSachXeMay1 WHERE So_Khung = @SoKhung", con))
                            {
                                cmd.Parameters.AddWithValue("@SoKhung", SoKhung);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Xoá xe máy thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    // Reload lại bảng sau khi xoá thành công
                                    LoadTable();
                                    // Xóa dữ liệu trên các control sau khi xoá thành công
                                    ClearInputs();
                                }
                                else
                                {
                                    MessageBox.Show("Xoá xe máy thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn xe máy bạn muốn xoá.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cbbXiLanh_DropDown(object sender, EventArgs e)
        {
            cbbXiLanh.Items.Clear();
            cbbXiLanh.Items.Add("70");
            cbbXiLanh.Items.Add("80");
            cbbXiLanh.Items.Add("90");
            cbbXiLanh.Items.Add("100");
            cbbXiLanh.Items.Add("110");

        }

        private void btnLamMoi_Click(object sender, EventArgs e)
        {
            txtHangXe.Clear();
            txtMau.Clear(); 
            txtSoKhung.Clear(); 
            txtSoMay.Clear();
            txtTenXe.Clear();
            cbbXiLanh.Text = "";
            pcAnh.Image = null;
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (dgvDanhSachXeMay.SelectedRows.Count > 0)
            {
                // Lấy thông tin xe từ các controls
                string Mau = txtMau.Text;
                string HangXe = txtHangXe.Text;
                string TenXe = txtTenXe.Text;
                //chuyển đổi giá trị chuỗi vào kiểu số nguyên
                int XiLanh;
                if (!int.TryParse(cbbXiLanh.Text, out XiLanh))
                {
                    MessageBox.Show("Dung tích xi lanh phải là một số nguyên!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Lấy số khung và số máy từ dòng được chọn trong DataGridView
                DataGridViewRow selectedRow = dgvDanhSachXeMay.SelectedRows[0];
                string SoKhung = selectedRow.Cells["So_Khung"].Value.ToString();
                string SoMay = selectedRow.Cells["So_May"].Value.ToString();

                // Kiểm tra xem số khung hoặc số máy đã bị sửa
                if (SoKhung != txtSoKhung.Text || SoMay != txtSoMay.Text)
                {
                    MessageBox.Show("Không được phép sửa số khung hoặc số máy.", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Hiển thị hộp thoại xác nhận sửa thông tin
                DialogResult result = MessageBox.Show("Bạn có chắc muốn sửa thông tin xe này?", "Xác nhận sửa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // Thực hiện sửa thông tin xe (trừ số khung và số máy)
                    Sua(SoKhung, SoMay, Mau, XiLanh, HangXe, TenXe);
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn một xe máy để sửa", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



        private void Sua(string SoKhung, string SoMay, string Mau, int XiLanh, string HangXe, string TenXe)
        {
            try
            {
                using (SqlConnection sqlConnection = new SqlConnection(conn))
                {
                    sqlConnection.Open();

                    // Cập nhật thông tin xe (trừ số khung và số máy) trong cơ sở dữ liệu dựa trên số khung
                    string query = "UPDATE DanhSachXeMay1 SET Mau = @Mau, Dung_Tich_Xi_Lanh = @XiLanh, Hang_Xe = @HangXe, Ten_Xe = @TenXe WHERE So_Khung = @SoKhung";
                    using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                    {
                        sqlCommand.Parameters.AddWithValue("@SoKhung", SoKhung);
                        sqlCommand.Parameters.AddWithValue("@Mau", Mau);
                        sqlCommand.Parameters.AddWithValue("@XiLanh", XiLanh);
                        sqlCommand.Parameters.AddWithValue("@HangXe", HangXe);
                        sqlCommand.Parameters.AddWithValue("@TenXe", TenXe);

                        sqlCommand.ExecuteNonQuery();
                        MessageBox.Show("Sửa thông tin xe thành công!!");

                        // Load lại dữ liệu sau khi sửa
                        LoadTable();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void btnThoat_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("Bạn có muốn thoát không ?","Thông báo!!!", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
