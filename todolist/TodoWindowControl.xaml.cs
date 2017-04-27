//------------------------------------------------------------------------------
// <copyright file="TodoWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace todolist
{
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
        private Solution _solution;
        private string _listFile;
        private ViewModel _model;
        //private XDocument _xTaskList;
        //private XElement _xTasks;

        public List<TodoItem> TaskList = new List<TodoItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="TodoWindowControl"/> class.
        /// </summary>
        public TodoWindowControl(TodoWindow window)
        {
            this.InitializeComponent();
            MinWidth = 150;

            TodoWindowPackage.TheSolution.AdviseSolutionEvents(this, out uint cookie);
            GetSolution();
            DataContext = new ViewModel(_listFile, new XmlFileService());
            _model = (ViewModel) DataContext;
            if (File.Exists(_listFile))
            {
                _model.FileName = _listFile;
                _model.OpenListFromFile();
            }
        }

        private void GetSolution()
        {
            if (Package.GetGlobalService(typeof(SDTE)) is DTE2 dte) _solution = dte.Solution;
            if (_solution.FullName != string.Empty)
            {
                string solutionDir = System.IO.Path.GetDirectoryName(_solution.FullName);
                if (solutionDir != null) _listFile = Path.Combine(solutionDir, "TaskList.xml");
            }
            else
            {
                //ButtonAdd.IsEnabled = false;
                //TextBox.IsEnabled = false;
                //ButtonRemoveDone.IsEnabled = false;
            }
        }
        
        ///// <summary>
        ///// Handles click on the button by displaying a message box.
        ///// </summary>
        ///// <param name="sender">The event sender.</param>
        ///// <param name="e">The event args.</param>
        //[SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        //[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        //private void buttonAdd_Click(object sender, RoutedEventArgs e)
        //{
        //    if (TextBox.Text.Length > 0)
        //    {
        //        var item = new TodoItem(TextBox.Text);
                
        //        if (!File.Exists(_listFile))
        //            TryCreate(_listFile);

        //        AddItemToList(item);
        //        AddItemToFile(item);
        //        TaskList.Add(item);
        //        TextBox.Text = "";
                
        //        BuildList();
        //    }
        //}

        //private void AddItemToFile(TodoItem item)
        //{
        //    XElement xItem = new XElement("item");
        //    XElement xText = new XElement("text", item.Text);
        //    XElement xDone = new XElement("finished", item.Finished.ToString().ToLower());
        //    xItem.Add(xText, xDone);
        //    _xTasks.Add(xItem);
        //    try { _xTaskList.Save(_listFile); }
        //    catch (Exception ex) { MessageBox.Show(ex.Message); }
        //}

        //private void RemoveEvent(object sender, RoutedEventArgs e)
        //{
        //    var element = e.Source as FrameworkElement;
        //    var index = Int32.Parse(element.Name.Substring(3));
        //    ListBox.Items.RemoveAt(index);
        //    TaskList.RemoveAt(index);
        //    BuildList();
        //}

        //private void BuildList()
        //{
        //    var done = TaskList.Count(item => item.Finished);
        //    ListBox.Items.Clear();
        //    if (_xTaskList == null) return;
        //    {
        //        _xTaskList.Root?.RemoveAll();
        //        foreach (TodoItem item in TaskList)
        //        {
        //            AddItemToList(item);
        //            AddItemToFile(item);
        //        }
        //        ProgressBar.Maximum = TaskList.Count;
        //        ProgressBar.Value = done;
        //        ProgressText.Text = $"{done} of {TaskList.Count} tasks done";
        //        ButtonRemoveDone.IsEnabled = done > 0;
        //        try { _xTaskList.Save(_listFile); }
        //        catch (Exception ex) { MessageBox.Show(ex.Message); }
        //    }
        //}

        //private void AddItemToList(TodoItem item)
        //{
        //    var grid = new Grid
        //    {
        //        Name = "DynamicGrid",
        //        Width = ListBox.Width - 30,
        //        Margin = new Thickness(0, 0, 10, 0),
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };
        //    ColumnDefinition cd1 = new ColumnDefinition {Width = new GridLength(15)};
        //    ColumnDefinition cd2 = new ColumnDefinition();
        //    ColumnDefinition cd3 = new ColumnDefinition {Width = new GridLength(27)};
        //    grid.ColumnDefinitions.Add(cd1);
        //    grid.ColumnDefinitions.Add(cd2);
        //    grid.ColumnDefinitions.Add(cd3);

        //    var txt = new TextBox
        //    {
        //        Text = item.ToString(),
        //        Name = $"txt{ListBox.Items.Count}",
        //        MinWidth = ActualWidth - 62,
        //        Height = 20,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        VerticalContentAlignment = VerticalAlignment.Center,
        //        ToolTip = item.Text,
        //        Style = Resources["TxtStyle"] as Style,
        //        IsReadOnly = true
        //    };
        //    var check = new CheckBox
        //    {
        //        Content = txt,
        //        Height = 20,
        //        Style = Resources["CheckStyle"] as Style
        //    };
        //    if (item.Finished)
        //    {
        //        txt.TextDecorations.Add(TextDecorations.Strikethrough);
        //        check.IsChecked = true;
        //    }
        //    check.Name = $"chk{ListBox.Items.Count}";
        //    check.SetResourceReference(BackgroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.CommandBarCheckBoxDisabledBrushKey);
        //    check.Checked += Check_Checked;
        //    check.Unchecked += Check_Unchecked;
        //    grid.Children.Add(check);
        //    Grid.SetColumn(grid.Children[0], 1);

        //    var btnDel = new Button
        //    {
        //        Content = "X",
        //        Style = Resources["BtnStyle"] as Style,
        //        Height = 20
        //    };
        //    btnDel.SetResourceReference(ForegroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.SystemButtonTextBrushKey);
        //    grid.Children.Add(btnDel);
        //    btnDel.Name = $"btn{ListBox.Items.Count}";
        //    btnDel.Click += new RoutedEventHandler(RemoveEvent);
        //    Grid.SetColumn(grid.Children[1], 0);

        //    var btnEdit = new Button
        //    {
        //        Content = "Edit",
        //        Style = Resources["BtnStyle"] as Style,
        //        Height = 20
        //    };
        //    btnEdit.SetResourceReference(ForegroundProperty, Microsoft.VisualStudio.PlatformUI.EnvironmentColors.SystemButtonTextBrushKey);
        //    grid.Children.Add(btnEdit);
        //    btnEdit.Name = $"bed{ListBox.Items.Count}";
        //    btnEdit.Click += EditItem;
        //    Grid.SetColumn(grid.Children[2], 2);

        //    ListBox.Items.Add(grid);
        //}

        //private void EditItem(object sender, RoutedEventArgs e)
        //{
        //    var element = e.Source as FrameworkElement;
        //    var index = Int32.Parse(element.Name.Substring(3));
        //    var parent = LogicalTreeHelper.GetChildren(element.Parent);
        //    var children = parent.OfType<FrameworkElement>().ToList();
        //    var foundChild = from FrameworkElement child in children
        //                     where child.Name == $"chk{index}"
        //                     select child;
        //    var check = foundChild.First();
        //    var text = LogicalTreeHelper.GetChildren(check).OfType<TextBox>().First();
        //    text.IsReadOnly = false;
        //    text.Focus();
        //    text.Select(0,text.Text.Length);
        //    ButtonAdd.IsDefault = false;
        //    var btn = element as Button;
        //    if (btn != null)
        //    {
        //        btn.IsDefault = true;
        //        btn.Content = "OK";
        //        btn.Click += Btn_Click;
        //    }
        //}

        //private void Btn_Click(object sender, RoutedEventArgs e)
        //{
        //    // TODO: Move out common code.
        //    var element = e.Source as FrameworkElement;
        //    if (element != null)
        //    {
        //        var index = Int32.Parse(element.Name.Substring(3));
        //        var parent = LogicalTreeHelper.GetChildren(element.Parent);
        //        var children = parent.OfType<FrameworkElement>().ToList();
        //        var foundChild = from FrameworkElement child in children
        //            where child.Name == $"chk{index}"
        //            select child;
        //        var check = foundChild.First();
        //        var text = LogicalTreeHelper.GetChildren(check).OfType<TextBox>().First();
        //        TaskList[index].Text = text.Text;
        //        text.IsReadOnly = true;
        //    }
        //    var btn = element as Button;
        //    if (btn != null)
        //    {
        //        btn.Content = "Edit";
        //        btn.IsDefault = false;
        //        ButtonAdd.IsDefault = true;
        //        btn.Click -= Btn_Click;
        //    }
        //    BuildList();
        //}

        //private void Check_Unchecked(object sender, RoutedEventArgs e)
        //{
        //    var element = e.Source as FrameworkElement;
        //    if (element != null)
        //    {
        //        var index = Int32.Parse(element.Name.Substring(3));
        //        TaskList[index].Finished = false;
        //    }
        //    BuildList();
        //}

        //private void Check_Checked(object sender, RoutedEventArgs e)
        //{
        //    var element = e.Source as FrameworkElement;
        //    if (element != null)
        //    {
        //        var index = Int32.Parse(element.Name.Substring(3));
        //        TaskList[index].Finished = true;
        //    }
        //    BuildList();
        //}
        
        //private void TryCreate(string listFile)
        //{
        //    if (!File.Exists(listFile))
        //    {
        //        try
        //        {
        //            FileStream fs = File.Create(listFile);
        //            fs.Close();
        //            _xTaskList = new XDocument();
        //            _xTasks=new XElement("Tasks");
        //            _xTaskList.Add(_xTasks);
        //            _xTaskList.Save(listFile);
        //        }
        //        catch (Exception ex) { MessageBox.Show(ex.Message); }
        //    }
        //    else
        //        OpenTaskListFromFile();
        //}

        //private void OpenTaskListFromFile()
        //{
        //    TaskList = new List<TodoItem>();
        //    try
        //    {
        //        _xTaskList = XDocument.Load(_listFile);
        //        _xTasks = _xTaskList.Root;
        //        var xElement = _xTaskList.Element("Tasks");
        //        if (xElement != null)
        //            foreach (XElement element in xElement.Elements("item"))
        //            {
        //                Debug.Assert(element != null, "element != null");
        //                var o = element.Element("text");
        //                if (o == null) continue;
        //                var xElement1 = element.Element("finished");
        //                TodoItem item = new TodoItem(o.Value, xElement1 != null && bool.Parse(xElement1.Value));
        //                TaskList.Add(item);
        //            }
        //    }
        //    catch (Exception ex) { MessageBox.Show(ex.Message);}
        //    BuildList();
        //}

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
            //this.ButtonAdd.IsEnabled = true;
            //TextBox.IsEnabled = true;
            GetSolution();
            if (File.Exists(_listFile))
            {
                _model.FileName = _listFile;
                _model.OpenListFromFile();
            }
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
            //ButtonAdd.IsEnabled = false;
            //TextBox.IsEnabled = false;
            //ButtonRemoveDone.IsEnabled = false;
            //this.ListBox.Items.Clear();
            //TaskList.Clear();
            //ProgressBar.Value = 0;
            //ProgressText.Text = "Add tasks via \"Add\" button";
            _model.FileName = string.Empty;
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
            //if (TextBox.Text == "")
            {
                ImageBrush textImageBrush = new ImageBrush
                {
                    ImageSource =
                        new BitmapImage(new Uri(@"pack://application:,,,/todolist;component/Resources/faketext.gif",
                            UriKind.RelativeOrAbsolute)),
                    AlignmentX = AlignmentX.Left,
                    Stretch = Stretch.None
                };
                //TextBox.Background = textImageBrush;
            }
            //else
                //TextBox.Background = new SolidColorBrush(Color.FromRgb(128,128,128));
        }

        //private void ButtonRemoveDone_OnClick(object sender, RoutedEventArgs e)
        //{
        //    TaskList.RemoveAll(item => item.Finished);
        //    BuildList();
        //}

        //private void TodoWindowControl_OnSizeChanged(object sender, SizeChangedEventArgs e)
        //{
        //    AdjustSize();
        //}

        //private void TodoWindowControl_OnLoaded(object sender, RoutedEventArgs e)
        //{
        //    AdjustSize();
        //}

        //private void AdjustSize()
        //{
        //    Canvas.Width = ActualWidth - 20;
        //    //Canvas.Height = Math.Max(10, Math.Min(48, ActualHeight - 24));
        //    ListBox.Width = Canvas.Width;
        //    if (_listFile!=null)
        //        BuildList();
        //    TextBox.Width = ActualWidth - 80;
        //    ProgressBar.Width = ActualWidth - 109;
        //    ProgressText.Width = ActualWidth - 109;
        //    Wat.Width = ActualWidth - 20;
        //}

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var context = button.DataContext;
            var model = DataContext as ViewModel;
            model.RemoveItem(context);
        }

        private void TextBox_OnGotFocus(object sender, RoutedEventArgs e)
        {
            var box = e.Source as TextBox;
            box.SelectAll();
        }

        private void UIElement_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter && e.Key != Key.Escape) return;
            var element = (FrameworkElement)e.Source;
            var tBox = element as TextBox;
            var tmpTxt = tBox.Text;
            var children = LogicalTreeHelper
                    .GetChildren(element.Parent)
                    .OfType<FrameworkElement>()
                    .FirstOrDefault(i => i.Name == "EditButton")
                as ToggleButton;
            if (children == null) return;
            children.IsChecked = false;

            if (e.Key == Key.Escape)
            {
                tBox.Text = tmpTxt;
            }
        }
    }
}