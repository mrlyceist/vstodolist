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
        private readonly ViewModel _model;

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
            if (_solution.FullName == string.Empty) return;
            string solutionDir = System.IO.Path.GetDirectoryName(_solution.FullName);
            if (solutionDir != null) _listFile = Path.Combine(solutionDir, "TaskList.xml");
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

        private void NewTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var txtBox = e.Source as TextBox;
            if (txtBox == null) return;
            if (string.IsNullOrWhiteSpace(txtBox.Text))
            {
                ImageBrush textImageBrush = new ImageBrush
                {
                    ImageSource =
                            new BitmapImage(new Uri(@"pack://application:,,,/todolist;component/Resources/faketext.gif",
                                UriKind.RelativeOrAbsolute)),
                    AlignmentX = AlignmentX.Left,
                    Stretch = Stretch.None
                };
                txtBox.Background = textImageBrush; 
            }
            else
                txtBox.Background = new SolidColorBrush(Color.FromRgb(128, 128, 128));
            var txt = txtBox.Text;
            _model.NewItem = txt;
            e.Handled = true;
        }
    }
}