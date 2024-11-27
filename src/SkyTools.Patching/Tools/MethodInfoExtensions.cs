// <copyright file="MethodInfoExtensions.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Tools
{
    using System;
    using System.Reflection;

    /// <summary>
    /// A static class containing extension methods for the <see cref="MethodInfo"/> class.
    /// </summary>
    public static class MethodInfoExtensions
    {
        /// <summary>Returns a string representation of the <see cref="MethodInfo"/> object including the method's class.</summary>
        /// <param name="method">The method to get a string representation of.</param>
        /// <returns>A string representation of the method.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the argument is null.</exception>
        public static string ToFullString(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            string result = method.ToString();
            int spaceIndex = result.IndexOf(' ');
            return spaceIndex < 0 ? result : result.Insert(spaceIndex + 1, method.ReflectedType.Name + ".");
        }
    }
}
