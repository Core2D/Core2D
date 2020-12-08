﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Core2D.Containers
{
    public partial class Library<T> : Library
    {
        [AutoNotify] private ImmutableArray<T> _items;
        [AutoNotify] private T _selected;

        public void SetSelected(T item) => Selected = item;

        public override object Copy(IDictionary<object, object> shared)
        {
            throw new NotImplementedException();
        }

        public override bool IsDirty()
        {
            var isDirty = base.IsDirty();

            foreach (var item in Items)
            {
                if (item is ViewModelBase viewModelBase)
                {
                    isDirty |= viewModelBase.IsDirty();
                }
            }

            return isDirty;
        }

        public override void Invalidate()
        {
            base.Invalidate();

            foreach (var item in Items)
            {
                if (item is ViewModelBase viewModelBase)
                {
                    viewModelBase.Invalidate();
                }
            }
        }
    }
}
