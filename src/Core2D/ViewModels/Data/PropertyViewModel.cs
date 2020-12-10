﻿using System.Collections.Generic;

namespace Core2D.Data
{
    public partial class PropertyViewModel : ViewModelBase
    {
        [AutoNotify] private string _value;

        public override object Copy(IDictionary<object, object> shared)
        {
            return new PropertyViewModel()
            {
                Name = Name,
                Value = Value
            };
        }

        public override bool IsDirty()
        {
            var isDirty = base.IsDirty();
            return isDirty;
        }

        public override void Invalidate()
        {
            base.Invalidate();
        }

        public override string ToString() => _value.ToString();
    }
}