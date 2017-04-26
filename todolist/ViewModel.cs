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



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}