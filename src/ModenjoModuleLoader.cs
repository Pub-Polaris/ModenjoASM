using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace ModenjoASM
{
    /// <summary>
    /// Discovers module DLLs under &lt;mod&gt;/modules and invokes every
    /// concrete <see cref="IModenjoModule"/> exactly once.
    /// </summary>
    internal static class ModenjoModuleLoader
    {
        public static void Load(string modRoot)
        {
            string modulesDir = Path.Combine(modRoot, "modules");
            if (!Directory.Exists(modulesDir))
            {
                Debug.LogWarning($"[ModenjoASM] modules directory not found: {modulesDir}");
                return;
            }

            foreach (string path in Directory.GetFiles(modulesDir, "*.dll", SearchOption.TopDirectoryOnly))
            {
                LoadAssembly(path);
            }
        }

        private static void LoadAssembly(string path)
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(path);
                int count = 0;
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface
                        || !typeof(IModenjoModule).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    if (Activator.CreateInstance(type) is not IModenjoModule module)
                    {
                        Debug.LogWarning($"[ModenjoASM] module type could not be created: {type.FullName}");
                        continue;
                    }

                    module.Initialize();
                    count++;
                    Debug.Log($"[ModenjoASM] module loaded: {module.Id} ({type.FullName})");
                }

                if (count == 0)
                {
                    Debug.LogWarning($"[ModenjoASM] no IModenjoModule found in {Path.GetFileName(path)}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModenjoASM] failed to load module {path}: {ex}");
            }
        }
    }
}
