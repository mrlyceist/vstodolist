//------------------------------------------------------------------------------
// <copyright file="TodoWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace todolist
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Interaction logic for TodoWindowControl.
    /// </summary>
    public partial class TodoWindowControl : UserControl
    {
        public TodoWindow parent;
        private Solution solution;
        private string listFile;

        public List<TodoItem> TaskList = new List<TodoItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoWindowControl"/> class.
        /// </summary>
        public TodoWindowControl(TodoWindow window)
        {
            this.InitializeComponent();
            parent = window;

            CheckFile();
        }

        private void CheckFile()
        {
            DTE2 dte = Package.GetGlobalService(typeof(SDTE)) as DTE2;
            solution = dte.Solution;
            if (solution.FullName != string.Empty)
            {
                string solutionDir = System.IO.Path.GetDirectoryName(solution.FullName);
                listFile = Path.Combine(solutionDir, "TaskList.xml");
                buttonAdd.IsEnabled = true;
            }
            else
            {
                buttonAdd.IsEnabled = false;
            }
        }

        internal void UpdateList(TodoItem item)
        {
            //var index = listBox.SelectedIndex;
            //listBox.Items.RemoveAt(index);
            //listBox.Items.Insert(index, item);
            //listBox.SelectedItem = index;
        }

        internal void CheckForErrors()
        {
            //foreach (TodoItem item in listBox.Items.Cast<TodoItem>().Where(item => item.DueDate < DateTime.Now))
            //    ReportError($"ToDo Item is out of date: {item.ToString()}");
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void buttonAdd_Click(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length > 0)
            {
                var item = new TodoItem(this, textBox.Text);
                
                AddItemToList(item);
                TaskList.Add(item);

                var outputWindow = parent.GetVsService(typeof (SVsOutputWindow)) as IVsOutputWindow;
                IVsOutputWindowPane pane;
                Guid guidGeneralPane = VSConstants.GUID_OutWindowGeneralPane;
                outputWindow.GetPane(ref guidGeneralPane, out pane);
                pane?.OutputString($"ToDo Item created: {item.ToString()}\r'n");

                TryCreate(listFile);
                
                TrackSelection();
                CheckForErrors();
            }
        }

        private void RemoveEvent(object sender, RoutedEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var index = Int32.Parse(element.Name.Substring(3));
            listBox.Items.RemoveAt(index);
            TaskList.RemoveAt(index);
            BuildList();
        }

        private void BuildList()
        {
            listBox.Items.Clear();
            foreach (TodoItem item in TaskList)
            {
                AddItemToList(item);
            }
        }

        private void AddItemToList(TodoItem item)
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var txt = new TextBlock {Text = item.ToString()};
            var check = new CheckBox { Content = txt };
            if (item.Finished)
            {
                txt.TextDecorations.Add(TextDecorations.Strikethrough);
                check.IsChecked = true;
            }
            check.Name = $"chk{listBox.Items.Count}";
            check.Checked += Check_Checked;
            check.Unchecked += Check_Unchecked;
            grid.Children.Add(check);
            Grid.SetColumn(grid.Children[0], 0);

            var btn = new Button { Content = "  X  " };
            grid.Children.Add(btn);
            btn.Name = $"btn{listBox.Items.Count}";
            btn.Click += new RoutedEventHandler(RemoveEvent);
            Grid.SetColumn(grid.Children[1], 1);

            listBox.Items.Add(grid);
        }

        private void Check_Unchecked(object sender, RoutedEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var index = Int32.Parse(element.Name.Substring(3));
            TaskList[index].Finished = false;
            BuildList();
        }

        private void Check_Checked(object sender, RoutedEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var index = Int32.Parse(element.Name.Substring(3));
            TaskList[index].Finished = true;
            BuildList();
        }

        private SelectionContainer mySelContainer;
        private System.Collections.ArrayList mySelItems;
        private IVsWindowFrame frame = null;

        private void TrackSelection()
        {
            if (frame == null)
            {
                var shell = parent.GetVsService(typeof(SVsUIShell)) as IVsUIShell;
                if (shell != null)
                {
                    var guidPropertyBrowser = new
                    Guid(ToolWindowGuids.PropertyBrowser);
                    shell.FindToolWindow((uint)__VSFINDTOOLWIN.FTW_fForceCreate,
                    ref guidPropertyBrowser, out frame);
                }
            }
            frame?.Show();
            if (mySelContainer == null)
                mySelContainer = new SelectionContainer();

            mySelItems = new System.Collections.ArrayList();

            var selected = listBox.SelectedItem as TodoItem;
            if (selected != null)
                mySelItems.Add(selected);

            mySelContainer.SelectedObjects = mySelItems;

            ITrackSelection track = parent.GetVsService(typeof(STrackSelection))
                as ITrackSelection;

            track?.OnSelectChange(mySelContainer);
        }

        private void listBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrackSelection();
        }

        private TodoWindowTaskProvider taskProvider;

        private void CreateProvider()
        {
            if (taskProvider==null)
                taskProvider = new TodoWindowTaskProvider(parent) {ProviderName = "To Do"};
        }

        private void ClearError()
        {
            CreateProvider();
            taskProvider.Tasks.Clear();
        }

        private void ReportError(string p)
        {
            CreateProvider();
            var errorTask = new Task
            {
                CanDelete = false,
                Category = TaskCategory.Comments,
                Text = p
            };

            taskProvider.Tasks.Add(errorTask);
            taskProvider.Show();

            var taskList = parent.GetVsService(typeof (SVsTaskList)) as IVsTaskList2;
            if(taskList==null)
                return;

            var guidProvider = typeof (TodoWindowTaskProvider).GUID;
            taskList.SetActiveProvider(ref guidProvider);
        }

        [Guid("72de1eAD-a00c-4f57-bff7-57edb162d0be")]
        public class TodoWindowTaskProvider : TaskProvider
        {
            public TodoWindowTaskProvider(IServiceProvider sp)
                : base(sp)
            {
            } 
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            if (solution.FullName != string.Empty)
            {
                string solutionDir = System.IO.Path.GetDirectoryName(solution.FullName);
                //MessageBox.Show(solutionDir);
                var listFile = Path.Combine(solutionDir, "TaskList.xml");
                TryCreate(listFile);
            }
            else
            {
                MessageBox.Show("WAT");
            }
            

        }

        private void TryCreate(string listFile)
        {
            if (File.Exists(listFile)) return;
            try
            {
                File.Create(listFile);
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }
    }
}