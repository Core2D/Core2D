
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Core2D.Editor.Recent
{
    /// <summary>
    /// Recent files.
    /// </summary>
    public class Recents : ObservableObject
    {
        private ImmutableArray<RecentFile> _files = ImmutableArray.Create<RecentFile>();
        private RecentFile _current = default;

        /// <summary>
        /// Gets or sets recent file entries.
        /// </summary>
        public ImmutableArray<RecentFile> Files
        {
            get => _files;
            set => Update(ref _files, value);
        }

        /// <summary>
        /// Gets or sets current recent file.
        /// </summary>
        public RecentFile Current
        {
            get => _current;
            set => Update(ref _current, value);
        }

        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new <see cref="Recents"/> instance.
        /// </summary>
        /// <param name="files">The recent files.</param>
        /// <param name="current">The current recent file.</param>
        /// <returns>The new instance of the <see cref="Recents"/> class.</returns>
        public static Recents Create(ImmutableArray<RecentFile> files, RecentFile current)
        {
            return new Recents()
            {
                Files = files,
                Current = current
            };
        }
    }
}
