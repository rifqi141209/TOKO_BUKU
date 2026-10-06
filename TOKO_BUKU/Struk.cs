using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace TOKO_BUKU
{
    public class Struk : Form
    {
        // Hanya sisakan komponen yang benar-benar digunakan
        private RichTextBox richTextBox2;
        private Button button1;
        private Button button2;
        private PrintDocument printDocument2;

        private string isiStruk;
        private int indexKarakterCetak = 0;

        public Struk(string teksStruk)
        {
            InitializeComponent();

            // 1. Simpan teks dari kasir ke variabel global
            this.isiStruk = teksStruk;

            // 2. Tampilkan teks di RichTextBox
            richTextBox2.Font = new Font("Consolas", 9, FontStyle.Regular);
            richTextBox2.Text = teksStruk;

            // 3. Hubungkan tombol dengan fungsinya
            button1.Click += TombolCetak_Click;
            button2.Click += TombolTutup_Click;

            // 4. Hubungkan event PrintDocument
            printDocument2.BeginPrint += Proses_MulaiCetak;
        }

        private void InitializeComponent()
        {
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.printDocument2 = new System.Drawing.Printing.PrintDocument();
            this.SuspendLayout();
            // 
            // richTextBox2
            // 
            this.richTextBox2.BackColor = System.Drawing.Color.White;
            this.richTextBox2.Location = new System.Drawing.Point(12, 12);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.ReadOnly = true;
            this.richTextBox2.Size = new System.Drawing.Size(296, 391);
            this.richTextBox2.TabIndex = 1;
            this.richTextBox2.Text = "";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 420);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 32);
            this.button1.TabIndex = 2;
            this.button1.Text = "Print";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(222, 420);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(86, 32);
            this.button2.TabIndex = 3;
            this.button2.Text = "Tutup";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // printDocument2
            // 
            this.printDocument2.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument2_PrintPage);
            // 
            // Struk
            // 
            this.ClientSize = new System.Drawing.Size(320, 464);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.richTextBox2);
            this.Name = "Struk";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Cetak Struk";
            this.ResumeLayout(false);

        }

        private void TombolCetak_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDialog dialogCetak = new PrintDialog())
                {
                    dialogCetak.Document = printDocument2; // Pastikan mengarah ke printDocument2

                    if (dialogCetak.ShowDialog(this) == DialogResult.OK)
                    {
                        printDocument2.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencetak struk: " + ex.Message, "Error Cetak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void TombolTutup_Click(object sender, EventArgs e)
        {
            this.Close(); // Menutup form struk
        }

        private void Proses_MulaiCetak(object sender, PrintEventArgs e)
        {
            indexKarakterCetak = 0; // Reset hitungan karakter setiap kali mulai print
        }

        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Font fontCetak = new Font("Consolas", 9)) // Font disamakan dengan UI
            {
                // Margin default Windows terlalu besar untuk struk (kertas kecil), kita set manual ke 10
                float marginKiri = 10;
                float marginAtas = 10;

                int karakter = 0;
                int baris = 0;

                string sisaTeks = isiStruk.Substring(indexKarakterCetak);

                if (string.IsNullOrEmpty(sisaTeks))
                {
                    e.HasMorePages = false;
                    return;
                }

                // Hitung batas area kertas
                SizeF areaCetak = new SizeF(e.PageBounds.Width - 20, e.PageBounds.Height - 20);

                e.Graphics.MeasureString(sisaTeks, fontCetak,
                    areaCetak, StringFormat.GenericTypographic, out karakter, out baris);

                string teksHalamanIni = sisaTeks.Substring(0, Math.Min(karakter, sisaTeks.Length));

                e.Graphics.DrawString(teksHalamanIni, fontCetak, Brushes.Black,
                    new RectangleF(marginKiri, marginAtas, areaCetak.Width, areaCetak.Height),
                    StringFormat.GenericTypographic);

                indexKarakterCetak += karakter;
                e.HasMorePages = indexKarakterCetak < isiStruk.Length;
            }
        }
    }
}