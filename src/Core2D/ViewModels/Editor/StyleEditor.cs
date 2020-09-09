﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Core2D.Containers;
using Core2D.History;
using Core2D.Renderer;
using Core2D.Shapes;
using Core2D.Style;

namespace Core2D.Editor
{
    /// <summary>
    /// Style editor.
    /// </summary>
    public class StyleEditor : ObservableObject, IStyleEditor
    {
        private const NumberStyles _numberStyles = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;
        private readonly IServiceProvider _serviceProvider;
        private IShapeStyle _shapeStyleCopy;
        private IColor _strokeCopy;
        private IColor _fillCopy;
        private ITextStyle _textStyleCopy;
        private IArrowStyle _endArrowStyleCopy;
        private IArrowStyle _startArrowStyleCopy;
        private ILineStyle _lineStyleCopy;

        /// <summary>
        /// Initialize new instance of <see cref="StyleEditor"/> class.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        public StyleEditor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void OnCopyStyle()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var style = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style;
                _shapeStyleCopy = (IShapeStyle)style?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteStyle()
        {
            if (_shapeStyleCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var previous = shape.Style;
                var next = (IShapeStyle)_shapeStyleCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => shape.Style = p);
                shape.Style = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyStroke()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var stroke = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.Stroke;
                _strokeCopy = (IColor)stroke?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteStroke()
        {
            if (_strokeCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.Stroke;
                var next = (IColor)_strokeCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.Stroke = p);
                style.Stroke = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyFill()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var fill = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.Fill;
                _fillCopy = (IColor)fill?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteFill()
        {
            if (_fillCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.Fill;
                var next = (IColor)_fillCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.Fill = p);
                style.Fill = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyLineStyle()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var lineStyle = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.LineStyle;
                _lineStyleCopy = (ILineStyle)lineStyle?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteLineStyle()
        {
            if (_lineStyleCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.LineStyle;
                var next = (ILineStyle)_lineStyleCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.LineStyle = p);
                style.LineStyle = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyStartArrowStyle()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var startArrowStyle = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.StartArrowStyle;
                _startArrowStyleCopy = (IArrowStyle)startArrowStyle?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteStartArrowStyle()
        {
            if (_startArrowStyleCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.StartArrowStyle;
                var next = (IArrowStyle)_startArrowStyleCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.StartArrowStyle = p);
                style.StartArrowStyle = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyEndArrowStyle()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var endArrowStyle = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.EndArrowStyle;
                _endArrowStyleCopy = (IArrowStyle)endArrowStyle?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteEndArrowStyle()
        {
            if (_endArrowStyleCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.EndArrowStyle;
                var next = (IArrowStyle)_endArrowStyleCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.EndArrowStyle = p);
                style.EndArrowStyle = next;
            }
        }

        /// <inheritdoc/>
        public void OnCopyTextStyle()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();

            if (editor.PageState?.SelectedShapes != null)
            {
                var textStyle = editor.PageState?.SelectedShapes.FirstOrDefault()?.Style?.TextStyle;
                _textStyleCopy = (ITextStyle)textStyle?.Copy(null);
            }
        }

        /// <inheritdoc/>
        public void OnPasteTextStyle()
        {
            if (_textStyleCopy == null)
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();

            foreach (var shape in GetShapes(editor))
            {
                var style = shape.Style;

                var previous = style.TextStyle;
                var next = (ITextStyle)_textStyleCopy?.Copy(null);
                editor.Project?.History?.Snapshot(previous, next, (p) => style.TextStyle = p);
                style.TextStyle = next;
            }
        }

        private void SetThickness(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.Thickness;
                var next = value;
                history?.Snapshot(previous, next, (p) => style.Thickness = p);
                style.Thickness = next;
            }
        }

        private void SetLineCap(IBaseShape shape, LineCap value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.LineCap;
                var next = value;
                history?.Snapshot(previous, next, (p) => style.LineCap = p);
                style.LineCap = next;
            }
        }

        private void SetDashes(IBaseShape shape, string value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.Dashes;
                var next = value;
                history?.Snapshot(previous, next, (p) => style.Dashes = p);
                style.Dashes = next;
            }
        }

        private void SetDashOffset(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.DashOffset;
                var next = value;
                history?.Snapshot(previous, next, (p) => style.DashOffset = p);
                style.DashOffset = next;
            }
        }

        private void SetStroke(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.Stroke;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => style.Stroke = p);
                style.Stroke = next;
            }
        }

        private void SetStrokeTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                if (style.Stroke is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private void SetFill(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                var previous = style.Fill;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => style.Fill = p);
                style.Fill = next;
            }
        }

        private void SetFillTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null)
            {
                if (style.Fill is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private void SetFontName(IBaseShape shape, string value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.TextStyle != null)
            {
                var textStyle = style.TextStyle;

                var previous = textStyle.FontName;
                var next = value;
                history?.Snapshot(previous, next, (p) => textStyle.FontName = p);
                textStyle.FontName = next;
            }
        }

        private void SetFontStyle(IBaseShape shape, FontStyleFlags value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.TextStyle != null && style.TextStyle.FontStyle != null)
            {
                var fontStyle = style.TextStyle.FontStyle;

                var previous = fontStyle.Flags;
                var next = fontStyle.Flags ^ value;
                history?.Snapshot(previous, next, (p) => fontStyle.Flags = p);
                fontStyle.Flags = next;
            }
        }

        private void SetFontSize(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.TextStyle != null)
            {
                var textStyle = style.TextStyle;

                var previous = textStyle.FontSize;
                var next = value;
                history?.Snapshot(previous, next, (p) => textStyle.FontSize = p);
                textStyle.FontSize = next;
            }
        }

        private void SetTextHAlignment(IBaseShape shape, TextHAlignment value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.TextStyle != null)
            {
                var textStyle = style.TextStyle;

                var previous = textStyle.TextHAlignment;
                var next = value;
                history?.Snapshot(previous, next, (p) => textStyle.TextHAlignment = p);
                textStyle.TextHAlignment = next;
            }
        }

        private void SetTextVAlignment(IBaseShape shape, TextVAlignment value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.TextStyle != null)
            {
                var textStyle = style.TextStyle;

                var previous = textStyle.TextVAlignment;
                var next = value;
                history?.Snapshot(previous, next, (p) => textStyle.TextVAlignment = p);
                textStyle.TextVAlignment = next;
            }
        }

        private void ToggleLineIsCurved(IBaseShape shape, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null)
            {
                var lineStyle = style.LineStyle;

                var previous = lineStyle.IsCurved;
                var next = !lineStyle.IsCurved;
                history?.Snapshot(previous, next, (p) => lineStyle.IsCurved = p);
                lineStyle.IsCurved = next;
            }
        }

        private void SetLineCurvature(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null)
            {
                var lineStyle = style.LineStyle;

                var previous = lineStyle.Curvature;
                var next = value;
                history?.Snapshot(previous, next, (p) => lineStyle.Curvature = p);
                lineStyle.Curvature = next;
            }
        }

        private void SetLineCurveOrientation(IBaseShape shape, CurveOrientation value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null)
            {
                var lineStyle = style.LineStyle;

                var previous = lineStyle.CurveOrientation;
                var next = value;
                history?.Snapshot(previous, next, (p) => lineStyle.CurveOrientation = p);
                lineStyle.CurveOrientation = next;
            }
        }

        private void SetLineFixedLength(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null && style.LineStyle.FixedLength != null)
            {
                var fixedLength = style.LineStyle.FixedLength;

                var previous = fixedLength.Length;
                var next = value;
                history?.Snapshot(previous, next, (p) => fixedLength.Length = p);
                fixedLength.Length = next;
            }
        }

        private void ToggleLineFixedLengthFlags(IBaseShape shape, LineFixedLengthFlags value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null && style.LineStyle.FixedLength != null)
            {
                var fixedLength = style.LineStyle.FixedLength;

                var previous = fixedLength.Flags;
                var next = fixedLength.Flags ^ value;
                history?.Snapshot(previous, next, (p) => fixedLength.Flags = p);
                fixedLength.Flags = next;
            }
        }

        private void ToggleLineFixedLengthStartTrigger(IBaseShape shape, ShapeStateFlags value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null && style.LineStyle.FixedLength != null && style.LineStyle.FixedLength.StartTrigger != null)
            {
                var startTrigger = style.LineStyle.FixedLength.StartTrigger;

                var previous = startTrigger.Flags;
                var next = startTrigger.Flags ^ value;
                history?.Snapshot(previous, next, (p) => startTrigger.Flags = p);
                startTrigger.Flags = next;
            }
        }

        private void ToggleLineFixedLengthEndTrigger(IBaseShape shape, ShapeStateFlags value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.LineStyle != null && style.LineStyle.FixedLength != null && style.LineStyle.FixedLength.EndTrigger != null)
            {
                var endTrigger = style.LineStyle.FixedLength.EndTrigger;

                var previous = endTrigger.Flags;
                var next = endTrigger.Flags ^ value;
                history?.Snapshot(previous, next, (p) => endTrigger.Flags = p);
                endTrigger.Flags = next;
            }
        }

        private void SetStartArrowType(IBaseShape shape, ArrowType value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.ArrowType;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.ArrowType = p);
                startArrowStyle.ArrowType = next;
            }
        }

        private void ToggleStartArrowIsStroked(IBaseShape shape, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.IsStroked;
                var next = !startArrowStyle.IsStroked;
                history?.Snapshot(previous, next, (p) => startArrowStyle.IsStroked = p);
                startArrowStyle.IsStroked = next;
            }
        }

        private void ToggleStartArrowIsFilled(IBaseShape shape, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.IsFilled;
                var next = !startArrowStyle.IsFilled;
                history?.Snapshot(previous, next, (p) => startArrowStyle.IsFilled = p);
                startArrowStyle.IsFilled = next;
            }
        }

        private void SetStartArrowRadiusX(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.RadiusX;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.RadiusX = p);
                startArrowStyle.RadiusX = next;
            }
        }

        private void SetStartArrowRadiusY(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.RadiusY;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.RadiusY = p);
                startArrowStyle.RadiusY = next;
            }
        }

        private void SetStartArrowThickness(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.Thickness;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.Thickness = p);
                startArrowStyle.Thickness = next;
            }
        }

        private void SetStartArrowLineCap(IBaseShape shape, LineCap value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.LineCap;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.LineCap = p);
                startArrowStyle.LineCap = next;
            }
        }

        private void SetStartArrowDashes(IBaseShape shape, string value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.Dashes;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.Dashes = p);
                startArrowStyle.Dashes = next;
            }
        }

        private void SetStartArrowDashOffset(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.DashOffset;
                var next = value;
                history?.Snapshot(previous, next, (p) => startArrowStyle.DashOffset = p);
                startArrowStyle.DashOffset = next;
            }
        }

        private void SetStartArrowStroke(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.Stroke;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => startArrowStyle.Stroke = p);
                startArrowStyle.Stroke = next;
            }
        }

        private void SetStartArrowStrokeTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                if (style.StartArrowStyle.Stroke is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private void SetStartArrowFill(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                var startArrowStyle = style.StartArrowStyle;

                var previous = startArrowStyle.Fill;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => startArrowStyle.Fill = p);
                startArrowStyle.Fill = next;
            }
        }

        private void SetStartArrowFillTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.StartArrowStyle != null)
            {
                if (style.StartArrowStyle.Fill is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private void SetEndArrowType(IBaseShape shape, ArrowType value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.ArrowType;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.ArrowType = p);
                endArrowStyle.ArrowType = next;
            }
        }

        private void ToggleEndArrowIsStroked(IBaseShape shape, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.IsStroked;
                var next = !endArrowStyle.IsStroked;
                history?.Snapshot(previous, next, (p) => endArrowStyle.IsStroked = p);
                endArrowStyle.IsStroked = next;
            }
        }

        private void ToggleEndArrowIsFilled(IBaseShape shape, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.IsFilled;
                var next = !endArrowStyle.IsFilled;
                history?.Snapshot(previous, next, (p) => endArrowStyle.IsFilled = p);
                endArrowStyle.IsFilled = next;
            }
        }

        private void SetEndArrowRadiusX(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.RadiusX;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.RadiusX = p);
                endArrowStyle.RadiusX = next;
            }
        }

        private void SetEndArrowRadiusY(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.RadiusY;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.RadiusY = p);
                endArrowStyle.RadiusY = next;
            }
        }

        private void SetEndArrowThickness(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.Thickness;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.Thickness = p);
                endArrowStyle.Thickness = next;
            }
        }

        private void SetEndArrowLineCap(IBaseShape shape, LineCap value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.LineCap;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.LineCap = p);
                endArrowStyle.LineCap = next;
            }
        }

        private void SetEndArrowDashes(IBaseShape shape, string value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.Dashes;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.Dashes = p);
                endArrowStyle.Dashes = next;
            }
        }

        private void SetEndArrowDashOffset(IBaseShape shape, double value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.DashOffset;
                var next = value;
                history?.Snapshot(previous, next, (p) => endArrowStyle.DashOffset = p);
                endArrowStyle.DashOffset = next;
            }
        }

        private void SetEndArrowStroke(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.Stroke;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => endArrowStyle.Stroke = p);
                endArrowStyle.Stroke = next;
            }
        }

        private void SetEndArrowStrokeTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                if (style.EndArrowStyle.Stroke is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private void SetEndArrowFill(IBaseShape shape, IColor value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                var endArrowStyle = style.EndArrowStyle;

                var previous = endArrowStyle.Fill;
                var next = (IColor)value.Copy(null);
                history?.Snapshot(previous, next, (p) => endArrowStyle.Fill = p);
                endArrowStyle.Fill = next;
            }
        }

        private void SetEndArrowFillTransparency(IBaseShape shape, byte value, IHistory history)
        {
            var style = shape.Style;
            if (style != null && style.EndArrowStyle != null)
            {
                if (style.EndArrowStyle.Fill is IArgbColor argbColor)
                {
                    var previous = argbColor.A;
                    var next = value;
                    history?.Snapshot(previous, next, (p) => argbColor.A = p);
                    argbColor.A = next;
                }
            }
        }

        private IEnumerable<IBaseShape> GetShapes(IProjectEditor editor)
        {
            if (editor.PageState?.SelectedShapes?.Count > 0)
            {
                foreach (var shape in editor.PageState.SelectedShapes)
                {
                    if (shape is IGroupShape group)
                    {
                        var groupShapes = ProjectContainer.GetAllShapes(group.Shapes);
                        foreach (var child in groupShapes)
                        {
                            yield return child;
                        }
                    }
                    else
                    {
                        yield return shape;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetThickness(string thickness)
        {
            if (!double.TryParse(thickness, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetThickness(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetLineCap(string lineCap)
        {
            if (!Enum.TryParse<LineCap>(lineCap, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetLineCap(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetDashes(string dashes)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetDashes(shape, dashes, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetDashOffset(string dashOffset)
        {
            if (!double.TryParse(dashOffset, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetDashOffset(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetStroke(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStroke(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetStrokeTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStrokeTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetFill(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetFill(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetFillTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetFillTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetFontName(string fontName)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetFontName(shape, fontName, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetFontSize(string fontSize)
        {
            if (!double.TryParse(fontSize, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetFontSize(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetFontStyle(string fontStyle)
        {
            if (!Enum.TryParse<FontStyleFlags>(fontStyle, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetFontStyle(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetTextHAlignment(string alignment)
        {
            if (!Enum.TryParse<TextHAlignment>(alignment, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetTextHAlignment(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetTextVAlignment(string alignment)
        {
            if (!Enum.TryParse<TextVAlignment>(alignment, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetTextVAlignment(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleLineIsCurved()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleLineIsCurved(shape, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetLineCurvature(string curvature)
        {
            if (!double.TryParse(curvature, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetLineCurvature(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetLineCurveOrientation(string curveOrientation)
        {
            if (!Enum.TryParse<CurveOrientation>(curveOrientation, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetLineCurveOrientation(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetLineFixedLength(string length)
        {
            if (!double.TryParse(length, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetLineFixedLength(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleLineFixedLengthFlags(string flags)
        {
            if (!Enum.TryParse<LineFixedLengthFlags>(flags, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleLineFixedLengthFlags(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleLineFixedLengthStartTrigger(string trigger)
        {
            if (!Enum.TryParse<ShapeStateFlags>(trigger, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleLineFixedLengthStartTrigger(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleLineFixedLengthEndTrigger(string trigger)
        {
            if (!Enum.TryParse<ShapeStateFlags>(trigger, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleLineFixedLengthEndTrigger(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowType(string type)
        {
            if (!Enum.TryParse<ArrowType>(type, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowType(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleStartArrowIsStroked()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleStartArrowIsStroked(shape, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleToggleStartArrowIsFilled()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleStartArrowIsFilled(shape, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowRadiusX(string radius)
        {
            if (!double.TryParse(radius, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowRadiusX(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowRadiusY(string radius)
        {
            if (!double.TryParse(radius, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowRadiusY(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowThickness(string thickness)
        {
            if (!double.TryParse(thickness, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowThickness(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowLineCap(string lineCap)
        {
            if (!Enum.TryParse<LineCap>(lineCap, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowLineCap(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowDashes(string dashes)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowDashes(shape, dashes, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowDashOffset(string dashOffset)
        {
            if (!double.TryParse(dashOffset, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowDashOffset(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowStroke(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowStroke(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowStrokeTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowStrokeTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowFill(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowFill(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetStartArrowFillTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetStartArrowFillTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowType(string type)
        {
            if (!Enum.TryParse<ArrowType>(type, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowType(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleToggleEndArrowIsStroked()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleEndArrowIsStroked(shape, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleToggleEndArrowIsFilled()
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                ToggleEndArrowIsFilled(shape, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowRadiusX(string radius)
        {
            if (!double.TryParse(radius, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowRadiusX(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowRadiusY(string radius)
        {
            if (!double.TryParse(radius, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowRadiusY(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowThickness(string thickness)
        {
            if (!double.TryParse(thickness, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowThickness(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowLineCap(string lineCap)
        {
            if (!Enum.TryParse<LineCap>(lineCap, true, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowLineCap(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowDashes(string dashes)
        {
            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowDashes(shape, dashes, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowDashOffset(string dashOffset)
        {
            if (!double.TryParse(dashOffset, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowDashOffset(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowStroke(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowStroke(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowStrokeTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowStrokeTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowFill(string color)
        {
            IColor value;
            try
            {
                value = ArgbColor.Parse(color);
            }
            catch
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowFill(shape, value, history);
            }
        }

        /// <inheritdoc/>
        public void OnStyleSetEndArrowFillTransparency(string alpha)
        {
            if (!byte.TryParse(alpha, _numberStyles, CultureInfo.InvariantCulture, out var value))
            {
                return;
            }

            var editor = _serviceProvider.GetService<IProjectEditor>();
            var history = editor.Project?.History;

            foreach (var shape in GetShapes(editor))
            {
                SetEndArrowFillTransparency(shape, value, history);
            }

            editor.Project?.CurrentContainer?.InvalidateLayer();
        }
    }
}
