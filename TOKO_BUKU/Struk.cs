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
        private string receiptText;
        private int printCharIndex = 0;

        public Struk(string text)
        {
            receiptText = text ?? string.Empty;
            InitializeComponent();
            richTextBox1.Text = receiptText;
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
            this.Text = "Struk";
            this.ResumeLayout(false);

        }

        private void NewMethod()
        {
            this.buttonClose.Click += (s, e) => this.Close();
        }

        private void ButtonPrint_Click(object sender, EventArgs e)
        {
            try
            {
                using (PrintDialog dlg = new PrintDialog())
                {
                    dlg.Document = printDocument1;
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        printDocument1.Print();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print gagal: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintDocument1_BeginPrint(object sender, PrintEventArgs e)
        {
            printCharIndex = 0;
        }

        private void PrintDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            using (Font printFont = new Font("Consolas", 10))
            {
                float leftMargin = e.MarginBounds.Left;
                float topMargin = e.MarginBounds.Top;
                int charsFitted = 0;
                int linesFilled = 0;

                string remaining = receiptText.Substring(printCharIndex);
                if (string.IsNullOrEmpty(remaining))
                {
                    e.HasMorePages = false;
                    return;
                }

                // Measure how many characters fit
                e.Graphics.MeasureString(remaining, printFont, new SizeF(e.MarginBounds.Width, e.MarginBounds.Height), StringFormat.GenericTypographic, out charsFitted, out linesFilled);

                string pageText = remaining.Substring(0, Math.Min(charsFitted, remaining.Length));
                e.Graphics.DrawString(pageText, printFont, Brushes.Black, new RectangleF(leftMargin, topMargin, e.MarginBounds.Width, e.MarginBounds.Height), StringFormat.GenericTypographic);

                printCharIndex += charsFitted;
                e.HasMorePages = printCharIndex < receiptText.Length;
            }
        }
    }
}
