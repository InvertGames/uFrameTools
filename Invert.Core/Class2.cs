﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Invert.Core
{
    public interface IDebugLogger
    {
        void Log(string message);
        void LogException(Exception ex);
    }

    public class DefaultLogger : IDebugLogger
    {
        public void Log(string message)
        {
            File.AppendAllText("uframe-log.txt", message + "\r\n\r\n");
        }

        public void LogException(Exception ex)
        {
            File.AppendAllText("uframe-log.txt", ex.Message + "\r\n\r\n" + ex.StackTrace +"\r\n\r\n");
        }
    }

    public interface IEventManager
    {
        
    }
    public class EventManager<T> : IEventManager
    {
        private List<T> _listeners;

        public List<T> Listeners
        {
            get { return _listeners ?? (_listeners = new List<T>()); }
            set { _listeners = value; }
        }

        public void Signal(Action<T> action)
        {
            foreach (var item in Listeners)
            {
                action(item);
            }
        }

        public Action Subscribe(T listener)
        {
            if (!Listeners.Contains(listener))
                Listeners.Add(listener);

            return () => { Unsubscribe(listener); };
        }

        private void Unsubscribe(T listener)
        {
            Listeners.Remove(listener);
        }
    }

    public static class InvertApplication
    {
        public static bool IsTestMode { get; set; }
        public static IDebugLogger Logger
        {
            get { return _logger ?? (_logger = new DefaultLogger()); }
            set { _logger = value; }
        }

        private static uFrameContainer _container;
        private static ICorePlugin[] _plugins;
        private static IDebugLogger _logger;
        private static Dictionary<Type, IEventManager> _eventManagers;

        public static List<Assembly> CachedAssemblies { get; set; }
        static InvertApplication()
        {
            CachedAssemblies = new List<Assembly>
            {
                typeof (int).Assembly, typeof (List<>).Assembly
            };
            //CachedAssemblies.Add(typeof(ICollection<>).Assembly); 
            //foreach (var assembly in Assembly.GetEntryAssembly().GetReferencedAssemblies())
            //{
            //    Debug.WriteLine(assembly.FullName);
            //    AppDomain.CurrentDomain.Load(assembly);
            //}
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
               // Debug.WriteLine(assembly.FullName);
                if (assembly.FullName.StartsWith("Invert"))
                {
                    CachedAssemblies.Add(assembly);
                }
            }
        }

        public static void LoadPluginsFolder(string pluginsFolder)
        {
            if (!Directory.Exists(pluginsFolder))
            {
                Directory.CreateDirectory(pluginsFolder);
            }
            foreach (var plugin in Directory.GetFiles(pluginsFolder, "*.dll"))
            {
                var assembly = Assembly.LoadFrom(plugin);
                assembly = AppDomain.CurrentDomain.Load(assembly.GetName());
                InvertApplication.CachedAssemblies.Add(assembly);
            }
        }
        public static uFrameContainer Container
        {
            get
            {
                if (_container != null) return _container;
                _container = new uFrameContainer();
                InitializeContainer(_container);
                return _container;
            }
            set
            {
                _container = value;
                if (_container == null)
                {
                    EventManagers.Clear();
                }
            }
        }

        public static IEnumerable<Type> GetDerivedTypes<T>(bool includeAbstract = false, bool includeBase = true)
        {
            var type = typeof(T);
            if (includeBase)
                yield return type;
            if (includeAbstract)
            {
                foreach (var assembly in CachedAssemblies)
                {
                    //if (!assembly.FullName.StartsWith("Invert")) continue;
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
                var items = new List<Type>();
                foreach (var assembly in CachedAssemblies)
                {
                    //if (!assembly.FullName.StartsWith("Invert")) continue;
                    try
                    {
                        foreach (var t in assembly
                            .GetTypes()
                            .Where(x => type.IsAssignableFrom(x) && !x.IsAbstract))
                        {
                            items.Add(t);
                        }
                    }
                    catch (Exception ex)
                    {
                        InvertApplication.Log(ex.Message);
                    }
                }
                foreach (var item in items)
                    yield return item;
            }
        }

        public static Type FindType(string name)
        {
            //if (string.IsNullOrEmpty(name)) return null;

            foreach (var assembly in CachedAssemblies)
            {
               // if (!assembly.FullName.StartsWith("Invert") && !assembly.FullName.StartsWith("Syste")) continue;
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

            foreach (var assembly in CachedAssemblies)
            {
               // if (!assembly.FullName.StartsWith("Invert") && !assembly.FullName.StartsWith("Syste")) continue;
                try
                {
                    foreach (var item in assembly.GetTypes())
                    {
                        if (item.Name == name)
                            return item;
                    }
                }
                catch (Exception ex)
                {
                    continue;
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
        public static int MainThreadId
        {
            get; set;
        }
        public static bool IsMainThread
        {
            get { return System.Threading.Thread.CurrentThread.ManagedThreadId == MainThreadId; }
        }
        private static void InitializeContainer(IUFrameContainer container)
        {
            _plugins = null;
            MainThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            container.RegisterInstance<IUFrameContainer>(container);
            var pluginTypes = GetDerivedTypes<ICorePlugin>(false, false).ToArray();
            // Load all plugins
            foreach (var diagramPlugin in pluginTypes)
            {
                if (pluginTypes.Any(p => p.BaseType == diagramPlugin)) continue;
                var pluginInstance = Activator.CreateInstance((Type) diagramPlugin) as ICorePlugin;
                if (pluginInstance == null) continue;
                container.RegisterInstance(pluginInstance, diagramPlugin.Name, false);
                container.RegisterInstance(pluginInstance.GetType(), pluginInstance);
            }

            container.InjectAll();

            foreach (var diagramPlugin in Plugins.OrderBy(p => p.LoadPriority).Where(p=>!p.Ignore))
            {
                if (diagramPlugin.Enabled)
                    diagramPlugin.Initialize(Container);
            }

            foreach (var diagramPlugin in Plugins.OrderBy(p => p.LoadPriority).Where(p => !p.Ignore))
            {

                if (diagramPlugin.Enabled)
                {
                    container.Inject(diagramPlugin);
                    diagramPlugin.Loaded(Container);
                }
                   
            }

        }

        private static Dictionary<Type, IEventManager> EventManagers
        {
            get { return _eventManagers ?? (_eventManagers = new Dictionary<Type, IEventManager>()); }
            set { _eventManagers = value; }
        }

        /// <summary>
        /// Subscribes to a series of related events defined by an interface.
        /// </summary>
        /// <typeparam name="TEvents">The interface type the describes the events.</typeparam>
        /// <param name="listener">The listener that implements the event interface TEvents.</param>
        public static Action ListenFor<TEvents>(TEvents listener)
        {
            IEventManager manager;
            if (!EventManagers.TryGetValue(typeof (TEvents), out manager))
            {
                EventManagers.Add(typeof (TEvents), manager = new EventManager<TEvents>());
            }
            var m = manager as EventManager<TEvents>;
            if (m.Listeners.Contains(listener))
                return () => m.Listeners.Remove(listener);
            return m.Subscribe(listener);
        }

        /// <summary>
        /// Signals and event to all listeners
        /// </summary>
        /// <typeparam name="TEvents">The lambda that invokes the action.</typeparam>
        public static void SignalEvent<TEvents>(Action<TEvents> action)
        {
            IEventManager manager;
            if (!EventManagers.TryGetValue(typeof(TEvents), out manager))
            {
                EventManagers.Add(typeof(TEvents), manager = new EventManager<TEvents>());
            }
            var m = manager as EventManager<TEvents>;
            m.Signal(action);
        }
        public static void Log(string s)
        {
#if DEBUG
            Logger.Log(s);
            //Debug.Log(s);
#endif
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, TAttribute>> GetPropertiesWithAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            return GetPropertiesWithAttribute<TAttribute>(obj.GetType());
        }

        public static IEnumerable<KeyValuePair<PropertyInfo, TAttribute>> GetPropertiesWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            foreach (var source in type.GetProperties(flags))
            {
                var attribute = source.GetCustomAttributes(typeof (TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<PropertyInfo, TAttribute>(source, (TAttribute)attribute);
            }
        }
        public static IEnumerable<KeyValuePair<ConstructorInfo, TAttribute>> GetConstructorsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            foreach (var source in type.GetConstructors(flags))
            {
                var attribute = source.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<ConstructorInfo, TAttribute>(source, (TAttribute)attribute);
            }
        }
        public static IEnumerable<KeyValuePair<MethodInfo, TAttribute>> GetMethodsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            foreach (var source in type.GetMethods(flags))
            {
                var attribute = source.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<MethodInfo, TAttribute>(source, (TAttribute)attribute);
            }
        }
        public static IEnumerable<KeyValuePair<FieldInfo, TAttribute>> GetFieldsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            foreach (var source in type.GetFields(flags))
            {
                var attribute = source.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<FieldInfo, TAttribute>(source, (TAttribute)attribute);
            }
        }
        public static IEnumerable<KeyValuePair<EventInfo, TAttribute>> GetEventsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            foreach (var source in type.GetEvents(flags))
            {
                var attribute = source.GetCustomAttributes(typeof(TAttribute), true).OfType<TAttribute>().FirstOrDefault();
                if (attribute == null) continue;
                yield return new KeyValuePair<EventInfo, TAttribute>(source, (TAttribute)attribute);
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

        

        public static Type GetGenericParameter(this Type type)
        {
            var t = type;
            while (t != null)
            {
                if (t.IsGenericType)
                {
                    return t.GetGenericArguments().FirstOrDefault();
                }
                t = t.BaseType;
            }
            return null;
        }

        public static void LogException(Exception exception)
        {
            Logger.LogException(exception);
        }

        public static void LogError(string format)
        {
            Logger.Log(format);
        }
    }
    //public interface ISubscribable<T>
    //{
        
    //}

    //public static class SubscribableExtensions
    //{
    //    public static Action Watch<T>(this ISubscribable<T> t, T listener)
    //    {
    //        return InvertApplication.ListenFor(listener);
    //    }
    //    public static void Signal<T>(this ISubscribable<T> t, Action<T> action)
    //    {
    //        InvertApplication.SignalEvent(action);
    //    }
    //}

}
