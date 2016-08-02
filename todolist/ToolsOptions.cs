using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace todolist
{
    class ToolsOptions : DialogPage
    {
        private double daysAhead;

        public double DaysAhead
        {
            get { return daysAhead; }
            set { daysAhead = value; }
        }
    }
}
