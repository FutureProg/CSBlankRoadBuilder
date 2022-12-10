// <copyright file="PatcherBase.cs" company="algernon (K. Algernon A. Sheppard)">
// Copyright (c) algernon (K. Algernon A. Sheppard). All rights reserved.
// Licensed under the MIT license. See LICENSE.txt file in the project root for full license information.
// </copyright>

namespace AlgernonCommons.Patching
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using CitiesHarmony.API;
    using HarmonyLib;

    /// <summary>
    /// Harmony patching class.
    /// </summary>
    public class PatcherBase
    {
        // Unique Harmony ID.
        private string _harmonyID;

        /// <summary>
        /// Gets or sets this patcher instance's unique Harmony identifier.
        /// </summary>
        public string HarmonyID { get => _harmonyID; set => _harmonyID = value; }

        /// <summary>
        /// Gets or sets a value indicating whether patches are currently applied (true) or not (false).
        /// </summary>
        public bool Patched { get; protected set; }

        /// <summary>
        /// Prints MethodInfo data as a a nicely-formatted string.
        /// </summary>
        /// <param name="method">MethodInfo to log.</param>
        /// <returns>MethodInfo data as human-readable string.</returns>
        public static string PrintMethod(MethodBase method)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(method.DeclaringType);
            sb.Append(".");
            sb.Append(method.Name);
            sb.Append("(");
            bool firstParam = true;
            foreach (ParameterInfo param in method.GetParameters())
            {
                // Separate by comma and space for everything after the first parameter.
                if (firstParam)
                {
                    firstParam = false;
                }
                else
                {
                    sb.Append(", ");
                }

                sb.Append(param.ParameterType.Name);
                sb.Append(" ");
                sb.Append(param.Name);
            }

            sb.Append(")");
            return sb.ToString();
        }

        /// <summary>
        /// Apply all Harmony patches.
        /// </summary>
        public virtual void PatchAll()
        {
            // Don't do anything if already patched.
            if (!Patched)
            {
                // Ensure Harmony is ready before patching.
                if (HarmonyHelper.IsHarmonyInstalled)
                {
                    Logging.Message("deploying Harmony patches");

                    // Apply all annotated patches and update flag.
                    Harmony harmonyInstance = new Harmony(_harmonyID);
                    harmonyInstance.PatchAll();

                    // Perform any additional patching actions.
                    OnPatchAll(harmonyInstance);

                    Patched = true;
                }
                else
                {
                    Logging.Message("Harmony not ready");
                }
            }
        }

        /// <summary>
        /// Remove all Harmony patches.
        /// </summary>
        public virtual void UnpatchAll()
        {
            // Only unapply if patches appplied.
            if (Patched)
            {
                Logging.Message("reverting Harmony patches");

                // Unapply patches, but only with our HarmonyID.
                Harmony harmonyInstance = new Harmony(_harmonyID);
                harmonyInstance.UnpatchAll(_harmonyID);

                Patched = false;
            }
        }

        /// <summary>
        /// Applies a Harmony prefix to the specified method.
        /// </summary>
        /// <param name="target">Target method.</param>
        /// <param name="patch">Harmony Prefix patch.</param>
        public void PrefixMethod(MethodInfo target, MethodInfo patch)
        {
            Harmony harmonyInstance = new Harmony(HarmonyID);
            harmonyInstance.Patch(target, prefix: new HarmonyMethod(patch));

            Logging.Message("prefixed ", PrintMethod(target), " to ", PrintMethod(patch));
        }

        /// <summary>
        /// Applies a Harmony prefix to the specified method.
        /// </summary>
        /// <param name="target">Target method.</param>
        /// <param name="patch">Harmony Prefix patch.</param>
        public void PostfixMethod(MethodInfo target, MethodInfo patch)
        {
            Harmony harmonyInstance = new Harmony(HarmonyID);
            harmonyInstance.Patch(target, postfix: new HarmonyMethod(patch));

            Logging.Message("postfixed ", PrintMethod(target), " to ", PrintMethod(patch));
        }

        /// <summary>
        /// Applies a Harmony prefix to the given type and method name, with a patch of the same name from a different type.
        /// </summary>
        /// <param name="targetType">Target type to patch.</param>
        /// <param name="patchType">Type containing patch method.</param>
        /// <param name="methodName">Method name.</param>
        public void PrefixMethod(Type targetType, Type patchType, string methodName)
        {
            PrefixMethod(
                AccessTools.Method(targetType, methodName),
                AccessTools.Method(patchType, methodName));
        }

        /// <summary>
        /// Applies a Harmony postfix to the given type and method name, with a patch of the same name from a different type.
        /// </summary>
        /// <param name="targetType">Target type to patch.</param>
        /// <param name="patchType">Type containing patch method.</param>
        /// <param name="methodName">Method name.</param>
        public void PostfixMethod(Type targetType, Type patchType, string methodName)
        {
            PostfixMethod(
                AccessTools.Method(targetType, methodName),
                AccessTools.Method(patchType, methodName));
        }

        /// <summary>
        /// Reverts a Harmony ptach to the specified method.
        /// </summary>
        /// <param name="target">Target method.</param>
        /// <param name="patch">Patch to revert.</param>
        public void UnpatchMethod(MethodInfo target, MethodInfo patch)
        {
            Harmony harmonyInstance = new Harmony(HarmonyID);
            harmonyInstance.Unpatch(target, patch);
        }

        /// <summary>
        /// Reverts a Harmony prefix to the given type and method name, with a patch of the same name from a different type.
        /// </summary>
        /// <param name="targetType">Target type to patch.</param>
        /// <param name="patchType">Type containing patch method.</param>
        /// <param name="methodName">Method name.</param>
        public void UnpatchMethod(Type targetType, Type patchType, string methodName)
        {
            UnpatchMethod(
                targetType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance),
                patchType.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance));
        }

        /// <summary>
        /// Lists all methods patched by Harmony.
        /// </summary>
        public void ListMethods()
        {
            Harmony harmonyInstance = new Harmony(HarmonyID);
            StringBuilder logMessage = new StringBuilder("Listing Harmony patches:");

            // Get all patched methods via Harmony instance and iterate through.
            IEnumerable<MethodBase> patchedMethods = harmonyInstance.GetPatchedMethods();
            foreach (MethodBase patchedMethod in patchedMethods)
            {
                // Add the method info as header.
                logMessage.Append(patchedMethod.DeclaringType);
                logMessage.Append(".");
                logMessage.AppendLine(patchedMethod.Name);

                // Get Harmony patch info for this method and log details.
                Patches patches = Harmony.GetPatchInfo(patchedMethod);

                // Print out patch owners.
                foreach (string owner in patches.Owners)
                {
                    logMessage.Append("    ");
                    logMessage.AppendLine(owner);
                }

                // Print out patch indexes and types.
                foreach (Patch prefix in patches.Prefixes)
                {
                    logMessage.Append("        Prefix ");
                    logMessage.Append(prefix.index);
                    logMessage.Append(": ");
                    logMessage.AppendLine(prefix.owner);
                }

                foreach (Patch postfix in patches.Prefixes)
                {
                    logMessage.Append("        Prefix ");
                    logMessage.Append(postfix.index);
                    logMessage.Append(": ");
                    logMessage.AppendLine(postfix.owner);
                }

                foreach (Patch transpiler in patches.Prefixes)
                {
                    logMessage.Append("        Transpiler ");
                    logMessage.Append(transpiler.index);
                    logMessage.Append(": ");
                    logMessage.AppendLine(transpiler.owner);
                }

                foreach (Patch finalizer in patches.Finalizers)
                {
                    logMessage.Append("        Finalizer ");
                    logMessage.Append(finalizer.index);
                    logMessage.Append(": ");
                    logMessage.AppendLine(finalizer.owner);
                }
            }

            // Write message to log.
            Logging.Message(logMessage);
        }

        /// <summary>
        /// Performs any additional actions (such as custom patching) after PatchAll is called.
        /// </summary>
        /// <param name="harmonyInstance">Haromny instance for patching.</param>
        protected virtual void OnPatchAll(Harmony harmonyInstance)
        {
        }
    }
}