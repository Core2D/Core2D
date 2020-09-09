﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Core2D;
using Core2D.Containers;
using Core2D.Data;
using Core2D.Path;
using Core2D.Path.Segments;
using Core2D.Renderer;
using Core2D.Scripting;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D
{
    /// <summary>
    /// View model factory.
    /// </summary>
    public class Factory : IFactory
    {
#nullable disable
        /// <inheritdoc/>
        public ILibrary<T> CreateLibrary<T>(string name)
        {
            return new Library<T>()
            {
                Name = name,
                Items = ImmutableArray.Create<T>(),
                Selected = default
            };
        }

        /// <inheritdoc/>
        public ILibrary<T> CreateLibrary<T>(string name, IEnumerable<T> items)
        {
            return new Library<T>()
            {
                Name = name,
                Items = ImmutableArray.CreateRange<T>(items),
                Selected = items.FirstOrDefault()
            };
        }
#nullable restore
        /// <inheritdoc/>
        public IValue CreateValue(string content)
        {
            return new Value()
            {
                Content = content
            };
        }

        /// <inheritdoc/>
        public IProperty CreateProperty(IContext owner, string name, string value)
        {
            return new Property()
            {
                Name = name,
                Value = value,
                Owner = owner
            };
        }

        /// <inheritdoc/>
        public IColumn CreateColumn(IDatabase owner, string name, bool isVisible = true)
        {
            return new Column()
            {
                Name = name,
                IsVisible = isVisible,
                Owner = owner
            };
        }

        /// <inheritdoc/>
        public IRecord CreateRecord(IDatabase owner, ImmutableArray<IValue> values)
        {
            return new Record()
            {
                Values = values,
                Owner = owner
            };
        }

        /// <inheritdoc/>
        public IRecord CreateRecord(IDatabase owner, string id, ImmutableArray<IValue> values)
        {
            var record = new Record()
            {
                Values = values,
                Owner = owner
            };

            if (!string.IsNullOrWhiteSpace(id))
            {
                record.Id = id;
            }

            return record;
        }

        /// <inheritdoc/>
        public IRecord CreateRecord(IDatabase owner, string value)
        {
            return new Record()
            {
                Values = ImmutableArray.CreateRange(
                    Enumerable.Repeat(
                        value,
                        owner.Columns.Length).Select(c => CreateValue(c))),
                Owner = owner
            };
        }

        /// <inheritdoc/>
        public IContext CreateContext()
        {
            return new Context()
            {
                Properties = ImmutableArray.Create<IProperty>()
            };
        }

        /// <inheritdoc/>
        public IContext CreateContext(IRecord record)
        {
            return new Context()
            {
                Properties = ImmutableArray.Create<IProperty>(),
                Record = record
            };
        }

        /// <inheritdoc/>
        public IDatabase CreateDatabase(string name, string idColumnName = "Id")
        {
            return new Database()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = ImmutableArray.Create<IColumn>(),
                Records = ImmutableArray.Create<IRecord>()
            };
        }

        /// <inheritdoc/>
        public IDatabase CreateDatabase(string name, ImmutableArray<IColumn> columns, string idColumnName = "Id")
        {
            return new Database()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = columns,
                Records = ImmutableArray.Create<IRecord>()
            };
        }

        /// <inheritdoc/>
        public IDatabase CreateDatabase(string name, ImmutableArray<IColumn> columns, ImmutableArray<IRecord> records, string idColumnName = "Id")
        {
            return new Database()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = columns,
                Records = records
            };
        }

        /// <inheritdoc/>
        public IDatabase FromFields(string name, IEnumerable<string[]> fields, string idColumnName = "Id")
        {
            var db = CreateDatabase(name, idColumnName);
            var tempColumns = fields.FirstOrDefault().Select(c => CreateColumn(db, c));
            var columns = ImmutableArray.CreateRange<IColumn>(tempColumns);

            if (columns.Length >= 1 && columns[0].Name == idColumnName)
            {
                db.Columns = columns;

                // Use existing record Id.
                var tempRecords = fields
                    .Skip(1)
                    .Select(v =>
                            CreateRecord(
                                db,
                                v.FirstOrDefault(),
                                ImmutableArray.CreateRange<IValue>(v.Select(c => CreateValue(c)))));

                db.Records = ImmutableArray.CreateRange<IRecord>(tempRecords);
            }
            else
            {
                db.Columns = columns;

                // Create records with new Id.
                var tempRecords = fields
                    .Skip(1)
                    .Select(v =>
                            CreateRecord(
                                db,
                                ImmutableArray.CreateRange<IValue>(v.Select(c => CreateValue(c)))));

                db.Records = ImmutableArray.CreateRange<IRecord>(tempRecords);
            }

            return db;
        }
#nullable disable
        /// <inheritdoc/>
        public ICache<TKey, TValue> CreateCache<TKey, TValue>(Action<TValue> dispose = null)
        {
            return new Cache<TKey, TValue>(dispose);
        }
#nullable restore
        /// <inheritdoc/>
        public IShapeState CreateShapeState(ShapeStateFlags flags = ShapeStateFlags.Default)
        {
            return new ShapeState()
            {
                Flags = flags
            };
        }

        /// <inheritdoc/>
        public IShapeRendererState CreateShapeRendererState()
        {
            var state = new ShapeRendererState()
            {
                PanX = 0.0,
                PanY = 0.0,
                ZoomX = 1.0,
                ZoomY = 1.0,
                DrawShapeState = CreateShapeState(ShapeStateFlags.Visible),
                SelectedShapes = default
            };

            state.SelectionStyle =
                CreateShapeStyle(
                    "Selection",
                    0x7F, 0x33, 0x33, 0xFF,
                    0x4F, 0x33, 0x33, 0xFF,
                    1.0);

            state.HelperStyle =
                CreateShapeStyle(
                    "Helper",
                    0xFF, 0x00, 0xBF, 0xFF,
                    0xFF, 0x00, 0xBF, 0xFF,
                    1.0);

            state.DrawDecorators = true;
            state.DrawPoints = true;

            state.PointStyle = 
                CreateShapeStyle(
                    "Point",
                    0xFF, 0x00, 0xBF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF,
                    2.0);
            state.SelectedPointStyle =
                CreateShapeStyle(
                    "SelectionPoint",
                    0xFF, 0x00, 0xBF, 0xFF,
                    0xFF, 0x00, 0xBF, 0xFF,
                    2.0);
            state.PointSize = 4.0;

            return state;
        }

        /// <inheritdoc/>
        public ILineSegment CreateLineSegment(IPointShape point, bool isStroked = true)
        {
            return new LineSegment()
            {
                Point = point,
                IsStroked = isStroked
            };
        }

        /// <inheritdoc/>
        public IArcSegment CreateArcSegment(IPointShape point, IPathSize size, double rotationAngle, bool isLargeArc, SweepDirection sweepDirection, bool isStroked = true)
        {
            return new ArcSegment()
            {
                Point = point,
                Size = size,
                RotationAngle = rotationAngle,
                IsLargeArc = isLargeArc,
                SweepDirection = sweepDirection,
                IsStroked = isStroked
            };
        }

        /// <inheritdoc/>
        public IQuadraticBezierSegment CreateQuadraticBezierSegment(IPointShape point1, IPointShape point2, bool isStroked = true)
        {
            return new QuadraticBezierSegment()
            {
                Point1 = point1,
                Point2 = point2,
                IsStroked = isStroked
            };
        }

        /// <inheritdoc/>
        public ICubicBezierSegment CreateCubicBezierSegment(IPointShape point1, IPointShape point2, IPointShape point3, bool isStroked = true)
        {
            return new CubicBezierSegment()
            {
                Point1 = point1,
                Point2 = point2,
                Point3 = point3,
                IsStroked = isStroked
            };
        }

        /// <inheritdoc/>
        public IPathSize CreatePathSize(double width = 0.0, double height = 0.0)
        {
            return new PathSize()
            {
                Width = width,
                Height = height
            };
        }

        /// <inheritdoc/>
        public IPathGeometry CreatePathGeometry()
        {
            return new PathGeometry()
            {
                Figures = ImmutableArray.Create<IPathFigure>(),
                FillRule = FillRule.Nonzero
            };
        }

        /// <inheritdoc/>
        public IGeometryContext CreateGeometryContext()
        {
            return new PathGeometryContext(this, CreatePathGeometry());
        }

        /// <inheritdoc/>
        public IGeometryContext CreateGeometryContext(IPathGeometry geometry)
        {
            return new PathGeometryContext(this, geometry);
        }

        /// <inheritdoc/>
        public IPathGeometry CreatePathGeometry(ImmutableArray<IPathFigure> figures, FillRule fillRule = FillRule.Nonzero)
        {
            return new PathGeometry()
            {
                Figures = figures,
                FillRule = fillRule
            };
        }

        /// <inheritdoc/>
        public IPathFigure CreatePathFigure(bool isClosed = false)
        {
            return new PathFigure()
            {
                StartPoint = CreatePointShape(),
                Segments = ImmutableArray.Create<IPathSegment>(),
                IsClosed = isClosed
            };
        }

        /// <inheritdoc/>
        public IPathFigure CreatePathFigure(IPointShape startPoint, bool isClosed = false)
        {
            return new PathFigure()
            {
                StartPoint = startPoint,
                Segments = ImmutableArray.Create<IPathSegment>(),
                IsClosed = isClosed
            };
        }

        /// <inheritdoc/>
        public IPointShape CreatePointShape(double x = 0.0, double y = 0.0, PointAlignment alignment = PointAlignment.None, string name = "")
        {
            var pointShape = new PointShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = default,
                X = x,
                Y = y,
                Alignment = alignment
            };
            return pointShape;
        }

        /// <inheritdoc/>
        public ILineShape CreateLineShape(IPointShape start, IPointShape end, IShapeStyle style, bool isStroked = true, string name = "")
        {
            var lineShape = new LineShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = false
            };

            lineShape.Start = start;
            lineShape.End = end;

            return lineShape;
        }

        /// <inheritdoc/>
        public ILineShape CreateLineShape(double x1, double y1, double x2, double y2, IShapeStyle style, bool isStroked = true, string name = "")
        {
            var lineShape = new LineShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = false
            };

            lineShape.Start = CreatePointShape(x1, y1);
            lineShape.Start.Owner = lineShape;

            lineShape.End = CreatePointShape(x2, y2);
            lineShape.End.Owner = lineShape;

            return lineShape;
        }

        /// <inheritdoc/>
        public ILineShape CreateLineShape(double x, double y, IShapeStyle style, bool isStroked = true, string name = "")
        {
            return CreateLineShape(x, y, x, y, style, isStroked, name);
        }

        /// <inheritdoc/>
        public IArcShape CreateArcShape(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var arcShape = new ArcShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            arcShape.Point1 = CreatePointShape(x1, y1);
            arcShape.Point1.Owner = arcShape;

            arcShape.Point2 = CreatePointShape(x2, y2);
            arcShape.Point2.Owner = arcShape;

            arcShape.Point3 = CreatePointShape(x3, y3);
            arcShape.Point3.Owner = arcShape;

            arcShape.Point4 = CreatePointShape(x4, y4);
            arcShape.Point4.Owner = arcShape;

            return arcShape;
        }

        /// <inheritdoc/>
        public IArcShape CreateArcShape(double x, double y, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            return CreateArcShape(x, y, x, y, x, y, x, y, style, isStroked, isFilled, name);
        }

        /// <inheritdoc/>
        public IArcShape CreateArcShape(IPointShape point1, IPointShape point2, IPointShape point3, IPointShape point4, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var arcShape = new ArcShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            arcShape.Point1 = point1;
            arcShape.Point2 = point2;
            arcShape.Point3 = point3;
            arcShape.Point4 = point4;

            return arcShape;
        }

        /// <inheritdoc/>
        public IQuadraticBezierShape CreateQuadraticBezierShape(double x1, double y1, double x2, double y2, double x3, double y3, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var quadraticBezierShape = new QuadraticBezierShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            quadraticBezierShape.Point1 = CreatePointShape(x1, y1);
            quadraticBezierShape.Point1.Owner = quadraticBezierShape;

            quadraticBezierShape.Point2 = CreatePointShape(x2, y2);
            quadraticBezierShape.Point2.Owner = quadraticBezierShape;

            quadraticBezierShape.Point3 = CreatePointShape(x3, y3);
            quadraticBezierShape.Point3.Owner = quadraticBezierShape;

            return quadraticBezierShape;
        }

        /// <inheritdoc/>
        public IQuadraticBezierShape CreateQuadraticBezierShape(double x, double y, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            return CreateQuadraticBezierShape(x, y, x, y, x, y, style, isStroked, isFilled, name);
        }

        /// <inheritdoc/>
        public IQuadraticBezierShape CreateQuadraticBezierShape(IPointShape point1, IPointShape point2, IPointShape point3, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var quadraticBezierShape = new QuadraticBezierShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            quadraticBezierShape.Point1 = point1;
            quadraticBezierShape.Point2 = point2;
            quadraticBezierShape.Point3 = point3;

            return quadraticBezierShape;
        }

        /// <inheritdoc/>
        public ICubicBezierShape CreateCubicBezierShape(double x1, double y1, double x2, double y2, double x3, double y3, double x4, double y4, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var cubicBezierShape = new CubicBezierShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            cubicBezierShape.Point1 = CreatePointShape(x1, y1);
            cubicBezierShape.Point1.Owner = cubicBezierShape;

            cubicBezierShape.Point2 = CreatePointShape(x2, y2);
            cubicBezierShape.Point2.Owner = cubicBezierShape;

            cubicBezierShape.Point3 = CreatePointShape(x3, y3);
            cubicBezierShape.Point3.Owner = cubicBezierShape;

            cubicBezierShape.Point4 = CreatePointShape(x4, y4);
            cubicBezierShape.Point4.Owner = cubicBezierShape;

            return cubicBezierShape;
        }

        /// <inheritdoc/>
        public ICubicBezierShape CreateCubicBezierShape(double x, double y, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            return CreateCubicBezierShape(x, y, x, y, x, y, x, y, style, isStroked, isFilled, name);
        }

        /// <inheritdoc/>
        public ICubicBezierShape CreateCubicBezierShape(IPointShape point1, IPointShape point2, IPointShape point3, IPointShape point4, IShapeStyle style, bool isStroked = true, bool isFilled = false, string name = "")
        {
            var cubicBezierShape = new CubicBezierShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled
            };

            cubicBezierShape.Point1 = point1;
            cubicBezierShape.Point2 = point2;
            cubicBezierShape.Point3 = point3;
            cubicBezierShape.Point4 = point4;

            return cubicBezierShape;
        }

        /// <inheritdoc/>
        public IRectangleShape CreateRectangleShape(double x1, double y1, double x2, double y2, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            var rectangleShape = new RectangleShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Text = text
            };

            rectangleShape.TopLeft = CreatePointShape(x1, y1);
            rectangleShape.TopLeft.Owner = rectangleShape;

            rectangleShape.BottomRight = CreatePointShape(x2, y2);
            rectangleShape.BottomRight.Owner = rectangleShape;

            return rectangleShape;
        }

        /// <inheritdoc/>
        public IRectangleShape CreateRectangleShape(double x, double y, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            return CreateRectangleShape(x, y, x, y, style, isStroked, isFilled, text, name);
        }

        /// <inheritdoc/>
        public IRectangleShape CreateRectangleShape(IPointShape topLeft, IPointShape bottomRight, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            var rectangleShape = new RectangleShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Text = text
            };

            rectangleShape.TopLeft = topLeft;
            rectangleShape.BottomRight = bottomRight;

            return rectangleShape;
        }

        /// <inheritdoc/>
        public IEllipseShape CreateEllipseShape(double x1, double y1, double x2, double y2, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            var ellipseShape = new EllipseShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Text = text,
            };

            ellipseShape.TopLeft = CreatePointShape(x1, y1);
            ellipseShape.TopLeft.Owner = ellipseShape;

            ellipseShape.BottomRight = CreatePointShape(x2, y2);
            ellipseShape.BottomRight.Owner = ellipseShape;

            return ellipseShape;
        }

        /// <inheritdoc/>
        public IEllipseShape CreateEllipseShape(double x, double y, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            return CreateEllipseShape(x, y, x, y, style, isStroked, isFilled, text, name);
        }

        /// <inheritdoc/>
        public IEllipseShape CreateEllipseShape(IPointShape topLeft, IPointShape bottomRight, IShapeStyle style, bool isStroked = true, bool isFilled = false, string? text = null, string name = "")
        {
            var ellipseShape = new EllipseShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                TopLeft = topLeft,
                BottomRight = bottomRight,
                Text = text,
            };

            ellipseShape.TopLeft = topLeft;
            ellipseShape.BottomRight = bottomRight;

            return ellipseShape;
        }

        /// <inheritdoc/>
        public IPathShape CreatePathShape(IShapeStyle style, IPathGeometry geometry, bool isStroked = true, bool isFilled = true)
        {
            var pathShape = new PathShape()
            {
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Geometry = geometry
            };

            if (geometry != null)
            {
                geometry.Owner = pathShape;

                foreach (var figure in geometry.Figures)
                {
                    figure.Owner = pathShape;

                    foreach (var segment in figure.Segments)
                    {
                        segment.Owner = pathShape;
                    }
                } 
            }

            return pathShape;
        }

        /// <inheritdoc/>
        public IPathShape CreatePathShape(string name, IShapeStyle style, IPathGeometry geometry, bool isStroked = true, bool isFilled = true)
        {
            var pathShape = new PathShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Geometry = geometry
            };

            if (geometry != null)
            {
                geometry.Owner = pathShape;

                foreach (var figure in geometry.Figures)
                {
                    figure.Owner = pathShape;

                    foreach (var segment in figure.Segments)
                    {
                        segment.Owner = pathShape;
                    }
                }
            }
            return pathShape;
        }

        /// <inheritdoc/>
        public ITextShape CreateTextShape(double x1, double y1, double x2, double y2, IShapeStyle style, string text, bool isStroked = true, string name = "")
        {
            var textShape = new TextShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                Text = text
            };

            textShape.TopLeft = CreatePointShape(x1, y1);
            textShape.TopLeft.Owner = textShape;

            textShape.BottomRight = CreatePointShape(x2, y2);
            textShape.BottomRight.Owner = textShape;

            return textShape;
        }

        /// <inheritdoc/>
        public ITextShape CreateTextShape(double x, double y, IShapeStyle style, string text, bool isStroked = true, string name = "")
        {
            return CreateTextShape(x, y, x, y, style, text, isStroked, name);
        }

        /// <inheritdoc/>
        public ITextShape CreateTextShape(IPointShape topLeft, IPointShape bottomRight, IShapeStyle style, string text, bool isStroked = true, string name = "")
        {
            var textShape = new TextShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                TopLeft = topLeft,
                BottomRight = bottomRight,
                Text = text
            };

            textShape.TopLeft = topLeft;
            textShape.BottomRight = bottomRight;

            return textShape;
        }

        /// <inheritdoc/>
        public IImageShape CreateImageShape(double x1, double y1, double x2, double y2, IShapeStyle style, string key, bool isStroked = false, bool isFilled = false, string? text = null, string name = "")
        {
            var imageShape = new ImageShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                Key = key,
                Text = text
            };

            imageShape.TopLeft = CreatePointShape(x1, y1);
            imageShape.TopLeft.Owner = imageShape;

            imageShape.BottomRight = CreatePointShape(x2, y2);
            imageShape.BottomRight.Owner = imageShape;

            return imageShape;
        }

        /// <inheritdoc/>
        public IImageShape CreateImageShape(double x, double y, IShapeStyle style, string key, bool isStroked = false, bool isFilled = false, string? text = null, string name = "")
        {
            return CreateImageShape(x, y, x, y, style, key, isStroked, isFilled, text, name);
        }

        /// <inheritdoc/>
        public IImageShape CreateImageShape(IPointShape topLeft, IPointShape bottomRight, IShapeStyle style, string key, bool isStroked = false, bool isFilled = false, string? text = null, string name = "")
        {
            var imageShape = new ImageShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Style = style,
                IsStroked = isStroked,
                IsFilled = isFilled,
                TopLeft = topLeft,
                BottomRight = bottomRight,
                Key = key,
                Text = text
            };

            imageShape.TopLeft = topLeft;
            imageShape.BottomRight = bottomRight;

            return imageShape;
        }

        /// <inheritdoc/>
        public IGroupShape CreateGroupShape(string name = "g")
        {
            return new GroupShape()
            {
                Name = name,
                State = CreateShapeState(ShapeStateFlags.Visible | ShapeStateFlags.Printable | ShapeStateFlags.Standalone),
                Data = CreateContext(),
                Connectors = ImmutableArray.Create<IPointShape>(),
                Shapes = ImmutableArray.Create<IBaseShape>()
            };
        }

        /// <inheritdoc/>
        public IArgbColor CreateArgbColor(byte a = 0xFF, byte r = 0x00, byte g = 0x00, byte b = 0x00)
        {
            return new ArgbColor()
            {
                A = a,
                R = r,
                G = g,
                B = b
            };
        }

        /// <inheritdoc/>
        public IArrowStyle CreateArrowStyle(ArrowType arrowType = ArrowType.None, bool isStroked = true, bool isFilled = false, double radiusX = 5.0, double radiusY = 3.0)
        {
            return new ArrowStyle()
            {
                ArrowType = arrowType,
                IsFilled = isFilled,
                RadiusX = radiusX,
                RadiusY = radiusY
            };
        }

        /// <inheritdoc/>
        public IArrowStyle CreateArrowStyle(IBaseStyle source, ArrowType arrowType = ArrowType.None, bool isStroked = true, bool isFilled = false, double radiusX = 5.0, double radiusY = 5.0)
        {
            return new ArrowStyle(source)
            {
                ArrowType = arrowType,
                IsStroked = isStroked,
                IsFilled = isFilled,
                RadiusX = radiusX,
                RadiusY = radiusY
            };
        }

        /// <inheritdoc/>
        public IArrowStyle CreateArrowStyle(string name, IBaseStyle source, ArrowType arrowType = ArrowType.None, bool isStroked = true, bool isFilled = false, double radiusX = 5.0, double radiusY = 5.0)
        {
            return new ArrowStyle(source)
            {
                Name = name,
                ArrowType = arrowType,
                IsStroked = isStroked,
                IsFilled = isFilled,
                RadiusX = radiusX,
                RadiusY = radiusY
            };
        }

        /// <inheritdoc/>
        public IFontStyle CreateFontStyle(FontStyleFlags flags = FontStyleFlags.Regular)
        {
            return new FontStyle()
            {
                Flags = flags
            };
        }

        /// <inheritdoc/>
        public ILineFixedLength CreateLineFixedLength(LineFixedLengthFlags flags = LineFixedLengthFlags.Disabled, IShapeState? startTrigger = null, IShapeState? endTrigger = null, double length = 15.0)
        {
            return new LineFixedLength()
            {
                Flags = flags,
                StartTrigger = startTrigger ?? CreateShapeState(ShapeStateFlags.Connector | ShapeStateFlags.Output),
                EndTrigger = endTrigger ?? CreateShapeState(ShapeStateFlags.Connector | ShapeStateFlags.Input),
                Length = length
            };
        }

        /// <inheritdoc/>
        public ILineStyle CreateLineStyle(string name = "", bool isCurved = false, double curvature = 50.0, CurveOrientation curveOrientation = CurveOrientation.Auto, ILineFixedLength? fixedLength = null)
        {
            return new LineStyle()
            {
                Name = name,
                IsCurved = isCurved,
                Curvature = curvature,
                CurveOrientation = curveOrientation,
                FixedLength = fixedLength ?? CreateLineFixedLength()
            };
        }

        /// <inheritdoc/>
        public IShapeStyle CreateShapeStyle(string? name = null, byte sa = 0xFF, byte sr = 0x00, byte sg = 0x00, byte sb = 0x00, byte fa = 0xFF, byte fr = 0x00, byte fg = 0x00, byte fb = 0x00, double thickness = 2.0, ITextStyle? textStyle = null, ILineStyle? lineStyle = null, IArrowStyle? startArrowStyle = null, IArrowStyle? endArrowStyle = null, LineCap lineCap = LineCap.Round, string? dashes = default, double dashOffset = 0.0)
        {
            var style = new ShapeStyle()
            {
                Name = name,
                Stroke = CreateArgbColor(sa, sr, sg, sb),
                Fill = CreateArgbColor(fa, fr, fg, fb),
                Thickness = thickness,
                LineCap = lineCap,
                Dashes = dashes,
                DashOffset = dashOffset,
                LineStyle = lineStyle ?? CreateLineStyle(),
                TextStyle = textStyle ?? CreateTextStyle()
            };

            style.StartArrowStyle = startArrowStyle ?? CreateArrowStyle(style);
            style.EndArrowStyle = endArrowStyle ?? CreateArrowStyle(style);

            return style;
        }

        /// <inheritdoc/>
        public IShapeStyle CreateShapeStyle(string name, IColor stroke, IColor fill, double thickness, ITextStyle textStyle, ILineStyle lineStyle, IArrowStyle startArrowStyle, IArrowStyle endArrowStyle)
        {
            return new ShapeStyle()
            {
                Name = name,
                Stroke = stroke,
                Fill = fill,
                Thickness = thickness,
                LineCap = LineCap.Round,
                Dashes = default,
                DashOffset = 0.0,
                LineStyle = lineStyle,
                TextStyle = textStyle,
                StartArrowStyle = startArrowStyle,
                EndArrowStyle = endArrowStyle
            };
        }

        /// <inheritdoc/>
        public ITextStyle CreateTextStyle(string name = "", string fontName = "Calibri", string fontFile = @"C:\Windows\Fonts\calibri.ttf", double fontSize = 12.0, IFontStyle? fontStyle = null, TextHAlignment textHAlignment = TextHAlignment.Center, TextVAlignment textVAlignment = TextVAlignment.Center)
        {
            return new TextStyle()
            {
                Name = name,
                FontName = fontName,
                FontFile = fontFile,
                FontSize = fontSize,
                FontStyle = fontStyle ?? CreateFontStyle(FontStyleFlags.Regular),
                TextHAlignment = textHAlignment,
                TextVAlignment = textVAlignment
            };
        }

        /// <inheritdoc/>
        public IOptions CreateOptions()
        {
            return new Options()
            {
                SnapToGrid = true,
                SnapX = 15.0,
                SnapY = 15.0,
                HitThreshold = 7.0,
                MoveMode = MoveMode.Point,
                DefaultIsStroked = true,
                DefaultIsFilled = false,
                DefaultIsClosed = true,
                DefaultFillRule = FillRule.EvenOdd,
                TryToConnect = false
            };
        }

        /// <inheritdoc/>
        public IScript CreateScript(string name = "Script", string code = "")
        {
            return new Script()
            {
                Name = name,
                Code = code
            };
        }

        /// <inheritdoc/>
        public ILayerContainer CreateLayerContainer(string name = "Layer", IPageContainer? owner = null, bool isVisible = true)
        {
            return new LayerContainer()
            {
                Name = name,
                Owner = owner,
                Shapes = ImmutableArray.Create<IBaseShape>(),
                IsVisible = isVisible
            };
        }

        /// <inheritdoc/>
        public IPageContainer CreatePageContainer(string name = "Page")
        {
            var page = new PageContainer()
            {
                Name = name,
                Layers = ImmutableArray.Create<ILayerContainer>(),
                Data = CreateContext()
            };

            var builder = page.Layers.ToBuilder();
            builder.Add(CreateLayerContainer("Layer1", page));
            page.Layers = builder.ToImmutable();

            page.CurrentLayer = page.Layers.FirstOrDefault();
            page.WorkingLayer = CreateLayerContainer("Working", page);
            page.HelperLayer = CreateLayerContainer("Helper", page);

            return page;
        }

        /// <inheritdoc/>
        public IPageContainer CreateTemplateContainer(string name = "Template", double width = 840, double height = 600)
        {
            var template = new PageContainer()
            {
                Name = name,
                Layers = ImmutableArray.Create<ILayerContainer>(),
                Data = CreateContext()
            };

            template.Background = CreateArgbColor(0x00, 0xFF, 0xFF, 0xFF);
            template.Width = width;
            template.Height = height;

            template.IsGridEnabled = false;
            template.IsBorderEnabled = false;
            template.GridOffsetLeft = 0.0;
            template.GridOffsetTop = 0.0;
            template.GridOffsetRight = 0.0;
            template.GridOffsetBottom = 0.0;
            template.GridCellWidth = 10.0;
            template.GridCellHeight = 10.0;
            template.GridStrokeColor = CreateArgbColor(0xFF, 0xDE, 0xDE, 0xDE);
            template.GridStrokeThickness = 1.0;

            var builder = template.Layers.ToBuilder();
            builder.Add(CreateLayerContainer("TemplateLayer1", template));
            template.Layers = builder.ToImmutable();

            template.CurrentLayer = template.Layers.FirstOrDefault();
            template.WorkingLayer = CreateLayerContainer("TemplateWorking", template);
            template.HelperLayer = CreateLayerContainer("TemplateHelper", template);

            return template;
        }

        /// <inheritdoc/>
        public IDocumentContainer CreateDocumentContainer(string name = "Document")
        {
            return new DocumentContainer()
            {
                Name = name,
                Pages = ImmutableArray.Create<IPageContainer>()
            };
        }

        /// <inheritdoc/>
        public IProjectContainer CreateProjectContainer(string name = "Project")
        {
            return new ProjectContainer()
            {
                Name = name,
                Options = CreateOptions(),
                StyleLibraries = ImmutableArray.Create<ILibrary<IShapeStyle>>(),
                GroupLibraries = ImmutableArray.Create<ILibrary<IGroupShape>>(),
                Databases = ImmutableArray.Create<IDatabase>(),
                Templates = ImmutableArray.Create<IPageContainer>(),
                Scripts = ImmutableArray.Create<IScript>(),
                Documents = ImmutableArray.Create<IDocumentContainer>()
            };
        }

        private IEnumerable<string> GetUsedKeys(IProjectContainer project)
        {
            return ProjectContainer.GetAllShapes<IImageShape>(project).Select(i => i.Key).Distinct();
        }

        private IProjectContainer ReadProjectContainer(ZipArchiveEntry projectEntry, IFileSystem fileIO, IJsonSerializer serializer)
        {
            using var entryStream = projectEntry.Open();
            return serializer.Deserialize<ProjectContainer>(fileIO.ReadUtf8Text(entryStream));
        }

        private void WriteProjectContainer(IProjectContainer project, ZipArchiveEntry projectEntry, IFileSystem fileIO, IJsonSerializer serializer)
        {
            using var jsonStream = projectEntry.Open();
            fileIO.WriteUtf8Text(jsonStream, serializer.Serialize(project));
        }

        private void ReadImages(IImageCache cache, ZipArchive archive, IFileSystem fileIO)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.StartsWith("Images\\"))
                {
                    using var entryStream = entry.Open();
                    var bytes = fileIO.ReadBinary(entryStream);
                    cache.AddImage(entry.FullName, bytes);
                }
            }
        }

        private void WriteImages(IImageCache cache, IEnumerable<string> keys, ZipArchive archive, IFileSystem fileIO)
        {
            foreach (var key in keys)
            {
                var imageEntry = archive.CreateEntry(key);
                using var imageStream = imageEntry.Open();
                fileIO.WriteBinary(imageStream, cache.GetImage(key));
            }
        }

        /// <inheritdoc/>
        public IProjectContainer OpenProjectContainer(string path, IFileSystem fileIO, IJsonSerializer serializer)
        {
            using var stream = fileIO.Open(path);
            return OpenProjectContainer(stream, fileIO, serializer);
        }

        /// <inheritdoc/>
        public void SaveProjectContainer(IProjectContainer project, string path, IFileSystem fileIO, IJsonSerializer serializer)
        {
            if (project is IImageCache imageCache)
            {
                using var stream = fileIO.Create(path);
                SaveProjectContainer(project, imageCache, stream, fileIO, serializer);
            }
        }

        /// <inheritdoc/>
        public IProjectContainer OpenProjectContainer(Stream stream, IFileSystem fileIO, IJsonSerializer serializer)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Read);
            var projectEntry = archive.Entries.FirstOrDefault(e => e.FullName == "Project.json");
            var project = ReadProjectContainer(projectEntry, fileIO, serializer);
            if (project is IImageCache imageCache)
            {
                ReadImages(imageCache, archive, fileIO);
            }
            return project;
        }

        /// <inheritdoc/>
        public void SaveProjectContainer(IProjectContainer project, IImageCache imageCache, Stream stream, IFileSystem fileIO, IJsonSerializer serializer)
        {
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
            var projectEntry = archive.CreateEntry("Project.json");
            WriteProjectContainer(project, projectEntry, fileIO, serializer);
            var keys = GetUsedKeys(project);
            WriteImages(imageCache, keys, archive, fileIO);
        }
    }
}
