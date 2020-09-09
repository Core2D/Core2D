﻿using System;
using System.Collections.Generic;
using Core2D;
using Core2D.Editor.Tools.Selection;
using Core2D.Editor.Tools.Settings;
using Core2D.Input;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Editor.Tools
{
    /// <summary>
    /// Text tool.
    /// </summary>
    public class ToolText : ObservableObject, IEditorTool
    {
        public enum State { TopLeft, BottomRight }
        private readonly IServiceProvider _serviceProvider;
        private ToolSettingsText _settings;
        private State _currentState = State.TopLeft;
        private ITextShape _text;
        private ToolTextSelection _selection;

        /// <inheritdoc/>
        public string Title => "Text";

        /// <summary>
        /// Gets or sets the tool settings.
        /// </summary>
        public ToolSettingsText Settings
        {
            get => _settings;
            set => Update(ref _settings, value);
        }

        /// <summary>
        /// Initialize new instance of <see cref="ToolText"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ToolText(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            _settings = new ToolSettingsText();
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void LeftDown(InputArgs args)
        {
            var factory = _serviceProvider.GetService<IFactory>();
            var editor = _serviceProvider.GetService<IProjectEditor>();
            (decimal sx, decimal sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.TopLeft:
                    {
                        var style = editor.Project.CurrentStyleLibrary?.Selected != null ?
                            editor.Project.CurrentStyleLibrary.Selected :
                            editor.Factory.CreateShapeStyle(ProjectEditorConfiguration.DefaulStyleName);
                        _text = factory.CreateTextShape(
                            (double)sx, (double)sy,
                            (IShapeStyle)style.Copy(null),
                            "Text",
                            editor.Project.Options.DefaultIsStroked);

                        var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                        if (result != null)
                        {
                            _text.TopLeft = result;
                        }

                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_text);
                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                        ToStateBottomRight();
                        Move(_text);
                        _currentState = State.BottomRight;
                        editor.IsToolIdle = false;
                    }
                    break;
                case State.BottomRight:
                    {
                        if (_text != null)
                        {
                            _text.BottomRight.X = (double)sx;
                            _text.BottomRight.Y = (double)sy;

                            var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                            if (result != null)
                            {
                                _text.BottomRight = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_text);
                            Finalize(_text);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, _text);

                            Reset();
                        }
                    }
                    break;
            }
        }

        /// <inheritdoc/>
        public void LeftUp(InputArgs args)
        {
        }

        /// <inheritdoc/>
        public void RightDown(InputArgs args)
        {
            switch (_currentState)
            {
                case State.TopLeft:
                    break;
                case State.BottomRight:
                    Reset();
                    break;
            }
        }

        /// <inheritdoc/>
        public void RightUp(InputArgs args)
        {
        }

        /// <inheritdoc/>
        public void Move(InputArgs args)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            (decimal sx, decimal sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.TopLeft:
                    if (editor.Project.Options.TryToConnect)
                    {
                        editor.TryToHoverShape((double)sx, (double)sy);
                    }
                    break;
                case State.BottomRight:
                    {
                        if (_text != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape((double)sx, (double)sy);
                            }
                            _text.BottomRight.X = (double)sx;
                            _text.BottomRight.Y = (double)sy;
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            Move(_text);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.BottomRight"/>.
        /// </summary>
        public void ToStateBottomRight()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            _selection = new ToolTextSelection(
                _serviceProvider,
                editor.Project.CurrentContainer.HelperLayer,
                _text,
                editor.PageState.HelperStyle);

            _selection.ToStateBottomRight();
        }

        /// <inheritdoc/>
        public void Move(IBaseShape shape)
        {
            _selection.Move();
        }

        /// <inheritdoc/>
        public void Finalize(IBaseShape shape)
        {
        }

        /// <inheritdoc/>
        public void Reset()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            switch (_currentState)
            {
                case State.TopLeft:
                    break;
                case State.BottomRight:
                    {
                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_text);
                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                    }
                    break;
            }

            _currentState = State.TopLeft;
            editor.IsToolIdle = true;

            if (_selection != null)
            {
                _selection.Reset();
                _selection = null;
            }
        }
    }
}
