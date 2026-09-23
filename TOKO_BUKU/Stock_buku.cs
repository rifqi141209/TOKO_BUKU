using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TOKO_BUKU;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;x

namespace TOKO_BUKU
{
    public partial class Stock_buku : Form
    {
        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

        public Stock_buku()
        {
            InitializeComponent();
            TampilDataStok();

            button1.Click += button1_Click;
            dataGridView1.CellClick += dataGridView1_CellClick;

            label1.Click += label1_Click;
            label5.Click += label5_Click;
            button3.Click += button3_Click;
        }

        private void TampilDataStok()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "SELECT kode_buku, judul, pengarang, penerbit, harga, stok FROM books";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dataGridView1.DataSource = dt;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal memuat data stok: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
            {
                MessageBox.Show("Kode Buku dan Stok baru wajib diisi!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string query = "UPDATE books SET stok = @stok WHERE kode_buku = @kode";
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@stok", int.Parse(textBox3.Text));
                        cmd.Parameters.AddWithValue("@kode", textBox2.Text);

                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            MessageBox.Show("Stok buku berhasil diperbarui!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            textBox2.Clear();
                            textBox3.Clear();
                            TampilDataStok();
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
                MessageBox.Show("Gagal memperbarui stok: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];

                if (row.Cells["kode_buku"].Value != null && row.Cells["stok"].Value != null)
                {
                    textBox2.Text = row.Cells["kode_buku"].Value.ToString();
                    textBox3.Text = row.Cells["stok"].Value.ToString();
                }
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
            PindahHalaman<Kasir>();
        }

        private void label5_Click(object sender, EventArgs e)
        {
            PindahHalaman<Stock_buku>();
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