using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using TOKO_BUKU;

namespace TOKO_BUKU
{
    public partial class Laporan_penjualan : Form
    {
        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

        public Laporan_penjualan()
        {
            InitializeComponent();

            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePicker2.Value = DateTime.Now;

            TampilkanLaporan();

            dateTimePicker1.ValueChanged += DateTimePicker_ValueChanged;
            dateTimePicker2.ValueChanged += DateTimePicker_ValueChanged;

            dataGridView1.CellClick += dataGridView1_CellClick;
            button3.Click += button3_Click;
        }

        private void TampilkanLaporan()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string tglMulai = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                    string tglSelesai = dateTimePicker2.Value.ToString("yyyy-MM-dd 23:59:59");

                    string queryData = "SELECT * FROM transactions WHERE tanggal BETWEEN @tgl1 AND @tgl2";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(queryData, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@tgl1", tglMulai);
                        adapter.SelectCommand.Parameters.AddWithValue("@tgl2", tglSelesai);

                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }

                    string queryRingkasan = "SELECT COUNT(*), SUM(total_harga) FROM transactions WHERE tanggal BETWEEN @tgl1 AND @tgl2";
                    using (MySqlCommand cmd = new MySqlCommand(queryRingkasan, conn))
                    {
                        cmd.Parameters.AddWithValue("@tgl1", tglMulai);
                        cmd.Parameters.AddWithValue("@tgl2", tglSelesai);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                long totalTransaksi = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                                label12.Text = totalTransaksi.ToString("N0");

                                decimal totalPenjualan = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                                label13.Text = "Rp " + totalPenjualan.ToString("N0");
                            }
                        }
                    }

                    MuatGrafik(tglMulai, tglSelesai, conn);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==========================================================
        // FUNGSI UNTUK MENAMPILKAN DETAIL TRANSAKSI SAAT TABEL DIKLIK
        // ==========================================================
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                // Pastikan nama kolom 'id_transaksi' sesuai dengan struktur database Anda
                if (row.Cells["id_transaksi"].Value != null)
                {
                    string idTransaksi = row.Cells["id_transaksi"].Value.ToString();
                    TampilkanDetailTransaksi(idTransaksi);
                }
            }
        }

        private void TampilkanDetailTransaksi(string idTransaksi)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"SELECT td.id_detail, b.kode_buku, b.judul, td.jumlah, td.subtotal 
                                     FROM transaction_details td 
                                     JOIN books b ON td.id_buku = b.id_buku 
                                     WHERE td.id_transaksi = @id";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@id", idTransaksi);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        //dataGridView.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat detail transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MuatGrafik(string tgl1, string tgl2, MySqlConnection conn)
        {
            try
            {
                if (chart1.Series.IndexOf("Series1") == -1)
                {
                    chart1.Series.Clear();
                    chart1.Series.Add("Series1");
                }

                string queryGrafik = "SELECT tanggal, total_harga FROM transactions WHERE tanggal BETWEEN @tgl1 AND @tgl2 ORDER BY tanggal ASC";
                using (MySqlCommand cmd = new MySqlCommand(queryGrafik, conn))
                {
                    cmd.Parameters.AddWithValue("@tgl1", tgl1);
                    cmd.Parameters.AddWithValue("@tgl2", tgl2);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        chart1.Series["Series1"].Points.Clear();
                        while (reader.Read())
                        {
                            string tgl = Convert.ToDateTime(reader["tanggal"]).ToString("dd/MM");
                            double harga = Convert.ToDouble(reader["total_harga"]);
                            chart1.Series["Series1"].Points.AddXY(tgl, harga);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            TampilkanLaporan();
        }

        private void PindahHalaman<T>() where T : Form, new()
        {
            if (this is T) return;

            T F = Application.OpenForms.OfType<T>().FirstOrDefault();
            if (F == null)
            {
                F = new T();
                F.StartPosition = FormStartPosition.CenterScreen;
            }

            F.Show();
            F.BringToFront();
            this.Hide();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            PindahHalaman<Admin_dashboard>();
        }
        private void label5_Click(object sender, EventArgs e)
        {
            PindahHalaman<Data_buku>();
        }
        private void label6_Click(object sender, EventArgs e)
        {
            PindahHalaman<Laporan_penjualan>();
        }
        private void label7_Click(object sender, EventArgs e)
        {
            PindahHalaman<Backup>();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                var login = new Login();
                login.StartPosition = FormStartPosition.CenterScreen;
                login.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak Bisa Logout: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}