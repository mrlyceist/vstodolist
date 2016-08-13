//------------------------------------------------------------------------------
// <copyright file="TodoWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using EnvDTE;
using EnvDTE80;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace todolist
{
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using System;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// Interaction logic for TodoWindowControl.
    /// </summary>
    public partial class TodoWindowControl : UserControl, IVsSolutionEvents
    {
        private Solution solution;
        private string listFile;
        private XDocument xTaskList;
        private XElement xTasks;

        public List<TodoItem> TaskList = new List<TodoItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoWindowControl"/> class.
        /// </summary>
        public TodoWindowControl(TodoWindow window)
        {
            this.InitializeComponent();
            MinWidth = 150;

            uint cookie = 0;
            TodoWindowPackage.theSolution.AdviseSolutionEvents(this, out cookie);
            GetSolution();
        }

        private void GetSolution()
        {
            DTE2 dte = Package.GetGlobalService(typeof (SDTE)) as DTE2;
            solution = dte.Solution;
            if (solution.FullName != string.Empty)
            {
                string solutionDir = System.IO.Path.GetDirectoryName(solution.FullName);
                listFile = Path.Combine(solutionDir, "TaskList.xml");
            }
            else
            {
                buttonAdd.IsEnabled = false;
                textBox.IsEnabled = false;
                ButtonRemoveDone.IsEnabled = false;
            }
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
                var item = new TodoItem(/*this,*/ textBox.Text);
                
                if (!File.Exists(listFile))
                    TryCreate(listFile);

                AddItemToList(item);
                AddItemToFile(item);
                TaskList.Add(item);
                textBox.Text = "";
                
                BuildList();
            }
        }

        private void AddItemToFile(TodoItem item)
        {
            XElement xItem = new XElement("item");
            XElement xText = new XElement("text", item.Name);
            XElement xDone = new XElement("finished", item.Finished.ToString().ToLower());
            xItem.Add(xText, xDone);
            xTasks.Add(xItem);
            try { xTaskList.Save(listFile); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
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
            var done = TaskList.Count(item => item.Finished);
            listBox.Items.Clear();
            xTaskList.Root.RemoveAll();
            foreach (TodoItem item in TaskList)
            {
                AddItemToList(item);
                AddItemToFile(item);
            }
            ProgressBar.Maximum = TaskList.Count;
            ProgressBar.Value = done;
            ProgressText.Text = $"{done} of {TaskList.Count} tasks done";
            if (done > 0)
                ButtonRemoveDone.IsEnabled = true;
            else
                ButtonRemoveDone.IsEnabled = false;
            try { xTaskList.Save(listFile); }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void AddItemToList(TodoItem item)
        {
            var grid = new Grid();
            grid.Name = "DynamicGrid";
            grid.Width = listBox.Width - 30;
            grid.Margin=new Thickness(0,0,10,0);
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            ColumnDefinition cd1 = new ColumnDefinition();
            cd1.Width = new GridLength(15);
            ColumnDefinition cd2 = new ColumnDefinition();
            //cd2.Width=GridLength.Auto;
            ColumnDefinition cd3 = new ColumnDefinition();
            cd3.Width = new GridLength(27);
            grid.ColumnDefinitions.Add(cd1);
            grid.ColumnDefinitions.Add(cd2);
            grid.ColumnDefinitions.Add(cd3);

            var txt = new TextBox {Text = item.ToString()};
            txt.Name = $"txt{listBox.Items.Count}";
            txt.MinWidth = ActualWidth - 62;
            txt.Height = 20;
            txt.HorizontalAlignment = HorizontalAlignment.Stretch;
            txt.VerticalContentAlignment = VerticalAlignment.Center;
            txt.ToolTip = item.Name;
            txt.Style=Resources["TxtStyle"] as Style;
            txt.IsReadOnly = true;
            var check = new CheckBox { Content = txt };
            check.Height = 20;
            check.Style = Resources["CheckStyle"] as Style;
            if (item.Finished)
            {
                txt.TextDecorations.Add(TextDecorations.Strikethrough);
                check.IsChecked = true;
            }
            check.Name = $"chk{listBox.Items.Count}";
            check.SetResourceReference(BackgroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.CommandBarCheckBoxDisabledBrushKey);
            check.Checked += Check_Checked;
            check.Unchecked += Check_Unchecked;
            grid.Children.Add(check);
            Grid.SetColumn(grid.Children[0], 1);

            var btnDel = new Button { Content = "X" };
            btnDel.Style = Resources["BtnStyle"] as Style;
            btnDel.Height = 20;
            btnDel.SetResourceReference(ForegroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.SystemButtonTextBrushKey);
            grid.Children.Add(btnDel);
            btnDel.Name = $"btn{listBox.Items.Count}";
            btnDel.Click += new RoutedEventHandler(RemoveEvent);
            Grid.SetColumn(grid.Children[1], 0);

            var btnEdit = new Button {Content = "Edit"};
            btnEdit.Style = Resources["BtnStyle"] as Style;
            btnEdit.Height = 20;
            btnEdit.SetResourceReference(ForegroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.SystemButtonTextBrushKey);
            grid.Children.Add(btnEdit);
            btnEdit.Name = $"bed{listBox.Items.Count}";
            btnEdit.Click += new RoutedEventHandler(EditItem);
            Grid.SetColumn(grid.Children[2], 2);

            listBox.Items.Add(grid);
        }

        private void EditItem(object sender, RoutedEventArgs e)
        {
            var element = e.Source as FrameworkElement;
            var index = Int32.Parse(element.Name.Substring(3));
            var parent = LogicalTreeHelper.GetChildren(element.Parent);
            var children = parent.OfType<FrameworkElement>().ToList();
            var foundChild = from FrameworkElement child in children
                             where child.Name == $"chk{index}"
                             select child;
            var check = foundChild.First();
            var text = LogicalTreeHelper.GetChildren(check).OfType<TextBox>().First();
            text.IsReadOnly = false;
            text.Focus();
            text.Select(0,text.Text.Length);
            buttonAdd.IsDefault = false;
            var btn = element as Button;
            btn.IsDefault = true;
            btn.Content = "OK";
            btn.Click += Btn_Click;

            //MessageBox.Show($"{check.Name},\n {papa.OfType<TextBox>().First()}");
        }

        private void Btn_Click(object sender, RoutedEventArgs e)
        {

            var element = e.Source as FrameworkElement;
            var index = Int32.Parse(element.Name.Substring(3));
            var parent = LogicalTreeHelper.GetChildren(element.Parent);
            var children = parent.OfType<FrameworkElement>().ToList();
            var foundChild = from FrameworkElement child in children
                             where child.Name == $"chk{index}"
                             select child;
            var check = foundChild.First();
            var text = LogicalTreeHelper.GetChildren(check).OfType<TextBox>().First();
            TaskList[index].Name = text.Text;
            text.IsReadOnly = true;
            var btn = element as Button;
            btn.Content = "Edit";
            btn.IsDefault = false;
            buttonAdd.IsDefault = true;
            btn.Click -= Btn_Click;
            BuildList();
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
        
        private void TryCreate(string listFile)
        {
            if (!File.Exists(listFile))
            {
                try
                {
                    FileStream fs = File.Create(listFile);
                    fs.Close();
                    xTaskList = new XDocument();
                    xTasks=new XElement("Tasks");
                    xTaskList.Add(xTasks);
                    xTaskList.Save(listFile);
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
            else
                OpenTaskListFromFile();
        }

        private void OpenTaskListFromFile()
        {
            TaskList = new List<TodoItem>();
            try
            {
                xTaskList = XDocument.Load(listFile);
                xTasks = xTaskList.Root;
                foreach (XElement element in xTaskList.Element("Tasks").Elements("item"))
                {
                    Debug.Assert(element != null, "element != null");
                    TodoItem item = new TodoItem(element.Element("text").Value, bool.Parse(element.Element("finished").Value));
                    TaskList.Add(item);
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message);}
            BuildList();
        }

        #region Interface Implementation
        /// <summary>
        /// Notifies listening clients that the solution has been opened.
        /// </summary>
        /// <param name="pUnkReserved">[in] Reserved for future use.</param><param name="fNewSolution">[in] true if the solution is being created. false if the solution was created previously or is being loaded.</param>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            this.buttonAdd.IsEnabled = true;
            textBox.IsEnabled = true;
            //ButtonRemoveDone.IsEnabled = true;
            GetSolution();
            if (File.Exists(listFile))
                OpenTaskListFromFile();
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Notifies listening clients that a solution has been closed.
        /// </summary>
        /// <param name="pUnkReserved">[in] Reserved for future use.</param>
        /// <returns>
        /// If the method succeeds, it returns <see cref="F:Microsoft.VisualStudio.VSConstants.S_OK"/>. If it fails, it returns an error code.
        /// </returns>
        public int OnAfterCloseSolution(object pUnkReserved)
        {
            buttonAdd.IsEnabled = false;
            textBox.IsEnabled = false;
            ButtonRemoveDone.IsEnabled = false;
            this.listBox.Items.Clear();
            TaskList.Clear();
            ProgressBar.Value = 0;
            ProgressText.Text = "Add tasks via \"Add\" button";
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }
        #endregion

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (textBox.Text == "")
            {
                ImageBrush textImageBrush = new ImageBrush();
                textImageBrush.ImageSource = new BitmapImage(new Uri(@"pack://application:,,,/todolist;component/Resources/faketext.gif", UriKind.RelativeOrAbsolute));
                textImageBrush.AlignmentX = AlignmentX.Left;
                textImageBrush.Stretch = Stretch.None;
                textBox.Background = textImageBrush;
            }
            else
                textBox.Background = new SolidColorBrush(Color.FromRgb(128,128,128));
        }

        private void ButtonRemoveDone_OnClick(object sender, RoutedEventArgs e)
        {
            TaskList.RemoveAll(item => item.Finished == true);
            BuildList();
        }

        private void TodoWindowControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustSize();
        }

        private void TodoWindowControl_OnLoaded(object sender, RoutedEventArgs e)
        {
            AdjustSize();
        }

        private void AdjustSize()
        {
            Canvas.Width = ActualWidth - 20;
            //Canvas.Height = Math.Max(10, Math.Min(48, ActualHeight - 24));
            listBox.Width = Canvas.Width;
            if (listFile!=null)
                BuildList();
            textBox.Width = ActualWidth - 80;
            ProgressBar.Width = ActualWidth - 109;
            ProgressText.Width = ActualWidth - 109;
            wat.Width = ActualWidth - 20;
        }
    }
}