﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Path.Segments;
using Core2D.Shapes;
using System;
using System.Collections.Generic;

namespace Core2D.Path
{
    /// <summary>
    /// Path geometry context.
    /// </summary>
    public class XPathGeometryContext : XGeometryContext
    {
        private XPathGeometry _geometry;
        private XPathFigure _currentFigure;

        /// <summary>
        /// Initializes a new instance of the <see cref="XPathGeometryContext"/> class.
        /// </summary>
        public XPathGeometryContext(XPathGeometry geometry)
        {
            if (geometry == null)
            {
                throw new ArgumentNullException(nameof(geometry));
            }

            _geometry = geometry;
        }

        /// <inheritdoc/>
        public override void BeginFigure(XPoint startPoint, bool isFilled = true, bool isClosed = true)
        {
            _currentFigure = XPathFigure.Create(startPoint, isFilled, isClosed);
            _geometry.Figures.Add(_currentFigure);
        }

        /// <inheritdoc/>
        public override void SetClosedState(bool isClosed)
        {
            _currentFigure.IsClosed = isClosed;
        }

        /// <inheritdoc/>
        public override void LineTo(XPoint point, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XLineSegment.Create(
                point,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void ArcTo(XPoint point, XPathSize size, double rotationAngle = 0.0, bool isLargeArc = false, XSweepDirection sweepDirection = XSweepDirection.Clockwise, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XArcSegment.Create(
                point,
                size,
                rotationAngle,
                isLargeArc,
                sweepDirection,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void CubicBezierTo(XPoint point1, XPoint point2, XPoint point3, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XCubicBezierSegment.Create(
                point1,
                point2,
                point3,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void QuadraticBezierTo(XPoint point1, XPoint point2, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XQuadraticBezierSegment.Create(
                point1,
                point2,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void PolyLineTo(IList<XPoint> points, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XPolyLineSegment.Create(
                points,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void PolyCubicBezierTo(IList<XPoint> points, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XPolyCubicBezierSegment.Create(
                points,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }

        /// <inheritdoc/>
        public override void PolyQuadraticBezierTo(IList<XPoint> points, bool isStroked = true, bool isSmoothJoin = true)
        {
            var segment = XPolyQuadraticBezierSegment.Create(
                points,
                isStroked,
                isSmoothJoin);
            _currentFigure.Segments.Add(segment);
        }
    }
}
