using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using todolist.Annotations;

namespace todolist
{
    public class TodoItem : INotifyPropertyChanged
    {
        private string _text;
        private bool _finished;

        [Description("Text of the ToDo item")]
        [Category("ToDo Fields")]
        [XmlElement("text")]
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        [Description("Finished state of ToDo item")]
        [Category("ToDo Fields")]
        [XmlElement("finished")]
        public bool Finished
        {
            get => _finished;
            set
            {
                _finished = value;
                OnPropertyChanged(nameof(Finished));
            }
        }

        public TodoItem() { }

        public TodoItem(string text)
        {
            _text = text;
            _finished = false;
        }

        public TodoItem(string text, bool finished)
        {
            _text = text;
            _finished = finished;
        }

        public override string ToString()
        {
            return _text;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
