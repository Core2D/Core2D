﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Core2D.Avalonia.Controls.Data;
using Core2D.Avalonia.Controls.Path;
using Core2D.Avalonia.Controls.Project;
using Core2D.Avalonia.Controls.Shapes;
using Core2D.Avalonia.Controls.State;
using Core2D.Avalonia.Controls.Style;
using Core2D.Avalonia.Views;
using Core2D.Data;
using Core2D.Data.Database;
using Core2D.Editor;
using Core2D.Editor.Designer;
using Core2D.Editor.Factories;
using Core2D.Editor.Input;
using Core2D.Editor.Interfaces;
using Core2D.Editor.Views;
using Core2D.Interfaces;
using Core2D.Path;
using Core2D.Path.Segments;
using Core2D.Project;
using Core2D.Renderer;
using Core2D.Shape;
using Core2D.Shapes;
using Core2D.Style;
using Renderer.Avalonia;
using Serializer.Newtonsoft;
using Serializer.Xaml;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using TextFieldReader.CsvHelper;
using TextFieldWriter.CsvHelper;

namespace Core2D.Avalonia
{
    /// <summary>
    /// Encapsulates a Core2D Avalonia application.
    /// </summary>
    public class App : Application, IEditorApplication
    {
        private IFileSystem _fileIO;
        private ILog _log;
        private ImmutableArray<IFileWriter> _writers;
        private ProjectEditor _editor;
        private Windows.MainWindow _mainWindow;
        private string _recentFileName = "Core2D.recent";
        private string _logFileName = "Core2D.log";

        /// <summary>
        /// Initializes static data.
        /// </summary>
        static App()
        {
            InitializeDesigner();
        }

        /// <summary>
        /// Initializes designer.
        /// </summary>
        static void InitializeDesigner()
        {
            if (Design.IsDesignMode)
            {
                DesignerContext.InitializeContext(
                    new AvaloniaRenderer(),
                    new AvaloniaTextClipboard(),
                    new NewtonsoftTextSerializer(),
                    new PortableXamlSerializer());
            }
        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        /// <summary>
        /// Initialize application context and displays main window.
        /// </summary>
        /// <param name="fileIO">The file system instance.</param>
        /// <param name="log">The log instance.</param>
        /// <param name="writers">The file writers.</param>
        public void Start(IFileSystem fileIO, ILog log, ImmutableArray<IFileWriter> writers)
        {
            _fileIO = fileIO;
            _log = log;
            _writers = writers;

            _log.Initialize(System.IO.Path.Combine(_fileIO.GetAssemblyPath(null), _logFileName));

            try
            {
                InitializeEditor(_fileIO, _log, _writers);
                LoadRecent();
                _mainWindow = new Windows.MainWindow();
                _mainWindow.Closed += (sender, e) => SaveRecent();
                _mainWindow.DataContext = _editor;
                _mainWindow.Show();
                Run(_mainWindow);
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _log?.LogError($"{ex.InnerException.Message}{Environment.NewLine}{ex.InnerException.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Load recent project files list.
        /// </summary>
        private void LoadRecent()
        {
            try
            {
                var path = System.IO.Path.Combine(_fileIO.GetAssemblyPath(null), _recentFileName);
                if (_fileIO.Exists(path))
                {
                    _editor.OnLoadRecent(path);
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Save recent project files list.
        /// </summary>
        private void SaveRecent()
        {
            try
            {
                var path = System.IO.Path.Combine(_fileIO.GetAssemblyPath(null), _recentFileName);
                _editor.OnSaveRecent(path);
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <summary>
        /// Initialize <see cref="ProjectEditor"/> object.
        /// </summary>
        /// <param name="fileIO">The file system instance.</param>
        /// <param name="log">The log instance.</param>
        /// <param name="writers">The file writers.</param>
        public void InitializeEditor(IFileSystem fileIO, ILog log, ImmutableArray<IFileWriter> writers)
        {
            _editor = new ProjectEditor()
            {
                CurrentTool = Tool.Selection,
                CurrentPathTool = PathTool.Line,
                Application = this,
                Log = log,
                FileIO = fileIO,
                CommandManager = new CommandManager(),
                Renderers = new ShapeRenderer[] { new AvaloniaRenderer(), new AvaloniaRenderer() },
                ProjectFactory = new ProjectFactory(),
                TextClipboard = new AvaloniaTextClipboard(),
                JsonSerializer = new NewtonsoftTextSerializer(),
                XamlSerializer = new PortableXamlSerializer(),
                FileWriters = writers,
                CsvReader = new CsvHelperReader(),
                CsvWriter = new CsvHelperWriter(),
                GetImageKey = async () => await (this as IEditorApplication).OnGetImageKeyAsync()
            }.Defaults();

            _editor.InitializeCommands();
            _editor.CommandManager.RegisterCommands();
        }

        /// <inheritdoc/>
        async Task<string> IEditorApplication.OnGetImageKeyAsync()
        {
            try
            {
                var dlg = new OpenFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                var result = await dlg.ShowAsync(_mainWindow);
                if (result != null)
                {
                    var path = result.FirstOrDefault();
                    using (var stream = _fileIO.Open(path))
                    {
                        var bytes = _fileIO.ReadBinary(stream);
                        var key = _editor?.Project?.AddImageFromFile(path, bytes);
                        return key;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                return null;
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnOpenAsync(string path)
        {
            try
            {
                if (path == null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Project", Extensions = { "project" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                    var result = await dlg.ShowAsync(_mainWindow);
                    if (result != null)
                    {
                        _editor?.OnOpen(result.FirstOrDefault());
                        _editor?.Invalidate?.Invoke();
                    }
                }
                else
                {
                    if (_fileIO.Exists(path))
                    {
                        _editor.OnOpen(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnSaveAsync()
        {
            try
            {
                if (!string.IsNullOrEmpty(_editor?.ProjectPath))
                {
                    _editor?.OnSave(_editor?.ProjectPath);
                }
                else
                {
                    await (this as IEditorApplication).OnSaveAsAsync();
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnSaveAsAsync()
        {
            try
            {
                if (_editor?.Project != null)
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Project", Extensions = { "project" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                    dlg.InitialFileName = _editor.Project.Name;
                    dlg.DefaultExtension = "project";
                    var result = await dlg.ShowAsync(_mainWindow);
                    if (result != null)
                    {
                        _editor?.OnSave(result);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportXamlAsync(string path)
        {
            try
            {
                if (path == null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.AllowMultiple = true;
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Xaml", Extensions = { "xaml" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });

                    var results = await dlg.ShowAsync(_mainWindow);
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            _editor?.OnImportXaml(result);
                        }
                    }
                }
                else
                {
                    if (_fileIO.Exists(path))
                    {
                        _editor?.OnImportXaml(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportXamlAsync(object item)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Xaml", Extensions = { "xaml" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = _editor?.GetName(item);
                dlg.DefaultExtension = "xaml";
                var result = await dlg.ShowAsync(_mainWindow);
                if (result != null)
                {
                    _editor?.OnExportXaml(result, item);
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportJsonAsync(string path)
        {
            try
            {
                if (path == null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.AllowMultiple = true;
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });

                    var results = await dlg.ShowAsync(_mainWindow);
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            _editor?.OnImportJson(result);
                        }
                    }
                }
                else
                {
                    if (_fileIO.Exists(path))
                    {
                        _editor?.OnImportJson(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportJsonAsync(object item)
        {
            try
            {
                var dlg = new SaveFileDialog();
                dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = _editor?.GetName(item);
                dlg.DefaultExtension = "json";
                var result = await dlg.ShowAsync(_mainWindow);
                if (result != null)
                {
                    _editor?.OnExportJson(result, item);
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportAsync(object item)
        {
            try
            {
                string name = string.Empty;

                if (item is XContainer)
                {
                    name = (item as XContainer).Name;
                }
                else if (item is XDocument)
                {
                    name = (item as XDocument).Name;
                }
                else if (item is XProject)
                {
                    name = (item as XProject).Name;
                }
                else if (item is ProjectEditor)
                {
                    var editor = (item as ProjectEditor);
                    if (editor?.Project == null)
                        return;

                    name = editor?.Project?.Name;
                    item = editor?.Project;
                }
                else if (item == null)
                {
                    if (_editor?.Project == null)
                        return;

                    name = _editor?.Project?.Name;
                    item = _editor?.Project;
                }

                var dlg = new SaveFileDialog();
                foreach (var writer in _editor?.FileWriters)
                {
                    dlg.Filters.Add(new FileDialogFilter() { Name = writer.Name, Extensions = { writer.Extension } });
                }
                dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                dlg.InitialFileName = name;
                dlg.DefaultExtension = _editor?.FileWriters.FirstOrDefault()?.Extension;
                var result = await dlg.ShowAsync(_mainWindow);
                if (result != null)
                {
                    string ext = System.IO.Path.GetExtension(result).ToLower().TrimStart('.');
                    IFileWriter writer = _editor?.FileWriters.Where(w => string.Compare(w.Extension, ext, StringComparison.OrdinalIgnoreCase) == 0).FirstOrDefault();
                    if (writer != null)
                    {
                        _editor?.OnExport(result, item, writer);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportDataAsync()
        {
            try
            {
                if (_editor?.Project != null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Csv", Extensions = { "csv" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                    var result = await dlg.ShowAsync(_mainWindow);
                    if (result != null)
                    {
                        var path = result.FirstOrDefault();
                        _editor?.OnImportData(path);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportDataAsync()
        {
            try
            {
                var database = _editor?.Project?.CurrentDatabase;
                if (database != null)
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Csv", Extensions = { "csv" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                    dlg.InitialFileName = database.Name;
                    dlg.DefaultExtension = "csv";
                    var result = await dlg.ShowAsync(_mainWindow);
                    if (result != null)
                    {
                        _editor?.OnExportData(result, database);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnUpdateDataAsync()
        {
            try
            {
                var database = _editor?.Project?.CurrentDatabase;
                if (database != null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Csv", Extensions = { "csv" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "All", Extensions = { "*" } });
                    var result = await dlg.ShowAsync(_mainWindow);
                    if (result != null)
                    {
                        var path = result.FirstOrDefault();
                        _editor?.OnUpdateData(path, database);
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportObjectAsync(string path)
        {
            try
            {
                if (path == null)
                {
                    var dlg = new OpenFileDialog();
                    dlg.AllowMultiple = true;
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Xaml", Extensions = { "xaml" } });
                    var results = await dlg.ShowAsync(_mainWindow);
                    if (results != null)
                    {
                        foreach (var result in results)
                        {
                            string resultExtension = System.IO.Path.GetExtension(result);
                            if (string.Compare(resultExtension, ".json", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _editor?.OnImportJson(result);
                            }
                            else if (string.Compare(resultExtension, ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
                            {
                                _editor?.OnImportJson(result);
                            }
                        }
                    }
                }
                else
                {
                    if (_fileIO.Exists(path))
                    {
                        string resultExtension = System.IO.Path.GetExtension(path);
                        if (string.Compare(resultExtension, ".json", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _editor?.OnImportJson(path);
                        }
                        else if (string.Compare(resultExtension, ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _editor?.OnImportJson(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportObjectAsync(object item)
        {
            try
            {
                if (item != null)
                {
                    var dlg = new SaveFileDialog();
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Json", Extensions = { "json" } });
                    dlg.Filters.Add(new FileDialogFilter() { Name = "Xaml", Extensions = { "xaml" } });
                    dlg.InitialFileName = _editor?.GetName(item);
                    dlg.DefaultExtension = "json";
                    var path = await dlg.ShowAsync(_mainWindow);
                    if (path != null)
                    {
                        string resultExtension = System.IO.Path.GetExtension(path);
                        if (string.Compare(resultExtension, ".json", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _editor?.OnExportJson(path, item);
                        }
                        else if (string.Compare(resultExtension, ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            _editor?.OnExportXaml(path, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnCopyAsEmfAsync()
        {
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportAsEmfAsync(string path)
        {
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnZoomResetAsync()
        {
            _editor.ResetZoom?.Invoke();
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnZoomAutoFitAsync()
        {
            _editor.AutoFitZoom?.Invoke();
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnLoadWindowLayout()
        {
            _editor.LoadLayout?.Invoke();
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnSaveWindowLayoutAsync()
        {
            _editor.SaveLayout?.Invoke();
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnResetWindowLayoutAsync()
        {
            _editor.ResetLayout?.Invoke();
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnShowObjectBrowserAsync()
        {
            var browser = new Windows.BrowserWindow();
            browser.DataContext = _editor;
            browser.Show();

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnShowDocumentViewerAsync()
        {
            var document = new Windows.DocumentWindow();
            document.DataContext = _editor;
            document.Show();

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        void IEditorApplication.OnCloseView()
        {
            _mainWindow?.Close();
        }
    }
}
