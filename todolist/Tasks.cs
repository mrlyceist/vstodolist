using System.Collections.Generic;
using System.Xml.Serialization;

namespace todolist
{
    [XmlRoot("Tasks")]
    internal class Tasks
    {
        [XmlElement("item")]
        public List<TodoItem> Items { get; set; }
    }
}