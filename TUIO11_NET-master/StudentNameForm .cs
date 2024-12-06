using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Forms;


    public class StudentNameForm : Form
    {
        private TextBox nameTextBox;
        private Button saveButton;

        public string StudentName { get; private set; }

        public StudentNameForm()
        {
            // Initialize components
            Text = "Enter Student Name";
            Width = 300;
            Height = 150;

            Label nameLabel = new Label
            {
                Text = "Student Name:",
                Top = 20,
                Left = 20,
                Width = 100
            };
            Controls.Add(nameLabel);

            nameTextBox = new TextBox
            {
                Top = 20,
                Left = 130,
                Width = 130
            };
            Controls.Add(nameTextBox);

            saveButton = new Button
            {
                Text = "Save",
                Top = 60,
                Left = 100,
                Width = 80
            };
            saveButton.Click += SaveButton_Click;
            Controls.Add(saveButton);
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            StudentName = nameTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(StudentName))
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Please enter a valid name.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

