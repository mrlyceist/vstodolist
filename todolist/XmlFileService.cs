using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace todolist
{
    internal class XmlFileService : IFileService
    {
        public List<TodoItem> Open(string fileName)
        {
            List<TodoItem> items = new List<TodoItem>();
            XmlSerializer serializer = new XmlSerializer(typeof(Tasks));
            using (FileStream fs = new FileStream(fileName, FileMode.OpenOrCreate))
            {
                var itemList = serializer.Deserialize(fs) as Tasks;
                items = itemList?.Items;
            }
            return items;
        }

        public void Save(string fileName, List<TodoItem> items)
        {
            var itemList = new Tasks { Items = items };
            XmlSerializer serializer = new XmlSerializer(typeof(Tasks));
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(fs, itemList);
            }
        }
    }
}