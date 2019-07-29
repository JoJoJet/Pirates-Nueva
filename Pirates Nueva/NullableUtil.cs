using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Pirates_Nueva
{
    /// <summary>
    /// Helper classes for dealing with nullable reference types.
    /// </summary>
    public static class NullableUtil
    {
        /// <summary>
        /// Throws an exception declaring that an object is uninitialized.
        /// Example: <para />
        /// <code>string SomeProperty => this.backingField ?? ThrowNotInitalized&lt;string&gt;();</code>
        /// </summary>
        /// <param name="typeName">The name of the uninitialized type.</param>
        [DoesNotReturn]
        public static T ThrowNotInitialized<T>(string typeName = "instance")
            => throw new InvalidOperationException($"This {typeName} is uninitialized!");
        /// <summary>
        /// Throws an exception declaring that an object is uninitialized.
        /// </summary>
        [DoesNotReturn]
        public static void ThrowNotInitialized(string typeName = "instance")
            => throw new InvalidOperationException($"This {typeName} is uninitialized!");
    }
}
