// <copyright file="MethodPatcher.cs" company="dymanoid">
// Copyright (c) dymanoid. All rights reserved.
// </copyright>

namespace SkyTools.Patching
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Harmony;
    using SkyTools.Tools;

    /// <summary>
    /// A class that uses Harmony library for redirecting the game's methods.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Patcher", Justification = "Reviewed")]
    public sealed class MethodPatcher
    {
        private readonly Patcher patcher;
        private readonly string id;
        private readonly IEnumerable<IPatch> patches;

        /// <summary>Initializes a new instance of the <see cref="MethodPatcher"/> class.</summary>
        /// <param name="id">The unique ID to use in Harmony.</param>
        /// <param name="patches">The patches to process by this object.</param>
        /// <exception cref="ArgumentException">Thrown when no patches specified.</exception>
        public MethodPatcher(string id, IEnumerable<IPatch> patches)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("The ID cannot be null or an empty string", nameof(id));
            }

            if (patches == null || !patches.Any())
            {
                throw new ArgumentException("At least one patch is required");
            }

            this.id = id;
            this.patches = patches;
            var harmony = HarmonyInstance.Create(id);
            patcher = new Patcher(harmony);
        }

        /// <summary>Applies all patches this object knows about.</summary>
        public void Apply()
        {
            Revert();

            int applied = 0;
            foreach (IPatch patch in patches)
            {
                patch.ApplyPatch(patcher);
                ++applied;
            }

            Log.Info($"{applied} Harmony method patches with ID '{id}' successfully applied.");
        }

        /// <summary>Reverts all patches, if any applied.</summary>
        public void Revert()
        {
            foreach (IPatch patch in patches)
            {
                try
                {
                    patch.RevertPatch(patcher);
                }
                catch (Exception ex)
                {
                    Log.Error($"Failed reverting a Harmony method patch {patch} with ID '{id}'. Error message: {ex}");
                }
            }
        }

        private sealed class Patcher : IPatcher
        {
            private readonly HarmonyInstance harmony;

            public Patcher(HarmonyInstance harmony)
            {
                this.harmony = harmony;
            }

            public void ApplyPatch(MethodInfo method, MethodInfo prefixCall, MethodInfo postfixCall)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                if (prefixCall == null && postfixCall == null)
                {
                    throw new ArgumentException($"Both {nameof(prefixCall)} and {nameof(postfixCall)} cannot be null at the same time.");
                }

                harmony.Patch(method, new HarmonyMethod(prefixCall), new HarmonyMethod(postfixCall));
            }

            public void RevertPatch(MethodInfo method)
            {
                if (method == null)
                {
                    throw new ArgumentNullException(nameof(method));
                }

                harmony.RemovePatch(method, HarmonyPatchType.All, harmony.Id);
            }
        }
    }
}
