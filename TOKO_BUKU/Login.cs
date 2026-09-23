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

namespace TOKO_BUKU
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {

            string connString = "server=localhost;database=TOKO_BUKU;uid=root;pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connString))
            {
                try
                {
                    conn.Open();

                    string query = "SELECT role FROM user WHERE username = @username AND password = @password";

                    MySqlCommand cmd = new MySqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@password", txtPassword.Text);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["role"].ToString();


                            if (role == "admin")
                            {
                                MessageBox.Show("Login berhasil sebagai Admin!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);

                                // Arahkan ke Form Admin
                                Admin_dashboard formAdmin = new Admin_dashboard();

                                formAdmin.FormClosed += (s, args) => this.Close();
                                formAdmin.Show();
                                this.Hide();
                            }
                            else if (role == "kasir")
                            {
                                MessageBox.Show("Login berhasil sebagai Kasir!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);


                                Kasir formUser = new Kasir();

                                formUser.FormClosed += (s, args) => this.Close();
                                formUser.Show();
                                this.Hide();
                            }
                        }
                        else
                        {
                            MessageBox.Show("Username atau Password salah!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Koneksi database gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }
    }
}