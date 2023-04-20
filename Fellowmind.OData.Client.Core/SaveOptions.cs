using System;

namespace Fellowmind.OData.Client.Core
{
    /// <summary>
    /// Save option flags to modify how the save operation is executed.
    /// </summary>
    [Flags]
    public enum SaveOptions
    {
        /// <summary>
        /// No flags. Save with Microsoft.OData.Client default functionality.
        /// </summary>
        None = 0,

        /// <summary>
        /// Batches all changes in the DataContext into one transaction. On by default.
        /// </summary>
        BatchWithSingleChangeset = 1,

        /// <summary>
        /// Includes only changes detected by change tracking. On by default.
        /// </summary>
        PostOnlySetProperties = 2,

        /// <summary>
        /// Set this flag to skip context re-creation after save.
        /// </summary>
        SkipContextReCreationAfterSave = 4,
    }
}
