﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using netDxf;
using netDxf.Entities;
using netDxf.Header;
using netDxf.Objects;
using netDxf.Tables;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Renderer.Dxf
{
    /// <summary>
    /// Native netDxf shape renderer.
    /// </summary>
    public class DxfRenderer : Core2D.Renderer.ShapeRenderer
    {
        private Core2D.Renderer.Cache<string, ImageDefinition> _biCache = Core2D.Renderer.Cache<string, ImageDefinition>.Create();
        private double _pageWidth;
        private double _pageHeight;
        private string _outputPath;
        private Layer _currentLayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DxfRenderer"/> class.
        /// </summary>
        public DxfRenderer()
        {
            ClearCache(isZooming: false);
        }

        /// <summary>
        /// Creates a new <see cref="DxfRenderer"/> instance.
        /// </summary>
        /// <returns>The new instance of the <see cref="DxfRenderer"/> class.</returns>
        public static Core2D.Renderer.ShapeRenderer Create() => new DxfRenderer();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="page"></param>
        public void Save(string path, Core2D.Project.XPage page)
        {
            _outputPath = System.IO.Path.GetDirectoryName(path);
            var dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Add(dxf, page);

            dxf.Save(path);
            ClearCache(isZooming: false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="document"></param>
        public void Save(string path, Core2D.Project.XDocument document)
        {
            _outputPath = System.IO.Path.GetDirectoryName(path);
            var dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Add(dxf, document);

            dxf.Save(path);
            ClearCache(isZooming: false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="project"></param>
        public void Save(string path, Core2D.Project.XProject project)
        {
            _outputPath = System.IO.Path.GetDirectoryName(path);
            var dxf = new DxfDocument(DxfVersion.AutoCad2010);

            Add(dxf, project);

            dxf.Save(path);
            ClearCache(isZooming: false);
        }

        private void Add(DxfDocument dxf, Core2D.Project.XPage page)
        {
            if (page.Template != null)
            {
                _pageWidth = page.Template.Width;
                _pageHeight = page.Template.Height;
                Draw(dxf, page.Template, page.Data.Properties, page.Data.Record);
            }
            else
            {
                throw new NullReferenceException("Container template must be set.");
            }

            Draw(dxf, page, page.Data.Properties, page.Data.Record);
        }

        private void Add(DxfDocument dxf, Core2D.Project.XDocument document)
        {
            foreach (var page in document.Pages)
            {
                var layout = new Layout(page.Name)
                {
                    PlotSettings = new PlotSettings()
                    {
                        PaperSizeName = $"{page.Template.Name}_({page.Template.Width}_x_{page.Template.Height}_MM)",
                        LeftMargin = 0.0,
                        BottomMargin = 0.0,
                        RightMargin = 0.0,
                        TopMargin = 0.0,
                        PaperSize = new Vector2(page.Template.Width, page.Template.Height),
                        Origin =  new Vector2(0.0, 0.0),
                        PaperUnits = PlotPaperUnits.Milimeters,
                        PaperRotation = PlotRotation.NoRotation
                    }
                };
                dxf.Layouts.Add(layout);
                dxf.ActiveLayout = layout.Name;

                Add(dxf, page);
            }
        }

        private void Add(DxfDocument dxf, Core2D.Project.XProject project)
        {
            foreach (var document in project.Documents)
            {
                Add(dxf, document);
            }
        }

        private static double LineweightFactor = 96.0 / 2540.0;

        private static short[] Lineweights = { -3, -2, -1, 0, 5, 9, 13, 15, 18, 20, 25, 30, 35, 40, 50, 53, 60, 70, 80, 90, 100, 106, 120, 140, 158, 200, 211 };

        private static short ToLineweight(double thickness)
        {
            short lineweight = (short)(thickness / LineweightFactor);
            return Lineweights.OrderBy(x => Math.Abs((long)x - lineweight)).First();
        }

        private static AciColor ToColor(Core2D.Style.ArgbColor color) => new AciColor(color.R, color.G, color.B);

        private static short ToTransparency(Core2D.Style.ArgbColor color) => (short)(90.0 - color.A * 90.0 / 255.0);

        private double ToDxfX(double x) => x;

        private double ToDxfY(double y) => _pageHeight - y;

        private Line CreateLine(double x1, double y1, double x2, double y2)
        {
            double _x1 = ToDxfX(x1);
            double _y1 = ToDxfY(y1);
            double _x2 = ToDxfX(x2);
            double _y2 = ToDxfY(y2);
            return new Line(new Vector3(_x1, _y1, 0), new Vector3(_x2, _y2, 0));
        }

        private Ellipse CreateEllipse(double x, double y, double width, double height)
        {
            double _cx = ToDxfX(x + width / 2.0);
            double _cy = ToDxfY(y + height / 2.0);
            double minor = Math.Min(height, width);
            double major = Math.Max(height, width);

            return new Ellipse()
            {
                Center = new Vector3(_cx, _cy, 0),
                MajorAxis = major,
                MinorAxis = minor,
                StartAngle = 0.0,
                EndAngle = 360.0,
                Rotation = height > width ? 90.0 : 0.0
            };
        }

        private Ellipse CreateEllipticalArc(Core2D.Shapes.XArc arc, double dx, double dy)
        {
            var a = Core2D.Math.Arc.GdiArc.FromXArc(arc, dx, dy);

            double _cx = ToDxfX(a.X + a.Width / 2.0);
            double _cy = ToDxfY(a.Y + a.Height / 2.0);
            double minor = Math.Min(a.Height, a.Width);
            double major = Math.Max(a.Height, a.Width);
            double startAngle = -a.EndAngle;
            double endAngle = -a.StartAngle;
            double rotation = 0;

            if (a.Height > a.Width)
            {
                startAngle += 90;
                endAngle += 90;
                rotation = -90;
            }

            return new Ellipse()
            {
                Center = new Vector3(_cx, _cy, 0),
                MajorAxis = major,
                MinorAxis = minor,
                StartAngle = startAngle,
                EndAngle = endAngle,
                Rotation = rotation
            };
        }

        private Spline CreateQuadraticSpline(double p1x, double p1y, double p2x, double p2y, double p3x, double p3y)
        {
            double _p1x = ToDxfX(p1x);
            double _p1y = ToDxfY(p1y);
            double _p2x = ToDxfX(p2x);
            double _p2y = ToDxfY(p2y);
            double _p3x = ToDxfX(p3x);
            double _p3y = ToDxfY(p3y);

            return new Spline(
                new List<SplineVertex>
                {
                    new SplineVertex(_p1x, _p1y, 0.0),
                    new SplineVertex(_p2x, _p2y, 0.0),
                    new SplineVertex(_p3x, _p3y, 0.0)
                }, 2);
        }

        private Spline CreateCubicSpline(double p1x, double p1y, double p2x, double p2y, double p3x, double p3y, double p4x, double p4y)
        {
            double _p1x = ToDxfX(p1x);
            double _p1y = ToDxfY(p1y);
            double _p2x = ToDxfX(p2x);
            double _p2y = ToDxfY(p2y);
            double _p3x = ToDxfX(p3x);
            double _p3y = ToDxfY(p3y);
            double _p4x = ToDxfX(p4x);
            double _p4y = ToDxfY(p4y);

            return new Spline(
                new List<SplineVertex>
                {
                    new SplineVertex(_p1x, _p1y, 0.0),
                    new SplineVertex(_p2x, _p2y, 0.0),
                    new SplineVertex(_p3x, _p3y, 0.0),
                    new SplineVertex(_p4x, _p4y, 0.0)
                }, 3);
        }

        private void DrawLineInternal(DxfDocument dxf, Layer layer, Core2D.Style.BaseStyle style, bool isStroked, double x1, double y1, double x2, double y2)
        {
            if (isStroked)
            {
                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                var dxfLine = CreateLine(x1, y1, x2, y2);

                dxfLine.Layer = layer;
                dxfLine.Color = stroke;
                dxfLine.Transparency.Value = strokeTansparency;
                dxfLine.Lineweight.Value = lineweight;

                dxf.AddEntity(dxfLine);
            }
        }

        private void DrawRectangleInternal(DxfDocument dxf, Layer layer, bool isFilled, bool isStroked, Core2D.Style.BaseStyle style, ref Core2D.Math.Rect2 rect)
        {
            double x = rect.X;
            double y = rect.Y;
            double w = rect.Width;
            double h = rect.Height;

            if (isFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                var bounds =
                    new List<HatchBoundaryPath>
                    {
                        new HatchBoundaryPath(
                            new List<EntityObject>
                            {
                                CreateLine(x, y, x + w, y),
                                CreateLine(x + w, y, x + w, y + h),
                                CreateLine(x + w, y + h, x, y + h),
                                CreateLine(x, y + h, x, y)
                            })
                    };

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = layer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (isStroked)
            {
                DrawLineInternal(dxf, layer, style, true, x, y, x + w, y);
                DrawLineInternal(dxf, layer, style, true, x + w, y, x + w, y + h);
                DrawLineInternal(dxf, layer, style, true, x + w, y + h, x, y + h);
                DrawLineInternal(dxf, layer, style, true, x, y + h, x, y);
            }
        }

        private void DrawEllipseInternal(DxfDocument dxf, Layer layer, bool isFilled, bool isStroked, Core2D.Style.BaseStyle style, ref Core2D.Math.Rect2 rect)
        {
            var dxfEllipse = CreateEllipse(rect.X, rect.Y, rect.Width, rect.Height);

            if (isFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                // TODO: The netDxf does not create hatch for Ellipse with end angle equal to 360.
                var bounds =
                    new List<HatchBoundaryPath>
                    {
                        new HatchBoundaryPath(
                            new List<EntityObject>
                            {
                                (Ellipse)dxfEllipse.Clone()
                            })
                    };

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = layer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (isStroked)
            {
                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                dxfEllipse.Layer = layer;
                dxfEllipse.Color = stroke;
                dxfEllipse.Transparency.Value = strokeTansparency;
                dxfEllipse.Lineweight.Value = lineweight;

                dxf.AddEntity(dxfEllipse);
            }
        }

        private void DrawGridInternal(DxfDocument dxf, Layer layer, Core2D.Style.ShapeStyle style, double offsetX, double offsetY, double cellWidth, double cellHeight, ref Core2D.Math.Rect2 rect)
        {
            double ox = rect.X;
            double oy = rect.Y;
            double sx = ox + offsetX;
            double sy = oy + offsetY;
            double ex = ox + rect.Width;
            double ey = oy + rect.Height;

            for (double gx = sx; gx < ex; gx += cellWidth)
            {
                DrawLineInternal(dxf, layer, style, true, gx, oy, gx, ey);
            }

            for (double gy = sy; gy < ey; gy += cellHeight)
            {
                DrawLineInternal(dxf, layer, style, true, ox, gy, ex, gy);
            }
        }

        private void CreateHatchBoundsAndEntitiess(Core2D.Path.XPathGeometry pg, double dx, double dy, out IList<HatchBoundaryPath> bounds, out ICollection<EntityObject> entities)
        {
            bounds = new List<HatchBoundaryPath>();
            entities = new List<EntityObject>();

            // TODO: FillMode = pg.FillRule == XFillRule.EvenOdd ? FillMode.Alternate : FillMode.Winding;

            foreach (var pf in pg.Figures)
            {
                var edges = new List<EntityObject>();
                var startPoint = pf.StartPoint;

                foreach (var segment in pf.Segments)
                {
                    if (segment is Core2D.Path.Segments.XArcSegment)
                    {
                        throw new NotSupportedException("Not supported segment type: " + segment.GetType());
                        //var arcSegment = segment as XArcSegment;
                        // TODO: Convert WPF/SVG elliptical arc segment format to DXF ellipse arc.
                        //startPoint = arcSegment.Point;
                    }
                    else if (segment is Core2D.Path.Segments.XCubicBezierSegment)
                    {
                        var cubicBezierSegment = segment as Core2D.Path.Segments.XCubicBezierSegment;
                        var dxfSpline = CreateCubicSpline(
                            startPoint.X + dx,
                            startPoint.Y + dy,
                            cubicBezierSegment.Point1.X + dx,
                            cubicBezierSegment.Point1.Y + dy,
                            cubicBezierSegment.Point2.X + dx,
                            cubicBezierSegment.Point2.Y + dy,
                            cubicBezierSegment.Point3.X + dx,
                            cubicBezierSegment.Point3.Y + dy);
                        edges.Add(dxfSpline);
                        entities.Add((Spline)dxfSpline.Clone());
                        startPoint = cubicBezierSegment.Point3;
                    }
                    else if (segment is Core2D.Path.Segments.XLineSegment)
                    {
                        var lineSegment = segment as Core2D.Path.Segments.XLineSegment;
                        var dxfLine = CreateLine(
                            startPoint.X + dx,
                            startPoint.Y + dy,
                            lineSegment.Point.X + dx,
                            lineSegment.Point.Y + dy);
                        edges.Add(dxfLine);
                        entities.Add((Line)dxfLine.Clone());
                        startPoint = lineSegment.Point;
                    }
                    else if (segment is Core2D.Path.Segments.XPolyCubicBezierSegment)
                    {
                        var polyCubicBezierSegment = segment as Core2D.Path.Segments.XPolyCubicBezierSegment;
                        if (polyCubicBezierSegment.Points.Count >= 3)
                        {
                            var dxfSpline = CreateCubicSpline(
                                startPoint.X + dx,
                                startPoint.Y + dy,
                                polyCubicBezierSegment.Points[0].X + dx,
                                polyCubicBezierSegment.Points[0].Y + dy,
                                polyCubicBezierSegment.Points[1].X + dx,
                                polyCubicBezierSegment.Points[1].Y + dy,
                                polyCubicBezierSegment.Points[2].X + dx,
                                polyCubicBezierSegment.Points[2].Y + dy);
                            edges.Add(dxfSpline);
                            entities.Add((Spline)dxfSpline.Clone());
                        }

                        if (polyCubicBezierSegment.Points.Count > 3
                            && polyCubicBezierSegment.Points.Count % 3 == 0)
                        {
                            for (int i = 3; i < polyCubicBezierSegment.Points.Count; i += 3)
                            {
                                var dxfSpline = CreateCubicSpline(
                                    polyCubicBezierSegment.Points[i - 1].X + dx,
                                    polyCubicBezierSegment.Points[i - 1].Y + dy,
                                    polyCubicBezierSegment.Points[i].X + dx,
                                    polyCubicBezierSegment.Points[i].Y + dy,
                                    polyCubicBezierSegment.Points[i + 1].X + dx,
                                    polyCubicBezierSegment.Points[i + 1].Y + dy,
                                    polyCubicBezierSegment.Points[i + 2].X + dx,
                                    polyCubicBezierSegment.Points[i + 2].Y + dy);
                                edges.Add(dxfSpline);
                                entities.Add((Spline)dxfSpline.Clone());
                            }
                        }

                        startPoint = polyCubicBezierSegment.Points.Last();
                    }
                    else if (segment is Core2D.Path.Segments.XPolyLineSegment)
                    {
                        var polyLineSegment = segment as Core2D.Path.Segments.XPolyLineSegment;
                        if (polyLineSegment.Points.Count >= 1)
                        {
                            var dxfLine = CreateLine(
                                startPoint.X + dx,
                                startPoint.Y + dy,
                                polyLineSegment.Points[0].X + dx,
                                polyLineSegment.Points[0].Y + dy);
                            edges.Add(dxfLine);
                            entities.Add((Line)dxfLine.Clone());
                        }

                        if (polyLineSegment.Points.Count > 1)
                        {
                            for (int i = 1; i < polyLineSegment.Points.Count; i++)
                            {
                                var dxfLine = CreateLine(
                                    polyLineSegment.Points[i - 1].X + dx,
                                    polyLineSegment.Points[i - 1].Y + dy,
                                    polyLineSegment.Points[i].X + dx,
                                    polyLineSegment.Points[i].Y + dy);
                                edges.Add(dxfLine);
                                entities.Add((Line)dxfLine.Clone());
                            }
                        }

                        startPoint = polyLineSegment.Points.Last();
                    }
                    else if (segment is Core2D.Path.Segments.XPolyQuadraticBezierSegment)
                    {
                        var polyQuadraticSegment = segment as Core2D.Path.Segments.XPolyQuadraticBezierSegment;
                        if (polyQuadraticSegment.Points.Count >= 2)
                        {
                            var dxfSpline = CreateQuadraticSpline(
                                startPoint.X + dx,
                                startPoint.Y + dy,
                                polyQuadraticSegment.Points[0].X + dx,
                                polyQuadraticSegment.Points[0].Y + dy,
                                polyQuadraticSegment.Points[1].X + dx,
                                polyQuadraticSegment.Points[1].Y + dy);
                            edges.Add(dxfSpline);
                            entities.Add((Spline)dxfSpline.Clone());
                        }

                        if (polyQuadraticSegment.Points.Count > 2
                            && polyQuadraticSegment.Points.Count % 2 == 0)
                        {
                            for (int i = 3; i < polyQuadraticSegment.Points.Count; i += 3)
                            {
                                var dxfSpline = CreateQuadraticSpline(
                                    polyQuadraticSegment.Points[i - 1].X + dx,
                                    polyQuadraticSegment.Points[i - 1].Y + dy,
                                    polyQuadraticSegment.Points[i].X + dx,
                                    polyQuadraticSegment.Points[i].Y + dy,
                                    polyQuadraticSegment.Points[i + 1].X + dx,
                                    polyQuadraticSegment.Points[i + 1].Y + dy);
                                edges.Add(dxfSpline);
                                entities.Add((Spline)dxfSpline.Clone());
                            }
                        }

                        startPoint = polyQuadraticSegment.Points.Last();
                    }
                    else if (segment is Core2D.Path.Segments.XQuadraticBezierSegment)
                    {
                        var quadraticBezierSegment = segment as Core2D.Path.Segments.XQuadraticBezierSegment;
                        var dxfSpline = CreateQuadraticSpline(
                            startPoint.X + dx,
                            startPoint.Y + dy,
                            quadraticBezierSegment.Point1.X + dx,
                            quadraticBezierSegment.Point1.Y + dy,
                            quadraticBezierSegment.Point2.X + dx,
                            quadraticBezierSegment.Point2.Y + dy);
                        edges.Add(dxfSpline);
                        entities.Add((Spline)dxfSpline.Clone());
                        startPoint = quadraticBezierSegment.Point2;
                    }
                    else
                    {
                        throw new NotSupportedException("Not supported segment type: " + segment.GetType());
                    }
                }

                // TODO: Add support for pf.IsClosed

                var path = new HatchBoundaryPath(edges);
                bounds.Add(path);
            }
        }

        /// <inheritdoc/>
        public override void ClearCache(bool isZooming)
        {
            if (!isZooming)
            {
                _biCache.Reset();
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Project.XContainer container, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            var dxf = dc as DxfDocument;

            foreach (var layer in container.Layers)
            {
                var dxfLayer = new Layer(layer.Name)
                {
                    IsVisible = layer.IsVisible
                };

                dxf.Layers.Add(dxfLayer);

                _currentLayer = dxfLayer;

                Draw(dc, layer, db, r);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Project.XLayer layer, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            var dxf = dc as DxfDocument;

            foreach (var shape in layer.Shapes)
            {
                if (shape.State.Flags.HasFlag(State.DrawShapeState.Flags))
                {
                    shape.Draw(dxf, this, 0, 0, db, r);
                }
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XLine line, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!line.IsStroked)
                return;

            var dxf = dc as DxfDocument;

            double _x1 = line.Start.X + dx;
            double _y1 = line.Start.Y + dy;
            double _x2 = line.End.X + dx;
            double _y2 = line.End.Y + dy;

            Core2D.Shapes.XLine.SetMaxLength(line, ref _x1, ref _y1, ref _x2, ref _y2);

            // TODO: Draw line start arrow.

            // TODO: Draw line end arrow.

            DrawLineInternal(dxf, _currentLayer, line.Style, line.IsStroked, _x1, _y1, _x2, _y2);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XRectangle rectangle, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!rectangle.IsStroked && !rectangle.IsFilled && !rectangle.IsGrid)
                return;

            var dxf = dc as DxfDocument;
            var style = rectangle.Style;
            var rect = Core2D.Math.Rect2.Create(rectangle.TopLeft, rectangle.BottomRight, dx, dy);

            DrawRectangleInternal(dxf, _currentLayer, rectangle.IsFilled, rectangle.IsStroked, style, ref rect);

            if (rectangle.IsGrid)
            {
                DrawGridInternal(
                    dxf,
                    _currentLayer,
                    style,
                    rectangle.OffsetX, rectangle.OffsetY,
                    rectangle.CellWidth, rectangle.CellHeight,
                    ref rect);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XEllipse ellipse, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!ellipse.IsStroked && !ellipse.IsFilled)
                return;

            var dxf = dc as DxfDocument;
            var style = ellipse.Style;
            var rect = Core2D.Math.Rect2.Create(ellipse.TopLeft, ellipse.BottomRight, dx, dy);

            DrawEllipseInternal(dxf, _currentLayer, ellipse.IsFilled, ellipse.IsStroked, style, ref rect);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XArc arc, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            var dxf = dc as DxfDocument;
            var style = arc.Style;

            var dxfEllipse = CreateEllipticalArc(arc, dx, dy);

            if (arc.IsFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                // TODO: The netDxf does not create hatch for Ellipse with end angle equal to 360.
                var bounds =
                    new List<HatchBoundaryPath>
                    {
                        new HatchBoundaryPath(
                            new List<EntityObject>
                            {
                                (Ellipse)dxfEllipse.Clone()
                            })
                    };

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = _currentLayer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (arc.IsStroked)
            {
                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                dxfEllipse.Layer = _currentLayer;
                dxfEllipse.Color = stroke;
                dxfEllipse.Transparency.Value = strokeTansparency;
                dxfEllipse.Lineweight.Value = lineweight;

                dxf.AddEntity(dxfEllipse);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XCubicBezier cubicBezier, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!cubicBezier.IsStroked && !cubicBezier.IsFilled)
                return;

            var dxf = dc as DxfDocument;
            var style = cubicBezier.Style;

            var dxfSpline = CreateCubicSpline(
                cubicBezier.Point1.X + dx,
                cubicBezier.Point1.Y + dy,
                cubicBezier.Point2.X + dx,
                cubicBezier.Point2.Y + dy,
                cubicBezier.Point3.X + dx,
                cubicBezier.Point3.Y + dy,
                cubicBezier.Point4.X + dx,
                cubicBezier.Point4.Y + dy);

            if (cubicBezier.IsFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                var bounds =
                    new List<HatchBoundaryPath>
                    {
                        new HatchBoundaryPath(
                            new List<EntityObject>
                            {
                                (Spline)dxfSpline.Clone()
                            })
                    };

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = _currentLayer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (cubicBezier.IsStroked)
            {
                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                dxfSpline.Layer = _currentLayer;
                dxfSpline.Color = stroke;
                dxfSpline.Transparency.Value = strokeTansparency;
                dxfSpline.Lineweight.Value = lineweight;

                dxf.AddEntity(dxfSpline);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XQuadraticBezier quadraticBezier, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!quadraticBezier.IsStroked && !quadraticBezier.IsFilled)
                return;

            var dxf = dc as DxfDocument;
            var style = quadraticBezier.Style;

            var dxfSpline = CreateQuadraticSpline(
                quadraticBezier.Point1.X + dx,
                quadraticBezier.Point1.Y + dy,
                quadraticBezier.Point2.X + dx,
                quadraticBezier.Point2.Y + dy,
                quadraticBezier.Point3.X + dx,
                quadraticBezier.Point3.Y + dy);

            if (quadraticBezier.IsFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                var bounds =
                    new List<HatchBoundaryPath>
                    {
                        new HatchBoundaryPath(
                            new List<EntityObject>
                            {
                                (Spline)dxfSpline.Clone()
                            })
                    };

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = _currentLayer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (quadraticBezier.IsStroked)
            {
                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                dxfSpline.Layer = _currentLayer;
                dxfSpline.Color = stroke;
                dxfSpline.Transparency.Value = strokeTansparency;
                dxfSpline.Lineweight.Value = lineweight;

                dxf.AddEntity(dxfSpline);
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XText text, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            var dxf = dc as DxfDocument;

            var tbind = text.BindText(db, r);
            if (string.IsNullOrEmpty(tbind))
                return;

            var style = text.Style;
            var stroke = ToColor(style.Stroke);
            var strokeTansparency = ToTransparency(style.Stroke);

            var attachmentPoint = default(MTextAttachmentPoint);
            double x, y;
            var rect = Core2D.Math.Rect2.Create(text.TopLeft, text.BottomRight, dx, dy);

            switch (text.Style.TextStyle.TextHAlignment)
            {
                default:
                case Core2D.Style.TextHAlignment.Left:
                    x = rect.X;
                    break;
                case Core2D.Style.TextHAlignment.Center:
                    x = rect.X + rect.Width / 2.0;
                    break;
                case Core2D.Style.TextHAlignment.Right:
                    x = rect.X + rect.Width;
                    break;
            }

            switch (text.Style.TextStyle.TextVAlignment)
            {
                default:
                case Core2D.Style.TextVAlignment.Top:
                    y = rect.Y;
                    break;
                case Core2D.Style.TextVAlignment.Center:
                    y = rect.Y + rect.Height / 2.0;
                    break;
                case Core2D.Style.TextVAlignment.Bottom:
                    y = rect.Y + rect.Height;
                    break;
            }

            switch (text.Style.TextStyle.TextVAlignment)
            {
                default:
                case Core2D.Style.TextVAlignment.Top:
                    switch (text.Style.TextStyle.TextHAlignment)
                    {
                        default:
                        case Core2D.Style.TextHAlignment.Left:
                            attachmentPoint = MTextAttachmentPoint.TopLeft;
                            break;
                        case Core2D.Style.TextHAlignment.Center:
                            attachmentPoint = MTextAttachmentPoint.TopCenter;
                            break;
                        case Core2D.Style.TextHAlignment.Right:
                            attachmentPoint = MTextAttachmentPoint.TopRight;
                            break;
                    }
                    break;
                case Core2D.Style.TextVAlignment.Center:
                    switch (text.Style.TextStyle.TextHAlignment)
                    {
                        default:
                        case Core2D.Style.TextHAlignment.Left:
                            attachmentPoint = MTextAttachmentPoint.MiddleLeft;
                            break;
                        case Core2D.Style.TextHAlignment.Center:
                            attachmentPoint = MTextAttachmentPoint.MiddleCenter;
                            break;
                        case Core2D.Style.TextHAlignment.Right:
                            attachmentPoint = MTextAttachmentPoint.MiddleRight;
                            break;
                    }
                    break;
                case Core2D.Style.TextVAlignment.Bottom:
                    switch (text.Style.TextStyle.TextHAlignment)
                    {
                        default:
                        case Core2D.Style.TextHAlignment.Left:
                            attachmentPoint = MTextAttachmentPoint.BottomLeft;
                            break;
                        case Core2D.Style.TextHAlignment.Center:
                            attachmentPoint = MTextAttachmentPoint.BottomCenter;
                            break;
                        case Core2D.Style.TextHAlignment.Right:
                            attachmentPoint = MTextAttachmentPoint.BottomRight;
                            break;
                    }
                    break;
            }

            var ts = new TextStyle(style.TextStyle.FontName, style.TextStyle.FontFile);
            var dxfMText = new MText(
                new Vector3(ToDxfX(x), ToDxfY(y), 0),
                text.Style.TextStyle.FontSize * 72.0 / 96.0,
                rect.Width,
                ts);
            dxfMText.AttachmentPoint = attachmentPoint;

            var options = new MTextFormattingOptions(dxfMText.Style);
            var fs = text.Style.TextStyle.FontStyle;
            if (fs != null)
            {
                options.Bold = fs.Flags.HasFlag(Core2D.Style.FontStyleFlags.Bold);
                options.Italic = fs.Flags.HasFlag(Core2D.Style.FontStyleFlags.Italic);
                options.Underline = fs.Flags.HasFlag(Core2D.Style.FontStyleFlags.Underline);
                options.StrikeThrough = fs.Flags.HasFlag(Core2D.Style.FontStyleFlags.Strikeout);
            }

            options.Aligment = MTextFormattingOptions.TextAligment.Default;
            options.Color = null;
            dxfMText.Write(tbind, options);

            dxfMText.Layer = _currentLayer;
            dxfMText.Transparency.Value = strokeTansparency;
            dxfMText.Color = stroke;

            dxf.AddEntity(dxfMText);
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XImage image, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            var dxf = dc as DxfDocument;

            var bytes = State.ImageCache.GetImage(image.Key);
            if (bytes != null)
            {
                var rect = Core2D.Math.Rect2.Create(image.TopLeft, image.BottomRight, dx, dy);

                var dxfImageDefinitionCached = _biCache.Get(image.Key);
                if (dxfImageDefinitionCached != null)
                {
                    var dxfImage = new Image(
                        dxfImageDefinitionCached,
                        new Vector3(ToDxfX(rect.X), ToDxfY(rect.Y + rect.Height), 0),
                        rect.Width,
                        rect.Height);
                    dxf.AddEntity(dxfImage);
                }
                else
                {
                    if (State.ImageCache == null || string.IsNullOrEmpty(image.Key))
                        return;

                    var path = System.IO.Path.Combine(_outputPath, System.IO.Path.GetFileName(image.Key));
                    System.IO.File.WriteAllBytes(path, bytes);
                    var dxfImageDefinition = new ImageDefinition(path);

                    _biCache.Set(image.Key, dxfImageDefinition);

                    var dxfImage = new Image(
                        dxfImageDefinition,
                        new Vector3(ToDxfX(rect.X), ToDxfY(rect.Y + rect.Height), 0),
                        rect.Width,
                        rect.Height);
                    dxf.AddEntity(dxfImage);
                }
            }
        }

        /// <inheritdoc/>
        public override void Draw(object dc, Core2D.Shapes.XPath path, double dx, double dy, ImmutableArray<Core2D.Data.XProperty> db, Core2D.Data.Database.XRecord r)
        {
            if (!path.IsStroked && !path.IsFilled)
                return;

            var dxf = dc as DxfDocument;
            var style = path.Style;

            IList<HatchBoundaryPath> bounds;
            ICollection<EntityObject> entities;
            CreateHatchBoundsAndEntitiess(path.Geometry, dx, dy, out bounds, out entities);
            if (entities == null || bounds == null)
                return;

            if (path.IsFilled)
            {
                var fill = ToColor(style.Fill);
                var fillTransparency = ToTransparency(style.Fill);

                var hatch = new Hatch(HatchPattern.Solid, bounds, false);
                hatch.Layer = _currentLayer;
                hatch.Color = fill;
                hatch.Transparency.Value = fillTransparency;

                dxf.AddEntity(hatch);
            }

            if (path.IsStroked)
            {
                // TODO: Add support for Closed paths.

                var stroke = ToColor(style.Stroke);
                var strokeTansparency = ToTransparency(style.Stroke);
                var lineweight = ToLineweight(style.Thickness);

                foreach (var entity in entities)
                {
                    entity.Layer = _currentLayer;
                    entity.Color = stroke;
                    entity.Transparency.Value = strokeTansparency;
                    entity.Lineweight.Value = lineweight;
                    dxf.AddEntity(entity);
                }
            }
        }
    }
}
