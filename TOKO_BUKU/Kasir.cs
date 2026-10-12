using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TOKO_BUKU
{
    public partial class Kasir : Form
    {
        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";
        DataTable keranjang = new DataTable();
        decimal totalBelanjaAktif = 0;
        decimal totalDiskonAktif = 0;


        private Form currentChild = null;

        public Kasir()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            AturKeranjang();
            MuatDataBuku();
            AturPlaceholderCari();

            listBox1.DoubleClick += ListBox1_DoubleClick;

            
            label1.Click += (s, e) => PindahHalaman<Kasir>();
            label5.Click += (s, e) => PindahHalaman<Stock_buku>();

   
            this.Resize += (s, e) =>
            {
                if (currentChild != null && panelContent != null)
                {
                    currentChild.Dock = DockStyle.Fill;
                }
            };
        }

        private void AturKeranjang()
        {
            keranjang.Columns.Add("id_buku", typeof(int));
            keranjang.Columns.Add("kode_buku", typeof(string));
            keranjang.Columns.Add("judul", typeof(string));
            keranjang.Columns.Add("harga", typeof(decimal));
            keranjang.Columns.Add("jumlah", typeof(int));
            keranjang.Columns.Add("diskon", typeof(decimal));
            keranjang.Columns.Add("diskon_total", typeof(decimal));
            keranjang.Columns.Add("subtotal", typeof(decimal));

            dataGridView1.DataSource = keranjang;
        }

        private void AturPlaceholderCari()
        {
            textBox2.Text = "Cari Judul / Kode Buku.....";
            textBox2.ForeColor = System.Drawing.Color.Gray;

            textBox2.Enter += (s, e) => {
                if (textBox2.Text == "Cari Judul / Kode Buku.....") { textBox2.Text = ""; textBox2.ForeColor = System.Drawing.Color.Black; }
            };
            textBox2.Leave += (s, e) => {
                if (string.IsNullOrWhiteSpace(textBox2.Text)) { AturPlaceholderCari(); MuatDataBuku(); }
            };
        }

        private void MuatDataBuku(string keyword = "")
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM books";

                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query += " WHERE judul LIKE @key OR kode_buku LIKE @key";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        if (!string.IsNullOrEmpty(keyword)) cmd.Parameters.AddWithValue("@key", "%" + keyword + "%");

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            listBox1.Items.Clear();
                            while (reader.Read())
                            {
                                listBox1.Items.Add($"{reader["kode_buku"]} | {reader["judul"]} | {reader["pengarang"]} | Rp {Convert.ToDecimal(reader["harga"]):N0} | Stok: {reader["stok"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal memuat buku: " + ex.Message); }
        }

        private void btnCari_Click(object sender, EventArgs e)
        {
            string keyword = (textBox2.Text == "Cari Judul / Kode Buku.....") ? "" : textBox2.Text;
            MuatDataBuku(keyword);
        }

        private void ListBox1_DoubleClick(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string kodeBuku = listBox1.SelectedItem.ToString().Split('|')[0].Trim();
                TambahKeKeranjang(kodeBuku);
            }
        }

        private void TambahKeKeranjang(string kodeBuku)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM books WHERE kode_buku = @kode", conn);
                    cmd.Parameters.AddWithValue("@kode", kodeBuku);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int stok = reader.GetInt32("stok");
                            if (stok <= 0)
                            {
                                MessageBox.Show("Stok buku habis!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            string kode = reader.GetString("kode_buku");
                            decimal harga = reader.GetDecimal("harga");
                    decimal diskon = 0m;
                    try { diskon = reader["diskon"] != DBNull.Value ? Convert.ToDecimal(reader["diskon"]) : 0m; } catch { diskon = 0m; }

                            DataRow[] adaDiKeranjang = keranjang.Select($"kode_buku = '{kode}'");
                            if (adaDiKeranjang.Length > 0)
                            {
                                int jmlLama = Convert.ToInt32(adaDiKeranjang[0]["jumlah"]);
                                if (jmlLama + 1 > stok)
                                {
                                    MessageBox.Show("Jumlah melebihi stok tersedia!"); return;
                                }
                        int newJumlah = jmlLama + 1;
                        adaDiKeranjang[0]["jumlah"] = newJumlah;

                        
                        decimal diskonTotal = newJumlah * harga * diskon / 100m;
                        decimal subtotal = newJumlah * harga;
                        adaDiKeranjang[0]["diskon"] = diskon;
                        adaDiKeranjang[0]["diskon_total"] = diskonTotal;
                        adaDiKeranjang[0]["subtotal"] = subtotal;
                            }
                            else
                            {
                        decimal diskonTotal = 1 * harga * diskon / 100m;
                        decimal subtotal = 1 * harga;
                        keranjang.Rows.Add(reader.GetInt32("id_buku"), kode, reader.GetString("judul"), harga, 1, diskon, diskonTotal, subtotal);
                            }
                            HitungTotalBelanja();
                        }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Gagal masuk keranjang: " + ex.Message); }
        }

        private void HitungTotalBelanja()
        {
           
            decimal sumSubtotal = 0m;
            totalDiskonAktif = 0m;
            foreach (DataRow row in keranjang.Rows)
            {
                try
                {
                    
                    sumSubtotal += row.Table.Columns.Contains("subtotal") ? Convert.ToDecimal(row["subtotal"]) : 0m;
                    totalDiskonAktif += row.Table.Columns.Contains("diskon_total") ? Convert.ToDecimal(row["diskon_total"]) : 0m;
                }
                catch { }
            }

            totalBelanjaAktif = sumSubtotal - totalDiskonAktif;
            try
            {
              
                label8.Text = "Rp " + sumSubtotal.ToString("N0");
                label12.Text = "Rp " + totalDiskonAktif.ToString("N0");
                label9.Text = "Rp " + totalBelanjaAktif.ToString("N0");
            }
            catch
            {
                
            }
        }

        private void btnHapus_Click(object sender, EventArgs e)
        {
            HapusDataKeranjangTerpilih();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            HapusDataKeranjangTerpilih();
        }

        private void HapusDataKeranjangTerpilih()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                for (int i = dataGridView1.SelectedRows.Count - 1; i >= 0; i--)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.SelectedRows[i].Index);
                }
                HitungTotalBelanja();
            }
        }

        private void dataGridView1_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["jumlah"].Index)
            {
                var row = dataGridView1.Rows[e.RowIndex];
                decimal harga = Convert.ToDecimal(row.Cells["harga"].Value);
                int jumlah = Convert.ToInt32(row.Cells["jumlah"].Value);
                decimal diskon = 0m;
                try { diskon = row.Cells["diskon"] != null && row.Cells["diskon"].Value != null ?
                        Convert.ToDecimal(row.Cells["diskon"].Value) : 0m; } 
                catch { diskon = 0m; }
                decimal diskonTotal = jumlah * harga * diskon / 100m;
                decimal subtotal = jumlah * harga;

                row.Cells["diskon_total"].Value = diskonTotal;
                row.Cells["subtotal"].Value = subtotal;
                HitungTotalBelanja();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)) return;
            if (!decimal.TryParse(textBox1.Text, out _))
            {
                textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
                textBox1.SelectionStart = textBox1.Text.Length;
            }
        }

        private void btnBayar_Click(object sender, EventArgs e)
        {
            if (keranjang.Rows.Count == 0)
            {
                MessageBox.Show("Tidak ada buku yang dibeli. Silakan pilih buku untuk dibeli.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Masukkan jumlah uang pembayaran!");
                return;
            }

            if (!decimal.TryParse(textBox1.Text, out decimal uangBayar))
            {
                MessageBox.Show("Format uang bayar tidak valid!");
                return;
            }

            if (uangBayar < totalBelanjaAktif)
            {
                MessageBox.Show("Uang bayar kurang!"); return;
            }

            decimal kembalian = uangBayar - totalBelanjaAktif;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlTransaction trans = conn.BeginTransaction();

                try
                {
                    MySqlCommand cmdTrans = new MySqlCommand("INSERT INTO transactions (tanggal, total_harga, id_user) VALUES (NOW(), @total, @user)", conn, trans);
                    cmdTrans.Parameters.AddWithValue("@total", totalBelanjaAktif);
                    cmdTrans.Parameters.AddWithValue("@user", 1);
                    cmdTrans.ExecuteNonQuery();
                    long idTransaksiBaru = cmdTrans.LastInsertedId;

                    foreach (DataRow row in keranjang.Rows)
                    {
                        MySqlCommand cmdDetail = new MySqlCommand("INSERT INTO transaction_details (id_transaksi, id_buku, jumlah, subtotal) VALUES (@id_trans, @id_buku, @jml, @sub)", conn, trans);
                        cmdDetail.Parameters.AddWithValue("@id_trans", idTransaksiBaru);
                        cmdDetail.Parameters.AddWithValue("@id_buku", row["id_buku"]);
                        cmdDetail.Parameters.AddWithValue("@jml", row["jumlah"]);
                        cmdDetail.Parameters.AddWithValue("@sub", row["subtotal"]);
                        cmdDetail.ExecuteNonQuery();

                        MySqlCommand cmdStok = new MySqlCommand("UPDATE books SET stok = stok - @jml WHERE id_buku = @id_buku", conn, trans);
                        cmdStok.Parameters.AddWithValue("@jml", row["jumlah"]);
                        cmdStok.Parameters.AddWithValue("@id_buku", row["id_buku"]);
                        cmdStok.ExecuteNonQuery();
                    }

                    trans.Commit();

                    string teksStruk = BuatTeksStruk(idTransaksiBaru, totalBelanjaAktif, uangBayar, kembalian);
                    new Struk(teksStruk).ShowDialog(this);

                    MessageBox.Show($"Transaksi Berhasil!\nKembalian: Rp {kembalian:N0}", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    keranjang.Clear();
                    HitungTotalBelanja();
                    textBox1.Clear();
                    MuatDataBuku();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    MessageBox.Show("Transaksi dibatalkan karena terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string BuatTeksStruk(long idTransaksi, decimal total, decimal bayar, decimal kembali)
        {
            var sb = new StringBuilder();
            int width = 40;
            Func<string, string> center = s => s.PadLeft((width + s.Length) / 2).PadRight(width);

            sb.AppendLine("========================================");
            sb.AppendLine(center("TOKO BUKU"));
            sb.AppendLine("========================================");
            sb.AppendLine($"No: {idTransaksi}\nTanggal: {DateTime.Now:yyyy-MM-dd HH:mm}");
            sb.AppendLine("----------------------------------------");
            sb.AppendLine(string.Format("{0,-6}{1,-18}{2,3}{3,9}", "Kode", "Judul", "Jml", "Subtotal"));

            foreach (DataRow row in keranjang.Rows)
            {
                string judul = row["judul"].ToString();
                judul = judul.Length > 15 ? judul.Substring(0, 15) + ".." : judul;
                sb.AppendLine(string.Format("{0,-6}{1,-18}{2,3} {3,9:N0}", row["kode_buku"], judul, row["jumlah"], row["subtotal"]));
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "TOTAL DISKON:", totalDiskonAktif));
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "TOTAL:", total));
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "BAYAR:", bayar));
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "KEMBALIAN:", kembali));
            sb.AppendLine("========================================");
            sb.AppendLine(center("TERIMA KASIH"));

            return sb.ToString();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            var login = new Login();
            login.StartPosition = FormStartPosition.CenterScreen;
            login.Show();
            this.Hide();
        }


        private void PindahHalaman<T>() where T : Form, new()
        {
            if (typeof(T) == typeof(Kasir))
            {
                ShowKasirDashboard();
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

                if (panelUtamaKasir != null)
                    panelUtamaKasir.Visible = false;

              
                child.TopLevel = false;
                child.FormBorderStyle = FormBorderStyle.None;
                child.StartPosition = FormStartPosition.Manual;
                child.Dock = DockStyle.Fill;

                panelContent.Controls.Add(child);
                currentChild = child;
                panelContent.Visible = true;

                child.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal membuka halaman: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void ShowKasirDashboard()
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

             
                if (panelUtamaKasir != null)
                    panelUtamaKasir.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal kembali ke halaman Kasir: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void CenterChildInPanel(Form child)
        {
           
            if (child == null) return;
            child.Dock = DockStyle.Fill;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    if (!dataGridView1.Rows[e.RowIndex].IsNewRow)
                    {
                        DialogResult dialog = MessageBox.Show("Hapus data dari keranjang?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (dialog == DialogResult.Yes)
                        {
                            dataGridView1.Rows.RemoveAt(e.RowIndex);
                            HitungTotalBelanja();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi Kesalahan: " + ex.Message);
            }
        }
    }
}