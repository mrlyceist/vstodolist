using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using todolist.Annotations;

namespace todolist
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly IFileService _fileService;

        //private TodoItem _selectedItem;

        private RelayCommand _addCommand;
        private string _newItemName;
        private ObservableCollection<TodoItem> _taskList;
        private string _fileName;

        public RelayCommand AddCommand
        {
            get
            {
                return _addCommand??
                       (_addCommand = new RelayCommand(obj =>
                       {
                           if (string.IsNullOrWhiteSpace(NewItemName)) return;
                           TodoItem item = new TodoItem(NewItemName);
                           TaskList.Add(item);
                           NewItemName = string.Empty;
                           //Item
                       }))
            }
        }

        public ObservableCollection<TodoItem> TaskList
        {
            get => _taskList;
            set
            {
                _taskList = value;
                OnPropertyChanged(nameof(TaskList));
            }
        }

        public string NewItemName
        {
            get => _newItemName;
            set
            {
                _newItemName = value; 
                OnPropertyChanged(nameof(NewItemName));
            }
        }

        public ViewModel(string fileName, IFileService fileService)
        {
            _fileName = fileName;
            _fileService = fileService;

            _taskList = new ObservableCollection<TodoItem>();
            _taskList.CollectionChanged += _taskList_CollectionChanged;
        }

        private void _taskList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems!=null)
                foreach (TodoItem item in e.NewItems)
                    //item
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}