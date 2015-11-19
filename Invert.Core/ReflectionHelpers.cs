using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Invert.Core {
    public static class ReflectionHelpers {
        private static readonly Dictionary<MembersWithAttributeKey, object> _membersWithAttributeDictionary = 
            new Dictionary<MembersWithAttributeKey, object>();

        private static readonly Dictionary<Type, string> _typeToAssemblyQualifiedName = new Dictionary<Type, string>();

        public static KeyValuePair<PropertyInfo, TAttribute>[] GetPropertiesWithAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            return GetPropertiesWithAttribute<TAttribute>(obj.GetType());
        }

        public static KeyValuePair<PropertyInfo, TAttribute>[] GetPropertiesWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute {
            return GetMembersWithAttribute<PropertyInfo, TAttribute>(type, flags, (t, f) => t.GetProperties(f));
        }

        public static KeyValuePair<ConstructorInfo, TAttribute>[] GetConstructorsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            return GetMembersWithAttribute<ConstructorInfo, TAttribute>(type, flags, (t, f) => t.GetConstructors(f));
        }

        public static KeyValuePair<MethodInfo, TAttribute>[] GetMethodsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            return GetMembersWithAttribute<MethodInfo, TAttribute>(type, flags, (t, f) => t.GetMethods(f));
        }

        public static KeyValuePair<FieldInfo, TAttribute>[] GetFieldsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            return GetMembersWithAttribute<FieldInfo, TAttribute>(type, flags, (t, f) => t.GetFields(f));
        }

        public static KeyValuePair<EventInfo, TAttribute>[] GetEventsWithAttribute<TAttribute>(this Type type, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) where TAttribute : Attribute
        {
            return GetMembersWithAttribute<EventInfo, TAttribute>(type, flags, (t, f) => t.GetEvents(f));
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this object obj) where TAttribute : Attribute
        {
            return GetPropertiesByAttribute<TAttribute>(obj.GetType());
        }

        public static IEnumerable<PropertyInfo> GetPropertiesByAttribute<TAttribute>(this Type type) where TAttribute : Attribute
        {
            return type.GetPropertiesWithAttribute<TAttribute>().Select(pair => pair.Key);
        }

        private static KeyValuePair<TMemberType, TAttribute>[] GetMembersWithAttribute<TMemberType, TAttribute>(Type type, BindingFlags flags, Func<Type, BindingFlags, TMemberType[]> getMembersFunc)
            where TAttribute : Attribute where TMemberType : MemberInfo {
            MembersWithAttributeKey key = new MembersWithAttributeKey(typeof(TAttribute), type, flags);

            object resultTemp;
            KeyValuePair<TMemberType, TAttribute>[] result;
            if (!_membersWithAttributeDictionary.TryGetValue(key, out resultTemp)) {
                var propertyAttributePairs = new List<KeyValuePair<TMemberType, TAttribute>>();
                foreach (var source in getMembersFunc(type, flags))
                {
                    var attribute = (TAttribute) Attribute.GetCustomAttribute(source, typeof(TAttribute));
                    if (attribute == null) continue;
                    propertyAttributePairs.Add(new KeyValuePair<TMemberType, TAttribute>(source, attribute));
                }
                result = propertyAttributePairs.ToArray();
                _membersWithAttributeDictionary.Add(key, result);
            } else {
                result = (KeyValuePair<TMemberType, TAttribute>[]) resultTemp;
            }

            return result;
        }

        public static string GetAssemblyQualifiedName(this Type type)
        {
            string typeName;
            if (!_typeToAssemblyQualifiedName.TryGetValue(type, out typeName))
            {
                typeName = type.AssemblyQualifiedName;
                _typeToAssemblyQualifiedName.Add(type, typeName);
            }

            return typeName;
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

        public static void ClearCache() {
            _membersWithAttributeDictionary.Clear();
            _typeToAssemblyQualifiedName.Clear();
        }

        private struct MembersWithAttributeKey : IEquatable<MembersWithAttributeKey> {
            public readonly Type AttributeType;
            public readonly Type Type;
            public readonly BindingFlags BindingFlags;

            public MembersWithAttributeKey(Type attributeType, Type type, BindingFlags bindingFlags) {
                AttributeType = attributeType;
                Type = type;
                BindingFlags = bindingFlags;
            }

            public bool Equals(MembersWithAttributeKey other) {
                return Type == other.Type && AttributeType == other.AttributeType && BindingFlags == other.BindingFlags;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) return false;
                return obj is MembersWithAttributeKey && Equals((MembersWithAttributeKey) obj);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = AttributeType != null ? AttributeType.GetHashCode() : 0;
                    hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                    hashCode = (hashCode * 397) ^ (int) BindingFlags;
                    return hashCode;
                }
            }
        }
    }
}