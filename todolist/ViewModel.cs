using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using todolist.Annotations;
using System.Linq;
using System.Windows;

namespace todolist
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly IFileService _fileService;

        private RelayCommand _addCommand;
        private RelayCommand _openCommand;
        private RelayCommand _removeDoneCommand;
        private ObservableCollection<TodoItem> _taskList;
        private string _newItemText;
        private string _progressText;

        #region Binding Commands
        /// <summary>
        /// binding command for adding <see cref="TodoItem"/> to tasklist
        /// </summary>
        public RelayCommand AddCommand
        {
            get
            {
                return _addCommand ??
                       (_addCommand = new RelayCommand(obj =>
                       {
                           AddToList();
                       }, obj => !string.IsNullOrWhiteSpace(_newItemText)));
            }
        }

        private void AddToList()
        {
            if (string.IsNullOrWhiteSpace(_newItemText)) return;
            TodoItem item = new TodoItem(_newItemText);
            TaskList.Add(item);
            NewItem = string.Empty;
            Item_PropertyChanged(null, null);
        }

        /// <summary>
        /// Binding command for opening tasklist from file
        /// </summary>
        public RelayCommand OpenCommand
        {
            get
            {
                return _openCommand ??
                       (_openCommand = new RelayCommand(obj =>
                       {
                           OpenListFromFile();
                       }));
            }
        }

        /// <summary>
        /// Binding command to remove all done tasks from tasklist
        /// </summary>
        public RelayCommand RemoveDoneCommand
        {
            get
            {
                return _removeDoneCommand ??
                       (_removeDoneCommand = new RelayCommand(obj =>
                       {
                           var list = _taskList.Where(i => !i.Finished).ToList();
                           _taskList.Clear();
                           foreach (TodoItem item in list)
                               _taskList.Add(item);
                           Item_PropertyChanged(null, null);
                       }, obj => _taskList.Any(i => i.Finished)));
            }
        }
        #endregion

        #region Binding Properties
        /// <summary>
        /// Tasklist itself. A collection of <see cref="TodoItem"/>s
        /// </summary>
        public ObservableCollection<TodoItem> TaskList
        {
            get => _taskList;
            set
            {
                _taskList = value;
                OnPropertyChanged(nameof(TaskList));
            }
        }

        /// <summary>
        /// text of new item to be added to tasklist
        /// </summary>
        public string NewItem
        {
            get => _newItemText;
            set
            {
                _newItemText = value; 
                OnPropertyChanged(nameof(NewItem));
            }
        }

        /// <summary>
        /// Shows count of finished tasks in tasklist
        /// </summary>
        public int DoneTasks
        {
            get => _taskList.Count(i => i.Finished);
            set => OnPropertyChanged(nameof(DoneTasks));
        }

        /// <summary>
        /// Shows count of all tasks in tasklist
        /// </summary>
        public int OveralTasks
        {
            get => _taskList.Count > 1 ? _taskList.Count : 1;
            set => OnPropertyChanged(nameof(OveralTasks));
        }

        /// <summary>
        /// Shows if tasklist is empty.
        /// </summary>
        public bool IsEmpty
        {
            // TODO: revert convertor logic
            get => _taskList.Count == 0;
            set => OnPropertyChanged(nameof(IsEmpty));
        }

        /// <summary>
        /// Text to show on progress bar
        /// </summary>
        public string ProgressText
        {
            get => _progressText;
            set
            {
                _progressText = value;
                OnPropertyChanged(nameof(ProgressText));
            }
        }

        public string FileName { get; set; }

        #endregion

        /// <summary>
        /// Creates ViewModel for xaml view.
        /// All logic is here.
        /// </summary>
        /// <param name="fileName">Full name of file where to save tasklist</param>
        /// <param name="fileService">Serializer implementation</param>
        public ViewModel(string fileName, IFileService fileService)
        {
            FileName = fileName;
            _fileService = fileService;

            _taskList = new ObservableCollection<TodoItem>();
            _taskList.CollectionChanged += TaskList_CollectionChanged;
        }

        /// <summary>
        /// Removes selected <see cref="TodoItem"/> from tasklist
        /// </summary>
        /// <param name="obj">TodoItem object</param>
        public void RemoveItem(object obj)
        {
            TodoItem item = obj as TodoItem;
            if (item == null) return;
            _taskList.Remove(item);
            Item_PropertyChanged(null, null);
        }

        /// <summary>
        /// Opens file with tasklist
        /// </summary>
        public void OpenListFromFile()
        {
            if (string.IsNullOrWhiteSpace(FileName)) return;
            try
            {
                var items = _fileService.Open(FileName);
                _taskList.Clear();
                foreach (TodoItem item in items)
                    _taskList.Add(item);
                Item_PropertyChanged(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Serializes <see cref="TaskList"/> and saves it to file
        /// </summary>
        private void SaveFile()
        {
            try
            {
                _fileService.Save(FileName, _taskList.ToList());
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        /// <summary>
        /// Fired every time when <see cref="TaskList"/> item is changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TaskList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems!=null)
                foreach (TodoItem item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            if (e.OldItems!=null)
                foreach (TodoItem item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            SaveFile();
        }

        /// <summary>
        /// Fired every time when any property of each <see cref="TodoItem"/> is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var count = _taskList.Count;
            DoneTasks = _taskList.Count(i => i.Finished);
            OveralTasks = count > 1 ? count : 1;
            IsEmpty = count == 0;
            ProgressText = count == 0 ? "Add tasks via \"Add\" button" : $"{DoneTasks} of {OveralTasks} tasks done";
            SaveFile();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}