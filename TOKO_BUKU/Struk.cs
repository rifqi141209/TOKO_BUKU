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
            this.richTextBox1 = new RichTextBox();
            this.buttonPrint = new Button();
            this.buttonClose = new Button();
            this.printDocument1 = new PrintDocument();

            this.SuspendLayout();


            this.richTextBox1.Font = new Font("Consolas", 10);
            this.richTextBox1.Location = new Point(12, 12);
            this.richTextBox1.Size = new Size(320, 420);
            this.richTextBox1.ReadOnly = true;
            this.richTextBox1.BorderStyle = BorderStyle.FixedSingle;
            this.richTextBox1.BackColor = Color.White;
            this.richTextBox1.WordWrap = false;


            this.buttonPrint.Text = "Cetak";
            this.buttonPrint.Location = new Point(12, 444);
            this.buttonPrint.Size = new Size(100, 30);
            this.buttonPrint.Click += ButtonPrint_Click;


            this.buttonClose.Text = "Tutup";
            this.buttonClose.Location = new Point(232, 444);
            this.buttonClose.Size = new Size(100, 30);
            NewMethod();

            this.printDocument1.BeginPrint += PrintDocument1_BeginPrint;
            this.printDocument1.PrintPage += PrintDocument1_PrintPage;


            this.ClientSize = new Size(344, 486);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.buttonPrint);
            this.Controls.Add(this.buttonClose);
            this.Text = "Struk";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

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
