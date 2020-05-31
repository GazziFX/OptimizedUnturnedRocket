using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rocket.Core.Utils
{
    public static class RocketHelper
    {
        public static bool IsUri(string uri) {
            if (string.IsNullOrEmpty(uri)) return false;
            if (Uri.TryCreate(uri, UriKind.Absolute, out var uriOut) && (uriOut.Scheme == Uri.UriSchemeHttp || uriOut.Scheme == Uri.UriSchemeHttps))
            {
                return true;
            }
            return false;
        }

        public static List<Type> GetTypes(List<Assembly> assemblies)
        {
            List<Type> allTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                allTypes.AddRange(types);
            }
            return allTypes;
        }

        public static List<Type> GetTypesFromParentClass(Assembly assembly, Type parentClass)
        {
            List<Type> allTypes = new List<Type>();
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (Type type in types)
            {
                if (type != null && type.IsSubclassOf(parentClass))
                {
                    allTypes.Add(type);
                }
            }
            return allTypes;
        }

        public static List<Type> GetTypesFromParentClass(List<Assembly> assemblies, Type parentClass)
        {
            List<Type> allTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                allTypes.AddRange(GetTypesFromParentClass(assembly, parentClass));
            }
            return allTypes;
        }

        public static List<Type> NewGetTypesFromInterface(List<Assembly> assemblies, Type Interface)
        {
            List<Type> allTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    types = e.Types;
                }
                foreach (Type type in types)
                {
                    if (type != null && Interface.IsAssignableFrom(type))
                    {
                        allTypes.Add(type);
                    }
                }
            }
            return allTypes;
        }

        public static List<Type> NewGetTypesFromInterface(Assembly assembly, Type Interface)
        {
            List<Type> allTypes = new List<Type>();
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (Type type in types)
            {
                if (type != null && Interface.IsAssignableFrom(type))
                {
                    allTypes.Add(type);
                }
            }
            return allTypes;
        }

        public static Type NewGetTypeFromInterface(Assembly assembly, Type Interface)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (Type type in types)
            {
                if (type != null && Interface.IsAssignableFrom(type))
                {
                    return type;
                }
            }
            return null;
        }

        internal static bool ImplementInterface(this Type thisType, Type ifaceType)
        {
            while (thisType != null)
            {
                Type[] interfaces = thisType.GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        if (interfaces[i] == ifaceType || (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType)))
                        {
                            return true;
                        }
                    }
                }
                thisType = thisType.BaseType;
            }
            return false;
        }

        public static List<Type> GetTypesFromInterface(List<Assembly> assemblies, string interfaceName)
        {
            List<Type> allTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                allTypes.AddRange(GetTypesFromInterface(assembly, interfaceName));
            }
            return allTypes;
        }

        public static List<Type> GetTypesFromInterface(Assembly assembly, string interfaceName)
        {
            List<Type> allTypes = new List<Type>();
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (Type type in types)
            {
                if (type != null && type.GetInterface(interfaceName) != null)
                {
                    allTypes.Add(type);
                }
            }
            return allTypes;
        }
    }
}
