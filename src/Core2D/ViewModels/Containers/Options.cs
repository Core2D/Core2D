﻿using System;
using System.Collections.Generic;
using Core2D.Path;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Containers
{
    /// <summary>
    /// Project options.
    /// </summary>
    public class Options : ObservableObject, IOptions
    {
        private bool _snapToGrid = true;
        private double _snapX = 15.0;
        private double _snapY = 15.0;
        private double _hitThreshold = 7.0;
        private MoveMode _moveMode = MoveMode.Point;
        private bool _defaultIsStroked = true;
        private bool _defaultIsFilled = false;
        private bool _defaultIsClosed = true;
        private FillRule _defaultFillRule = FillRule.EvenOdd;
        private bool _tryToConnect = false;

        /// <summary>
        /// Gets or sets how grid snapping is handled. 
        /// </summary>
        public bool SnapToGrid
        {
            get => _snapToGrid;
            set => Update(ref _snapToGrid, value);
        }

        /// <summary>
        /// Gets or sets how much grid X axis is snapped.
        /// </summary>
        public double SnapX
        {
            get => _snapX;
            set => Update(ref _snapX, value);
        }

        /// <summary>
        /// Gets or sets how much grid Y axis is snapped.
        /// </summary>
        public double SnapY
        {
            get => _snapY;
            set => Update(ref _snapY, value);
        }

        /// <summary>
        /// Gets or sets hit test threshold radius.
        /// </summary>
        public double HitThreshold
        {
            get => _hitThreshold;
            set => Update(ref _hitThreshold, value);
        }

        /// <summary>
        /// Gets or sets how selected shapes are moved.
        /// </summary>
        public MoveMode MoveMode
        {
            get => _moveMode;
            set => Update(ref _moveMode, value);
        }

        /// <summary>
        /// Gets or sets value indicating whether path/shape is stroked during creation.
        /// </summary>
        public bool DefaultIsStroked
        {
            get => _defaultIsStroked;
            set => Update(ref _defaultIsStroked, value);
        }

        /// <summary>
        /// Gets or sets value indicating whether path/shape is filled during creation.
        /// </summary>
        public bool DefaultIsFilled
        {
            get => _defaultIsFilled;
            set => Update(ref _defaultIsFilled, value);
        }

        /// <summary>
        /// Gets or sets value indicating whether path is closed during creation.
        /// </summary>
        public bool DefaultIsClosed
        {
            get => _defaultIsClosed;
            set => Update(ref _defaultIsClosed, value);
        }

        /// <summary>
        /// Gets or sets value indicating path fill rule during creation.
        /// </summary>
        public FillRule DefaultFillRule
        {
            get => _defaultFillRule;
            set => Update(ref _defaultFillRule, value);
        }

        /// <summary>
        /// Gets or sets how point connection is handled.
        /// </summary>
        public bool TryToConnect
        {
            get => _tryToConnect;
            set => Update(ref _tryToConnect, value);
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
            return isDirty;
        }

        /// <inheritdoc/>
        public override void Invalidate()
        {
            base.Invalidate();
        }

        /// <summary>
        /// Check whether the <see cref="SnapToGrid"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSnapToGrid() => _snapToGrid != default;

        /// <summary>
        /// Check whether the <see cref="SnapX"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSnapX() => _snapX != default;

        /// <summary>
        /// Check whether the <see cref="SnapY"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeSnapY() => _snapY != default;

        /// <summary>
        /// Check whether the <see cref="HitThreshold"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeHitThreshold() => _hitThreshold != default;

        /// <summary>
        /// Check whether the <see cref="MoveMode"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeMoveMode() => _moveMode != default;

        /// <summary>
        /// Check whether the <see cref="DefaultIsStroked"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultIsStroked() => _defaultIsStroked != default;

        /// <summary>
        /// Check whether the <see cref="DefaultIsFilled"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultIsFilled() => _defaultIsFilled != default;

        /// <summary>
        /// Check whether the <see cref="DefaultIsClosed"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultIsClosed() => _defaultIsClosed != default;

        /// <summary>
        /// Check whether the <see cref="DefaultFillRule"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeDefaultFillRule() => _defaultFillRule != default;

        /// <summary>
        /// Check whether the <see cref="TryToConnect"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializeTryToConnect() => _tryToConnect != default;
    }
}
