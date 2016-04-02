﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Path;
using Core2D.Shape;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Project
{
    /// <summary>
    /// Project options.
    /// </summary>
    public class XOptions : ObservableObject
    {
        private bool _snapToGrid = true;
        private double _snapX = 15.0;
        private double _snapY = 15.0;
        private double _hitThreshold = 7.0;
        private XMoveMode _moveMode = XMoveMode.Point;
        private bool _defaultIsStroked = true;
        private bool _defaultIsFilled = false;
        private bool _defaultIsClosed = true;
        private bool _defaultIsSmoothJoin = true;
        private XFillRule _defaultFillRule = XFillRule.EvenOdd;
        private bool _tryToConnect = false;
        private BaseShape _pointShape;
        private ShapeStyle _pointStyle;
        private ShapeStyle _selectionStyle;
        private ShapeStyle _helperStyle;

        /// <summary>
        /// Gets or sets how grid snapping is handled. 
        /// </summary>
        public bool SnapToGrid
        {
            get { return _snapToGrid; }
            set { Update(ref _snapToGrid, value); }
        }

        /// <summary>
        /// Gets or sets how much grid X axis is snapped.
        /// </summary>
        public double SnapX
        {
            get { return _snapX; }
            set { Update(ref _snapX, value); }
        }

        /// <summary>
        /// Gets or sets how much grid Y axis is snapped.
        /// </summary>
        public double SnapY
        {
            get { return _snapY; }
            set { Update(ref _snapY, value); }
        }

        /// <summary>
        /// Gets or sets hit test threshold radius.
        /// </summary>
        public double HitThreshold
        {
            get { return _hitThreshold; }
            set { Update(ref _hitThreshold, value); }
        }

        /// <summary>
        /// Gets or sets how selected shapes are moved.
        /// </summary>
        public XMoveMode MoveMode
        {
            get { return _moveMode; }
            set { Update(ref _moveMode, value); }
        }

        /// <summary>
        /// Gets or sets value indicating whether path/shape is stroked during creation.
        /// </summary>
        public bool DefaultIsStroked
        {
            get { return _defaultIsStroked; }
            set { Update(ref _defaultIsStroked, value); }
        }

        /// <summary>
        /// Gets or sets value indicating whether path/shape is filled during creation.
        /// </summary>
        public bool DefaultIsFilled
        {
            get { return _defaultIsFilled; }
            set { Update(ref _defaultIsFilled, value); }
        }

        /// <summary>
        /// Gets or sets value indicating whether path is closed during creation.
        /// </summary>
        public bool DefaultIsClosed
        {
            get { return _defaultIsClosed; }
            set { Update(ref _defaultIsClosed, value); }
        }

        /// <summary>
        /// Gets or sets value indicating whether path segment is smooth join during creation.
        /// </summary>
        public bool DefaultIsSmoothJoin
        {
            get { return _defaultIsSmoothJoin; }
            set { Update(ref _defaultIsSmoothJoin, value); }
        }

        /// <summary>
        /// Gets or sets value indicating path fill rule during creation.
        /// </summary>
        public XFillRule DefaultFillRule
        {
            get { return _defaultFillRule; }
            set { Update(ref _defaultFillRule, value); }
        }

        /// <summary>
        /// Gets or sets how point connection is handled.
        /// </summary>
        public bool TryToConnect
        {
            get { return _tryToConnect; }
            set { Update(ref _tryToConnect, value); }
        }

        /// <summary>
        /// Gets or sets shape used to draw points.
        /// </summary>
        public BaseShape PointShape
        {
            get { return _pointShape; }
            set { Update(ref _pointShape, value); }
        }

        /// <summary>
        /// Gets or sets point shape style.
        /// </summary>
        public ShapeStyle PointStyle
        {
            get { return _pointStyle; }
            set { Update(ref _pointStyle, value); }
        }

        /// <summary>
        /// Gets or sets selection rectangle style.
        /// </summary>
        public ShapeStyle SelectionStyle
        {
            get { return _selectionStyle; }
            set { Update(ref _selectionStyle, value); }
        }

        /// <summary>
        /// Gets or sets editor helper shapes style.
        /// </summary>
        public ShapeStyle HelperStyle
        {
            get { return _helperStyle; }
            set { Update(ref _helperStyle, value); }
        }

        /// <summary>
        /// Creates a new <see cref="XOptions"/> instance.
        /// </summary>
        /// <returns>The new instance of the <see cref="XOptions"/> class.</returns>
        public static XOptions Create()
        {
            var options = new XOptions()
            {
                SnapToGrid = true,
                SnapX = 15.0,
                SnapY = 15.0,
                HitThreshold = 7.0,
                MoveMode = XMoveMode.Point,
                DefaultIsStroked = true,
                DefaultIsFilled = false,
                DefaultIsClosed = true,
                DefaultIsSmoothJoin = true,
                DefaultFillRule = XFillRule.EvenOdd,
                TryToConnect = false
            };

            options.SelectionStyle =
                ShapeStyle.Create(
                    "Selection",
                    0x7F, 0x33, 0x33, 0xFF,
                    0x4F, 0x33, 0x33, 0xFF,
                    1.0);

            options.HelperStyle =
                ShapeStyle.Create(
                    "Helper",
                    0xFF, 0x00, 0x00, 0x00,
                    0xFF, 0x00, 0x00, 0x00,
                    1.0);

            options.PointStyle =
                ShapeStyle.Create(
                    "Point",
                    0xFF, 0x00, 0x00, 0x00,
                    0xFF, 0x00, 0x00, 0x00,
                    1.0);

            options.PointShape = RectanglePointShape(options.PointStyle);

            return options;
        }

        /// <summary>
        /// Creates a new <see cref="BaseShape"/> instance.
        /// </summary>
        /// <param name="pss">The point shape <see cref="ShapeStyle"/>.</param>
        /// <returns>The new instance of the <see cref="BaseShape"/> class.</returns>
        public static BaseShape EllipsePointShape(ShapeStyle pss)
        {
            return XEllipse.Create(-4, -4, 4, 4, pss, null, true, false);
        }

        /// <summary>
        /// Creates a new <see cref="BaseShape"/> instance.
        /// </summary>
        /// <param name="pss">The point shape <see cref="ShapeStyle"/>.</param>
        /// <returns>The new instance of the <see cref="BaseShape"/> class.</returns>
        public static BaseShape FilledEllipsePointShape(ShapeStyle pss)
        {
            return XEllipse.Create(-3, -3, 3, 3, pss, null, true, true);
        }

        /// <summary>
        /// Creates a new <see cref="BaseShape"/> instance.
        /// </summary>
        /// <param name="pss">The point shape <see cref="ShapeStyle"/>.</param>
        /// <returns>The new instance of the <see cref="BaseShape"/> class.</returns>
        public static BaseShape RectanglePointShape(ShapeStyle pss)
        {
            return XRectangle.Create(-4, -4, 4, 4, pss, null, true, false);
        }

        /// <summary>
        /// Creates a new <see cref="BaseShape"/> instance.
        /// </summary>
        /// <param name="pss">The point shape <see cref="ShapeStyle"/>.</param>
        /// <returns>The new instance of the <see cref="BaseShape"/> class.</returns>
        public static BaseShape FilledRectanglePointShape(ShapeStyle pss)
        {
            return XRectangle.Create(-3, -3, 3, 3, pss, null, true, true);
        }

        /// <summary>
        /// Creates a new <see cref="BaseShape"/> instance.
        /// </summary>
        /// <param name="pss">The point shape <see cref="ShapeStyle"/>.</param>
        /// <returns>The new instance of the <see cref="BaseShape"/> class.</returns>
        public static BaseShape CrossPointShape(ShapeStyle pss)
        {
            var g = XGroup.Create("PointShape");

            var builder = g.Shapes.ToBuilder();
            builder.Add(XLine.Create(-4, 0, 4, 0, pss, null));
            builder.Add(XLine.Create(0, -4, 0, 4, pss, null));
            g.Shapes = builder.ToImmutable();

            return g;
        }
    }
}
