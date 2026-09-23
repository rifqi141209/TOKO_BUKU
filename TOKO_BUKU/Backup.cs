using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TOKO_BUKU;

namespace TOKO_BUKU
{
    public partial class Backup : Form
    {
        // Sesuaikan dengan nama database MySQL kamu
        string connectionString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

        public Backup()
        {
            InitializeComponent();

            // Hubungkan event tombol secara manual
            button1.Click += button1_Click; // Tombol Proses Backup (.sql)
            button2.Click += button2_Click; // Tombol Pilih & Restore File
            button3.Click += button3_Click; // Tombol Logout

            // Hubungkan event menu navigasi sidebar
            label1.Click += label1_Click; // Dashboard
            label5.Click += label5_Click; // Data Buku
            label6.Click += label6_Click; // Laporan Penjualan
            label7.Click += label7_Click; // Backup (Tetap di sini)
        }


        private void button1_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "SQL Backup File (*.sql)|*.sql";
                saveFileDialog.Title = "Simpan File Backup Database";
                saveFileDialog.FileName = $"Backup_TokoBuku_{DateTime.Now:yyyyMMdd_HHmmss}.sql";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            using (MySqlCommand cmd = new MySqlCommand())
                            {
                                using (MySqlBackup mb = new MySqlBackup(cmd))
                                {
                                    cmd.Connection = conn;
                                    conn.Open();
                                    mb.ExportToFile(saveFileDialog.FileName);
                                    conn.Close();
                                }
                            }
                        }

                        MessageBox.Show("Backup data berhasil disimpan!", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Gagal melakukan backup: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "SQL Backup File (*.sql)|*.sql";
                openFileDialog.Title = "Pilih File Backup untuk Dipulihkan";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    DialogResult konfirmasi = MessageBox.Show(
                        "Memulihkan data akan menimpa data yang ada saat ini. Apakah Anda yakin?",
                        "Konfirmasi Restore",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (konfirmasi == DialogResult.Yes)
                    {
                        try
                        {
                            using (MySqlConnection conn = new MySqlConnection(connectionString))
                            {
                                using (MySqlCommand cmd = new MySqlCommand())
                                {
                                    using (MySqlBackup mb = new MySqlBackup(cmd))
                                    {
                                        cmd.Connection = conn;
                                        conn.Open();
                                        mb.ImportFromFile(openFileDialog.FileName);
                                        conn.Close();
                                    }
                                }
                            }

                            MessageBox.Show("Data berhasil dipulihkan.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("File tidak valid atau gagal memulihkan data: " + ex.Message, "Kesalahan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
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

        private void label1_Click(object sender, EventArgs e) { PindahHalaman<Admin_dashboard>(); }
        private void label5_Click(object sender, EventArgs e) { PindahHalaman<Data_buku>(); }
        private void label6_Click(object sender, EventArgs e) { PindahHalaman<Laporan_penjualan>(); }
        private void label7_Click(object sender, EventArgs e) { PindahHalaman<Backup>(); }

        // Tombol Logout
        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                var login = new Login();
                login.StartPosition = FormStartPosition.CenterScreen;
                login.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Tidak Bisa Logout: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Penutup Aplikasi Otomatis
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            Application.Exit();
        }
    }
}