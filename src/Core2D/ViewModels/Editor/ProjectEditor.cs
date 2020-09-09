﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Core2D.Containers;
using Core2D.Data;
using Core2D.Editor.History;
using Core2D.Editor.Recent;
using Core2D.Editor.Tools.Decorators;
using Core2D.Input;
using Core2D.Layout;
using Core2D.Renderer;
using Core2D.Scripting;
using Core2D.Shapes;
using Core2D.Style;
using Spatial;
using static System.Math;

namespace Core2D.Editor
{
    /// <summary>
    /// Project editor.
    /// </summary>
    public class ProjectEditor : ObservableObject, IProjectEditor
    {
        private readonly IServiceProvider _serviceProvider;
        private IShapeEditor _shapeEditor;
        private IProjectContainer _project;
        private string _projectPath;
        private bool _isProjectDirty;
        private ProjectObserver _observer;
        private bool _isToolIdle;
        private IEditorTool _currentTool;
        private IPathTool _currentPathTool;
        private ImmutableArray<RecentFile> _recentProjects;
        private RecentFile _currentRecentProject;
        private AboutInfo _aboutInfo;
        private readonly Lazy<ImmutableArray<IEditorTool>> _tools;
        private readonly Lazy<ImmutableArray<IPathTool>> _pathTools;
        private readonly Lazy<IHitTest> _hitTest;
        private readonly Lazy<ILog> _log;
        private readonly Lazy<IDataFlow> _dataFlow;
        private readonly Lazy<IShapeRenderer> _pageRenderer;
        private readonly Lazy<IShapeRenderer> _documentRenderer;
        private readonly Lazy<IFileSystem> _fileIO;
        private readonly Lazy<IFactory> _factory;
        private readonly Lazy<IContainerFactory> _containerFactory;
        private readonly Lazy<IShapeFactory> _shapeFactory;
        private readonly Lazy<ITextClipboard> _textClipboard;
        private readonly Lazy<IJsonSerializer> _jsonSerializer;
        private readonly Lazy<ImmutableArray<IFileWriter>> _fileWriters;
        private readonly Lazy<ImmutableArray<ITextFieldReader<IDatabase>>> _textFieldReaders;
        private readonly Lazy<ImmutableArray<ITextFieldWriter<IDatabase>>> _textFieldWriters;
        private readonly Lazy<IImageImporter> _imageImporter;
        private readonly Lazy<IScriptRunner> _scriptRunner;
        private readonly Lazy<IProjectEditorPlatform> _platform;
        private readonly Lazy<IEditorCanvasPlatform> _canvasPlatform;
        private readonly Lazy<IStyleEditor> _styleEditor;
        private readonly Lazy<IPathConverter> _pathConverter;
        private readonly Lazy<ISvgConverter> _svgConverter;

        /// <inheritdoc/>
        public IProjectContainer Project
        {
            get => _project;
            set => Update(ref _project, value);
        }

        /// <inheritdoc/>
        public string ProjectPath
        {
            get => _projectPath;
            set => Update(ref _projectPath, value);
        }

        /// <inheritdoc/>
        public bool IsProjectDirty
        {
            get => _isProjectDirty;
            set => Update(ref _isProjectDirty, value);
        }

        /// <inheritdoc/>
        public ProjectObserver Observer
        {
            get => _observer;
            set => Update(ref _observer, value);
        }

        /// <inheritdoc/>
        public bool IsToolIdle
        {
            get => _isToolIdle;
            set => Update(ref _isToolIdle, value);
        }

        /// <inheritdoc/>
        public IEditorTool CurrentTool
        {
            get => _currentTool;
            set => Update(ref _currentTool, value);
        }

        /// <inheritdoc/>
        public IPathTool CurrentPathTool
        {
            get => _currentPathTool;
            set => Update(ref _currentPathTool, value);
        }

        /// <inheritdoc/>
        public ImmutableArray<RecentFile> RecentProjects
        {
            get => _recentProjects;
            set => Update(ref _recentProjects, value);
        }

        /// <inheritdoc/>
        public RecentFile CurrentRecentProject
        {
            get => _currentRecentProject;
            set => Update(ref _currentRecentProject, value);
        }

        /// <inheritdoc/>
        public AboutInfo AboutInfo
        {
            get => _aboutInfo;
            set => Update(ref _aboutInfo, value);
        }

        /// <inheritdoc/>
        public ImmutableArray<IEditorTool> Tools => _tools.Value;

        /// <inheritdoc/>
        public ImmutableArray<IPathTool> PathTools => _pathTools.Value;

        /// <inheritdoc/>
        public IHitTest HitTest => _hitTest.Value;

        /// <inheritdoc/>
        public ILog Log => _log.Value;

        /// <inheritdoc/>
        public IDataFlow DataFlow => _dataFlow.Value;

        /// <inheritdoc/>
        public IShapeRenderer PageRenderer => _pageRenderer.Value;

        /// <inheritdoc/>
        public IShapeRendererState PageState => _pageRenderer.Value?.State;

        /// <inheritdoc/>
        public IShapeRenderer DocumentRenderer => _documentRenderer.Value;

        /// <inheritdoc/>
        public IShapeRendererState DocumentState => _documentRenderer.Value?.State;

        /// <inheritdoc/>
        public IFileSystem FileIO => _fileIO.Value;

        /// <inheritdoc/>
        public IFactory Factory => _factory.Value;

        /// <inheritdoc/>
        public IContainerFactory ContainerFactory => _containerFactory.Value;

        /// <inheritdoc/>
        public IShapeFactory ShapeFactory => _shapeFactory.Value;

        /// <inheritdoc/>
        public ITextClipboard TextClipboard => _textClipboard.Value;

        /// <inheritdoc/>
        public IJsonSerializer JsonSerializer => _jsonSerializer.Value;

        /// <inheritdoc/>
        public ImmutableArray<IFileWriter> FileWriters => _fileWriters.Value;

        /// <inheritdoc/>
        public ImmutableArray<ITextFieldReader<IDatabase>> TextFieldReaders => _textFieldReaders.Value;

        /// <inheritdoc/>
        public ImmutableArray<ITextFieldWriter<IDatabase>> TextFieldWriters => _textFieldWriters.Value;

        /// <inheritdoc/>
        public IImageImporter ImageImporter => _imageImporter.Value;

        /// <inheritdoc/>
        public IScriptRunner ScriptRunner => _scriptRunner.Value;

        /// <inheritdoc/>
        public IProjectEditorPlatform Platform => _platform.Value;

        /// <inheritdoc/>
        public IEditorCanvasPlatform CanvasPlatform => _canvasPlatform.Value;

        /// <inheritdoc/>
        public IStyleEditor StyleEditor => _styleEditor.Value;

        /// <inheritdoc/>
        public IPathConverter PathConverter => _pathConverter.Value;

        /// <inheritdoc/>
        public ISvgConverter SvgConverter => _svgConverter.Value;

        private object ScriptState { get; set; } = default;

        private IPageContainer PageToCopy { get; set; } = default;

        private IDocumentContainer DocumentToCopy { get; set; } = default;

        private IBaseShape HoveredShape { get; set; } = default;

        /// <summary>
        /// Initialize new instance of <see cref="ProjectEditor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ProjectEditor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _shapeEditor = new ShapeEditor(_serviceProvider);
            _recentProjects = ImmutableArray.Create<RecentFile>();
            _currentRecentProject = default;
            _tools = _serviceProvider.GetServiceLazily<IEditorTool[], ImmutableArray<IEditorTool>>((tools) => tools.Where(tool => !tool.GetType().Name.StartsWith("PathTool")).ToImmutableArray());
            _pathTools = _serviceProvider.GetServiceLazily<IPathTool[], ImmutableArray<IPathTool>>((tools) => tools.ToImmutableArray());
            _hitTest = _serviceProvider.GetServiceLazily<IHitTest>(hitTests => hitTests.Register(_serviceProvider.GetService<IBounds[]>()));
            _log = _serviceProvider.GetServiceLazily<ILog>();
            _dataFlow = _serviceProvider.GetServiceLazily<IDataFlow>();
            _pageRenderer = _serviceProvider.GetServiceLazily<IShapeRenderer>();
            _documentRenderer = _serviceProvider.GetServiceLazily<IShapeRenderer>();
            _fileIO = _serviceProvider.GetServiceLazily<IFileSystem>();
            _factory = _serviceProvider.GetServiceLazily<IFactory>();
            _containerFactory = _serviceProvider.GetServiceLazily<IContainerFactory>();
            _shapeFactory = _serviceProvider.GetServiceLazily<IShapeFactory>();
            _textClipboard = _serviceProvider.GetServiceLazily<ITextClipboard>();
            _jsonSerializer = _serviceProvider.GetServiceLazily<IJsonSerializer>();
            _fileWriters = _serviceProvider.GetServiceLazily<IFileWriter[], ImmutableArray<IFileWriter>>((writers) => writers.ToImmutableArray());
            _textFieldReaders = _serviceProvider.GetServiceLazily<ITextFieldReader<IDatabase>[], ImmutableArray<ITextFieldReader<IDatabase>>>((readers) => readers.ToImmutableArray());
            _textFieldWriters = _serviceProvider.GetServiceLazily<ITextFieldWriter<IDatabase>[], ImmutableArray<ITextFieldWriter<IDatabase>>>((writers) => writers.ToImmutableArray());
            _imageImporter = _serviceProvider.GetServiceLazily<IImageImporter>();
            _scriptRunner = _serviceProvider.GetServiceLazily<IScriptRunner>();
            _platform = _serviceProvider.GetServiceLazily<IProjectEditorPlatform>();
            _canvasPlatform = _serviceProvider.GetServiceLazily<IEditorCanvasPlatform>();
            _styleEditor = _serviceProvider.GetServiceLazily<IStyleEditor>();
            _pathConverter = _serviceProvider.GetServiceLazily<IPathConverter>();
            _svgConverter = _serviceProvider.GetServiceLazily<ISvgConverter>();
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public (decimal sx, decimal sy) TryToSnap(InputArgs args)
        {
            if (Project != null && Project.Options.SnapToGrid == true)
            {
                return (
                    PointUtil.Snap((decimal)args.X, (decimal)Project.Options.SnapX),
                    PointUtil.Snap((decimal)args.Y, (decimal)Project.Options.SnapY));
            }
            else
            {
                return ((decimal)args.X, (decimal)args.Y);
            }
        }

        /// <inheritdoc/>
        public string GetName(object item)
        {
            if (item != null)
            {
                if (item is IObservableObject observable)
                {
                    return observable.Name;
                }
            }
            return string.Empty;
        }

        /// <inheritdoc/>
        public void OnNew(object? item)
        {
            if (item is ILayerContainer layer)
            {
                if (layer.Owner is IPageContainer page)
                {
                    OnAddLayer(page); 
                }
            }
            else if (item is IPageContainer page)
            {
                OnNewPage(page);
            }
            else if (item is IDocumentContainer document)
            {
                OnNewPage(document);
            }
            else if (item is IProjectContainer)
            {
                OnNewDocument();
            }
            else if (item is IProjectEditor)
            {
                OnNewProject();
            }
            else if (item == null)
            {
                if (Project == null)
                {
                    OnNewProject();
                }
                else
                {
                    if (Project.CurrentDocument == null)
                    {
                        OnNewDocument();
                    }
                    else
                    {
                        OnNewPage(Project.CurrentDocument);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnNewPage(IPageContainer selected)
        {
            var document = Project?.Documents.FirstOrDefault(d => d.Pages.Contains(selected));
            if (document != null)
            {
                var page =
                    ContainerFactory?.GetPage(Project, ProjectEditorConfiguration.DefaultPageName)
                    ?? Factory.CreatePageContainer(ProjectEditorConfiguration.DefaultPageName);

                Project?.AddPage(document, page);
                Project?.SetCurrentContainer(page);
            }
        }

        /// <inheritdoc/>
        public void OnNewPage(IDocumentContainer selected)
        {
            var page =
                ContainerFactory?.GetPage(Project, ProjectEditorConfiguration.DefaultPageName)
                ?? Factory.CreatePageContainer(ProjectEditorConfiguration.DefaultPageName);

            Project?.AddPage(selected, page);
            Project?.SetCurrentContainer(page);
        }

        /// <inheritdoc/>
        public void OnNewDocument()
        {
            var document =
                ContainerFactory?.GetDocument(Project, ProjectEditorConfiguration.DefaultDocumentName)
                ?? Factory.CreateDocumentContainer(ProjectEditorConfiguration.DefaultDocumentName);

            Project?.AddDocument(document);
            Project?.SetCurrentDocument(document);
            Project?.SetCurrentContainer(document?.Pages.FirstOrDefault());
        }

        /// <inheritdoc/>
        public void OnNewProject()
        {
            OnUnload();
            OnLoad(ContainerFactory?.GetProject() ?? Factory.CreateProjectContainer(), string.Empty);
            CanvasPlatform?.ResetZoom?.Invoke();
            CanvasPlatform?.InvalidateControl?.Invoke();
        }

        /// <inheritdoc/>
        public void OnOpenProject(string path)
        {
            try
            {
                if (FileIO != null && JsonSerializer != null)
                {
                    if (!string.IsNullOrEmpty(path) && FileIO.Exists(path))
                    {
                        var project = Factory.OpenProjectContainer(path, FileIO, JsonSerializer);
                        if (project != null)
                        {
                            OnOpenProject(project, path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnOpenProject(IProjectContainer project, string path)
        {
            try
            {
                if (project != null)
                {
                    OnUnload();
                    OnLoad(project, path);
                    OnAddRecent(path, project.Name);
                    CanvasPlatform?.ResetZoom?.Invoke();
                    CanvasPlatform?.InvalidateControl?.Invoke();
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnCloseProject()
        {
            Project?.History?.Reset();
            OnUnload();
        }

        /// <inheritdoc/>
        public void OnSaveProject(string path)
        {
            try
            {
                if (Project != null && FileIO != null && JsonSerializer != null)
                {
                    var isDecoratorVisible = PageState.Decorator?.IsVisible == true;
                    if (isDecoratorVisible)
                    {
                        OnHideDecorator();
                    }

                    Factory.SaveProjectContainer(Project, path, FileIO, JsonSerializer);
                    OnAddRecent(path, Project.Name);

                    if (string.IsNullOrEmpty(ProjectPath))
                    {
                        ProjectPath = path;
                    }

                    IsProjectDirty = false;

                    if (isDecoratorVisible)
                    {
                        OnShowDecorator();
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnImportData(IProjectContainer project, string path, ITextFieldReader<IDatabase> reader)
        {
            try
            {
                if (project != null)
                {
                    using var stream = FileIO.Open(path);
                    var db = reader?.Read(stream);
                    if (db != null)
                    {
                        project.AddDatabase(db);
                        project.SetCurrentDatabase(db);
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnExportData(string path, IDatabase database, ITextFieldWriter<IDatabase> writer)
        {
            try
            {
                using var stream = FileIO.Create(path);
                writer?.Write(stream, database);
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnUpdateData(string path, IDatabase database, ITextFieldReader<IDatabase> reader)
        {
            try
            {
                using var stream = FileIO.Open(path);
                var db = reader?.Read(stream);
                if (db != null)
                {
                    Project?.UpdateDatabase(database, db);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnImportObject(object item, bool restore)
        {
            if (item is IShapeStyle style)
            {
                Project?.AddStyle(Project?.CurrentStyleLibrary, style);
            }
            else if (item is IList<IShapeStyle> styleList)
            {
                Project.AddItems(Project?.CurrentStyleLibrary, styleList);
            }
            else if (item is IBaseShape)
            {
                if (item is IGroupShape group)
                {
                    if (restore)
                    {
                        var shapes = Enumerable.Repeat(group, 1);
                        TryToRestoreRecords(shapes);
                    }
                    Project.AddGroup(Project?.CurrentGroupLibrary, group);
                }
                else
                {
                    Project?.AddShape(Project?.CurrentContainer?.CurrentLayer, item as IBaseShape);
                }
            }
            else if (item is IList<IGroupShape> groups)
            {
                if (restore)
                {
                    TryToRestoreRecords(groups);
                }
                Project.AddItems(Project?.CurrentGroupLibrary, groups);
            }
            else if (item is ILibrary<IShapeStyle> sl)
            {
                Project.AddStyleLibrary(sl);
            }
            else if (item is IList<ILibrary<IShapeStyle>> sll)
            {
                Project.AddStyleLibraries(sll);
            }
            else if (item is ILibrary<IGroupShape> gl)
            {
                TryToRestoreRecords(gl.Items);
                Project.AddGroupLibrary(gl);
            }
            else if (item is IList<ILibrary<IGroupShape>> gll)
            {
                var shapes = gll.SelectMany(x => x.Items);
                TryToRestoreRecords(shapes);
                Project.AddGroupLibraries(gll);
            }
            else if (item is IContext context)
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    OnApplyData(context);
                }
                else
                {
                    var container = Project?.CurrentContainer;
                    if (container != null)
                    {
                        container.Data = context;
                    }
                }
            }
            else if (item is IDatabase db)
            {
                Project?.AddDatabase(db);
                Project?.SetCurrentDatabase(db);
            }
            else if (item is ILayerContainer layer)
            {
                if (restore)
                {
                    TryToRestoreRecords(layer.Shapes);
                }
                Project?.AddLayer(Project?.CurrentContainer, layer);
            }
            else if (item is IPageContainer page)
            {
                if (page.Template == null)
                {
                    // Import as template.
                    if (restore)
                    {
                        var shapes = page.Layers.SelectMany(x => x.Shapes);
                        TryToRestoreRecords(shapes);
                    }
                    Project?.AddTemplate(page);
                }
                else
                {
                    // Import as page.
                    if (restore)
                    {
                        var shapes = Enumerable.Concat(
                            page.Layers.SelectMany(x => x.Shapes),
                            page.Template?.Layers.SelectMany(x => x.Shapes));
                        TryToRestoreRecords(shapes);
                    }
                    Project?.AddPage(Project?.CurrentDocument, page);
                }
            }
            else if (item is IList<IPageContainer> templates)
            {
                if (restore)
                {
                    var shapes = templates.SelectMany(x => x.Layers).SelectMany(x => x.Shapes);
                    TryToRestoreRecords(shapes);
                }

                // Import as templates.
                Project.AddTemplates(templates);
            }
            else if (item is IList<IScript> scripts)
            {
                // Import as scripts.
                Project.AddScripts(scripts);
            }
            else if (item is IScript script)
            {
                Project?.AddScript(script);
            }
            else if (item is IDocumentContainer document)
            {
                if (restore)
                {
                    var shapes = Enumerable.Concat(
                        document.Pages.SelectMany(x => x.Layers).SelectMany(x => x.Shapes),
                        document.Pages.SelectMany(x => x.Template.Layers).SelectMany(x => x.Shapes));
                    TryToRestoreRecords(shapes);
                }
                Project?.AddDocument(document);
            }
            else if (item is IOptions options)
            {
                if (Project != null)
                {
                    Project.Options = options;
                }
            }
            else if (item is IProjectContainer project)
            {
                OnUnload();
                OnLoad(project, string.Empty);
            }
            else
            {
                throw new NotSupportedException("Not supported import object.");
            }
        }

        /// <inheritdoc/>
        public void OnImportJson(string path)
        {
            try
            {
                var json = FileIO?.ReadUtf8Text(path);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var item = JsonSerializer.Deserialize<object>(json);
                    if (item != null)
                    {
                        OnImportObject(item, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnImportSvg(string path)
        {
            if (SvgConverter == null)
            {
                return;
            }
            var shapes = SvgConverter.Convert(path, out _, out _);
            if (shapes != null)
            {
                OnPasteShapes(shapes);
            }
        }

        /// <inheritdoc/>
        public void OnExportJson(string path, object item)
        {
            try
            {
                var json = JsonSerializer?.Serialize(item);
                if (!string.IsNullOrWhiteSpace(json))
                {
                    FileIO?.WriteUtf8Text(path, json);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnExport(string path, object item, IFileWriter writer)
        {
            try
            {
                using var stream = FileIO.Create(path);
                writer?.Save(stream, item, Project);
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task OnExecuteCode(string csharp)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(csharp))
                {
                    await ScriptRunner?.Execute(csharp, null);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task OnExecuteRepl(string csharp)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(csharp))
                {
                    ScriptState = await ScriptRunner?.Execute(csharp, ScriptState);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnResetRepl()
        {
            ScriptState = null;
        }

        /// <inheritdoc/>
        public async Task OnExecuteScriptFile(string path)
        {
            try
            {
                var csharp = FileIO?.ReadUtf8Text(path);
                if (!string.IsNullOrWhiteSpace(csharp))
                {
                    await OnExecuteCode(csharp);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public async Task OnExecuteScriptFile(string[] paths)
        {
            foreach (var path in paths)
            {
                await OnExecuteScriptFile(path);
            }
        }

        /// <inheritdoc/>
        public async Task OnExecuteScript(IScript script)
        {
            try
            {
                var csharp = script?.Code;
                if (!string.IsNullOrWhiteSpace(csharp))
                {
                    await OnExecuteRepl(csharp);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnUndo()
        {
            try
            {
                if (Project?.History.CanUndo() ?? false)
                {
                    Deselect();
                    Project?.History.Undo();
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnRedo()
        {
            try
            {
                if (Project?.History.CanRedo() ?? false)
                {
                    Deselect();
                    Project?.History.Redo();
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnCut(object item)
        {
            if (item is IPageContainer page)
            {
                PageToCopy = page;
                DocumentToCopy = default;
                Project?.RemovePage(page);
                Project?.SetCurrentContainer(Project?.CurrentDocument?.Pages.FirstOrDefault());
            }
            else if (item is IDocumentContainer document)
            {
                PageToCopy = default;
                DocumentToCopy = document;
                Project?.RemoveDocument(document);

                var selected = Project?.Documents.FirstOrDefault();
                Project?.SetCurrentDocument(selected);
                Project?.SetCurrentContainer(selected?.Pages.FirstOrDefault());
            }
            else if (item is IProjectEditor || item == null)
            {
                if (CanCopy())
                {
                    OnCopy(item);
                    OnDeleteSelected();
                }
            }
        }

        /// <inheritdoc/>
        public void OnCopy(object item)
        {
            if (item is IPageContainer page)
            {
                PageToCopy = page;
                DocumentToCopy = default;
            }
            else if (item is IDocumentContainer document)
            {
                PageToCopy = default;
                DocumentToCopy = document;
            }
            else if (item is IProjectEditor || item == null)
            {
                if (CanCopy())
                {
                    if (PageState?.SelectedShapes != null)
                    {
                        OnCopyShapes(PageState.SelectedShapes.ToList());
                    }
                }
            }
        }

        /// <inheritdoc/>
        public async void OnPaste(object item)
        {
            if (Project != null && item is IPageContainer page)
            {
                if (PageToCopy != null)
                {
                    var document = Project?.Documents.FirstOrDefault(d => d.Pages.Contains(page));
                    if (document != null)
                    {
                        int index = document.Pages.IndexOf(page);
                        var clone = Clone(PageToCopy);
                        Project.ReplacePage(document, clone, index);
                        Project.SetCurrentContainer(clone);
                    }
                }
            }
            else if (Project != null && item is IDocumentContainer document)
            {
                if (PageToCopy != null)
                {
                    var clone = Clone(PageToCopy);
                    Project?.AddPage(document, clone);
                    Project.SetCurrentContainer(clone);
                }
                else if (DocumentToCopy != null)
                {
                    int index = Project.Documents.IndexOf(document);
                    var clone = Clone(DocumentToCopy);
                    Project.ReplaceDocument(clone, index);
                    Project.SetCurrentDocument(clone);
                    Project.SetCurrentContainer(clone?.Pages.FirstOrDefault());
                }
            }
            else if (item is IProjectEditor || item == null)
            {
                if (await CanPaste())
                {
                    var text = await (TextClipboard?.GetText() ?? Task.FromResult(string.Empty));
                    if (!string.IsNullOrEmpty(text))
                    {
                        OnTryPaste(text);
                    }
                }
            }
            else if (item is string text)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    OnTryPaste(text);
                }
            }
        }

        /// <inheritdoc/>
        public void OnDelete(object item)
        {
            if (item is ILayerContainer layer)
            {
                Project?.RemoveLayer(layer);

                var selected = Project?.CurrentContainer?.Layers.FirstOrDefault();
                if (layer.Owner is IPageContainer owner)
                {
                    owner.SetCurrentLayer(selected);
                }
            }
            if (item is IPageContainer page)
            {
                Project?.RemovePage(page);

                var selected = Project?.CurrentDocument?.Pages.FirstOrDefault();
                Project?.SetCurrentContainer(selected);
            }
            else if (item is IDocumentContainer document)
            {
                Project?.RemoveDocument(document);

                var selected = Project?.Documents.FirstOrDefault();
                Project?.SetCurrentDocument(selected);
                Project?.SetCurrentContainer(selected?.Pages.FirstOrDefault());
            }
            else if (item is IProjectEditor || item == null)
            {
                OnDeleteSelected();
            }
        }

        /// <inheritdoc/>
        public void OnShowDecorator()
        {
            if (PageState == null)
            {
                return;
            }

            if (PageState.DrawDecorators == false)
            {
                return;
            }

            var shapes = PageState.SelectedShapes?.ToList();
            if (shapes == null || shapes.Count <= 0)
            {
                return;
            }

            if (PageState.Decorator == null)
            {
                PageState.Decorator = new BoxDecorator(_serviceProvider);
            }

            PageState.Decorator.Layer = Project.CurrentContainer.WorkingLayer;
            PageState.Decorator.Shapes = shapes;
            PageState.Decorator.Update(true);
            PageState.Decorator.Show();
        }

        /// <inheritdoc/>
        public void OnUpdateDecorator()
        {
            if (PageState == null)
            {
                return;
            }

            if (PageState.DrawDecorators == false)
            {
                return;
            }

            if (PageState.Decorator != null)
            {
                PageState.Decorator.Update(false);
            }
        }

        /// <inheritdoc/>
        public void OnHideDecorator()
        {
            if (PageState == null)
            {
                return;
            }

            if (PageState.DrawDecorators == false)
            {
                return;
            }

            if (PageState.Decorator != null)
            {
                PageState.Decorator.Hide();
            }
        }

        /// <inheritdoc/>
        public void OnShowOrHideDecorator()
        {
            if (PageState == null)
            {
                return;
            }

            if (PageState.DrawDecorators == false)
            {
                return;
            }
      
            if (PageState.SelectedShapes?.Count == 1 && PageState.SelectedShapes?.FirstOrDefault() is IPointShape)
            {
                OnHideDecorator();
                return;
            }

            if (PageState.SelectedShapes?.Count == 1 && PageState.SelectedShapes?.FirstOrDefault() is ILineShape)
            {
                OnHideDecorator();
                return;
            }

            if (PageState.SelectedShapes != null)
            {
                OnShowDecorator();
            }
            else
            {
                OnHideDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnSelectAll()
        {
            try
            {
                Deselect(Project?.CurrentContainer?.CurrentLayer);
                Select(
                    Project?.CurrentContainer?.CurrentLayer,
                    new HashSet<IBaseShape>(Project?.CurrentContainer?.CurrentLayer?.Shapes));
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnDeselectAll()
        {
            try
            {
                Deselect(Project?.CurrentContainer?.CurrentLayer);
                OnUpdateDecorator();
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnClearAll()
        {
            try
            {
                var container = Project?.CurrentContainer;
                if (container != null)
                {
                    foreach (var layer in container.Layers)
                    {
                        Project?.ClearLayer(layer);
                    }

                    container.WorkingLayer.Shapes = ImmutableArray.Create<IBaseShape>();
                    container.HelperLayer.Shapes = ImmutableArray.Create<IBaseShape>();

                    Project.CurrentContainer.InvalidateLayer();
                    OnHideDecorator();
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnCancel()
        {
            OnDeselectAll();
            OnResetTool();
        }

        /// <inheritdoc/>
        public void OnDuplicateSelected()
        {
            try
            {
                if (PageState?.SelectedShapes == null)
                {
                    return;
                }

                var json = JsonSerializer?.Serialize(PageState.SelectedShapes.ToList());
                if (string.IsNullOrEmpty(json))
                {
                    return;
                }

                var shapes = JsonSerializer?.Deserialize<IList<IBaseShape>>(json);
                if (shapes?.Count() <= 0)
                {
                    return;
                }

                OnPasteShapes(shapes);
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnGroupSelected()
        {
            var group = Group(PageState?.SelectedShapes, ProjectEditorConfiguration.DefaulGroupName);
            if (group != null)
            {
                Select(Project?.CurrentContainer?.CurrentLayer, group);
            }
        }

        /// <inheritdoc/>
        public void OnUngroupSelected()
        {
            var result = Ungroup(PageState?.SelectedShapes);
            if (result == true && PageState != null)
            {
                PageState.SelectedShapes = null;
                OnHideDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnRotateSelected(string degrees)
        {
            if (!double.TryParse(degrees, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                BoxLayout.Rotate(sources, (decimal)value, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnFlipHorizontalSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                BoxLayout.Flip(sources, FlipMode.Horizontal, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnFlipVerticalSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                BoxLayout.Flip(sources, FlipMode.Vertical, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnMoveUpSelected()
        {
            MoveBy(
                PageState?.SelectedShapes,
                0m,
                Project.Options.SnapToGrid ? (decimal)-Project.Options.SnapY : -1m);
        }

        /// <inheritdoc/>
        public void OnMoveDownSelected()
        {
            MoveBy(
                PageState?.SelectedShapes,
                0m,
                Project.Options.SnapToGrid ? (decimal)Project.Options.SnapY : 1m);
        }

        /// <inheritdoc/>
        public void OnMoveLeftSelected()
        {
            MoveBy(
                PageState?.SelectedShapes,
                Project.Options.SnapToGrid ? (decimal)-Project.Options.SnapX : -1m,
                0m);
        }

        /// <inheritdoc/>
        public void OnMoveRightSelected()
        {
            MoveBy(
                PageState?.SelectedShapes,
                Project.Options.SnapToGrid ? (decimal)Project.Options.SnapX : 1m,
                0m);
        }

        /// <inheritdoc/>
        public void OnStackHorizontallySelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Stack(items, StackMode.Horizontal, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnStackVerticallySelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Stack(items, StackMode.Vertical, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnDistributeHorizontallySelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Distribute(items, DistributeMode.Horizontal, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnDistributeVerticallySelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Distribute(items, DistributeMode.Vertical, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignLeftSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Left, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignCenteredSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Centered, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignRightSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Right, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignTopSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Top, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignCenterSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Center, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnAlignBottomSelected()
        {
            var shapes = PageState?.SelectedShapes;
            if (shapes != null)
            {
                var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                BoxLayout.Align(items, AlignMode.Bottom, Project?.History);
                OnUpdateDecorator();
            }
        }

        /// <inheritdoc/>
        public void OnBringToFrontSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                foreach (var s in sources)
                {
                    BringToFront(s);
                }
            }
        }

        /// <inheritdoc/>
        public void OnBringForwardSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                foreach (var s in sources)
                {
                    BringForward(s);
                }
            }
        }

        /// <inheritdoc/>
        public void OnSendBackwardSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                foreach (var s in sources.Reverse())
                {
                    SendBackward(s);
                }
            }
        }

        /// <inheritdoc/>
        public void OnSendToBackSelected()
        {
            var sources = PageState?.SelectedShapes;
            if (sources != null)
            {
                foreach (var s in sources.Reverse())
                {
                    SendToBack(s);
                }
            }
        }

        /// <inheritdoc/>
        public void OnCreatePath()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            var source = PageState?.SelectedShapes?.FirstOrDefault();

            if (sources != null && sources.Count == 1)
            {
                var path = PathConverter.ToPathShape(source);
                if (path != null)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    var index = shapesBuilder.IndexOf(source);
                    shapesBuilder[index] = path;

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, path);
                }
            }

            if (sources != null && sources.Count > 1)
            {
                var path = PathConverter.ToPathShape(sources);
                if (path == null)
                {
                    return;
                }

                var shapesBuilder = layer.Shapes.ToBuilder();

                foreach (var shape in sources)
                {
                    shapesBuilder.Remove(shape);
                }
                shapesBuilder.Add(path);

                var previous = layer.Shapes;
                var next = shapesBuilder.ToImmutable();
                Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                layer.Shapes = next;

                Select(layer, path);
            }
        }

        /// <inheritdoc/>
        public void OnCreateStrokePath()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            var source = PageState?.SelectedShapes?.FirstOrDefault();

            if (sources != null && sources.Count == 1)
            {
                var path = PathConverter.ToStrokePathShape(source);
                if (path != null)
                {
                    path.IsStroked = false;
                    path.IsFilled = true;

                    var shapesBuilder = layer.Shapes.ToBuilder();

                    var index = shapesBuilder.IndexOf(source);
                    shapesBuilder[index] = path;

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, path);
                }
            }

            if (sources != null && sources.Count > 1)
            {
                var paths = new List<IPathShape>();
                var shapes = new List<IBaseShape>();

                foreach (var s in sources)
                {
                    var path = PathConverter.ToStrokePathShape(s);
                    if (path != null)
                    {
                        path.IsStroked = false;
                        path.IsFilled = true;

                        paths.Add(path);
                        shapes.Add(s);
                    }
                }

                if (paths.Count > 0)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    for (int i = 0; i < paths.Count; i++)
                    {
                        var index = shapesBuilder.IndexOf(shapes[i]);
                        shapesBuilder[index] = paths[i];
                    }

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, new HashSet<IBaseShape>(paths));
                }
            }
        }

        /// <inheritdoc/>
        public void OnCreateFillPath()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            var source = PageState?.SelectedShapes?.FirstOrDefault();

            if (sources != null && sources.Count == 1)
            {
                var path = PathConverter.ToFillPathShape(source);
                if (path != null)
                {
                    path.IsStroked = false;
                    path.IsFilled = true;

                    var shapesBuilder = layer.Shapes.ToBuilder();

                    var index = shapesBuilder.IndexOf(source);
                    shapesBuilder[index] = path;

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, path);
                }
            }

            if (sources != null && sources.Count > 1)
            {
                var paths = new List<IPathShape>();
                var shapes = new List<IBaseShape>();

                foreach (var s in sources)
                {
                    var path = PathConverter.ToFillPathShape(s);
                    if (path != null)
                    {
                        path.IsStroked = false;
                        path.IsFilled = true;

                        paths.Add(path);
                        shapes.Add(s);
                    }
                }

                if (paths.Count > 0)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    for (int i = 0; i < paths.Count; i++)
                    {
                        var index = shapesBuilder.IndexOf(shapes[i]);
                        shapesBuilder[index] = paths[i];
                    }

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, new HashSet<IBaseShape>(paths));
                }
            }
        }

        /// <inheritdoc/>
        public void OnCreateWindingPath()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            var source = PageState?.SelectedShapes?.FirstOrDefault();

            if (sources != null && sources.Count == 1)
            {
                var path = PathConverter.ToWindingPathShape(source);
                if (path != null)
                {
                    path.IsStroked = false;
                    path.IsFilled = true;

                    var shapesBuilder = layer.Shapes.ToBuilder();

                    var index = shapesBuilder.IndexOf(source);
                    shapesBuilder[index] = path;

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, path);
                }
            }

            if (sources != null && sources.Count > 1)
            {
                var paths = new List<IPathShape>();
                var shapes = new List<IBaseShape>();

                foreach (var s in sources)
                {
                    var path = PathConverter.ToWindingPathShape(s);
                    if (path != null)
                    {
                        path.IsStroked = false;
                        path.IsFilled = true;

                        paths.Add(path);
                        shapes.Add(s);
                    }
                }

                if (paths.Count > 0)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    for (int i = 0; i < paths.Count; i++)
                    {
                        var index = shapesBuilder.IndexOf(shapes[i]);
                        shapesBuilder[index] = paths[i];
                    }

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, new HashSet<IBaseShape>(paths));
                }
            }
        }

        /// <inheritdoc/>
        public void OnPathSimplify()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            var source = PageState?.SelectedShapes?.FirstOrDefault();

            if (sources != null && sources.Count == 1)
            {
                var path = PathConverter.Simplify(source);
                if (path != null)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    var index = shapesBuilder.IndexOf(source);
                    shapesBuilder[index] = path;

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, path);
                }
            }

            if (sources != null && sources.Count > 1)
            {
                var paths = new List<IPathShape>();
                var shapes = new List<IBaseShape>();

                foreach (var s in sources)
                {
                    var path = PathConverter.Simplify(s);
                    if (path != null)
                    {
                        paths.Add(path);
                        shapes.Add(s);
                    }
                }

                if (paths.Count > 0)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    for (int i = 0; i < paths.Count; i++)
                    {
                        var index = shapesBuilder.IndexOf(shapes[i]);
                        shapesBuilder[index] = paths[i];
                    }

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, new HashSet<IBaseShape>(paths));
                }
            }
        }

        /// <inheritdoc/>
        public void OnPathBreak()
        {
            if (PathConverter == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
 
            if (sources != null && sources.Count >= 1)
            {
                var result = new List<IBaseShape>();
                var remove = new List<IBaseShape>();

                foreach (var s in sources)
                {
                    _shapeEditor.BreakShape(s, result, remove);
                }

                if (result.Count > 0)
                {
                    var shapesBuilder = layer.Shapes.ToBuilder();

                    for (int i = 0; i < remove.Count; i++)
                    {
                        shapesBuilder.Remove(remove[i]);
                    }

                    for (int i = 0; i < result.Count; i++)
                    {
                        shapesBuilder.Add(result[i]);
                    }

                    var previous = layer.Shapes;
                    var next = shapesBuilder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                    layer.Shapes = next;

                    Select(layer, new HashSet<IBaseShape>(result));
                }
            }
        }

        /// <inheritdoc/>
        public void OnPathOp(string op)
        {
            if (!Enum.TryParse<PathOp>(op, true, out var pathOp))
            {
                return;
            }

            if (PathConverter == null)
            {
                return;
            }

            var sources = PageState?.SelectedShapes;
            if (sources == null)
            {
                return;
            }

            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer == null)
            {
                return;
            }

            var path = PathConverter.Op(sources, pathOp);
            if (path == null)
            {
                return;
            }

            var shapesBuilder = layer.Shapes.ToBuilder();
            foreach (var shape in sources)
            {
                shapesBuilder.Remove(shape);
            }
            shapesBuilder.Add(path);

            var previous = layer.Shapes;
            var next = shapesBuilder.ToImmutable();
            Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
            layer.Shapes = next;

            Select(layer, path);
        }

        /// <inheritdoc/>
        public void OnToolNone()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "None");
        }

        /// <inheritdoc/>
        public void OnToolSelection()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Selection");
        }

        /// <inheritdoc/>
        public void OnToolPoint()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Point");
        }

        /// <inheritdoc/>
        public void OnToolLine()
        {
            if (CurrentTool.Title == "Path" && CurrentPathTool.Title != "Line")
            {
                CurrentPathTool?.Reset();
                CurrentPathTool = PathTools.FirstOrDefault(t => t.Title == "Line");
            }
            else
            {
                CurrentTool?.Reset();
                CurrentTool = Tools.FirstOrDefault(t => t.Title == "Line");
            }
        }

        /// <inheritdoc/>
        public void OnToolArc()
        {
            if (CurrentTool.Title == "Path" && CurrentPathTool.Title != "Arc")
            {
                CurrentPathTool?.Reset();
                CurrentPathTool = PathTools.FirstOrDefault(t => t.Title == "Arc");
            }
            else
            {
                CurrentTool?.Reset();
                CurrentTool = Tools.FirstOrDefault(t => t.Title == "Arc");
            }
        }

        /// <inheritdoc/>
        public void OnToolCubicBezier()
        {
            if (CurrentTool.Title == "Path" && CurrentPathTool.Title != "CubicBezier")
            {
                CurrentPathTool?.Reset();
                CurrentPathTool = PathTools.FirstOrDefault(t => t.Title == "CubicBezier");
            }
            else
            {
                CurrentTool?.Reset();
                CurrentTool = Tools.FirstOrDefault(t => t.Title == "CubicBezier");
            }
        }

        /// <inheritdoc/>
        public void OnToolQuadraticBezier()
        {
            if (CurrentTool.Title == "Path" && CurrentPathTool.Title != "QuadraticBezier")
            {
                CurrentPathTool?.Reset();
                CurrentPathTool = PathTools.FirstOrDefault(t => t.Title == "QuadraticBezier");
            }
            else
            {
                CurrentTool?.Reset();
                CurrentTool = Tools.FirstOrDefault(t => t.Title == "QuadraticBezier");
            }
        }

        /// <inheritdoc/>
        public void OnToolPath()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Path");
        }

        /// <inheritdoc/>
        public void OnToolRectangle()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Rectangle");
        }

        /// <inheritdoc/>
        public void OnToolEllipse()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Ellipse");
        }

        /// <inheritdoc/>
        public void OnToolText()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Text");
        }

        /// <inheritdoc/>
        public void OnToolImage()
        {
            CurrentTool?.Reset();
            CurrentTool = Tools.FirstOrDefault(t => t.Title == "Image");
        }

        /// <inheritdoc/>
        public void OnToolMove()
        {
            if (CurrentTool.Title == "Path" && CurrentPathTool.Title != "Move")
            {
                CurrentPathTool?.Reset();
                CurrentPathTool = PathTools.FirstOrDefault(t => t.Title == "Move");
            }
        }

        /// <inheritdoc/>
        public void OnResetTool()
        {
            CurrentTool?.Reset();
        }

        /// <inheritdoc/>
        public void OnToggleDefaultIsStroked()
        {
            if (Project?.Options != null)
            {
                Project.Options.DefaultIsStroked = !Project.Options.DefaultIsStroked;
            }
        }

        /// <inheritdoc/>
        public void OnToggleDefaultIsFilled()
        {
            if (Project?.Options != null)
            {
                Project.Options.DefaultIsFilled = !Project.Options.DefaultIsFilled;
            }
        }

        /// <inheritdoc/>
        public void OnToggleDefaultIsClosed()
        {
            if (Project?.Options != null)
            {
                Project.Options.DefaultIsClosed = !Project.Options.DefaultIsClosed;
            }
        }


        /// <inheritdoc/>
        public void OnToggleSnapToGrid()
        {
            if (Project?.Options != null)
            {
                Project.Options.SnapToGrid = !Project.Options.SnapToGrid;
            }
        }

        /// <inheritdoc/>
        public void OnToggleTryToConnect()
        {
            if (Project?.Options != null)
            {
                Project.Options.TryToConnect = !Project.Options.TryToConnect;
            }
        }

        /// <inheritdoc/>
        public void OnAddDatabase()
        {
            var db = Factory.CreateDatabase(ProjectEditorConfiguration.DefaultDatabaseName);
            Project.AddDatabase(db);
            Project.SetCurrentDatabase(db);
        }

        /// <inheritdoc/>
        public void OnRemoveDatabase(IDatabase db)
        {
            Project.RemoveDatabase(db);
            Project.SetCurrentDatabase(Project.Databases.FirstOrDefault());
        }

        /// <inheritdoc/>
        public void OnAddColumn(IDatabase db)
        {
            Project.AddColumn(db, Factory.CreateColumn(db, ProjectEditorConfiguration.DefaulColumnName));
        }

        /// <inheritdoc/>
        public void OnRemoveColumn(IColumn column)
        {
            Project.RemoveColumn(column);
        }

        /// <inheritdoc/>
        public void OnAddRecord(IDatabase db)
        {
            Project.AddRecord(db, Factory.CreateRecord(db, ProjectEditorConfiguration.DefaulValue));
        }

        /// <inheritdoc/>
        public void OnRemoveRecord(IRecord record)
        {
            Project.RemoveRecord(record);
        }

        /// <inheritdoc/>
        public void OnResetRecord(IContext data)
        {
            Project.ResetRecord(data);
        }

        /// <inheritdoc/>
        public void OnApplyRecord(IRecord record)
        {
            if (record != null)
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    foreach (var shape in PageState.SelectedShapes)
                    {
                        Project.ApplyRecord(shape.Data, record);
                    }
                }

                if (PageState.SelectedShapes == null)
                {
                    var container = Project?.CurrentContainer;
                    if (container != null)
                    {
                        Project?.ApplyRecord(container.Data, record);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnAddProperty(IContext data)
        {
            Project.AddProperty(data, Factory.CreateProperty(data, ProjectEditorConfiguration.DefaulPropertyName, ProjectEditorConfiguration.DefaulValue));
        }

        /// <inheritdoc/>
        public void OnRemoveProperty(IProperty property)
        {
            Project.RemoveProperty(property);
        }

        /// <inheritdoc/>
        public void OnAddGroupLibrary()
        {
            var gl = Factory.CreateLibrary<IGroupShape>(ProjectEditorConfiguration.DefaulGroupLibraryName);
            Project.AddGroupLibrary(gl);
            Project.SetCurrentGroupLibrary(gl);
        }

        /// <inheritdoc/>
        public void OnRemoveGroupLibrary(ILibrary<IGroupShape> library)
        {
            Project.RemoveGroupLibrary(library);
            Project.SetCurrentGroupLibrary(Project?.GroupLibraries.FirstOrDefault());
        }

        /// <inheritdoc/>
        public void OnAddGroup(ILibrary<IGroupShape> library)
        {
            if (Project != null && library != null)
            {
                if (PageState.SelectedShapes?.Count == 1 && PageState.SelectedShapes?.FirstOrDefault() is IGroupShape group)
                {
                    var clone = CloneShape(group);
                    if (clone != null)
                    {
                        Project?.AddGroup(library, clone);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnRemoveGroup(IGroupShape group)
        {
            if (Project != null && group != null)
            {
                var library = Project.RemoveGroup(group);
                library?.SetSelected(library?.Items.FirstOrDefault());
            }
        }

        /// <inheritdoc/>
        public void OnInsertGroup(IGroupShape group)
        {
            if (Project?.CurrentContainer != null)
            {
                OnDropShapeAsClone(group, 0.0, 0.0);
            }
        }

        /// <inheritdoc/>
        public void OnAddLayer(IPageContainer container)
        {
            if (container != null)
            {
                Project.AddLayer(container, Factory.CreateLayerContainer(ProjectEditorConfiguration.DefaultLayerName, container));
            }
        }

        /// <inheritdoc/>
        public void OnRemoveLayer(ILayerContainer layer)
        {
            if (layer != null)
            {
                Project.RemoveLayer(layer);
                if (layer.Owner is IPageContainer owner)
                {
                    owner.SetCurrentLayer(owner.Layers.FirstOrDefault());
                }
            }
        }

        /// <inheritdoc/>
        public void OnAddStyleLibrary()
        {
            var sl = Factory.CreateLibrary<IShapeStyle>(ProjectEditorConfiguration.DefaulStyleLibraryName);
            Project.AddStyleLibrary(sl);
            Project.SetCurrentStyleLibrary(sl);
        }

        /// <inheritdoc/>
        public void OnRemoveStyleLibrary(ILibrary<IShapeStyle> library)
        {
            Project.RemoveStyleLibrary(library);
            Project.SetCurrentStyleLibrary(Project?.StyleLibraries.FirstOrDefault());
        }

        /// <inheritdoc/>
        public void OnAddStyle(ILibrary<IShapeStyle> library)
        {
            if (PageState?.SelectedShapes != null)
            {
                foreach (var shape in PageState.SelectedShapes)
                {
                    if (shape.Style != null)
                    {
                        var style = (IShapeStyle)shape.Style.Copy(null);
                        Project.AddStyle(library, style);
                    }
                }
            }
            else
            {
                var style = Factory.CreateShapeStyle(ProjectEditorConfiguration.DefaulStyleName);
                Project.AddStyle(library, style);
            }
        }

        /// <inheritdoc/>
        public void OnRemoveStyle(IShapeStyle style)
        {
            var library = Project.RemoveStyle(style);
            library?.SetSelected(library?.Items.FirstOrDefault());
        }

        /// <inheritdoc/>
        public void OnApplyStyle(IShapeStyle style)
        {
            if (style != null)
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    foreach (var shape in PageState.SelectedShapes)
                    {
                        Project?.ApplyStyle(shape, style);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnApplyData(IContext data)
        {
            if (data != null)
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    foreach (var shape in PageState.SelectedShapes)
                    {
                        Project?.ApplyData(shape, data);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnAddShape(IBaseShape shape)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null && shape != null)
            {
                Project.AddShape(layer, shape);
            }
        }

        /// <inheritdoc/>
        public void OnRemoveShape(IBaseShape shape)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null && shape != null)
            {
                Project.RemoveShape(layer, shape);
                Project.CurrentContainer.CurrentShape = layer.Shapes.FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public void OnAddTemplate()
        {
            if (Project != null)
            {
                var template = ContainerFactory.GetTemplate(Project, "Empty");
                if (template == null)
                {
                    template = Factory.CreateTemplateContainer(ProjectEditorConfiguration.DefaultTemplateName);
                }

                Project.AddTemplate(template);
            }
        }

        /// <inheritdoc/>
        public void OnRemoveTemplate(IPageContainer template)
        {
            if (template != null)
            {
                Project?.RemoveTemplate(template);
                Project?.SetCurrentTemplate(Project?.Templates.FirstOrDefault());
            }
        }

        /// <inheritdoc/>
        public void OnEditTemplate(IPageContainer template)
        {
            if (Project != null && template != null)
            {
                Project.SetCurrentContainer(template);
                Project.CurrentContainer?.InvalidateLayer();
            }
        }

        /// <inheritdoc/>
        public void OnAddScript()
        {
            if (Project != null)
            {
                var script = Factory.CreateScript(ProjectEditorConfiguration.DefaultScriptName);
                Project.AddScript(script);
            }
        }

        /// <inheritdoc/>
        public void OnRemoveScript(IScript script)
        {
            if (script != null)
            {
                Project?.RemoveScript(script);
                Project?.SetCurrentScript(Project?.Scripts.FirstOrDefault());
            }
        }

        /// <inheritdoc/>
        public void OnApplyTemplate(IPageContainer template)
        {
            var page = Project?.CurrentContainer;
            if (page != null && template != null && page != template)
            {
                Project.ApplyTemplate(page, template);
                Project.CurrentContainer.InvalidateLayer();
            }
        }

        /// <inheritdoc/>
        public string OnGetImageKey(string path)
        {
            using var stream = FileIO.Open(path);
            var bytes = FileIO.ReadBinary(stream);
            if (Project is IImageCache imageCache)
            {
                var key = imageCache.AddImageFromFile(path, bytes);
                return key;
            }
            return default;
        }

        /// <inheritdoc/>
        public async Task<string> OnAddImageKey(string path)
        {
            if (Project != null)
            {
                if (path == null || string.IsNullOrEmpty(path))
                {
                    var key = await (ImageImporter.GetImageKeyAsync() ?? Task.FromResult(string.Empty));
                    if (key == null || string.IsNullOrEmpty(key))
                    {
                        return null;
                    }

                    return key;
                }
                else
                {
                    byte[] bytes;
                    using (var stream = FileIO?.Open(path))
                    {
                        bytes = FileIO?.ReadBinary(stream);
                    }
                    if (Project is IImageCache imageCache)
                    {
                        var key = imageCache.AddImageFromFile(path, bytes);
                        return key;
                    }
                    return null;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public void OnRemoveImageKey(string key)
        {
            if (key != null)
            {
                if (Project is IImageCache imageCache)
                {
                    imageCache.RemoveImage(key);
                }
            }
        }

        /// <inheritdoc/>
        public void OnSelectedItemChanged(IObservableObject item)
        {
            if (Project != null)
            {
                Project.Selected = item;
            }
        }

        /// <inheritdoc/>
        public void OnAddPage(object item)
        {
            if (Project?.CurrentDocument != null)
            {
                var page =
                    ContainerFactory?.GetPage(Project, ProjectEditorConfiguration.DefaultPageName)
                    ?? Factory.CreatePageContainer(ProjectEditorConfiguration.DefaultPageName);

                Project.AddPage(Project.CurrentDocument, page);
                Project.SetCurrentContainer(page);
            }
        }

        /// <inheritdoc/>
        public void OnInsertPageBefore(object item)
        {
            if (Project?.CurrentDocument != null)
            {
                if (item is IPageContainer selected)
                {
                    int index = Project.CurrentDocument.Pages.IndexOf(selected);
                    var page =
                        ContainerFactory?.GetPage(Project, ProjectEditorConfiguration.DefaultPageName)
                        ?? Factory.CreatePageContainer(ProjectEditorConfiguration.DefaultPageName);

                    Project.AddPageAt(Project.CurrentDocument, page, index);
                    Project.SetCurrentContainer(page);
                }
            }
        }

        /// <inheritdoc/>
        public void OnInsertPageAfter(object item)
        {
            if (Project?.CurrentDocument != null)
            {
                if (item is IPageContainer selected)
                {
                    int index = Project.CurrentDocument.Pages.IndexOf(selected);
                    var page =
                        ContainerFactory?.GetPage(Project, ProjectEditorConfiguration.DefaultPageName)
                        ?? Factory.CreatePageContainer(ProjectEditorConfiguration.DefaultPageName);

                    Project.AddPageAt(Project.CurrentDocument, page, index + 1);
                    Project.SetCurrentContainer(page);
                }
            }
        }

        /// <inheritdoc/>
        public void OnAddDocument(object item)
        {
            if (Project != null)
            {
                var document =
                    ContainerFactory?.GetDocument(Project, ProjectEditorConfiguration.DefaultDocumentName)
                    ?? Factory.CreateDocumentContainer(ProjectEditorConfiguration.DefaultDocumentName);

                Project.AddDocument(document);
                Project.SetCurrentDocument(document);
                Project.SetCurrentContainer(document?.Pages.FirstOrDefault());
            }
        }

        /// <inheritdoc/>
        public void OnInsertDocumentBefore(object item)
        {
            if (Project != null)
            {
                if (item is IDocumentContainer selected)
                {
                    int index = Project.Documents.IndexOf(selected);
                    var document =
                        ContainerFactory?.GetDocument(Project, ProjectEditorConfiguration.DefaultDocumentName)
                        ?? Factory.CreateDocumentContainer(ProjectEditorConfiguration.DefaultDocumentName);

                    Project.AddDocumentAt(document, index);
                    Project.SetCurrentDocument(document);
                    Project.SetCurrentContainer(document?.Pages.FirstOrDefault());
                }
            }
        }

        /// <inheritdoc/>
        public void OnInsertDocumentAfter(object item)
        {
            if (Project != null)
            {
                if (item is IDocumentContainer selected)
                {
                    int index = Project.Documents.IndexOf(selected);
                    var document =
                        ContainerFactory?.GetDocument(Project, ProjectEditorConfiguration.DefaultDocumentName)
                        ?? Factory.CreateDocumentContainer(ProjectEditorConfiguration.DefaultDocumentName);

                    Project.AddDocumentAt(document, index + 1);
                    Project.SetCurrentDocument(document);
                    Project.SetCurrentContainer(document?.Pages.FirstOrDefault());
                }
            }
        }

        /// <inheritdoc/>
        private void SetRenderersImageCache(IImageCache cache)
        {
            if (PageRenderer != null)
            {
                PageRenderer.ClearCache();
                PageRenderer.State.ImageCache = cache;
            }

            if (DocumentRenderer != null)
            {
                DocumentRenderer.ClearCache();
                DocumentRenderer.State.ImageCache = cache;
            }
        }

        /// <inheritdoc/>
        public void OnLoad(IProjectContainer project, string? path = null)
        {
            if (project != null)
            {
                Deselect();
                if (project is IImageCache imageCache)
                {
                    SetRenderersImageCache(imageCache);
                }
                Project = project;
                Project.History = new StackHistory();
                ProjectPath = path;
                IsProjectDirty = false;
                Observer = new ProjectObserver(this);
            }
        }

        /// <inheritdoc/>
        public void OnUnload()
        {
            if (Observer != null)
            {
                Observer?.Dispose();
                Observer = null;
            }

            if (Project?.History != null)
            {
                Project.History.Reset();
                Project.History = null;
            }

            if (Project != null)
            {
                if (Project is IImageCache imageCache)
                {
                    imageCache.PurgeUnusedImages(new HashSet<string>());
                }
                Deselect();
                SetRenderersImageCache(null);
                Project = null;
                ProjectPath = string.Empty;
                IsProjectDirty = false;
                GC.Collect();
            }
        }

        /// <inheritdoc/>
        public void OnInvalidateCache()
        {
            try
            {
                if (PageRenderer != null)
                {
                    PageRenderer.ClearCache();
                }

                if (DocumentRenderer != null)
                {
                    DocumentRenderer.ClearCache();
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnAddRecent(string path, string name)
        {
            if (_recentProjects != null)
            {
                var q = _recentProjects.Where(x => x.Path.ToLower() == path.ToLower()).ToList();
                var builder = _recentProjects.ToBuilder();

                if (q.Count() > 0)
                {
                    foreach (var r in q)
                    {
                        builder.Remove(r);
                    }
                }

                builder.Insert(0, RecentFile.Create(name, path));

                RecentProjects = builder.ToImmutable();
                CurrentRecentProject = _recentProjects.FirstOrDefault();
            }
        }

        /// <inheritdoc/>
        public void OnLoadRecent(string path)
        {
            if (JsonSerializer != null)
            {
                try
                {
                    var json = FileIO.ReadUtf8Text(path);
                    var recent = JsonSerializer.Deserialize<Recents>(json);
                    if (recent != null)
                    {
                        var remove = recent.Files.Where(x => FileIO?.Exists(x.Path) == false).ToList();
                        var builder = recent.Files.ToBuilder();

                        foreach (var file in remove)
                        {
                            builder.Remove(file);
                        }

                        RecentProjects = builder.ToImmutable();

                        if (recent.Current != null
                            && (FileIO?.Exists(recent.Current.Path) ?? false))
                        {
                            CurrentRecentProject = recent.Current;
                        }
                        else
                        {
                            CurrentRecentProject = _recentProjects.FirstOrDefault();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log?.LogException(ex);
                }
            }
        }

        /// <inheritdoc/>
        public void OnSaveRecent(string path)
        {
            if (JsonSerializer != null)
            {
                try
                {
                    var recent = Recents.Create(_recentProjects, _currentRecentProject);
                    var json = JsonSerializer.Serialize(recent);
                    FileIO.WriteUtf8Text(path, json);
                }
                catch (Exception ex)
                {
                    Log?.LogException(ex);
                }
            }
        }

        /// <inheritdoc/>
        public bool CanUndo()
        {
            return Project?.History?.CanUndo() ?? false;
        }

        /// <inheritdoc/>
        public bool CanRedo()
        {
            return Project?.History?.CanRedo() ?? false;
        }

        /// <inheritdoc/>
        public bool CanCopy()
        {
            return PageState?.SelectedShapes != null;
        }

        /// <inheritdoc/>
        public async Task<bool> CanPaste()
        {
            try
            {
                return await (TextClipboard?.ContainsText() ?? Task.FromResult(false));
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
            return false;
        }

        /// <inheritdoc/>
        public void OnCopyShapes(IList<IBaseShape> shapes)
        {
            try
            {
                var json = JsonSerializer?.Serialize(shapes);
                if (!string.IsNullOrEmpty(json))
                {
                    TextClipboard?.SetText(json);
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnTryPaste(string text)
        {
            try
            {
                var exception = default(Exception);

                try
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        var pathShape = PathConverter?.FromSvgPathData(text, isStroked: false, isFilled: true);
                        if (pathShape != null)
                        {
                            OnPasteShapes(Enumerable.Repeat<IBaseShape>(pathShape, 1));
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                try
                {
                    var shapes = JsonSerializer?.Deserialize<IList<IBaseShape>>(text);
                    if (shapes?.Count() > 0)
                    {
                        OnPasteShapes(shapes);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                throw exception;
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <summary>
        /// Generates record dictionary with id as the key.
        /// </summary>
        /// <returns>The record dictionary with id as the key.</returns>
        private IDictionary<string, IRecord> GenerateRecordDictionaryById()
        {
            return Project?.Databases
                .Where(d => d?.Records != null && d?.Records.Length > 0)
                .SelectMany(d => d.Records)
                .ToDictionary(s => s.Id);
        }

        /// <summary>
        /// Try to restore shape records.
        /// </summary>
        /// <param name="shapes">The shapes collection.</param>
        private void TryToRestoreRecords(IEnumerable<IBaseShape> shapes)
        {
            try
            {
                if (Project?.Databases == null)
                {
                    return;
                }

                var records = GenerateRecordDictionaryById();

                // Try to restore shape record.
                foreach (var shape in ProjectContainer.GetAllShapes(shapes))
                {
                    if (shape?.Data?.Record == null)
                    {
                        continue;
                    }

                    if (records.TryGetValue(shape.Data.Record.Id, out var record))
                    {
                        // Use existing record.
                        shape.Data.Record = record;
                    }
                    else
                    {
                        // Create Imported database.
                        if (Project?.CurrentDatabase == null && shape.Data.Record.Owner is IDatabase owner)
                        {
                            var db = Factory.CreateDatabase(
                                ProjectEditorConfiguration.ImportedDatabaseName,
                                owner.Columns);
                            Project.AddDatabase(db);
                            Project.SetCurrentDatabase(db);
                        }

                        // Add missing data record.
                        shape.Data.Record.Owner = Project.CurrentDatabase;
                        Project?.AddRecord(Project?.CurrentDatabase, shape.Data.Record);

                        // Recreate records dictionary.
                        records = GenerateRecordDictionaryById();
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <summary>
        /// Restore shape project references afer closing.
        /// </summary>
        /// <param name="shape">The shape to restore.</param>
        private void RestoreShape(IBaseShape shape)
        {
            var shapes = Enumerable.Repeat(shape, 1).ToList();
            TryToRestoreRecords(shapes);
        }

        /// <inheritdoc/>
        public void OnPasteShapes(IEnumerable<IBaseShape> shapes)
        {
            try
            {
                Deselect(Project?.CurrentContainer?.CurrentLayer);
                TryToRestoreRecords(shapes);
                Project.AddShapes(Project?.CurrentContainer?.CurrentLayer, shapes);
                OnSelect(shapes);
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public void OnSelect(IEnumerable<IBaseShape> shapes)
        {
            if (shapes?.Count() == 1)
            {
                Select(Project?.CurrentContainer?.CurrentLayer, shapes.FirstOrDefault());
            }
            else
            {
                Select(Project?.CurrentContainer?.CurrentLayer, new HashSet<IBaseShape>(shapes));
            }
        }

        /// <inheritdoc/>
        public T CloneShape<T>(T shape) where T : IBaseShape
        {
            try
            {
                if (JsonSerializer is IJsonSerializer serializer)
                {
                    var json = serializer.Serialize(shape);
                    if (!string.IsNullOrEmpty(json))
                    {
                        var clone = serializer.Deserialize<T>(json);
                        if (clone != null)
                        {
                            RestoreShape(clone);
                            return clone;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }

            return default;
        }

        /// <inheritdoc/>
        public ILayerContainer Clone(ILayerContainer container)
        {
            try
            {
                var json = JsonSerializer?.Serialize(container);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = JsonSerializer?.Deserialize<ILayerContainer>(json);
                    if (clone != null)
                    {
                        var shapes = clone.Shapes;
                        TryToRestoreRecords(shapes);
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }

            return default;
        }

        /// <inheritdoc/>
        public IPageContainer Clone(IPageContainer container)
        {
            try
            {
                var template = container?.Template;
                var json = JsonSerializer?.Serialize(container);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = JsonSerializer?.Deserialize<IPageContainer>(json);
                    if (clone != null)
                    {
                        var shapes = clone.Layers.SelectMany(l => l.Shapes);
                        TryToRestoreRecords(shapes);
                        clone.Template = template;
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }

            return default;
        }

        /// <inheritdoc/>
        public IDocumentContainer Clone(IDocumentContainer document)
        {
            try
            {
                var templates = document?.Pages.Select(c => c?.Template)?.ToArray();
                var json = JsonSerializer?.Serialize(document);
                if (!string.IsNullOrEmpty(json))
                {
                    var clone = JsonSerializer?.Deserialize<IDocumentContainer>(json);
                    if (clone != null)
                    {
                        for (int i = 0; i < clone.Pages.Length; i++)
                        {
                            var container = clone.Pages[i];
                            var shapes = container.Layers.SelectMany(l => l.Shapes);
                            TryToRestoreRecords(shapes);
                            container.Template = templates[i];
                        }
                        return clone;
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }

            return default;
        }

        /// <inheritdoc/>
        public async Task<bool> OnDropFiles(string[] files, double x, double y)
        {
            try
            {
                if (files?.Length < 1)
                {
                    return false;
                }

                bool result = false;

                foreach (var path in files)
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    string ext = System.IO.Path.GetExtension(path);

                    if (string.Compare(ext, ProjectEditorConfiguration.ProjectExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OnOpenProject(path);
                        result = true;
                    }
                    else if (string.Compare(ext, ProjectEditorConfiguration.CsvExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var reader = TextFieldReaders.FirstOrDefault(x => x.Extension == "csv");
                        if (reader != null)
                        {
                            OnImportData(Project, path, reader);
                            result = true;
                        }
                    }
                    else if (string.Compare(ext, ProjectEditorConfiguration.XlsxExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        var reader = TextFieldReaders.FirstOrDefault(x => x.Extension == "xlsx");
                        if (reader != null)
                        {
                            OnImportData(Project, path, reader);
                            result = true;
                        }
                    }
                    else if (string.Compare(ext, ProjectEditorConfiguration.JsonExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OnImportJson(path);
                        result = true;
                    }
                    else if (string.Compare(ext, ProjectEditorConfiguration.ScriptExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        await OnExecuteScriptFile(path);
                        result = true;
                    }
                    else if (string.Compare(ext, ProjectEditorConfiguration.SvgExtension, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        OnImportSvg(path);
                        result = true;
                    }
                    else if (ProjectEditorConfiguration.ImageExtensions.Any(x => string.Compare(ext, x, StringComparison.OrdinalIgnoreCase) == 0))
                    {
                        var key = OnGetImageKey(path);
                        if (key != null && !string.IsNullOrEmpty(key))
                        {
                            OnDropImageKey(key, x, y);
                        }
                        result = true;
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }

            return false;
        }

        /// <inheritdoc/>
        public void OnDropImageKey(string key, double x, double y)
        {
            var selected = Project.CurrentStyleLibrary?.Selected != null ?
                Project.CurrentStyleLibrary.Selected :
                Factory.CreateShapeStyle(ProjectEditorConfiguration.DefaulStyleName);
            var style = (IShapeStyle)selected.Copy(null);
            var layer = Project?.CurrentContainer?.CurrentLayer;
            decimal sx = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)x, (decimal)Project.Options.SnapX) : (decimal)x;
            decimal sy = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)y, (decimal)Project.Options.SnapY) : (decimal)y;

            var image = Factory.CreateImageShape((double)sx, (double)sy, style, key);
            image.BottomRight.X = (double)(sx + 320m);
            image.BottomRight.Y = (double)(sy + 180m);

            Project.AddShape(layer, image);
        }

        /// <inheritdoc/>
        public bool OnDropShape(IBaseShape shape, double x, double y, bool bExecute = true)
        {
            try
            {
                var layer = Project?.CurrentContainer?.CurrentLayer;
                if (layer != null)
                {
                    if (bExecute == true)
                    {
                        OnDropShapeAsClone(shape, x, y);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
            return false;
        }

        /// <inheritdoc/>
        public void OnDropShapeAsClone<T>(T shape, double x, double y) where T : IBaseShape
        {
            decimal sx = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)x, (decimal)Project.Options.SnapX) : (decimal)x;
            decimal sy = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)y, (decimal)Project.Options.SnapY) : (decimal)y;

            try
            {
                var clone = CloneShape(shape);
                if (clone != null)
                {
                    Deselect(Project?.CurrentContainer?.CurrentLayer);
                    clone.Move(null, sx, sy);

                    Project.AddShape(Project?.CurrentContainer?.CurrentLayer, clone);

                    // Select(Project?.CurrentContainer?.CurrentLayer, clone);

                    if (Project.Options.TryToConnect)
                    {
                        if (clone is IGroupShape group)
                        {
                            var shapes = ProjectContainer.GetAllShapes<ILineShape>(Project?.CurrentContainer?.CurrentLayer?.Shapes);
                            TryToConnectLines(shapes, group.Connectors);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
        }

        /// <inheritdoc/>
        public bool OnDropRecord(IRecord record, double x, double y, bool bExecute = true)
        {
            try
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    if (bExecute)
                    {
                        OnApplyRecord(record);
                    }
                    return true;
                }
                else
                {
                    var layer = Project?.CurrentContainer?.CurrentLayer;
                    if (layer != null)
                    {
                        var shapes = layer.Shapes.Reverse();
                        double radius = Project.Options.HitThreshold / PageState.ZoomX;
                        var result = HitTest.TryToGetShape(shapes, new Point2(x, y), radius, PageState.ZoomX);
                        if (result != null)
                        {
                            if (bExecute)
                            {
                                Project?.ApplyRecord(result.Data, record);
                            }
                            return true;
                        }
                        else
                        {
                            if (bExecute)
                            {
                                OnDropRecordAsGroup(record, x, y);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
            return false;
        }

        /// <inheritdoc/>
        public void OnDropRecordAsGroup(IRecord record, double x, double y)
        {
            var selected = Project.CurrentStyleLibrary?.Selected != null ?
                Project.CurrentStyleLibrary.Selected :
                Factory.CreateShapeStyle(ProjectEditorConfiguration.DefaulStyleName);
            var style = (IShapeStyle)selected.Copy(null);
            var layer = Project?.CurrentContainer?.CurrentLayer;
            decimal sx = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)x, (decimal)Project.Options.SnapX) : (decimal)x;
            decimal sy = Project.Options.SnapToGrid ? PointUtil.Snap((decimal)y, (decimal)Project.Options.SnapY) : (decimal)y;

            var g = Factory.CreateGroupShape(ProjectEditorConfiguration.DefaulGroupName);

            g.Data.Record = record;

            var length = record.Values.Length;
            double px = (double)sx;
            double py = (double)sy;
            double width = 150;
            double height = 15;

            var db = record.Owner as IDatabase;

            for (int i = 0; i < length; i++)
            {
                var column = db.Columns[i];
                if (column.IsVisible)
                {
                    var binding = "{" + db.Columns[i].Name + "}";
                    var text = Factory.CreateTextShape(px, py, px + width, py + height, style, binding);
                    g.AddShape(text);
                    py += height;
                }
            }

            var rectangle = Factory.CreateRectangleShape((double)sx, (double)sy, (double)sx + width, (double)sy + (length * height), style);
            g.AddShape(rectangle);

            var pt = Factory.CreatePointShape((double)sx + (width / 2), (double)sy);
            var pb = Factory.CreatePointShape((double)sx + (width / 2), (double)sy + (length * height));
            var pl = Factory.CreatePointShape((double)sx, (double)sy + ((length * height) / 2));
            var pr = Factory.CreatePointShape((double)sx + width, (double)sy + ((length * height) / 2));

            g.AddConnectorAsNone(pt);
            g.AddConnectorAsNone(pb);
            g.AddConnectorAsNone(pl);
            g.AddConnectorAsNone(pr);

            Project.AddShape(layer, g);
        }

        /// <inheritdoc/>
        public bool OnDropStyle(IShapeStyle style, double x, double y, bool bExecute = true)
        {
            try
            {
                if (PageState?.SelectedShapes?.Count > 0)
                {
                    if (bExecute == true)
                    {
                        OnApplyStyle(style);
                    }
                    return true;
                }
                else
                {
                    var layer = Project.CurrentContainer?.CurrentLayer;
                    if (layer != null)
                    {
                        var shapes = layer.Shapes.Reverse();
                        double radius = Project.Options.HitThreshold / PageState.ZoomX;
                        var result = HitTest.TryToGetShape(shapes, new Point2(x, y), radius, PageState.ZoomX);
                        if (result != null)
                        {
                            if (bExecute == true)
                            {
                                Project.ApplyStyle(result, style);
                            }
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
            return false;
        }

        /// <inheritdoc/>
        public bool OnDropTemplate(IPageContainer template, double x, double y, bool bExecute = true)
        {
            try
            {
                var page = Project?.CurrentContainer;
                if (page != null && template != null && page != template)
                {
                    if (bExecute)
                    {
                        OnApplyTemplate(template);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log?.LogException(ex);
            }
            return false;
        }

        /// <inheritdoc/>
        public void OnDeleteSelected()
        {
            if (Project?.CurrentContainer?.CurrentLayer == null || PageState == null)
            {
                return;
            }

            if (PageState.SelectedShapes?.Count > 0)
            {
                var layer = Project.CurrentContainer.CurrentLayer;

                var builder = layer.Shapes.ToBuilder();
                foreach (var shape in PageState.SelectedShapes)
                {
                    builder.Remove(shape);
                }

                var previous = layer.Shapes;
                var next = builder.ToImmutable();
                Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                layer.Shapes = next;

                PageState.SelectedShapes = default;
                layer.InvalidateLayer();

                OnHideDecorator();
            }
        }

        /// <inheritdoc/>
        public void Deselect()
        {
            if (PageState?.SelectedShapes != null)
            {
                PageState.SelectedShapes = default;
            }

            OnHideDecorator();
        }

        /// <inheritdoc/>
        public void Select(ILayerContainer layer, IBaseShape shape)
        {
            if (PageState != null)
            {
                PageState.SelectedShapes = new HashSet<IBaseShape>() { shape };

                if (PageState.DrawPoints == true)
                {
                    OnHideDecorator();
                }
                else
                {
                    if (shape is IPointShape || shape is ILineShape)
                    {
                        OnHideDecorator();
                    }
                    else
                    {
                        OnShowDecorator();
                    }
                }
            }

            if (layer.Owner is IPageContainer owner)
            {
                owner.CurrentShape = shape;
            }

            if (layer != null)
            {
                layer.InvalidateLayer();
            }
            else
            {
                CanvasPlatform?.InvalidateControl?.Invoke();
            }
        }

        /// <inheritdoc/>
        public void Select(ILayerContainer layer, ISet<IBaseShape> shapes)
        {
            if (PageState != null)
            {
                PageState.SelectedShapes = shapes;

                OnShowDecorator();
            }

            if (layer.Owner is IPageContainer owner && owner.CurrentShape != null)
            {
                owner.CurrentShape = default;
            }

            if (layer != null)
            {
                layer.InvalidateLayer();
            }
            else
            {
                CanvasPlatform?.InvalidateControl?.Invoke();
            }
        }

        /// <inheritdoc/>
        public void Deselect(ILayerContainer layer)
        {
            Deselect();

            if (layer.Owner is IPageContainer owner && owner.CurrentShape != null)
            {
                owner.CurrentShape = default;
            }

            if (layer != null)
            {
                layer.InvalidateLayer();
            }
            else
            {
                if (CanvasPlatform?.InvalidateControl != null)
                {
                    CanvasPlatform?.InvalidateControl();
                }
            }
        }

        /// <inheritdoc/>
        public bool TryToSelectShape(ILayerContainer layer, double x, double y, bool deselect = true)
        {
            if (layer != null)
            {
                var shapes = layer.Shapes.Reverse();
                double radius = Project.Options.HitThreshold / PageState.ZoomX;

                var point = HitTest.TryToGetPoint(shapes, new Point2(x, y), radius, PageState.ZoomX);
                if (point != null)
                {
                    Select(layer, point);
                    return true;
                }

                var shape = HitTest.TryToGetShape(shapes, new Point2(x, y), radius, PageState.ZoomX);
                if (shape != null)
                {
                    Select(layer, shape);
                    return true;
                }

                if (deselect)
                {
                    Deselect(layer);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryToSelectShapes(ILayerContainer layer, IRectangleShape rectangle, bool deselect = true, bool includeSelected = false)
        {
            if (layer != null)
            {
                var rect = Rect2.FromPoints(
                    rectangle.TopLeft.X,
                    rectangle.TopLeft.Y,
                    rectangle.BottomRight.X,
                    rectangle.BottomRight.Y);
                var shapes = layer.Shapes;
                double radius = Project.Options.HitThreshold / PageState.ZoomX;
                var result = HitTest.TryToGetShapes(shapes, rect, radius, PageState.ZoomX);
                if (result != null)
                {
                    if (result.Count > 0)
                    {
                        if (includeSelected)
                        {
                            if (PageState != null)
                            {
                                if (PageState.SelectedShapes != null)
                                {
                                    foreach (var shape in PageState.SelectedShapes)
                                    {
                                        if (result.Contains(shape))
                                        {
                                            result.Remove(shape);
                                        }
                                        else
                                        {
                                            result.Add(shape);
                                        }
                                    }
                                }
                            }

                            if (result.Count > 0)
                            {
                                if (result.Count == 1)
                                {
                                    Select(layer, result.FirstOrDefault());
                                }
                                else
                                {
                                    Select(layer, result);
                                }
                                return true;
                            }
                        }
                        else
                        {
                            if (result.Count == 1)
                            {
                                Select(layer, result.FirstOrDefault());
                            }
                            else
                            {
                                Select(layer, result);
                            }
                            return true;
                        }
                    }
                }

                if (deselect)
                {
                    Deselect(layer);
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public void Hover(ILayerContainer layer, IBaseShape shape)
        {
            if (layer != null)
            {
                Select(layer, shape);
                HoveredShape = shape;
            }
        }

        /// <inheritdoc/>
        public void Dehover(ILayerContainer layer)
        {
            if (layer != null && HoveredShape != null)
            {
                HoveredShape = default;
                Deselect(layer);
            }
        }

        /// <inheritdoc/>
        public bool TryToHoverShape(double x, double y)
        {
            if (Project?.CurrentContainer?.CurrentLayer == null)
            {
                return false;
            }

            if (PageState.SelectedShapes?.Count > 1)
            {
                return false;
            }

            if (!(PageState.SelectedShapes?.Count == 1 && HoveredShape != PageState.SelectedShapes?.FirstOrDefault()))
            {
                var shapes = Project.CurrentContainer?.CurrentLayer?.Shapes.Reverse();

                double radius = Project.Options.HitThreshold / PageState.ZoomX;
                var point = HitTest.TryToGetPoint(shapes, new Point2(x, y), radius, PageState.ZoomX);
                if (point != null)
                {
                    Hover(Project.CurrentContainer?.CurrentLayer, point);
                    return true;
                }
                else
                {
                    var shape = HitTest.TryToGetShape(shapes, new Point2(x, y), radius, PageState.ZoomX);
                    if (shape != null)
                    {
                        Hover(Project.CurrentContainer?.CurrentLayer, shape);
                        return true;
                    }
                    else
                    {
                        if (PageState.SelectedShapes?.Count == 1 && HoveredShape == PageState.SelectedShapes?.FirstOrDefault())
                        {
                            Dehover(Project.CurrentContainer?.CurrentLayer);
                        }
                    }
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public IPointShape TryToGetConnectionPoint(double x, double y)
        {
            if (Project.Options.TryToConnect)
            {
                var shapes = Project.CurrentContainer.CurrentLayer.Shapes.Reverse();
                double radius = Project.Options.HitThreshold / PageState.ZoomX;
                return HitTest.TryToGetPoint(shapes, new Point2(x, y), radius, PageState.ZoomX);
            }
            return null;
        }

        private void SwapLineStart(ILineShape line, IPointShape point)
        {
            if (line?.Start != null && point != null)
            {
                var previous = line.Start;
                var next = point;
                Project?.History?.Snapshot(previous, next, (p) => line.Start = p);
                line.Start = next;
            }
        }

        private void SwapLineEnd(ILineShape line, IPointShape point)
        {
            if (line?.End != null && point != null)
            {
                var previous = line.End;
                var next = point;
                Project?.History?.Snapshot(previous, next, (p) => line.End = p);
                line.End = next;
            }
        }

        /// <inheritdoc/>
        public bool TryToSplitLine(double x, double y, IPointShape point, bool select = false)
        {
            if (Project?.CurrentContainer == null || Project?.Options == null)
            {
                return false;
            }

            var shapes = Project.CurrentContainer.CurrentLayer.Shapes.Reverse();
            double radius = Project.Options.HitThreshold / PageState.ZoomX;
            var result = HitTest.TryToGetShape(shapes, new Point2(x, y), radius, PageState.ZoomX);

            if (result is ILineShape line)
            {
                if (!Project.Options.SnapToGrid)
                {
                    var a = new Point2(line.Start.X, line.Start.Y);
                    var b = new Point2(line.End.X, line.End.Y);
                    var target = new Point2(x, y);
                    var nearest = target.NearestOnLine(a, b);
                    point.X = nearest.X;
                    point.Y = nearest.Y;
                }

                var split = Factory.CreateLineShape(
                    x, y,
                    (IShapeStyle)line.Style.Copy(null),
                    line.IsStroked);

                double ds = point.DistanceTo(line.Start);
                double de = point.DistanceTo(line.End);

                if (ds < de)
                {
                    split.Start = line.Start;
                    split.End = point;
                    SwapLineStart(line, point);
                }
                else
                {
                    split.Start = point;
                    split.End = line.End;
                    SwapLineEnd(line, point);
                }

                Project.AddShape(Project.CurrentContainer.CurrentLayer, split);

                if (select)
                {
                    Select(Project.CurrentContainer.CurrentLayer, point);
                }

                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        public bool TryToSplitLine(ILineShape line, IPointShape p0, IPointShape p1)
        {
            if (Project?.Options == null)
            {
                return false;
            }

            // Points must be aligned horizontally or vertically.
            if (p0.X != p1.X && p0.Y != p1.Y)
            {
                return false;
            }

            // Line must be horizontal or vertical.
            if (line.Start.X != line.End.X && line.Start.Y != line.End.Y)
            {
                return false;
            }

            ILineShape split;
            if (line.Start.X > line.End.X || line.Start.Y > line.End.Y)
            {
                split = Factory.CreateLineShape(
                    p0,
                    line.End,
                    (IShapeStyle)line.Style.Copy(null),
                    line.IsStroked);

                SwapLineEnd(line, p1);
            }
            else
            {
                split = Factory.CreateLineShape(
                    p1,
                    line.End,
                    (IShapeStyle)line.Style.Copy(null),
                    line.IsStroked);

                SwapLineEnd(line, p0);
            }

            Project.AddShape(Project.CurrentContainer.CurrentLayer, split);

            return true;
        }

        /// <inheritdoc/>
        public bool TryToConnectLines(IEnumerable<ILineShape> lines, ImmutableArray<IPointShape> connectors)
        {
            if (connectors.Length > 0)
            {
                var lineToPoints = new Dictionary<ILineShape, IList<IPointShape>>();

                double threshold = Project.Options.HitThreshold / PageState.ZoomX;
                double scale = PageState.ZoomX;

                // Find possible connector to line connections.
                foreach (var connector in connectors)
                {
                    ILineShape result = null;
                    foreach (var line in lines)
                    {
                        double radius = Project.Options.HitThreshold / PageState.ZoomX;
                        if (HitTest.Contains(line, new Point2(connector.X, connector.Y), threshold, scale))
                        {
                            result = line;
                            break;
                        }
                    }

                    if (result != null)
                    {
                        if (lineToPoints.ContainsKey(result))
                        {
                            lineToPoints[result].Add(connector);
                        }
                        else
                        {
                            lineToPoints.Add(result, new List<IPointShape>());
                            lineToPoints[result].Add(connector);
                        }
                    }
                }

                // Try to split lines using connectors.
                bool success = false;
                foreach (var kv in lineToPoints)
                {
                    var line = kv.Key;
                    var points = kv.Value;
                    if (points.Count == 2)
                    {
                        var p0 = points[0];
                        var p1 = points[1];
                        bool horizontal = Abs(p0.Y - p1.Y) < threshold;
                        bool vertical = Abs(p0.X - p1.X) < threshold;

                        // Points are aligned horizontally.
                        if (horizontal && !vertical)
                        {
                            if (p0.X <= p1.X)
                            {
                                success = TryToSplitLine(line, p0, p1);
                            }
                            else
                            {
                                success = TryToSplitLine(line, p1, p0);
                            }
                        }

                        // Points are aligned vertically.
                        if (!horizontal && vertical)
                        {
                            if (p0.Y >= p1.Y)
                            {
                                success = TryToSplitLine(line, p1, p0);
                            }
                            else
                            {
                                success = TryToSplitLine(line, p0, p1);
                            }
                        }
                    }
                }

                return success;
            }

            return false;
        }

        private IGroupShape Group(ILayerContainer layer, ISet<IBaseShape> shapes, string name)
        {
            if (layer != null && shapes != null)
            {
                var source = layer.Shapes.ToBuilder();
                var group = Factory.CreateGroupShape(name);
                group.Group(shapes, source);

                var previous = layer.Shapes;
                var next = source.ToImmutable();
                Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                layer.Shapes = next;

                return group;
            }

            return null;
        }

        private void Ungroup(ILayerContainer layer, ISet<IBaseShape> shapes)
        {
            if (layer != null && shapes != null)
            {
                var source = layer.Shapes.ToBuilder();

                foreach (var shape in shapes)
                {
                    if (shape is IGroupShape group)
                    {
                        group.Ungroup(source);
                    }
                }

                var previous = layer.Shapes;
                var next = source.ToImmutable();
                Project?.History?.Snapshot(previous, next, (p) => layer.Shapes = p);
                layer.Shapes = next;
            }
        }

        /// <inheritdoc/>
        public IGroupShape Group(ISet<IBaseShape> shapes, string name)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null)
            {
                return Group(layer, shapes, name);
            }

            return null;
        }

        /// <inheritdoc/>
        public bool Ungroup(ISet<IBaseShape> shapes)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null && shapes != null)
            {
                Ungroup(layer, shapes);
                return true;
            }

            return false;
        }

        /// <inheritdoc/>
        private void Swap(IBaseShape shape, int sourceIndex, int targetIndex)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer?.Shapes != null)
            {
                if (sourceIndex < targetIndex)
                {
                    Project.SwapShape(layer, shape, targetIndex + 1, sourceIndex);
                }
                else
                {
                    if (layer.Shapes.Length + 1 > sourceIndex + 1)
                    {
                        Project.SwapShape(layer, shape, targetIndex, sourceIndex + 1);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void BringToFront(IBaseShape source)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null)
            {
                var items = layer.Shapes;
                int sourceIndex = items.IndexOf(source);
                int targetIndex = items.Length - 1;
                if (targetIndex >= 0 && sourceIndex != targetIndex)
                {
                    Swap(source, sourceIndex, targetIndex);
                }
            }
        }

        /// <inheritdoc/>
        public void BringForward(IBaseShape source)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null)
            {
                var items = layer.Shapes;
                int sourceIndex = items.IndexOf(source);
                int targetIndex = sourceIndex + 1;
                if (targetIndex < items.Length)
                {
                    Swap(source, sourceIndex, targetIndex);
                }
            }
        }

        /// <inheritdoc/>
        public void SendBackward(IBaseShape source)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null)
            {
                var items = layer.Shapes;
                int sourceIndex = items.IndexOf(source);
                int targetIndex = sourceIndex - 1;
                if (targetIndex >= 0)
                {
                    Swap(source, sourceIndex, targetIndex);
                }
            }
        }

        /// <inheritdoc/>
        public void SendToBack(IBaseShape source)
        {
            var layer = Project?.CurrentContainer?.CurrentLayer;
            if (layer != null)
            {
                var items = layer.Shapes;
                int sourceIndex = items.IndexOf(source);
                int targetIndex = 0;
                if (sourceIndex != targetIndex)
                {
                    Swap(source, sourceIndex, targetIndex);
                }
            }
        }

        /// <inheritdoc/>
        public void MoveShapesBy(IEnumerable<IBaseShape> shapes, decimal dx, decimal dy)
        {
            foreach (var shape in shapes)
            {
                if (!shape.State.Flags.HasFlag(ShapeStateFlags.Locked))
                {
                    shape.Move(null, dx, dy);
                }
            }
            OnUpdateDecorator();
        }

        private void MoveShapesByWithHistory(IEnumerable<IBaseShape> shapes, decimal dx, decimal dy)
        {
            MoveShapesBy(shapes, dx, dy);
            OnUpdateDecorator();

            var previous = new { DeltaX = -dx, DeltaY = -dy, Shapes = shapes };
            var next = new { DeltaX = dx, DeltaY = dy, Shapes = shapes };
            Project?.History?.Snapshot(previous, next, (s) => MoveShapesBy(s.Shapes, s.DeltaX, s.DeltaY));
        }

        /// <inheritdoc/>
        public void MoveBy(ISet<IBaseShape> shapes, decimal dx, decimal dy)
        {
            if (shapes != null)
            {
                switch (Project?.Options?.MoveMode)
                {
                    case MoveMode.Point:
                        {
                            var points = new List<IPointShape>();

                            foreach (var shape in shapes)
                            {
                                if (!shape.State.Flags.HasFlag(ShapeStateFlags.Locked))
                                {
                                    shape.GetPoints(points);
                                }
                            }

                            var distinct = points.Distinct().ToList();
                            MoveShapesByWithHistory(distinct, dx, dy);
                        }
                        break;
                    case MoveMode.Shape:
                        {
                            var items = shapes.Where(s => !s.State.Flags.HasFlag(ShapeStateFlags.Locked));
                            MoveShapesByWithHistory(items, dx, dy);
                        }
                        break;
                }
            }
        }

        /// <inheritdoc/>
        public void MoveItem<T>(ILibrary<T> library, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < targetIndex)
            {
                var item = library.Items[sourceIndex];
                var builder = library.Items.ToBuilder();
                builder.Insert(targetIndex + 1, item);
                builder.RemoveAt(sourceIndex);

                var previous = library.Items;
                var next = builder.ToImmutable();
                Project?.History?.Snapshot(previous, next, (p) => library.Items = p);
                library.Items = next;
            }
            else
            {
                int removeIndex = sourceIndex + 1;
                if (library.Items.Length + 1 > removeIndex)
                {
                    var item = library.Items[sourceIndex];
                    var builder = library.Items.ToBuilder();
                    builder.Insert(targetIndex, item);
                    builder.RemoveAt(removeIndex);

                    var previous = library.Items;
                    var next = builder.ToImmutable();
                    Project?.History?.Snapshot(previous, next, (p) => library.Items = p);
                    library.Items = next;
                }
            }
        }

        /// <inheritdoc/>
        public void SwapItem<T>(ILibrary<T> library, int sourceIndex, int targetIndex)
        {
            var item1 = library.Items[sourceIndex];
            var item2 = library.Items[targetIndex];
            var builder = library.Items.ToBuilder();
            builder[targetIndex] = item1;
            builder[sourceIndex] = item2;

            var previous = library.Items;
            var next = builder.ToImmutable();
            Project?.History?.Snapshot(previous, next, (p) => library.Items = p);
            library.Items = next;
        }

        /// <inheritdoc/>
        public void InsertItem<T>(ILibrary<T> library, T item, int index)
        {
            var builder = library.Items.ToBuilder();
            builder.Insert(index, item);

            var previous = library.Items;
            var next = builder.ToImmutable();
            Project?.History?.Snapshot(previous, next, (p) => library.Items = p);
            library.Items = next;
        }
    }
}
