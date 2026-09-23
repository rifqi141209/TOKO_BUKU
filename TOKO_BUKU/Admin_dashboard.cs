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
using TOKO_BUKU;

namespace TOKO_BUKU
{
    public partial class Admin_dashboard : Form
    {
        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

        public Admin_dashboard()
        {
            InitializeComponent();
            MuatStatistikDashboard();
            MuatTabelTransaksi();

            label1.Click += label1_Click;
            label5.Click += label5_Click;
            label6.Click += label6_Click;
            label7.Click += label7_Click;
            button3.Click += button3_Click;
        }

        private void MuatStatistikDashboard()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string queryBuku = "SELECT COUNT(*) FROM books";
                    using (MySqlCommand cmd = new MySqlCommand(queryBuku, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        totalbuku.Text = (result != null) ? result.ToString() : "0";
                    }

                    string queryStok = "SELECT SUM(stok) FROM books";
                    using (MySqlCommand cmd = new MySqlCommand(queryStok, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            long totalStok = Convert.ToInt64(result);
                            total_stoc.Text = totalStok.ToString("N0");
                        }
                        else
                        {
                            total_stoc.Text = "0";
                        }
                    }

                    string queryTransaksi = "SELECT COUNT(*) FROM transactions";
                    using (MySqlCommand cmd = new MySqlCommand(queryTransaksi, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        total_transaksi.Text = (result != null) ? result.ToString() : "0";
                    }

                    string queryPenjualan = "SELECT SUM(total_harga) FROM transactions";
                    using (MySqlCommand cmd = new MySqlCommand(queryPenjualan, conn))
                    {
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            decimal totalPendapatan = Convert.ToDecimal(result);
                            penjualan.Text = "Rp " + totalPendapatan.ToString("N0");
                        }
                        else
                        {
                            penjualan.Text = "Rp 0";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat statistik dashboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MuatTabelTransaksi()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM transactions";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    Transaksi.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat tabel transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PindahHalaman<T>() where T : Form, new()
        {
            if (this is T) return;

            T targetForm = Application.OpenForms.OfType<T>().FirstOrDefault();
            if (targetForm == null)
            {
                targetForm = new T();
                targetForm.StartPosition = FormStartPosition.CenterScreen;
            }

            targetForm.Show();
            targetForm.BringToFront();
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