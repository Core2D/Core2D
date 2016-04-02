﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Attributes;
using Core2D.Data.Database;
using System.Collections.Immutable;
using System.Linq;

namespace Core2D.Data
{
    /// <summary>
    /// Data context.
    /// </summary>
    public class XContext : ObservableObject
    {
        private ImmutableArray<XProperty> _properties;
        private XRecord _record;

        /// <summary>
        /// Initializes a new instance of the <see cref="XContext"/> class.
        /// </summary>
        public XContext()
            : base()
        {
            _properties = ImmutableArray.Create<XProperty>();
        }

        /// <summary>
        /// Gets or sets a collection <see cref="XProperty"/> that will be used during drawing.
        /// </summary>
        [Content]
        public ImmutableArray<XProperty> Properties
        {
            get { return _properties; }
            set { Update(ref _properties, value); }
        }

        /// <summary>
        /// Gets or sets shape data record.
        /// </summary>
        public XRecord Record
        {
            get { return _record; }
            set { Update(ref _record, value); }
        }

        /// <summary>
        /// Gets or sets <see cref="XProperty.Value"/> using name as key for <see cref="Properties"/> array values.
        /// </summary>
        /// <remarks>
        /// If property with the specified key does not exist it is created.
        /// </remarks>
        /// <param name="name">The property value.</param>
        /// <returns>The property value.</returns>
        public string this[string name]
        {
            get
            {
                var result = _properties.FirstOrDefault(p => p.Name == name);
                if (result != null)
                {
                    return result.Value;
                }
                return null;
            }
            set
            {
                if (value != null)
                {
                    var result = _properties.FirstOrDefault(p => p.Name == name);
                    if (result != null)
                    {
                        result.Value = value;
                    }
                    else
                    {
                        var property = XProperty.Create(this, name, value);
                        Properties = Properties.Add(property);
                    }
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="XContext"/> instance.
        /// </summary>
        /// <returns>The new instance of the <see cref="XContext"/> class.</returns>
        public static XContext Create()
        {
            return new XContext();
        }

        /// <summary>
        /// Creates a new <see cref="XContext"/> instance.
        /// </summary>
        /// <param name="record">The record instance.</param>
        /// <returns>The new instance of the <see cref="XContext"/> class.</returns>
        public static XContext Create(XRecord record)
        {
            return new XContext()
            {
                Record = record
            };
        }
    }
}
