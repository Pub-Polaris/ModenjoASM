using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ModenjoASM
{
    /// <summary>
    /// Runtime host for non-invasive Aotenjo extensions.
    ///
    /// Lua loads this assembly from the mod folder, calls CreateInstance on
    /// <see cref="Bootstrap"/>, then calls <see cref="Init"/>. The host creates
    /// its UI marker and loads module DLLs from the mod's modules directory.
    /// No game assembly is patched.
    /// </summary>
    public sealed class Bootstrap
    {
        public const string HostId = "ModenjoASM";

        private static bool _initialized;
        private static string _modRoot = string.Empty;

        public void Init()
        {
            Init(null);
        }

        public void Init(string? modDirectory)
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            _modRoot = ResolveModRoot(modDirectory);
            AppDomain.CurrentDomain.AssemblyResolve += ResolveModuleDependency;

            var tag = new GameObject("ModenjoASM.VersionTag");
            tag.AddComponent<ModenjoAsmVersionTag>();

            ModenjoModuleLoader.Load(_modRoot);
            Debug.Log($"[ModenjoASM] host initialized from {_modRoot}");
        }

        private static string ResolveModRoot(string? modDirectory)
        {
            if (!string.IsNullOrWhiteSpace(modDirectory) && Directory.Exists(modDirectory))
            {
                return Path.GetFullPath(modDirectory);
            }

            // Fallback: the host DLL lives directly in the mod folder.
            string assemblyPath = typeof(Bootstrap).Assembly.Location;
            string? assemblyDir = string.IsNullOrEmpty(assemblyPath)
                ? null
                : Path.GetDirectoryName(assemblyPath);
            return assemblyDir ?? Environment.CurrentDirectory;
        }

        private static Assembly? ResolveModuleDependency(object sender, ResolveEventArgs args)
        {
            if (string.IsNullOrEmpty(_modRoot))
            {
                return null;
            }

            string simpleName = new AssemblyName(args.Name).Name;
            string rootCandidate = Path.Combine(_modRoot, simpleName + ".dll");
            if (File.Exists(rootCandidate))
            {
                return Assembly.LoadFrom(rootCandidate);
            }

            string moduleCandidate = Path.Combine(_modRoot, "modules", simpleName + ".dll");
            return File.Exists(moduleCandidate) ? Assembly.LoadFrom(moduleCandidate) : null;
        }
    }
}
