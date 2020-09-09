﻿using System;
using System.Collections.Generic;
using Core2D;
using Core2D.Editor.Tools.Selection;
using Core2D.Editor.Tools.Settings;
using Core2D.Input;
using Core2D.Shapes;
using Core2D.Style;
using Spatial;
using Spatial.Arc;

namespace Core2D.Editor.Tools
{
    /// <summary>
    /// Arc tool.
    /// </summary>
    public class ToolArc : ObservableObject, IEditorTool
    {
        public enum State { Point1, Point2, Point3, Point4 }
        private readonly IServiceProvider _serviceProvider;
        private ToolSettingsArc _settings;
        private State _currentState = State.Point1;
        private IArcShape _arc;
        private bool _connectedPoint3;
        private bool _connectedPoint4;
        private ToolArcSelection _selection;

        /// <inheritdoc/>
        public string Title => "Arc";

        /// <summary>
        /// Gets or sets the tool settings.
        /// </summary>
        public ToolSettingsArc Settings
        {
            get => _settings;
            set => Update(ref _settings, value);
        }

        /// <summary>
        /// Initialize new instance of <see cref="ToolArc"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public ToolArc(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
            _settings = new ToolSettingsArc();
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
                case State.Point1:
                    {
                        var style = editor.Project.CurrentStyleLibrary?.Selected != null ?
                            editor.Project.CurrentStyleLibrary.Selected :
                            editor.Factory.CreateShapeStyle(ProjectEditorConfiguration.DefaulStyleName);
                        _connectedPoint3 = false;
                        _connectedPoint4 = false;
                        _arc = factory.CreateArcShape(
                            (double)sx, (double)sy,
                            (IShapeStyle)style.Copy(null),
                            editor.Project.Options.DefaultIsStroked,
                            editor.Project.Options.DefaultIsFilled);

                        var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                        if (result != null)
                        {
                            _arc.Point1 = result;
                        }

                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                        ToStatePoint2();
                        Move(_arc);
                        _currentState = State.Point2;
                        editor.IsToolIdle = false;
                    }
                    break;
                case State.Point2:
                    {
                        if (_arc != null)
                        {
                            _arc.Point2.X = (double)sx;
                            _arc.Point2.Y = (double)sy;
                            _arc.Point3.X = (double)sx;
                            _arc.Point3.Y = (double)sy;

                            var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                            if (result != null)
                            {
                                _arc.Point2 = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            ToStatePoint3();
                            Move(_arc);
                            _currentState = State.Point3;
                        }
                    }
                    break;
                case State.Point3:
                    {
                        if (_arc != null)
                        {
                            _arc.Point3.X = (double)sx;
                            _arc.Point3.Y = (double)sy;
                            _arc.Point4.X = (double)sx;
                            _arc.Point4.Y = (double)sy;

                            var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                            if (result != null)
                            {
                                _arc.Point3 = result;
                                _connectedPoint3 = true;
                            }
                            else
                            {
                                _connectedPoint3 = false;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_arc);
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            ToStatePoint4();
                            Move(_arc);
                            _currentState = State.Point4;
                        }
                    }
                    break;
                case State.Point4:
                    {
                        if (_arc != null)
                        {
                            _arc.Point4.X = (double)sx;
                            _arc.Point4.Y = (double)sy;

                            var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                            if (result != null)
                            {
                                _arc.Point4 = result;
                                _connectedPoint4 = true;
                            }
                            else
                            {
                                _connectedPoint4 = false;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_arc);
                            Finalize(_arc);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, _arc);

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
                case State.Point1:
                    break;
                case State.Point2:
                case State.Point3:
                case State.Point4:
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
                case State.Point1:
                    {
                        if (editor.Project.Options.TryToConnect)
                        {
                            editor.TryToHoverShape((double)sx, (double)sy);
                        }
                    }
                    break;
                case State.Point2:
                    {
                        if (_arc != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape((double)sx, (double)sy);
                            }
                            _arc.Point2.X = (double)sx;
                            _arc.Point2.Y = (double)sy;
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            Move(_arc);
                        }
                    }
                    break;
                case State.Point3:
                    {
                        if (_arc != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape((double)sx, (double)sy);
                            }
                            _arc.Point3.X = (double)sx;
                            _arc.Point3.Y = (double)sy;
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            Move(_arc);
                        }
                    }
                    break;
                case State.Point4:
                    {
                        if (_arc != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape((double)sx, (double)sy);
                            }
                            _arc.Point4.X = (double)sx;
                            _arc.Point4.Y = (double)sy;
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            Move(_arc);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.Point2"/>.
        /// </summary>
        public void ToStatePoint2()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            _selection = new ToolArcSelection(
                _serviceProvider,
                editor.Project.CurrentContainer.HelperLayer,
                _arc,
                editor.PageState.HelperStyle);

            _selection.ToStatePoint2();
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.Point3"/>.
        /// </summary>
        public void ToStatePoint3()
        {
            _selection.ToStatePoint3();
        }

        /// <summary>
        /// Transfer tool state to <see cref="State.Point4"/>.
        /// </summary>
        public void ToStatePoint4()
        {
            _selection.ToStatePoint4();
        }

        /// <inheritdoc/>
        public void Move(IBaseShape shape)
        {
            _selection.Move();
        }

        /// <inheritdoc/>
        public void Finalize(IBaseShape shape)
        {
            var arc = shape as IArcShape;
            var a = new WpfArc(
                Point2.FromXY(arc.Point1.X, arc.Point1.Y),
                Point2.FromXY(arc.Point2.X, arc.Point2.Y),
                Point2.FromXY(arc.Point3.X, arc.Point3.Y),
                Point2.FromXY(arc.Point4.X, arc.Point4.Y));

            if (!_connectedPoint3)
            {
                arc.Point3.X = a.Start.X;
                arc.Point3.Y = a.Start.Y;
            }

            if (!_connectedPoint4)
            {
                arc.Point4.X = a.End.X;
                arc.Point4.Y = a.End.Y;
            }
        }

        /// <inheritdoc/>
        public void Reset()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            switch (_currentState)
            {
                case State.Point1:
                    break;
                case State.Point2:
                case State.Point3:
                case State.Point4:
                    {
                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_arc);
                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                    }
                    break;
            }

            _currentState = State.Point1;
            editor.IsToolIdle = true;

            if (_selection != null)
            {
                _selection.Reset();
                _selection = null;
            }
        }
    }
}
