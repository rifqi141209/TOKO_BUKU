using MySql.Data.MySqlClient;
using System;
using System.Data;
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

        public Kasir()
        {
            InitializeComponent();

            AturKeranjang();
            MuatDataBuku();
            AturPlaceholderCari();

            listBox1.DoubleClick += ListBox1_DoubleClick;
            label1.Click += (s, e) => PindahHalaman<Kasir>(false);
            label5.Click += (s, e) => PindahHalaman<Stock_buku>(false);
        }

        private void AturKeranjang()
        {
            keranjang.Columns.Add("id_buku", typeof(int));
            keranjang.Columns.Add("kode_buku", typeof(string));
            keranjang.Columns.Add("judul", typeof(string));
            keranjang.Columns.Add("harga", typeof(decimal));
            keranjang.Columns.Add("jumlah", typeof(int));
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

                            DataRow[] adaDiKeranjang = keranjang.Select($"kode_buku = '{kode}'");
                            if (adaDiKeranjang.Length > 0)
                            {
                                int jmlLama = Convert.ToInt32(adaDiKeranjang[0]["jumlah"]);
                                if (jmlLama + 1 > stok)
                                {
                                    MessageBox.Show("Jumlah melebihi stok tersedia!"); return;
                                }
                                adaDiKeranjang[0]["jumlah"] = jmlLama + 1;
                                adaDiKeranjang[0]["subtotal"] = (jmlLama + 1) * harga;
                            }
                            else
                            {
                                keranjang.Rows.Add(reader.GetInt32("id_buku"), kode, reader.GetString("judul"), harga, 1, harga);
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
            totalBelanjaAktif = 0;
            foreach (DataRow row in keranjang.Rows) 
            {
                totalBelanjaAktif += Convert.ToDecimal(row["subtotal"]);
            }

            label8.Text = "Rp " + totalBelanjaAktif.ToString("N0");
            label9.Text = "Rp " + totalBelanjaAktif.ToString("N0");
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
                
                // Hitung ulang subtotal
                row.Cells["subtotal"].Value = harga * jumlah;
                HitungTotalBelanja();
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text)) return;
            if (!decimal.TryParse(textBox1.Text, out _))
            {
                // Hapus karakter terakhir jika bukan angka
                textBox1.Text = textBox1.Text.Substring(0, textBox1.Text.Length - 1);
                textBox1.SelectionStart = textBox1.Text.Length; 
            }
        }

        private void btnBayar_Click(object sender, EventArgs e)
        {
            // 1. CEK KERANJANG KOSONG DI AWAL
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

            // 2. CEK INPUT UANG MENGGUNAKAN TRYPARSE
            if (!decimal.TryParse(textBox1.Text, out decimal uangBayar))
            {
                MessageBox.Show("Format uang bayar tidak valid!");
                return;
            }

            // Menggunakan nilai murni dari variabel yang dihitung, bukan dari Label string
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
                    // Asumsi form Struk menerima parameter string pada constructor-nya
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
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "TOTAL:", total));
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "BAYAR:", bayar));
            sb.AppendLine(string.Format("{0,-30}{1,10:N0}", "KEMBALIAN:", kembali));
            sb.AppendLine("========================================");
            sb.AppendLine(center("TERIMA KASIH"));

            return sb.ToString();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            PindahHalaman<Login>();
        }

        private void PindahHalaman<T>(bool closeTopLevel = false) where T : Form, new()
        {
            var parent = Application.OpenForms.OfType<Admin_dashboard>().FirstOrDefault();
            if (parent == null)
            {
                if (this is T) return;
                T formBaru = Application.OpenForms.OfType<T>().FirstOrDefault() ?? new T() { StartPosition = FormStartPosition.CenterScreen };
                formBaru.Show();
                formBaru.BringToFront();
                this.Hide();
                return;
            }

            if (typeof(T) == typeof(Admin_dashboard))
            {
                parent.ShowDashboard();
                if (this.TopLevel) { if (closeTopLevel) this.Close(); else this.Hide(); }
                return;
            }

            parent.ShowChildForm(new T());
            if (this.TopLevel) { if (closeTopLevel) this.Close(); else this.Hide(); }
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