﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Core2D.Attributes;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Core2D.Data.Database
{
    /// <summary>
    /// Records database.
    /// </summary>
    public class XDatabase : ObservableObject
    {
        private string _name;
        private string _idColumnName;
        private ImmutableArray<XColumn> _columns;
        private ImmutableArray<XRecord> _records;
        private XRecord _currentRecord;

        /// <summary>
        /// Initializes a new instance of the <see cref="XDatabase"/> class.
        /// </summary>
        public XDatabase()
            : base()
        {
            _columns = ImmutableArray.Create<XColumn>();
            _records = ImmutableArray.Create<XRecord>();
        }

        /// <summary>
        /// Default Id column name.
        /// </summary>
        public const string DefaultIdColumnName = "Id";

        /// <summary>
        /// Gets or sets database name.
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { Update(ref _name, value); }
        }

        /// <summary>
        /// Gets or sets Id column name.
        /// </summary>
        public string IdColumnName
        {
            get { return _idColumnName; }
            set { Update(ref _idColumnName, value); }
        }

        /// <summary>
        /// Gets or sets database columns.
        /// </summary>
        public ImmutableArray<XColumn> Columns
        {
            get { return _columns; }
            set { Update(ref _columns, value); }
        }

        /// <summary>
        /// Gets or sets database records.
        /// </summary>
        [Content]
        public ImmutableArray<XRecord> Records
        {
            get { return _records; }
            set { Update(ref _records, value); }
        }

        /// <summary>
        /// Gets or sets database current record.
        /// </summary>
        public XRecord CurrentRecord
        {
            get { return _currentRecord; }
            set { Update(ref _currentRecord, value); }
        }

        /// <summary>
        /// Creates a new <see cref="XDatabase"/> instance.
        /// </summary>
        /// <param name="name">The database name.</param>
        /// <param name="idColumnName">The Id column name.</param>
        /// <returns>The new instance of the <see cref="XDatabase"/> class.</returns>
        public static XDatabase Create(string name, string idColumnName = DefaultIdColumnName)
        {
            return new XDatabase()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = ImmutableArray.Create<XColumn>(),
                Records = ImmutableArray.Create<XRecord>()
            };
        }

        /// <summary>
        /// Creates a new <see cref="XDatabase"/> instance.
        /// </summary>
        /// <param name="name">The database name.</param>
        /// <param name="columns">The database columns.</param>
        /// <param name="idColumnName">The Id column name.</param>
        /// <returns>The new instance of the <see cref="XDatabase"/> class.</returns>
        public static XDatabase Create(string name, ImmutableArray<XColumn> columns, string idColumnName = DefaultIdColumnName)
        {
            return new XDatabase()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = columns,
                Records = ImmutableArray.Create<XRecord>()
            };
        }

        /// <summary>
        /// Creates a new <see cref="XDatabase"/> instance.
        /// </summary>
        /// <param name="name">The database name.</param>
        /// <param name="columns">The database columns.</param>
        /// <param name="records">The database records.</param>
        /// <param name="idColumnName">The Id column name.</param>
        /// <returns>The new instance of the <see cref="XDatabase"/> class.</returns>
        public static XDatabase Create(string name, ImmutableArray<XColumn> columns, ImmutableArray<XRecord> records, string idColumnName = DefaultIdColumnName)
        {
            return new XDatabase()
            {
                Name = name,
                IdColumnName = idColumnName,
                Columns = columns,
                Records = records
            };
        }

        /// <summary>
        /// Creates a new <see cref="XDatabase"/> instance.
        /// </summary>
        /// <param name="name">The database name.</param>
        /// <param name="fields">The fields collection.</param>
        /// <param name="idColumnName">The Id column name.</param>
        /// <returns>The new instance of the <see cref="XDatabase"/> class.</returns>
        public static XDatabase FromFields(string name, IEnumerable<string[]> fields, string idColumnName = DefaultIdColumnName)
        {
            var db = XDatabase.Create(name, idColumnName);
            var tempColumns = fields.FirstOrDefault().Select(c => XColumn.Create(db, c));
            var columns = ImmutableArray.CreateRange<XColumn>(tempColumns);

            if (columns.Length >= 1 && columns[0].Name == idColumnName)
            {
                // Use existing record Id.
                var tempRecords = fields
                    .Skip(1)
                    .Select(v =>
                            XRecord.Create(
                                db,
                                v.FirstOrDefault(),
                                columns,
                                ImmutableArray.CreateRange<XValue>(v.Select(c => XValue.Create(c)))));
                var records = ImmutableArray.CreateRange<XRecord>(tempRecords);

                db.Columns = columns;
                db.Records = records;
            }
            else
            {
                // Create records with new Id.
                var tempRecords = fields
                    .Skip(1)
                    .Select(v =>
                            XRecord.Create(
                                db,
                                columns,
                                ImmutableArray.CreateRange<XValue>(v.Select(c => XValue.Create(c)))));
                var records = ImmutableArray.CreateRange<XRecord>(tempRecords);

                db.Columns = columns;
                db.Records = records;
            }

            return db;
        }

        /// <summary>
        /// Update the destination database using data from source database using Id column as identification.
        /// </summary>
        /// <param name="destination">The destination database.</param>
        /// <param name="source">The source database.</param>
        /// <param name="records">The updated records from destination database.</param>
        /// <returns>True if destination database was updated.</returns>
        public static bool Update(XDatabase destination, XDatabase source, out ImmutableArray<XRecord>.Builder records)
        {
            bool isDirty = false;
            records = null;

            if (source == null || destination == null)
            {
                return isDirty;
            }

            // Check the number of source database columns.
            if (source.Columns.Length <= 1)
            {
                return isDirty;
            }

            // Check for presence of the Id column in the source database.
            if (source.Columns[0].Name != destination.IdColumnName)
            {
                return isDirty;
            }

            // Check for matching columns length.
            if (source.Columns.Length - 1 != destination.Columns.Length)
            {
                return isDirty;
            }

            // Check for matching column names.
            for (int i = 1; i < source.Columns.Length; i++)
            {
                if (source.Columns[i].Name != destination.Columns[i - 1].Name)
                {
                    return isDirty;
                }
            }

            // Create updated records builder.
            records = destination.Records.ToBuilder();

            // Update or remove existing records.
            for (int i = 0; i < destination.Records.Length; i++)
            {
                var record = destination.Records[i];
                var result = source.Records.FirstOrDefault(r => r.Id == record.Id);
                if (result != null)
                {
                    // Update existing record.
                    for (int j = 1; j < result.Values.Length; j++)
                    {
                        var valuesBuilder = record.Values.ToBuilder();
                        valuesBuilder[j - 1] = result.Values[j];
                        record.Values = valuesBuilder.ToImmutable();
                    }
                    isDirty = true;
                }
                else
                {
                    // Remove existing record.
                    records.Remove(record);
                    isDirty = true;
                }
            }

            // Add new records.
            for (int i = 0; i < source.Records.Length; i++)
            {
                var record = source.Records[i];
                var result = destination.Records.FirstOrDefault(r => r.Id == record.Id);
                if (result == null)
                {
                    var r = source.Records[i];

                    // Use existing columns.
                    r.Columns = destination.Columns;

                    // Skip Id column.
                    r.Values = r.Values.Skip(1).ToImmutableArray();

                    // Add new record.
                    records.Add(r);
                    isDirty = true;
                }
            }

            return isDirty;
        }
    }
}
