using System;
using System.Windows.Forms;

namespace NotesApp
{
    public partial class EditForm : Form
    {
        private Note currentNote;

        public EditForm(Note note)
        {
            InitializeComponent();

            currentNote = note;

            cmbPriority.Items.Add("Высокий");
            cmbPriority.Items.Add("Средний");
            cmbPriority.Items.Add("Низкий");

            txtTitle.Text = note.Title;
            dtpReminderTime.Value = note.ReminderTime == DateTime.MinValue ? DateTime.Now : note.ReminderTime;
            cmbPriority.SelectedItem = string.IsNullOrEmpty(note.Priority) ? "Средний" : note.Priority;
            chkCompleted.Checked = note.IsCompleted;
            rtbDescription.Text = note.Description; 

            btnSave.Click += BtnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            this.Text = string.IsNullOrEmpty(note.Title) ? "Добавление заметки" : "Редактирование заметки";
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Введите название заметки!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            currentNote.Title = txtTitle.Text;
            currentNote.ReminderTime = dtpReminderTime.Value;
            currentNote.Priority = cmbPriority.SelectedItem.ToString();
            currentNote.IsCompleted = chkCompleted.Checked;
            currentNote.Description = rtbDescription.Text;  

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}