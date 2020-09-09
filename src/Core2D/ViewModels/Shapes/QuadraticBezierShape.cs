﻿using System;
using System.Collections.Generic;
using Core2D.Data;
using Core2D.Renderer;

namespace Core2D.Shapes
{
    /// <summary>
    /// Quadratic bezier shape.
    /// </summary>
    public class QuadraticBezierShape : BaseShape, IQuadraticBezierShape
    {
        private IPointShape _point1;
        private IPointShape _point2;
        private IPointShape _point3;

        /// <inheritdoc/>
        public override Type TargetType => typeof(IQuadraticBezierShape);

        /// <inheritdoc/>
        public IPointShape Point1
        {
            get => _point1;
            set => Update(ref _point1, value);
        }

        /// <inheritdoc/>
        public IPointShape Point2
        {
            get => _point2;
            set => Update(ref _point2, value);
        }

        /// <inheritdoc/>
        public IPointShape Point3
        {
            get => _point3;
            set => Update(ref _point3, value);
        }

        /// <inheritdoc/>
        public override void DrawShape(object dc, IShapeRenderer renderer)
        {
            if (State.Flags.HasFlag(ShapeStateFlags.Visible))
            {
                renderer.DrawQuadraticBezier(dc, this);
            }
        }

        /// <inheritdoc/>
        public override void DrawPoints(object dc, IShapeRenderer renderer)
        {
            if (renderer.State.SelectedShapes != null)
            {
                if (renderer.State.SelectedShapes.Contains(this))
                {
                    _point1.DrawShape(dc, renderer);
                    _point2.DrawShape(dc, renderer);
                    _point3.DrawShape(dc, renderer);
                }
                else
                {
                    if (renderer.State.SelectedShapes.Contains(_point1))
                    {
                        _point1.DrawShape(dc, renderer);
                    }

                    if (renderer.State.SelectedShapes.Contains(_point2))
                    {
                        _point2.DrawShape(dc, renderer);
                    }

                    if (renderer.State.SelectedShapes.Contains(_point3))
                    {
                        _point3.DrawShape(dc, renderer);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public override void Bind(IDataFlow dataFlow, object db, object r)
        {
            var record = Data?.Record ?? r;

            dataFlow.Bind(this, db, record);

            _point1.Bind(dataFlow, db, record);
            _point2.Bind(dataFlow, db, record);
            _point3.Bind(dataFlow, db, record);
        }

        /// <inheritdoc/>
        public override void Move(ISelection? selection, decimal dx, decimal dy)
        {
            if (!Point1.State.Flags.HasFlag(ShapeStateFlags.Connector))
            {
                Point1.Move(selection, dx, dy);
            }

            if (!Point2.State.Flags.HasFlag(ShapeStateFlags.Connector))
            {
                Point2.Move(selection, dx, dy);
            }

            if (!Point3.State.Flags.HasFlag(ShapeStateFlags.Connector))
            {
                Point3.Move(selection, dx, dy);
            }
        }

        /// <inheritdoc/>
        public override void Select(ISelection selection)
        {
            base.Select(selection);
            Point1.Select(selection);
            Point2.Select(selection);
            Point3.Select(selection);
        }

        /// <inheritdoc/>
        public override void Deselect(ISelection selection)
        {
            base.Deselect(selection);
            Point1.Deselect(selection);
            Point2.Deselect(selection);
            Point3.Deselect(selection);
        }

        /// <inheritdoc/>
        public override void GetPoints(IList<IPointShape> points)
        {
            points.Add(Point1);
            points.Add(Point2);
            points.Add(Point3);
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

            isDirty |= Point1.IsDirty();
            isDirty |= Point2.IsDirty();
            isDirty |= Point3.IsDirty();

            return isDirty;
        }

        /// <inheritdoc/>
        public override void Invalidate()
        {
            base.Invalidate();
            Point1.Invalidate();
            Point2.Invalidate();
            Point3.Invalidate();
        }

        /// <summary>
        /// Check whether the <see cref="Point1"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint1() => _point1 != null;

        /// <summary>
        /// Check whether the <see cref="Point2"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint2() => _point2 != null;

        /// <summary>
        /// Check whether the <see cref="Point3"/> property has changed from its default value.
        /// </summary>
        /// <returns>Returns true if the property has changed; otherwise, returns false.</returns>
        public virtual bool ShouldSerializePoint3() => _point3 != null;
    }
}
