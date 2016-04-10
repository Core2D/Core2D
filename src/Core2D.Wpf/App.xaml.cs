﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Editor;
using Core2D.Editor.Factories;
using Core2D.Editor.Interfaces;
using Core2D.Interfaces;
using Core2D.Project;
using Core2D.Renderer;
using Core2D.Shapes;
using Core2D.Style;
using FileWriter.Dxf;
using FileWriter.Emf;
using FileWriter.Pdf_wpf;
using Log.Trace;
using Microsoft.Win32;
using Renderer.Wpf;
using Serializer.Newtonsoft;
using Serializer.ProtoBuf;
using Serializer.Xaml;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using TextFieldReader.CsvHelper;
using TextFieldWriter.CsvHelper;

namespace Core2D.Wpf
{
    /// <summary>
    /// Encapsulates a Core2D WPF application.
    /// </summary>
    public partial class App : Application, IEditorApplication
    {
        private ProjectEditor _editor;
        private Windows.MainWindow _mainWindow;
        private bool _isLoaded = false;
        private string _recentFileName = "Core2D.recent";
        private string _logFileName = "Core2D.log";
        private bool _enableRecent = true;

        /// <summary>
        /// Raises the <see cref="Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Start();
        }

        /// <summary>
        /// Initialize application context and displays main window.
        /// </summary>
        public void Start()
        {
            using (ILog log = new TraceLog())
            {
                log.Initialize(System.IO.Path.Combine(GetAssemblyPath(), _logFileName));

                try
                {
                    InitializeEditor(log);
                    LoadRecent();
                    _mainWindow = new Windows.MainWindow();
                    _mainWindow.Loaded += (sender, e) => OnLoaded();
                    _mainWindow.Closed += (sender, e) => OnClosed();
                    _mainWindow.DataContext = _editor;
                    _mainWindow.ShowDialog();
                }
                catch (Exception ex)
                {
                    log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                    if (ex.InnerException != null)
                    {
                        log?.LogError($"{ex.InnerException.Message}{Environment.NewLine}{ex.InnerException.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Initialize main window after loaded.
        /// </summary>
        private void OnLoaded()
        {
            if (_isLoaded)
                return;
            else
                _isLoaded = true;
        }

        /// <summary>
        /// De-initialize main window after closed.
        /// </summary>
        private void OnClosed()
        {
            if (!_isLoaded)
                return;
            else
                _isLoaded = false;

            SaveRecent();
        }

        /// <summary>
        /// Gets the location of the assembly as specified originally.
        /// </summary>
        /// <returns>The location of the assembly as specified originally.</returns>
        private string GetAssemblyPath()
        {
            string codeBase = Assembly.GetExecutingAssembly().CodeBase;
            var uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return System.IO.Path.GetDirectoryName(path);
        }

        /// <summary>
        /// Load recent project files list.
        /// </summary>
        private void LoadRecent()
        {
            if (_enableRecent)
            {
                try
                {
                    var path = System.IO.Path.Combine(GetAssemblyPath(), _recentFileName);
                    if (System.IO.File.Exists(path))
                    {
                        _editor?.LoadRecent(path);
                    }
                }
                catch (Exception ex)
                {
                    _editor?.Log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Save recent project files list.
        /// </summary>
        private void SaveRecent()
        {
            if (_enableRecent)
            {
                try
                {
                    var path = System.IO.Path.Combine(GetAssemblyPath(), _recentFileName);
                    _editor?.SaveRecent(path);
                }
                catch (Exception ex)
                {
                    _editor?.Log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
        }

        /// <summary>
        /// Initialize <see cref="Editor"/> object.
        /// </summary>
        /// <param name="log">The log instance.</param>
        private void InitializeEditor(ILog log)
        {
            _editor = new ProjectEditor()
            {
                CurrentTool = Tool.Selection,
                CurrentPathTool = PathTool.Line,
                Application = this,
                Log = log,
                FileIO = new WpfFileSystem(),
                CommandManager = new WpfCommandManager(),
                Renderers = new ShapeRenderer[] { new WpfRenderer() },
                ProjectFactory = new ProjectFactory(),
                TextClipboard = new WpfTextClipboard(),
                ProtoBufSerializer = new ProtoBufStreamSerializer(),
                JsonSerializer = new NewtonsoftTextSerializer(),
                XamlSerializer = new PortableXamlSerializer(),
                PdfWriter = new PdfWriter(),
                DxfWriter = new DxfWriter(),
                CsvReader = new CsvHelperReader(),
                CsvWriter = new CsvHelperWriter(),
                GetImageKey = async () => await (this as IEditorApplication).OnGetImageKeyAsync()
            };

            _editor.InitializeCommands();
            _editor.CommandManager.RegisterCommands();
        }

        /// <inheritdoc/>
        async Task<string> IEditorApplication.OnGetImageKeyAsync()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "All (*.*)|*.*",
                FilterIndex = 0,
                FileName = ""
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                try
                {
                    var path = dlg.FileName;
                    var bytes = System.IO.File.ReadAllBytes(path);
                    var key = _editor?.Project.AddImageFromFile(path, bytes);
                    return await Task.Run(() => key);
                }
                catch (Exception ex)
                {
                    _editor?.Log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            }
            return null;
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnOpenAsync(string path)
        {
            if (path == null)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "Project (*.project)|*.project|All (*.*)|*.*",
                    FilterIndex = 0,
                    FileName = ""
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    _editor?.Open(dlg.FileName);
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    _editor?.Open(path);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnSaveAsync()
        {
            if (!string.IsNullOrEmpty(_editor?.ProjectPath))
            {
                _editor?.Save(_editor?.ProjectPath);
            }
            else
            {
                await (this as IEditorApplication).OnSaveAsAsync();
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnSaveAsAsync()
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Project (*.project)|*.project|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = _editor?.Project?.Name
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                _editor?.Save(dlg.FileName);
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportXamlAsync(string path)
        {
            if (path == null)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "Xaml (*.xaml)|*.xaml|All (*.*)|*.*",
                    FilterIndex = 0,
                    Multiselect = true,
                    FileName = ""
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    var results = dlg.FileNames;

                    foreach (var result in results)
                    {
                        _editor?.OnImportXaml(result);
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    _editor?.OnImportXaml(path);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportXamlAsync(object item)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Xaml (*.xaml)|*.xaml|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = _editor?.GetName(item)
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                _editor?.OnExportXaml(dlg.FileName, item);
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportJsonAsync(string path)
        {
            if (path == null)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "Json (*.json)|*.json|All (*.*)|*.*",
                    FilterIndex = 0,
                    Multiselect = true,
                    FileName = ""
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    var results = dlg.FileNames;

                    foreach (var result in results)
                    {
                        _editor?.OnImportJson(result);
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    _editor?.OnImportJson(path);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportJsonAsync(object item)
        {
            var dlg = new SaveFileDialog()
            {
                Filter = "Xaml (*.xaml)|*.xaml|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = _editor?.GetName(item)
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                _editor?.OnExportJson(dlg.FileName, item);
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportAsync(object item)
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
                if (_editor.Project == null)
                    return;

                name = _editor?.Project?.Name;
                item = _editor?.Project;
            }

            var dlg = new SaveFileDialog()
            {
                Filter = "Pdf (*.pdf)|*.pdf|Emf (*.emf)|*.emf|Dxf (*.dxf)|*.dxf|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = name
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                switch (dlg.FilterIndex)
                {
                    case 1:
                        _editor?.ExportAsPdf(dlg.FileName, item);
                        break;
                    case 2:
                        await (this as IEditorApplication).OnExportAsEmfAsync(dlg.FileName);
                        break;
                    case 3:
                        _editor?.ExportAsDxf(dlg.FileName, item);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportDataAsync()
        {
            var dlg = new OpenFileDialog()
            {
                Filter = "Csv (*.csv)|*.csv|All (*.*)|*.*",
                FilterIndex = 0,
                FileName = ""
            };

            if (dlg.ShowDialog(_mainWindow) == true)
            {
                _editor?.OnImportData(dlg.FileName);
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportDataAsync()
        {
            var database = _editor?.Project?.CurrentDatabase;
            if (database != null)
            {
                var dlg = new SaveFileDialog()
                {
                    Filter = "Csv (*.csv)|*.csv|All (*.*)|*.*",
                    FilterIndex = 0,
                    FileName = database.Name
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    _editor?.OnExportData(dlg.FileName, database);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnUpdateDataAsync()
        {
            var database = _editor?.Project?.CurrentDatabase;
            if (database != null)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "Csv (*.csv)|*.csv|All (*.*)|*.*",
                    FilterIndex = 0,
                    FileName = ""
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    _editor?.OnUpdateData(dlg.FileName, database);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnImportObjectAsync(string path)
        {
            if (path == null)
            {
                var dlg = new OpenFileDialog()
                {
                    Filter = "Json (*.json)|*.json|Xaml (*.xaml)|*.xaml",
                    Multiselect = true,
                    FilterIndex = 0
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    var results = dlg.FileNames;
                    var index = dlg.FilterIndex;

                    foreach (var result in results)
                    {
                        switch (index)
                        {
                            case 1:
                                _editor?.OnImportJson(result);
                                break;
                            case 2:
                                _editor?.OnImportXaml(result);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                if (System.IO.File.Exists(path))
                {
                    string resultExtension = System.IO.Path.GetExtension(path);
                    if (string.Compare(resultExtension, ".json", true) == 0)
                    {
                        _editor?.OnImportJson(path);
                    }
                    else if (string.Compare(resultExtension, ".xaml", true) == 0)
                    {
                        _editor?.OnImportJson(path);
                    }
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportObjectAsync(object item)
        {
            if (item != null)
            {
                var dlg = new SaveFileDialog()
                {
                    Filter = "Json (*.json)|*.json|Xaml (*.xaml)|*.xaml",
                    FilterIndex = 0,
                    FileName = _editor?.GetName(item)
                };

                if (dlg.ShowDialog(_mainWindow) == true)
                {
                    switch (dlg.FilterIndex)
                    {
                        case 1:
                            _editor?.OnExportJson(dlg.FileName, item);
                            break;
                        case 2:
                            _editor?.OnExportXaml(dlg.FileName, item);
                            break;
                        default:
                            break;
                    }
                }
            }
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnCopyAsEmfAsync()
        {
            var page = _editor?.Project?.CurrentContainer as XPage;
            if (page != null)
            {
                if (_editor?.Renderers[0]?.State?.SelectedShape != null)
                {
                    var shapes = Enumerable.Repeat(_editor.Renderers[0].State.SelectedShape, 1).ToList();
                    EmfWriter.SetClipboard(
                        shapes,
                        page.Template.Width,
                        page.Template.Height,
                        page.Data.Properties,
                        page.Data.Record,
                        _editor.Project);
                }
                else if (_editor?.Renderers?[0]?.State?.SelectedShapes != null)
                {
                    var shapes = _editor.Renderers[0].State.SelectedShapes.ToList();
                    EmfWriter.SetClipboard(
                        shapes,
                        page.Template.Width,
                        page.Template.Height,
                        page.Data.Properties,
                        page.Data.Record,
                        _editor.Project);
                }
                else
                {
                    EmfWriter.SetClipboard(page, _editor.Project);
                }
            }

            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnExportAsEmfAsync(string path)
        {
            try
            {
                var page = _editor?.Project?.CurrentContainer as XPage;
                if (page != null)
                {
                    EmfWriter.Save(path, page, _editor.Project as IImageCache);
                }
            }
            catch (Exception ex)
            {
                _editor?.Log?.LogError($"{ex.Message}{Environment.NewLine}{ex.StackTrace}");
            }

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
            await Task.Delay(0);
        }

        /// <inheritdoc/>
        async Task IEditorApplication.OnShowDocumentViewerAsync()
        {
            await Task.Delay(0);
        }

        /// <summary>
        /// Close application view.
        /// </summary>
        void IEditorApplication.OnCloseView()
        {
            _mainWindow?.Close();
        }
    }
}
