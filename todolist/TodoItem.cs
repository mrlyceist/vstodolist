using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.VisualStudio.Shell.Interop;

namespace todolist
{
    public class TodoItem
    {
        //private TodoWindowControl parent;
        [Description("Name of the ToDo item")]
        [Category("ToDo Fields")]
        public string Name { get; set; }

        [Description("Finished state of ToDo item")]
        [Category("ToDo Fields")]
        public bool Finished { get; set; }


        public TodoItem(string itemName)
        {
            Name = itemName;
            Finished = false;
        }

        public TodoItem(string itemName, bool done)
        {
            Name = itemName;
            Finished = done;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
