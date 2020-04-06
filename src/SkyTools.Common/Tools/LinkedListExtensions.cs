// <copyright file="LinkedListExtensions.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Tools
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A class containing extension methods for the <see cref="LinkedList{T}"/>.
    /// </summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Gets the linked list's firsts node that matches the specified <paramref name="predicate"/>,
        /// or null if no matching node was found.
        /// </summary>
        ///
        /// <typeparam name="T">The linked list item's type.</typeparam>
        ///
        /// <param name="list">The linked list to search in.</param>
        /// <param name="predicate">The predicate to use for the search.</param>
        ///
        /// <returns>The first linked list's node that matches the <paramref name="predicate"/>, or null.</returns>
        ///
        /// <exception cref="ArgumentNullException">Thrown when any argument is null.</exception>
        public static LinkedListNode<T> FirstOrDefaultNode<T>(this LinkedList<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (list.Count == 0)
            {
                return null;
            }

            var node = list.First;
            while (node != null)
            {
                if (predicate(node.Value))
                {
                    return node;
                }

                node = node.Next;
            }

            return null;
        }

        /// <summary>
        /// Copies the contents of the linked list to the specified <paramref name="target"/> collection.
        /// </summary>
        /// <typeparam name="T">The type of the items in the linked list.</typeparam>
        ///
        /// <param name="list">The linked list to copy items from.</param>
        /// <param name="target">The collection to store the items in.</param>
        ///
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="list"/> or <paramref name="target"/> is <c>null</c>.</exception>
        public static void CopyTo<T>(this LinkedList<T> list, ICollection<T> target)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (list.Count == 0)
            {
                return;
            }

            var node = list.First;
            while (node != null)
            {
                target.Add(node.Value);
                node = node.Next;
            }
        }
    }
}
