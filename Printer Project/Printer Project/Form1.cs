namespace Printer_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            listBox1 = new ListBox();
            button1 = new Button();
            button2 = new Button();
            textBox1 = new TextBox();
            backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            comboBox1 = new ComboBox();
            panel1 = new Panel();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(12, 88);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(445, 564);
            listBox1.TabIndex = 0;
            // 
            // button1
            // 
            button1.Location = new Point(12, 5);
            button1.Name = "button1";
            button1.Size = new Size(215, 38);
            button1.TabIndex = 1;
            button1.Text = "button1";
            button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            button2.Location = new Point(279, 5);
            button2.Name = "button2";
            button2.Size = new Size(178, 38);
            button2.TabIndex = 2;
            button2.Text = "button2";
            button2.UseVisualStyleBackColor = true;
            // 
            // textBox1
            // 
            textBox1.Location = new Point(12, 49);
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(288, 27);
            textBox1.TabIndex = 3;
            // 
            // comboBox1
            // 
            comboBox1.FormattingEnabled = true;
            comboBox1.Location = new Point(306, 48);
            comboBox1.Name = "comboBox1";
            comboBox1.Size = new Size(151, 28);
            comboBox1.TabIndex = 4;
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
            // 
            // panel1
            // 
            panel1.Location = new Point(463, 88);
            panel1.Name = "panel1";
            panel1.Size = new Size(453, 564);
            panel1.TabIndex = 5;
            panel1.Paint += panel1_Paint;
            // 
            // Form1
            // 
            ClientSize = new Size(928, 664);
            Controls.Add(panel1);
            Controls.Add(comboBox1);
            Controls.Add(textBox1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(listBox1);
            Name = "Form1";
            ResumeLayout(false);
            PerformLayout();
        }

        private ListBox listBox1;
        private Button button1;
        private Button button2;
        private TextBox textBox1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private ComboBox comboBox1;

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private Panel panel1;

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
