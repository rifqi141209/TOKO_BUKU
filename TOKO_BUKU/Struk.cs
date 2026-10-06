using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace TOKO_BUKU
{
    public class Struk : Form
    {
        
        private RichTextBox richTextBox1;
        private Button buttonPrint;
        private Button buttonClose;
        private PrintDocument printDocument1;

      
        private string isiStruk;
        private int indexKarakterCetak = 0;

        public Struk(string teksTruk)
        {
            InitializeComponent();
            isiStruk = teksTruk ?? "";
            richTextBox1.Text = isiStruk;         
            buttonClose.Click += (sender, e) => this.Close();
            buttonPrint.Click += TombolCetak_Click;
            printDocument1.BeginPrint += Proses_MulaiCetak;
            printDocument1.PrintPage += Proses_CetakHalaman;
        }

        private void InitializeComponent()
        {
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.buttonPrint = new System.Windows.Forms.Button();
            this.buttonClose = new System.Windows.Forms.Button();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.BackColor = System.Drawing.Color.White;
            this.richTextBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.richTextBox1.Font = new System.Drawing.Font("Consolas", 10F);
            this.richTextBox1.Location = new System.Drawing.Point(6, 12);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.Size = new System.Drawing.Size(320, 420);
            this.richTextBox1.TabIndex = 0;
            this.richTextBox1.Text = "";
            this.richTextBox1.WordWrap = false;
            // 
            // buttonPrint
            // 
            this.buttonPrint.Location = new System.Drawing.Point(12, 444);
            this.buttonPrint.Name = "buttonPrint";
            this.buttonPrint.Size = new System.Drawing.Size(100, 30);
            this.buttonPrint.TabIndex = 1;
            this.buttonPrint.Text = "Cetak";
            // 
            // buttonClose
            // 
            this.buttonClose.Location = new System.Drawing.Point(222, 444);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(100, 30);
            this.buttonClose.TabIndex = 2;
            this.buttonClose.Text = "Tutup";
            // 
            // Struk
            // 
            this.ClientSize = new System.Drawing.Size(335, 486);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.buttonClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "Struk";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pratinjau Struk";
            this.ResumeLayout(false);

        }

      
        private void TombolCetak_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDialog dialogCetak = new PrintDialog())
                {
                    dialogCetak.Document = printDocument1;

                    if (dialogCetak.ShowDialog(this) == DialogResult.OK)
                    {
                        printDocument1.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal mencetak struk: " + ex.Message, "Error Cetak", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

       
        private void Proses_MulaiCetak(object sender, PrintEventArgs e)
        {
            indexKarakterCetak = 0;
        }

      
        private void Proses_CetakHalaman(object sender, PrintPageEventArgs e)
        {
            using (Font fontCetak = new Font("Consolas", 10))
            {
                float marginKiri = e.MarginBounds.Left;
                float marginAtas = e.MarginBounds.Top;
                int karakter = 0;
                int baris = 0;

               
                string sisaTeks = isiStruk.Substring(indexKarakterCetak);

                if (string.IsNullOrEmpty(sisaTeks))
                {
                    e.HasMorePages = false;
                    return;
                }

                e.Graphics.MeasureString(sisaTeks, fontCetak,
                    new SizeF(e.MarginBounds.Width, e.MarginBounds.Height),
                    StringFormat.GenericTypographic, out karakter, out baris);

                string teksHalamanIni = sisaTeks.Substring(0, Math.Min(karakter, sisaTeks.Length));

                e.Graphics.DrawString(teksHalamanIni, fontCetak, Brushes.Black,
                    new RectangleF(marginKiri, marginAtas, e.MarginBounds.Width, e.MarginBounds.Height),
                    StringFormat.GenericTypographic);

                indexKarakterCetak += karakter;
                e.HasMorePages = indexKarakterCetak < isiStruk.Length;
            }
        }
    }
}