using System;

namespace NotesApp
{
    public class Note
    {
        public string Title { get; set; }
        public DateTime ReminderTime { get; set; }
        public string Priority { get; set; }
        public bool IsCompleted { get; set; }
        public string Description { get; set; }
        public bool ReminderShown { get; set; }  

        public Note()
        {
            Title = "";
            ReminderTime = DateTime.Now;
            Priority = "Средний";
            IsCompleted = false;
            Description = "";
            ReminderShown = false;  
        }
    }
}