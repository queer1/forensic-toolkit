namespace GrayHills.ForensicToolkit.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A queue of work items with common work item processing properties.
    /// </summary>
    /// <typeparam name="T">The type of work item to be queued.</typeparam>
    public class WorkQueue<T>
    {
        public int MaximumParallelWorkItems { get; set; }
    }
}
