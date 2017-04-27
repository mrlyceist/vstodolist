using System.Collections.Generic;

namespace todolist
{
    public interface IFileService
    {
        List<TodoItem> Open(string fileName);
        void Save(string fileName, List<TodoItem> items);
    }
}