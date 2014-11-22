using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.uFrame;

namespace Invert.Core
{
    public static class InvertApplication
    {
        private static uFrameContainer _container;
        private static ICorePlugin[] _plugins;

        public static uFrameContainer Container
        {
            get
            {
                if (_container != null) return _container;
                _container = new uFrameContainer();
                InitializeContainer(_container);
                return _container;
            }
            set { _container = value; }
        }

        public static IEnumerable<Type> GetDerivedTypes<T>(bool includeAbstract = false, bool includeBase = true)
        {
            var type = typeof(T);
            if (includeBase)
                yield return type;
            if (includeAbstract)
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => type.IsAssignableFrom(x)))
                    {
                        yield return t;
                    }
                }
            }
            else
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    foreach (var t in assembly
                        .GetTypes()
                        .Where(x => type.IsAssignableFrom(x) && !x.IsAbstract))
                    {
                        yield return t;
                    }
                }
            }
        }

        public static Type FindType(string name)
        {
            //if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = assembly.GetType(name);
                if (t != null)
                {
                    return t;
                }
            }
            return null;
        }

        public static Type FindTypeByName(string name)
        {
            //if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var item in assembly.GetTypes())
                {
                    if (item.Name == name)
                        return item;
                }

            }
            return null;
        }

        public static ICorePlugin[] Plugins
        {
            get
            {
                return _plugins ?? (_plugins = Container.ResolveAll<ICorePlugin>().ToArray());
            }
            set { _plugins = value; }
        }

        private static void InitializeContainer(IUFrameContainer container)
        {
            container.RegisterInstance<IUFrameContainer>(container);
            var pluginTypes = GetDerivedTypes<ICorePlugin>(false, false).ToArray();
            // Load all plugins
            foreach (var diagramPlugin in pluginTypes)
            {
                if (pluginTypes.Any(p => p.BaseType == diagramPlugin)) continue;
                container.RegisterInstance(Activator.CreateInstance((Type)diagramPlugin) as ICorePlugin, diagramPlugin.Name, false);
            }

            container.InjectAll();

            foreach (var diagramPlugin in Plugins.OrderBy(p => p.LoadPriority))
            {
                if (diagramPlugin.Enabled)
                    diagramPlugin.Initialize(Container);
            }

            foreach (var diagramPlugin in Plugins.OrderBy(p => p.LoadPriority))
            {
                if (diagramPlugin.Enabled)
                    diagramPlugin.Loaded();
            }

        }

        public static void Log(string s)
        {
#if DEBUG
            File.AppendAllText("uframe-log.txt", s + "\r\n\r\n");
            //Debug.Log(s);
#endif
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, TAttribute>> GetPropertiesWithAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            return GetPropertiesWithAttribute<TAttribute>(obj.GetType());
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, TAttribute>> GetPropertiesWithAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            foreach (var source in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = source.GetCustomAttributes(typeof (TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<PropertyInfo, TAttribute>(source, (TAttribute)attribute);
            }
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            return GetPropertiesByAttribute<TAttribute>(obj.GetType());
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetCustomAttributes(typeof(TAttribute), true).Length > 0);
        }
    }
}
