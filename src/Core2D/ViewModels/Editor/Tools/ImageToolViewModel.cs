﻿using System;
using System.Collections.Generic;
using Core2D;
using Core2D.Editor.Tools.Selection;
using Core2D.Input;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Editor.Tools
{
    public class ImageToolViewModel : ViewModelBase, IEditorTool
    {
        public enum State { TopLeft, BottomRight }
        private readonly IServiceProvider _serviceProvider;
        private State _currentState = State.TopLeft;
        private ImageShapeViewModel _image;
        private ImageSelection _selection;

        public string Title => "Image";

        public ImageToolViewModel(IServiceProvider serviceProvider) : base()
        {
            _serviceProvider = serviceProvider;
        }

        public override object Copy(IDictionary<object, object> shared)
        {
            throw new NotImplementedException();
        }

        public async void BeginDown(InputArgs args)
        {
            var factory = _serviceProvider.GetService<IFactory>();
            var editor = _serviceProvider.GetService<ProjectEditorViewModel>();
            (decimal sx, decimal sy) = editor.TryToSnap(args);
            switch (_currentState)
            {
                case State.TopLeft:
                    {
                        editor.IsToolIdle = false;
 
                        if (editor.ImageImporter == null)
                        {
                            editor.IsToolIdle = true;
                            return;
                        }

                        var key = await editor.ImageImporter.GetImageKeyAsync();
                        if (key == null || string.IsNullOrEmpty(key))
                        {
                            editor.IsToolIdle = true;
                            return;
                        }

                        var style = editor.Project.CurrentStyleLibrary?.Selected != null ?
                            editor.Project.CurrentStyleLibrary.Selected :
                            editor.Factory.CreateShapeStyle(ProjectEditorConfigurationViewModel.DefaulStyleName);
                        _image = factory.CreateImageShape(
                            (double)sx, (double)sy,
                            (ShapeStyleViewModel)style.Copy(null),
                            key);

                        var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                        if (result != null)
                        {
                            _image.TopLeft = result;
                        }

                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Add(_image);
                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                        ToStateBottomRight();
                        Move(_image);
                        _currentState = State.BottomRight;
                    }
                    break;
                case State.BottomRight:
                    {
                        if (_image != null)
                        {
                            _image.BottomRight.X = (double)sx;
                            _image.BottomRight.Y = (double)sy;

                            var result = editor.TryToGetConnectionPoint((double)sx, (double)sy);
                            if (result != null)
                            {
                                _image.BottomRight = result;
                            }

                            editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_image);
                            Finalize(_image);
                            editor.Project.AddShape(editor.Project.CurrentContainer.CurrentLayer, _image);

                            Reset();
                        }
                    }
                    break;
            }
        }

        public void BeginUp(InputArgs args)
        {
        }

        public void EndDown(InputArgs args)
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

        public void EndUp(InputArgs args)
        {
        }

        public void Move(InputArgs args)
        {
            var editor = _serviceProvider.GetService<ProjectEditorViewModel>();
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
                        if (_image != null)
                        {
                            if (editor.Project.Options.TryToConnect)
                            {
                                editor.TryToHoverShape((double)sx, (double)sy);
                            }
                            _image.BottomRight.X = (double)sx;
                            _image.BottomRight.Y = (double)sy;
                            editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                            Move(_image);
                        }
                    }
                    break;
            }
        }

        public void ToStateBottomRight()
        {
            var editor = _serviceProvider.GetService<ProjectEditorViewModel>();
            _selection = new ImageSelection(
                _serviceProvider,
                editor.Project.CurrentContainer.HelperLayer,
                _image,
                editor.PageState.HelperStyle);

            _selection.ToStateBottomRight();
        }

        public void Move(BaseShapeViewModel shape)
        {
            _selection.Move();
        }

        public void Finalize(BaseShapeViewModel shape)
        {
        }

        public void Reset()
        {
            var editor = _serviceProvider.GetService<ProjectEditorViewModel>();

            switch (_currentState)
            {
                case State.TopLeft:
                    break;
                case State.BottomRight:
                    {
                        editor.Project.CurrentContainer.WorkingLayer.Shapes = editor.Project.CurrentContainer.WorkingLayer.Shapes.Remove(_image);
                        editor.Project.CurrentContainer.WorkingLayer.InvalidateLayer();
                    }
                    break;
            }

            _currentState = State.TopLeft;

            if (_selection != null)
            {
                _selection.Reset();
                _selection = null;
            }
            
            editor.IsToolIdle = true;
        }
    }
}