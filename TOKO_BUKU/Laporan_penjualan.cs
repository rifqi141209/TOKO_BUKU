using System;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TOKO_BUKU
{
    public partial class Laporan_penjualan : Form
    {
        private readonly string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

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

                    string queryData = @"
                        SELECT t.*, 
                               GROUP_CONCAT(b.judul SEPARATOR ', ') AS judul_buku
                        FROM transactions t
                        LEFT JOIN transaction_details td ON td.id_transaksi = t.id_transaksi
                        LEFT JOIN books b ON td.id_buku = b.id_buku
                        WHERE t.tanggal BETWEEN @tgl1 AND @tgl2
                        GROUP BY t.id_transaksi
                        ORDER BY t.tanggal DESC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(queryData, conn))
                    using (DataTable dt = new DataTable())
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@tgl1", tglMulai);
                        adapter.SelectCommand.Parameters.AddWithValue("@tgl2", tglSelesai);

                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;

                        if (dataGridView1.Columns.Count > 0)
                        {
                            foreach (DataGridViewColumn col in dataGridView1.Columns)
                            {
                                col.HeaderText = col.Name.Replace("_", " ");
                            }
                        }

                        if (dataGridView1.Columns.Contains("judul buku"))
                        {
                            dataGridView1.Columns["judul buku"].HeaderText = "Judul Buku";
                        }
                        else if (dataGridView1.Columns.Contains("judul_buku"))
                        {
                            dataGridView1.Columns["judul_buku"].HeaderText = "Judul Buku";
                        }
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

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

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
                    using (DataTable dt = new DataTable())
                    {
                        adapter.SelectCommand.Parameters.AddWithValue("@id", idTransaksi);
                        adapter.Fill(dt);
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
                chart1.Series.Clear();
                var series = chart1.Series.Add("Series1");

                series.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Column;
                series.XValueType = System.Windows.Forms.DataVisualization.Charting.ChartValueType.DateTime;
                series.IsValueShownAsLabel = true;
                series.Label = "#VALY{N0}";

                var area = chart1.ChartAreas.Count > 0 ? chart1.ChartAreas[0] : null;
                if (area != null)
                {
                    area.AxisX.Interval = 1;
                    area.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Days;
                    area.AxisX.LabelStyle.Format = "dd/MM/yyyy";
                    area.AxisX.LabelStyle.Angle = -45;
                    area.AxisX.LabelStyle.IsEndLabelVisible = true;

                    area.AxisX.MajorGrid.Enabled = false;

                    area.AxisY.LabelStyle.Format = "N0";
                    area.AxisY.Title = "Total (Rp)";
                }

                string queryGrafik = @"
            SELECT 
                DATE(tanggal) AS tgl_harian, 
                SUM(total_harga) AS total_harian 
            FROM transactions 
            WHERE DATE(tanggal) BETWEEN @tgl1 AND @tgl2 
            GROUP BY DATE(tanggal) 
            ORDER BY DATE(tanggal) ASC";

                using (MySqlCommand cmd = new MySqlCommand(queryGrafik, conn))
                {
                    cmd.Parameters.AddWithValue("@tgl1", tgl1);
                    cmd.Parameters.AddWithValue("@tgl2", tgl2);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        series.Points.Clear();

                        while (reader.Read())
                        {
                            DateTime tanggal = Convert.ToDateTime(reader["tgl_harian"]);
                            double harga = reader.IsDBNull(reader.GetOrdinal("total_harian"))
                                           ? 0 : Convert.ToDouble(reader["total_harian"]);

                            series.Points.AddXY(tanggal, harga);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat grafik: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            TampilkanLaporan();
        }

        private void PindahHalaman<T>(bool closeTopLevel = false) where T : Form, new()
        {
            var parent = Application.OpenForms.OfType<Admin_dashboard>().FirstOrDefault();
            if (parent == null)
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
                return;
            }

            if (typeof(T) == typeof(Admin_dashboard))
            {
                parent.ShowDashboard();
                if (this.TopLevel)
                {
                    if (closeTopLevel) this.Close();
                    else this.Hide();
                }
                return;
            }

            parent.ShowChildForm(new T());
            if (this.TopLevel)
            {
                if (closeTopLevel) this.Close();
                else this.Hide();
            }
        }

        private void label1_Click(object sender, EventArgs e) => PindahHalaman<Admin_dashboard>(false);
        private void label5_Click(object sender, EventArgs e) => PindahHalaman<Data_buku>(false);
        private void label6_Click(object sender, EventArgs e) => PindahHalaman<Laporan_penjualan>(false);
        private void label7_Click(object sender, EventArgs e) => PindahHalaman<Backup>(false);

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