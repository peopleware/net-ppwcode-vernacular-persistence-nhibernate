using System;

using Common.Logging;

using ContextLog4Net = log4net.ThreadContext;

namespace PPWCode.Vernacular.NHibernate.III.Test.Log4Net
{
    /// <summary>
    ///     A global context for logger variables
    /// </summary>
    public class Log4NetNestedThreadVariablesContext : INestedVariablesContext
    {
        private const string NestedStackName = "NDC";

        /// <summary>Pushes a new context message into this stack.</summary>
        /// <param name="text">The new context message text.</param>
        /// <returns>
        ///     An <see cref="T:System.IDisposable" /> that can be used to clean up the context stack.
        /// </returns>
        public IDisposable Push(string text)
            => ContextLog4Net.Stacks[NestedStackName].Push(text);

        /// <summary>Removes the top context from this stack.</summary>
        /// <returns>The message in the context that was removed from the top of this stack.</returns>
        public string Pop()
            => ContextLog4Net.Stacks[NestedStackName].Pop();

        /// <summary>
        ///     Remove all items from nested context
        /// </summary>
        public void Clear()
            => ContextLog4Net.Stacks[NestedStackName].Clear();

        /// <summary>
        ///     Returns true if there is at least one item in the nested context; false, if empty
        /// </summary>
        public bool HasItems
            => ContextLog4Net.Stacks[NestedStackName].Count > 0;
    }
}
