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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TOKO_BUKU
{
    public partial class Data_buku : Form
    {

        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

        public Data_buku()
        {
            InitializeComponent();
            TampilData();
            AturPlaceholderCari();
            dataGridView1.CellClick += dataGridView1_CellClick;
            button3.Click += button3_Click;
        }

        private void TampilData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * FROM books", conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BersihkanForm()
        {
            textBox2.Clear();
            textBox7.Clear();
            textBox3.Clear();
            textBox6.Clear();
            textBox8.Clear();
            textBox5.Clear();
            textBox4.Clear();

            textBox2.Focus();
        }


        private void button2_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox7.Text) ||
                string.IsNullOrWhiteSpace(textBox3.Text) || string.IsNullOrWhiteSpace(textBox6.Text) ||
                string.IsNullOrWhiteSpace(textBox8.Text) || string.IsNullOrWhiteSpace(textBox5.Text))
            {
                MessageBox.Show("Data tidak boleh kosong.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "INSERT INTO books (kode_buku, judul, pengarang, penerbit, harga, stok) " +
                                   "VALUES (@kode, @judul, @pengarang, @penerbit, @harga, @stok)";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@kode", textBox2.Text);
                        cmd.Parameters.AddWithValue("@judul", textBox7.Text);
                        cmd.Parameters.AddWithValue("@pengarang", textBox3.Text);
                        cmd.Parameters.AddWithValue("@penerbit", textBox6.Text);
                        cmd.Parameters.AddWithValue("@harga", decimal.Parse(textBox8.Text));
                        cmd.Parameters.AddWithValue("@stok", int.Parse(textBox5.Text));

                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Buku berhasil ditambahkan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        BersihkanForm();
                        TampilData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Pilih data yang ingin di-update terlebih dahulu dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE books SET judul=@judul, pengarang=@pengarang, penerbit=@penerbit, harga=@harga, stok=@stok " +
                                   "WHERE kode_buku=@kode";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@kode", textBox2.Text);
                        cmd.Parameters.AddWithValue("@judul", textBox7.Text);
                        cmd.Parameters.AddWithValue("@pengarang", textBox3.Text);
                        cmd.Parameters.AddWithValue("@penerbit", textBox6.Text);
                        cmd.Parameters.AddWithValue("@harga", decimal.Parse(textBox8.Text));
                        cmd.Parameters.AddWithValue("@stok", int.Parse(textBox5.Text));

                        int baris = cmd.ExecuteNonQuery();
                        if (baris > 0)
                        {
                            MessageBox.Show("Buku berhasil diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            BersihkanForm();
                            TampilData();
                        }
                        else
                        {
                            MessageBox.Show("Kode Buku tidak ditemukan di database!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mengubah data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text))
            {
                MessageBox.Show("Pilih data yang ingin dihapus terlebih dahulu dari tabel!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Apakah Anda yakin ingin menghapus buku ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string query = "DELETE FROM books WHERE kode_buku=@kode";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@kode", textBox2.Text);
                            cmd.ExecuteNonQuery();
                            MessageBox.Show("Buku berhasil dihapus.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            BersihkanForm();
                            TampilData();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Gagal menghapus data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string placeholder = "Cari Judul / Kode Buku.....";
                    string cari = (textBox1.Text == placeholder) ? "" : textBox1.Text.Trim();

                    if (string.IsNullOrEmpty(cari))
                    {
                        
                        TampilData();
                        return;
                    }

                    string query = "SELECT * FROM books WHERE judul LIKE @cari OR kode_buku LIKE @cari OR pengarang LIKE @cari OR penerbit LIKE @cari";
                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    adapter.SelectCommand.Parameters.AddWithValue("@cari", "%" + cari + "%");

                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Pencarian gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AturPlaceholderCari()
        {
            textBox1.Text = "Cari Judul / Kode Buku.....";
            textBox1.ForeColor = Color.Gray;
            textBox1.Enter += TextBox1_Enter;
            textBox1.Leave += TextBox1_Leave;
            textBox1.KeyDown += TextBox1_KeyDown;
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                button1.PerformClick();
            }
        }

        private void TextBox1_Enter(object sender, EventArgs e)
        {
            if (textBox1.Text == "Cari Judul / Kode Buku.....")
            {
                textBox1.Text = "";
                textBox1.ForeColor = Color.Black;
            }
        }

        private void TextBox1_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                textBox1.Text = "Cari Judul / Kode Buku.....";
                textBox1.ForeColor = Color.Gray;
                TampilData();
            }
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow baris = this.dataGridView1.Rows[e.RowIndex];

                textBox2.Text = baris.Cells["kode_buku"].Value.ToString();
                textBox7.Text = baris.Cells["judul"].Value.ToString();
                textBox3.Text = baris.Cells["pengarang"].Value.ToString();
                textBox6.Text = baris.Cells["penerbit"].Value.ToString();
                textBox8.Text = baris.Cells["harga"].Value.ToString();
                textBox5.Text = baris.Cells["stok"].Value.ToString();
            }
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