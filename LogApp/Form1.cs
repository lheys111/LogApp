using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using NotesApp;

namespace NotesApp
{
    public partial class Form1 : Form
    {
        private List<Note> allNotes = new List<Note>();
        private List<Note> filteredNotes = new List<Note>();
        private string dataFile = "notes.json";

        public Form1()
        {
            InitializeComponent();
            this.Text = "Менеджер личных заметок с напоминаниями";

            SetupDataGridView();

            cmbPriorityFilter.Items.Add("Все");
            cmbPriorityFilter.Items.Add("Высокий");
            cmbPriorityFilter.Items.Add("Средний");
            cmbPriorityFilter.Items.Add("Низкий");
            cmbPriorityFilter.SelectedIndex = 0;

            btnAdd.Click += BtnAdd_Click;
            btnEdit.Click += BtnEdit_Click;
            btnDelete.Click += BtnDelete_Click;
            btnDeleteCompleted.Click += BtnDeleteCompleted_Click;
            btnRefresh.Click += BtnRefresh_Click;
            cmbPriorityFilter.SelectedIndexChanged += ApplyFilter;
            chkShowIncomplete.CheckedChanged += ApplyFilter;
            txtSearch.TextChanged += ApplyFilter;
            timerReminder.Tick += TimerReminder_Tick;

            LoadData();

            Timer timeTimer = new Timer();
            timeTimer.Interval = 1000;
            timeTimer.Tick += (s, e) => lblTime.Text = $"Время: {DateTime.Now:HH:mm:ss}";
            timeTimer.Start();

            Logger.Write("Запуск приложения");
        }

        private void SetupDataGridView()
        {
            dgvNotes.AutoGenerateColumns = false;
            dgvNotes.Columns.Clear();

            DataGridViewTextBoxColumn colTitle = new DataGridViewTextBoxColumn();
            colTitle.Name = "Title";
            colTitle.HeaderText = "Название заметки";
            colTitle.DataPropertyName = "Title";
            colTitle.Width = 200;
            dgvNotes.Columns.Add(colTitle);

            DataGridViewTextBoxColumn colTime = new DataGridViewTextBoxColumn();
            colTime.Name = "ReminderTime";
            colTime.HeaderText = "Дата и время";
            colTime.DataPropertyName = "ReminderTime";
            colTime.Width = 150;
            dgvNotes.Columns.Add(colTime);

            DataGridViewTextBoxColumn colPriority = new DataGridViewTextBoxColumn();
            colPriority.Name = "Priority";
            colPriority.HeaderText = "Приоритет";
            colPriority.DataPropertyName = "Priority";
            colPriority.Width = 100;
            dgvNotes.Columns.Add(colPriority);

            DataGridViewCheckBoxColumn colStatus = new DataGridViewCheckBoxColumn();
            colStatus.Name = "IsCompleted";
            colStatus.HeaderText = "Выполнено";
            colStatus.DataPropertyName = "IsCompleted";
            colStatus.Width = 80;
            dgvNotes.Columns.Add(colStatus);

            dgvNotes.CellFormatting += DgvNotes_CellFormatting;
        }

        private void DgvNotes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvNotes.Rows[e.RowIndex].DataBoundItem != null)
            {
                Note note = dgvNotes.Rows[e.RowIndex].DataBoundItem as Note;
                if (note != null)
                {
                    if (note.Priority == "Высокий")
                        dgvNotes.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightCoral;
                    else if (note.Priority == "Средний")
                        dgvNotes.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightYellow;
                    else if (note.Priority == "Низкий")
                        dgvNotes.Rows[e.RowIndex].DefaultCellStyle.BackColor = System.Drawing.Color.LightGreen;

                    if (note.IsCompleted)
                        dgvNotes.Rows[e.RowIndex].DefaultCellStyle.ForeColor = System.Drawing.Color.Gray;
                }
            }
        }

        private void ApplyFilter(object sender, EventArgs e)
        {
            var query = allNotes.AsEnumerable();

            if (cmbPriorityFilter.SelectedItem != null && cmbPriorityFilter.SelectedItem.ToString() != "Все")
            {
                string priority = cmbPriorityFilter.SelectedItem.ToString();
                query = query.Where(n => n.Priority == priority);
            }

            if (chkShowIncomplete.Checked)
            {
                query = query.Where(n => !n.IsCompleted);
            }

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string search = txtSearch.Text.ToLower();
                query = query.Where(n => n.Title.ToLower().Contains(search));
            }

            filteredNotes = query.ToList();
            dgvNotes.DataSource = null;
            dgvNotes.DataSource = filteredNotes;

            lblNoteCount.Text = $"Заметок: {filteredNotes.Count} (всего: {allNotes.Count})";
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(dataFile))
                {
                    string json = File.ReadAllText(dataFile);
                    allNotes = JsonConvert.DeserializeObject<List<Note>>(json) ?? new List<Note>();
                    Logger.Write($"Загрузка данных, загружено {allNotes.Count} заметок");
                }
                else
                {
                    allNotes = new List<Note>();
                    Logger.Write("Файл не найден, создан пустой список");
                }
                ApplyFilter(null, null);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Ошибка загрузки: {ex.Message}");
                MessageBox.Show("Ошибка загрузки: " + ex.Message);
                allNotes = new List<Note>();
                ApplyFilter(null, null);
            }
        }

        private void SaveData()
        {
            try
            {
                string json = JsonConvert.SerializeObject(allNotes, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                Logger.WriteError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            Note newNote = new Note();
            EditForm editForm = new EditForm(newNote);

            if (editForm.ShowDialog() == DialogResult.OK)
            {
                allNotes.Add(newNote);
                SaveData();
                ApplyFilter(null, null);
                Logger.Write($"Добавлена заметка: {newNote.Title}");
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (dgvNotes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заметку!");
                return;
            }

            Note selectedNote = dgvNotes.SelectedRows[0].DataBoundItem as Note;
            if (selectedNote != null)
            {
                string oldTitle = selectedNote.Title;
                EditForm editForm = new EditForm(selectedNote);

                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    SaveData();
                    ApplyFilter(null, null);
                    Logger.Write($"Редактирование: '{oldTitle}' → '{selectedNote.Title}'");
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvNotes.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите заметку!");
                return;
            }

            Note selectedNote = dgvNotes.SelectedRows[0].DataBoundItem as Note;
            if (selectedNote != null)
            {
                if (MessageBox.Show($"Удалить '{selectedNote.Title}'?", "Подтверждение",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    allNotes.Remove(selectedNote);
                    SaveData();
                    ApplyFilter(null, null);
                    Logger.Write($"Удалена заметка: {selectedNote.Title}");
                }
            }
        }

        private void BtnDeleteCompleted_Click(object sender, EventArgs e)
        {
            int count = allNotes.Count(n => n.IsCompleted);
            if (count == 0)
            {
                MessageBox.Show("Нет выполненных заметок!");
                return;
            }

            if (MessageBox.Show($"Удалить {count} выполненных заметок?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                allNotes.RemoveAll(n => n.IsCompleted);
                SaveData();
                ApplyFilter(null, null);
                Logger.Write($"Удалено {count} выполненных заметок");
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            Logger.Write("Обновление списка из файла");
        }

        private void TimerReminder_Tick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            var reminders = allNotes.Where(n => !n.IsCompleted && n.ReminderTime <= now && !n.ReminderShown).ToList();

            foreach (var note in reminders)
            {
                MessageBox.Show($"Напоминание!\n\n{note.Title}\n{note.Description}",
                    "Напоминание", MessageBoxButtons.OK, MessageBoxIcon.Information);
                note.ReminderShown = true;
                Logger.Write($"Сработало напоминание: {note.Title}");
            }
        }
    }
}