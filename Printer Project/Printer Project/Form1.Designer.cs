using System.IO;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace STLFileLister
{

    public class MainForm : Form
    {
        private TextBox folderPathTextBox;
        private TextBox selectedItemTextBox;
        private Button browseButton;
        private Button listFilesButton;
        private Button sendToPrinterButton;
        private ListBox filesListBox;
        private ComboBox searchParametersDropDown;
        private Panel textPanel1;
        private Panel textPanel2;
        private readonly HttpClient _httpClient;
        private TextBox urlText;
        private TextBox apiText;
        const string ext_Path = @"C:\Users\keyha\source\repos\Printer Project\Printer Project\TextFile1.txt";
        private readonly string _OctoprintAPIKey;
        private readonly string _OctoprintURL;

        public MainForm()
        {
            this.CenterToScreen();
            this.AutoScaleMode = AutoScaleMode.None;
            // Initialize components
            this.MinimumSize = new Size(1000, 400);
            this.Text = "STL File Lister";
            this.Width = 1000;
            this.Height = 400;
            this.BackColor = Color.Black;
            this.ForeColor = Color.Orange;
            this.AutoScaleDimensions = new SizeF(1000, 400);

            urlText = new TextBox();
            apiText = new TextBox();
            apiText.PasswordChar = '*';
            folderPathTextBox = new TextBox { Left = 10, Top = 10, Width = 400 };
            selectedItemTextBox = new TextBox { Left = this.ClientSize.Width - 100, Top = 10, Width = 100 };
            browseButton = new Button { Text = "Browse", Left = 420, Top = 10, Width = 75, Height = folderPathTextBox.Height };
            listFilesButton = new Button { Text = "List Files", Left = 500, Top = 10, Width = 75, Height = folderPathTextBox.Height };
            filesListBox = new ListBox { Left = 10, Top = 40, Width = 560, Height = this.ClientSize.Height - 75 };
            searchParametersDropDown = new ComboBox { Height = browseButton.Height, Top = 10, Width = this.ClientSize.Width / 10, Left = listFilesButton.Left + 80 };
            sendToPrinterButton = new Button { Text = "Send to Printer", Height = browseButton.Height * 2, Top = 10, Width = browseButton.Width * 3, Left = filesListBox.Left + filesListBox.Width + 100 };
            textPanel1 = new Panel();
            textPanel2 = new Panel();
            AdjustControlSizes();
            ComboBoxSetup();
            browseButton.Click += BrowseButton_Click;
            filesListBox.SelectedIndexChanged += SelectItem_in_List;
            filesListBox.TextChanged += ClearSelection;
            filesListBox.TextChanged += ResetText;
            selectedItemTextBox.TextChanged += ResetText;
            listFilesButton.Click += ListFilesButton_Click;
            searchParametersDropDown.SelectedIndexChanged += ListFilesButton_Click;
            this.Resize += MainForm_Resize;
            this.Controls.Add(folderPathTextBox);
            this.Controls.Add(searchParametersDropDown);
            this.Controls.Add(selectedItemTextBox);
            this.Controls.Add(browseButton);
            this.Controls.Add(listFilesButton);
            this.Controls.Add(filesListBox);
            this.Controls.Add(sendToPrinterButton);
            this.Controls.Add(textPanel1);
            this.Controls.Add(textPanel2);
            this.Controls.Add(apiText);
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPathTextBox.Text = folderDialog.SelectedPath;
                }
            }
        }
        private void ResetText(object sender, EventArgs e)
        {
            if (filesListBox.SelectedItem != null)
            {
                selectedItemTextBox.Text = filesListBox.SelectedItem.ToString();
            }
            else
            {
                selectedItemTextBox.Text = "";
            }
        }


        private void MainForm_Resize(object sender, EventArgs e)
        {
            AdjustControlSizes();
        }

        private void ComboBoxSetup()
        {
            searchParametersDropDown.Items.Insert(0, "");
            foreach (var line in File.ReadAllLines(ext_Path))
            {
                if (!line.Contains("3D File Types"))
                {
                    searchParametersDropDown.Items.Add("." + line.ToLower());
                }
            }

        }

        private void SelectItem_in_List(object sender, EventArgs e)
        {
            string str;
            if (filesListBox.SelectedItems == null)
            {
                str = "No Item";
                selectedItemTextBox.Text = str;
            }
            if (filesListBox.SelectedItems != null)
            {
                str = "";
                str = filesListBox.SelectedItem.ToString();
                selectedItemTextBox.Text = str;
            }

        }

        private void ClearSelection(object sender, EventArgs e)
        {
            filesListBox.ClearSelected();
        }
        private void AdjustControlSizes()
        {
            apiText.Left = sendToPrinterButton.Left;
            apiText.Top = filesListBox.Top + filesListBox.Height - apiText.Height;
            apiText.Width = sendToPrinterButton.Width / 2;
            ButtonBorderStyle borderStyle = ButtonBorderStyle.None;
            filesListBox.Width = this.ClientSize.Width / 3 * 2;
            filesListBox.Height = this.ClientSize.Height - this.ClientSize.Height / 8;
            folderPathTextBox.Width = (filesListBox.Width / 3) * 2 - 50;
            browseButton.Left = folderPathTextBox.Width + 20;
            listFilesButton.Left = browseButton.Left + 80;
            searchParametersDropDown.Left = listFilesButton.Left + 80;
            searchParametersDropDown.Width = filesListBox.Width + filesListBox.Left - searchParametersDropDown.Left;
            sendToPrinterButton.Left = filesListBox.Left + filesListBox.Width + 50;
            selectedItemTextBox.Left = sendToPrinterButton.Left + sendToPrinterButton.Width / 2 - selectedItemTextBox.Width / 2;
            selectedItemTextBox.Top = sendToPrinterButton.Top + 50;

            SetButtonColor(sendToPrinterButton);
            SetButtonColor(browseButton);
            SetButtonColor(listFilesButton);
            SetTextBoxColor(selectedItemTextBox, textPanel2);
            SetTextBoxColor(folderPathTextBox, textPanel1);
            filesListBox.BorderStyle = BorderStyle.FixedSingle;
            filesListBox.BackColor = Color.Black;
            filesListBox.ForeColor = Color.Orange;

        }
        private void SetButtonColor(Button button)
        {
            button.FlatAppearance.BorderSize = button.Size.Height / 15;
            button.ForeColor = Color.Orange;
            button.BackColor = Color.Black;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.Orange;
            button.FlatAppearance.MouseDownBackColor = Color.DarkGray;
        }
        private void SetTextBoxColor(TextBox box, Panel border)
        {
            box.ForeColor = Color.Orange;
            box.BackColor = Color.Black;
            box.BorderStyle = BorderStyle.FixedSingle;
            border.Left = box.Left - 1;
            border.Width = box.Width + 2;

            border.Height = box.Height + 2;
            border.Top = box.Top - 1;
            border.ForeColor = Color.Orange;
            border.BackColor = Color.Orange;
            border.BorderStyle = BorderStyle.None;
        }
        private void Form_Paint(object sender, PaintEventArgs e)
        {

        }


        private void ListFilesButton_Click(object sender, EventArgs e)
        {

            filesListBox.Items.Clear();

            string folderPath = folderPathTextBox.Text;

            if (Directory.Exists(folderPath))
            {
                try
                {


                    string[] stlFiles_CurrDirectory = Directory.GetFiles(folderPath, $"*{searchParametersDropDown.SelectedItem}", SearchOption.AllDirectories);
                    foreach (string d in Directory.GetDirectories(folderPath))
                    {
                        string[] stlFiles_SubDirectory = Directory.GetFiles(d, $"*{searchParametersDropDown.SelectedItem}", SearchOption.AllDirectories);
                        foreach (var file in stlFiles_SubDirectory)
                        {
                            filesListBox.Items.Add(Path.GetFileName(file));
                        }
                    }
                    foreach (var file in stlFiles_CurrDirectory)
                    {
                        filesListBox.Items.Add(Path.GetFileName(file));
                    }
                }
                catch
                {

                }

            }
            else
            {
                MessageBox.Show("Please enter a valid folder path.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            filesListBox.TopIndex = 0;
            filesListBox.Sorted = true;
        }
        

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}

