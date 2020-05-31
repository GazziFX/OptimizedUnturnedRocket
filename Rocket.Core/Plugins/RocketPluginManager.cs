using Rocket.API;
using Rocket.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Rocket.Core.Extensions;

namespace Rocket.Core.Plugins
{
    public sealed class RocketPluginManager
    {
        public delegate void PluginsLoaded();
        public event PluginsLoaded OnPluginsLoaded;

        private static List<Assembly> pluginAssemblies;
        private static List<GameObject> plugins = new List<GameObject>();
        private static Dictionary<string, string> libraries = new Dictionary<string, string>();

        public List<IRocketPlugin> GetPlugins()
        {
            var plugs = new List<IRocketPlugin>();
            foreach (var p in plugins)
            {
                var c = p.GetComponent<IRocketPlugin>();
                if (c != null)
                    plugs.Add(c);
            }
            return plugs;
        }

        public IRocketPlugin GetPlugin(Assembly assembly)
        {
            foreach (var plug in plugins)
            {
                var comp = plug.GetComponent<IRocketPlugin>();
                if (comp != null && comp.GetType().Assembly.Equals(assembly))
                {
                    return comp;
                }
            }
            return null;
        }

        public IRocketPlugin GetPlugin(string name)
        {
            foreach (var plug in plugins)
            {
                var comp = plug.GetComponent<IRocketPlugin>();
                if (comp != null && comp.Name.Equals(name))
                {
                    return comp;
                }
            }
            return null;
        }

        internal static void Awake() {
            AppDomain.CurrentDomain.AssemblyResolve += delegate (object sender, ResolveEventArgs args)
            {
                if (libraries.TryGetValue(args.Name, out string file))
                {
                    return Assembly.Load(File.ReadAllBytes(file));
                }
                else
                {
                    Logging.Logger.LogError("Could not find dependency: " + args.Name);
                }
                return null;
            };
        }


        internal void loadPlugins()
        {
            libraries = GetAssembliesFromDirectory(Environment.LibrariesDirectory);
            foreach(KeyValuePair<string,string> pair in GetAssembliesFromDirectory(Environment.PluginsDirectory))
            {
                if(!libraries.ContainsKey(pair.Key))
                    libraries.Add(pair.Key,pair.Value);
            }

            pluginAssemblies = LoadAssembliesFromDirectory(Environment.PluginsDirectory);
            List<Type> pluginImplemenations = RocketHelper.NewGetTypesFromInterface(pluginAssemblies, typeof(IRocketPlugin));
            foreach (Type pluginType in pluginImplemenations)
            {
                GameObject plugin = new GameObject(pluginType.Name, pluginType);
                UnityEngine.Object.DontDestroyOnLoad(plugin);
                plugins.Add(plugin);
            }
            OnPluginsLoaded.TryInvoke();
        }

        private void unloadPlugins() {
            for(int i = plugins.Count; i > 0; i--)
            {
                UnityEngine.Object.Destroy(plugins[i-1]);
            }
            plugins.Clear();
        }

        internal void Reload()
        {
            unloadPlugins();
            loadPlugins();
        }

        public static Dictionary<string, string> GetAssembliesFromDirectory(string directory, string extension = "*.dll")
        {
            Dictionary<string, string> l = new Dictionary<string, string>();
            IEnumerable<FileInfo> libraries = new DirectoryInfo(directory).GetFiles(extension, SearchOption.AllDirectories);
            foreach (FileInfo library in libraries)
            {
                try
                {
                    AssemblyName name = AssemblyName.GetAssemblyName(library.FullName);
                    l.Add(name.FullName, library.FullName);
                }
                catch { }
            }
            return l;
        }

        public static List<Assembly> LoadAssembliesFromDirectory(string directory, string extension = "*.dll")
        {
            List<Assembly> assemblies = new List<Assembly>();
            IEnumerable<FileInfo> pluginsLibraries = new DirectoryInfo(directory).GetFiles(extension, SearchOption.AllDirectories);

            foreach (FileInfo library in pluginsLibraries)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(library.FullName);//Assembly.Load(File.ReadAllBytes(library.FullName));

                    List<Type> types = RocketHelper.NewGetTypesFromInterface(assembly, typeof(IRocketPlugin));

                    if (types.Count == 1)
                    {
                        Logging.Logger.Log("Loading "+ assembly.GetName().Name +" from "+ assembly.Location);
                        assemblies.Add(assembly);
                    }
                    else
                    {
                        Logging.Logger.LogError("Invalid or outdated plugin assembly: " + assembly.GetName().Name);
                    }
                }
                catch (Exception ex)
                {
                    Logging.Logger.LogError(ex, "Could not load plugin assembly: " + library.Name);
                }
            }
            return assemblies;
        }
    }
}