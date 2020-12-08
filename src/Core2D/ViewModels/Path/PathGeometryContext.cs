﻿using System;
using Core2D.Shapes;

namespace Core2D.Path
{
    public partial class GeometryContext
    {
        private readonly IFactory _factory;
        private readonly PathGeometry _geometry;
        private PathFigure _currentFigure;

        public GeometryContext(IFactory factory, PathGeometry geometry)
        {
            _factory = factory;
            _geometry = geometry ?? throw new ArgumentNullException(nameof(geometry));
        }

        public void BeginFigure(PointShape startPoint, bool isClosed = true)
        {
            _currentFigure = _factory.CreatePathFigure(startPoint, isClosed);
            _geometry.Figures = _geometry.Figures.Add(_currentFigure);
        }

        public void SetClosedState(bool isClosed)
        {
            _currentFigure.IsClosed = isClosed;
        }

        public void LineTo(PointShape point)
        {
            var segment = _factory.CreateLineSegment(
                point);
            _currentFigure.Segments = _currentFigure.Segments.Add(segment);
        }

        public void ArcTo(PointShape point, PathSize size, double rotationAngle = 0.0, bool isLargeArc = false, SweepDirection sweepDirection = SweepDirection.Clockwise)
        {
            var segment = _factory.CreateArcSegment(
                point,
                size,
                rotationAngle,
                isLargeArc,
                sweepDirection);
            _currentFigure.Segments = _currentFigure.Segments.Add(segment);
        }

        public void CubicBezierTo(PointShape point1, PointShape point2, PointShape point3)
        {
            var segment = _factory.CreateCubicBezierSegment(
                point1,
                point2,
                point3);
            _currentFigure.Segments = _currentFigure.Segments.Add(segment);
        }

        public void QuadraticBezierTo(PointShape point1, PointShape point2)
        {
            var segment = _factory.CreateQuadraticBezierSegment(
                point1,
                point2);
            _currentFigure.Segments = _currentFigure.Segments.Add(segment);
        }
    }
}
