﻿#nullable disable
using static System.Math;

namespace Core2D.ViewModels.Shapes
{
    public static class PointShapeExtentions
    {
        public static double DistanceTo(this PointShapeViewModel point, PointShapeViewModel other)
        {
            double dx = point.X - other.X;
            double dy = point.Y - other.Y;
            return Sqrt(dx * dx + dy * dy);
        }
    }
}
