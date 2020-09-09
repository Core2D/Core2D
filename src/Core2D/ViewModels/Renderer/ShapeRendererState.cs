﻿using System;
using System.Collections.Generic;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Renderer
{
    /// <summary>
    /// Shape renderer state.
    /// </summary>
    public class ShapeRendererState : ObservableObject, IShapeRendererState
    {
        private double _panX;
        private double _panY;
        private double _zoomX;
        private double _zoomY;
        private IShapeState _drawShapeState;
        private ISet<IBaseShape> _selectedShapes;
        private IImageCache _imageCache;
        private bool _drawDecorators;
        private bool _drawPoints;
        private IShapeStyle _pointStyle;
        private IShapeStyle _selectedPointStyle;
        private double _pointSize;
        private IShapeStyle _selectionStyle;
        private IShapeStyle _helperStyle;
        private IDecorator _decorator;

        /// <inheritdoc/>
        public double PanX
        {
            get => _panX;
            set => Update(ref _panX, value);
        }

        /// <inheritdoc/>
        public double PanY
        {
            get => _panY;
            set => Update(ref _panY, value);
        }

        /// <inheritdoc/>
        public double ZoomX
        {
            get => _zoomX;
            set => Update(ref _zoomX, value);
        }

        /// <inheritdoc/>
        public double ZoomY
        {
            get => _zoomY;
            set => Update(ref _zoomY, value);
        }

        /// <inheritdoc/>
        public IShapeState DrawShapeState
        {
            get => _drawShapeState;
            set => Update(ref _drawShapeState, value);
        }

        /// <inheritdoc/>
        public ISet<IBaseShape> SelectedShapes
        {
            get => _selectedShapes;
            set => Update(ref _selectedShapes, value);
        }

        /// <inheritdoc/>
        public IImageCache ImageCache
        {
            get => _imageCache;
            set => Update(ref _imageCache, value);
        }

        /// <inheritdoc/>
        public bool DrawDecorators
        {
            get => _drawDecorators;
            set => Update(ref _drawDecorators, value);
        }

        /// <inheritdoc/>
        public bool DrawPoints
        {
            get => _drawPoints;
            set => Update(ref _drawPoints, value);
        }

        /// <inheritdoc/>
        public IShapeStyle PointStyle
        {
            get => _pointStyle;
            set => Update(ref _pointStyle, value);
        }

        /// <inheritdoc/>
        public IShapeStyle SelectedPointStyle
        {
            get => _selectedPointStyle;
            set => Update(ref _selectedPointStyle, value);
        }

        /// <inheritdoc/>
        public double PointSize
        {
            get => _pointSize;
            set => Update(ref _pointSize, value);
        }

        /// <inheritdoc/>
        public IShapeStyle SelectionStyle
        {
            get => _selectionStyle;
            set => Update(ref _selectionStyle, value);
        }

        /// <inheritdoc/>
        public IShapeStyle HelperStyle
        {
            get => _helperStyle;
            set => Update(ref _helperStyle, value);
        }

        /// <inheritdoc/>
        public IDecorator Decorator
        {
            get => _decorator;
            set => Update(ref _decorator, value);
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public override bool IsDirty()
        {
            var isDirty = base.IsDirty();

            isDirty |= DrawShapeState.IsDirty();

            return isDirty;
        }

        /// <inheritdoc/>
        public override void Invalidate()
        {
            base.Invalidate();

            DrawShapeState.Invalidate();
        }

        /// <summary>
        /// Check whether the <see cref="PanX"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePanX() => _panX != default;

        /// <summary>
        /// Check whether the <see cref="PanY"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePanY() => _panY != default;

        /// <summary>
        /// Check whether the <see cref="ZoomX"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeZoomX() => _zoomX != default;

        /// <summary>
        /// Check whether the <see cref="ZoomY"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeZoomY() => _zoomY != default;

        /// <summary>
        /// Check whether the <see cref="DrawShapeState"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDrawShapeState() => _drawShapeState != null;

        /// <summary>
        /// Check whether the <see cref="SelectedShapes"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSelectedShapes() => true;

        /// <summary>
        /// Check whether the <see cref="ImageCache"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeImageCache() => _imageCache != null;

        /// <summary>
        /// Check whether the <see cref="DrawDecorators"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDrawDecorators() => _drawDecorators != default;

        /// <summary>
        /// Check whether the <see cref="DrawPoints"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDrawPoints() => _drawPoints != default;

        /// <summary>
        /// Check whether the <see cref="PointStyle"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePointStyle() => _pointStyle != null;

        /// <summary>
        /// Check whether the <see cref="SelectedPointStyle"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSelectedPointStyle() => _selectedPointStyle != null;

        /// <summary>
        /// Check whether the <see cref="PointSize"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePointSize() => _pointSize != default;

        /// <summary>
        /// Check whether the <see cref="SelectionStyle"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSelectionStyle() => _selectionStyle != null;

        /// <summary>
        /// Check whether the <see cref="HelperStyle"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHelperStyle() => _helperStyle != null;

        /// <summary>
        /// Check whether the <see cref="Decorator"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDecorator() => _decorator != null;
    }
}
