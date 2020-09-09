﻿using System;
using System.Collections.Generic;

namespace Core2D.Editor.Tools.Settings
{
    /// <summary>
    /// None tool settings.
    /// </summary>
    public class ToolSettingsNone : ObservableObject, ISettings
    {
        /// <inheritdoc/>
        public override object Copy(IDictionary<object, object>? shared)
        {
            throw new NotImplementedException();
        }
    }
}
