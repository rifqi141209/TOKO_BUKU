using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace TOKO_BUKU
{
    public partial class Admin_dashboard : Form
    {
        private readonly string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";
        private Form currentChild = null;

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

            this.panelContent.Resize += (s, e) =>
            {
                if (currentChild != null)
                {
                    CenterChildInPanel(currentChild);
                }
            };
        }

        private void MuatStatistikDashboard()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string queryBuku = "SELECT COUNT(*) FROM books";
                    using (MySqlCommand konebuku = new MySqlCommand(queryBuku, conn))
                    {
                        object result = konebuku.ExecuteScalar();
                        totalbuku.Text = result != null ? result.ToString() : "0";
                    }

                    string queryStok = "SELECT SUM(stok) FROM books";
                    using (MySqlCommand konebuku = new MySqlCommand(queryStok, conn))
                    {
                        object result = konebuku.ExecuteScalar();
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
                    using (MySqlCommand konetrans = new MySqlCommand(queryTransaksi, conn))
                    {
                        object result = konetrans.ExecuteScalar();
                        total_transaksi.Text = result != null ? result.ToString() : "0";
                    }

                    string queryPenjualan = "SELECT SUM(total_harga) FROM transactions";
                    using (MySqlCommand konetrans = new MySqlCommand(queryPenjualan, conn))
                    {
                        object result = konetrans.ExecuteScalar();
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

                    using (MySqlDataAdapter kone = new MySqlDataAdapter(query, conn))
                    using (DataTable dt = new DataTable())
                    {
                        kone.Fill(dt);
                        Transaksi.DataSource = dt;

                        if (Transaksi.Columns.Count > 0)
                        {
                            foreach (DataGridViewColumn col in Transaksi.Columns)
                            {
                                col.HeaderText = col.Name.Replace("_", " ");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat tabel transaksi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PindahHalaman<T>() where T : Form, new()
        {
            if (typeof(T) == typeof(Admin_dashboard))
            {
                ShowDashboard();
                return;
            }

            T child = new T();
            ShowChildForm(child);
        }

        public void ShowChildForm(Form child)
        {
            try
            {
                if (currentChild != null)
                {
                    currentChild.Close();
                    currentChild.Dispose();
                    panelContent.Controls.Remove(currentChild);
                    currentChild = null;
                }

                panel2.Visible = false;
                Transaksi.Visible = false;
                label11.Visible = false;

                child.TopLevel = false;
                child.FormBorderStyle = FormBorderStyle.None;
                child.StartPosition = FormStartPosition.Manual;

                panelContent.Controls.Add(child);
                currentChild = child;
                panelContent.Visible = true;

                if (child.Width <= 0 || child.Height <= 0)
                {
                    child.Size = new Size(Math.Max(600, panelContent.ClientSize.Width - 40), Math.Max(300, panelContent.ClientSize.Height - 40));
                }

                CenterChildInPanel(child);
                child.Show();

                var side = child.Controls.Find("panel1", true).FirstOrDefault();
                if (side != null) side.Visible = false;

                var mainPanel = child.Controls.Find("panel7", true).FirstOrDefault() as Panel;
                if (mainPanel != null)
                {
                    mainPanel.Dock = DockStyle.Fill;
                    mainPanel.Location = new Point(0, 0);
                }

                var hdr = child.Controls.Find("label14", true).FirstOrDefault() as Label;
                if (hdr != null)
                {
                    hdr.Dock = DockStyle.Top;
                    hdr.Location = new Point(8, 8);
                }

                child.AutoScroll = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka halaman: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CenterChildInPanel(Form child)
        {
            if (child == null || panelContent == null) return;

            int x = Math.Max(0, (panelContent.ClientSize.Width - child.Width) / 2);
            int y = Math.Max(0, (panelContent.ClientSize.Height - child.Height) / 2);
            child.Location = new Point(x, y);
        }

        public void ShowDashboard()
        {
            try
            {
                if (currentChild != null)
                {
                    currentChild.Close();
                    currentChild.Dispose();
                    panelContent.Controls.Remove(currentChild);
                    currentChild = null;
                }

                panel2.Visible = true;
                Transaksi.Visible = true;
                label11.Visible = true;
                panelContent.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal kembali ke dashboard: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label1_Click(object sender, EventArgs e) => PindahHalaman<Admin_dashboard>();
        private void label5_Click(object sender, EventArgs e) => PindahHalaman<Data_buku>();
        private void label6_Click(object sender, EventArgs e) => PindahHalaman<Laporan_penjualan>();
        private void label7_Click(object sender, EventArgs e) => PindahHalaman<Backup>();

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