﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Invert.Core;
using Invert.uFrame.Editor;
using UnityEngine;

namespace Invert.Core.GraphDesigner
{ 
    public interface IJsonObject
    {
        void Serialize(JSONClass cls);
        void Deserialize(JSONClass cls, INodeRepository repository);

    }

    public static class JsonExtensions
    {
        public static void DeserializeProperty(this object obj, PropertyInfo property, JSONClass cls)
        {
            var propertyName = property.Name;
            if (cls[propertyName] == null) return;
            var propertyType = property.PropertyType;
            if (typeof(Enum).IsAssignableFrom(property.PropertyType))
            {
                var value = cls[propertyName].Value;
                property.SetValue(obj, Enum.Parse(propertyType, value), null);
            }
            else if (propertyType == typeof(int))
            {
                property.SetValue(obj, cls[propertyName].AsInt, null);
            }
            else if (propertyType == typeof(string))
            {
                property.SetValue(obj, cls[propertyName].Value, null);
            }
            else if (propertyType == typeof(float))
            {
                property.SetValue(obj, cls[propertyName].AsFloat, null);
            }
            else if (propertyType == typeof(bool))
            {
                property.SetValue(obj, cls[propertyName].AsBool, null);
            }
            else if (propertyType == typeof(double))
            {
                property.SetValue(obj, cls[propertyName].AsDouble, null);
            }
            else if (propertyType == typeof(Vector2))
            {
                property.SetValue(obj, cls[propertyName].AsVector2, null);
            }
            else if (propertyType == typeof(Vector3))
            {
                property.SetValue(obj, cls[propertyName].AsVector3, null);
            }
            else if (propertyType == typeof(Quaternion))
            {
                property.SetValue(obj, cls[propertyName].AsQuaternion, null);
            }
            else if (propertyType == typeof(Color))
            {
                property.SetValue(obj, (Color)cls[propertyName].AsVector4, null);
            }
        }
        public static void SerializeProperty(this object obj, PropertyInfo property, JSONClass cls)
        {
            var value = property.GetValue(obj, null);
            if (value != null)
            {
                var propertyName = property.Name;
                var propertyType = property.PropertyType;
                if (typeof(Enum).IsAssignableFrom(propertyType))
                {
                    cls.Add(propertyName, new JSONData(value.ToString()));
                }
                else if (propertyType == typeof(int))
                {
                    cls.Add(propertyName, new JSONData((int)value));
                }
                else if (propertyType == typeof(string))
                {
                    cls.Add(propertyName, new JSONData((string)value));
                }
                else if (propertyType == typeof(float))
                {
                    cls.Add(propertyName, new JSONData((float)value));
                }
                else if (propertyType == typeof(bool))
                {
                    cls.Add(propertyName, new JSONData((bool)value));
                }
                else if (propertyType == typeof(double))
                {
                    cls.Add(propertyName, new JSONData((double)value));
                }
                else if (propertyType == typeof(Color))
                {
                    var vCls = new JSONClass();
                    var color = (Color)value;
                    vCls.AsVector4 = new Vector4(color.r, color.g, color.b, color.a);
                    cls.Add(propertyName, vCls);
                }
                else if (propertyType == typeof(Vector2))
                {
                    var vCls = new JSONClass();
                    vCls.AsVector2 = (Vector2)value;
                    cls.Add(propertyName, vCls);
                }
                else if (propertyType == typeof(Vector3))
                {
                    var vCls = new JSONClass();
                    vCls.AsVector3 = (Vector3)value;
                    cls.Add(propertyName, vCls);
                }
                else if (propertyType == typeof(Quaternion))
                {
                    var vCls = new JSONClass();
                    vCls.AsQuaternion = (Quaternion)value;
                    cls.Add(propertyName, vCls);
                }
                else
                {
                    throw new Exception(
                        string.Format("{0} property can't be serialized. Override Serialize method to serialize it.",
                            propertyName));
                }
            }
        }
        public static void AddObject(this JSONClass cls, string name, IJsonObject jsonObject) 
        {
            cls.Add(name, SerializeObject(jsonObject));
        }
        public static IEnumerable<T> DeserializePrimitiveArray<T>(this JSONNode array, Func<JSONNode, T> deserialize)
        {
            if (array == null) yield break;
            foreach (JSONNode item in array.AsArray)
            {
                yield return deserialize(item);
            }
        }
        public static IEnumerable<T> DeserializePrimitiveArray<T>(this JSONArray array,Func<JSONNode,T> deserialize)
        {
            if (array == null) yield break;
            foreach (JSONNode item in array)
            {
                yield return deserialize(item);
            }
        }
        public static void AddPrimitiveArray<T>(this JSONClass cls,string name, IEnumerable<T> arr, Func<T,JSONNode> serializeItem)
        {
            var jsonArray = new JSONArray();
            foreach (var item in arr)
            {
                jsonArray.Add(serializeItem(item));
            }
            cls.Add(name,jsonArray);
        }
        public static void AddObjectArray<T>(this JSONClass cls,string name, IEnumerable<T> array) where T : IJsonObject
        {
            if (array == null) return;
            cls.Add(name, SerializeObjectArray(array));
        }
        public static JSONArray SerializeObjectArray<T>(this IEnumerable<T> array) where T : IJsonObject
        {
            var jsonArray = new JSONArray();
            foreach (var item in array)
            {
                jsonArray.Add(item.SerializeObject());
            }
            return jsonArray;
        }
        public static JSONClass SerializeObject(this IJsonObject obj)
        {
            var cls = new JSONClass() { { "_CLRType", obj.GetType().Name } };
            obj.Serialize(cls);
            return cls;
        }
        public static IEnumerable<T> DeserializeObjectArray<T>(this JSONNode array, INodeRepository repository)
        {
            return array.AsArray.DeserializeObjectArray<T>(repository);
        }
        public static IEnumerable<T> DeserializeObjectArray<T>(this JSONArray array, INodeRepository repository)
        {
            foreach (JSONNode item in array)
            {
                var obj = DeserializeObject(item,repository);
                if (obj != null)
                yield return (T)obj;
            }
        } 

        public static IJsonObject DeserializeObject(this JSONNode node, INodeRepository repository,Type genericTypeArg = null)
        {
            if (node == null) return null;
            var clrTypeString = node["_CLRType"].Value;
            if (string.IsNullOrEmpty(clrTypeString))
            {
                InvertApplication.Log("CLR Type is null can't load the type");
                return null;
            }
            var clrType = InvertApplication.FindType(clrTypeString);

            if (clrType == null)
                clrType = InvertApplication.FindTypeByName(clrTypeString);

            if (clrType == null)
            throw new Exception("Could not find type " + clrTypeString);
            if (clrType.IsGenericTypeDefinition && genericTypeArg != null) 
            {
                clrType = clrType.MakeGenericType(genericTypeArg);
            }
            else if (clrType.IsGenericTypeDefinition)
            {
                return null;
            }
            var obj = Activator.CreateInstance(clrType) as IJsonObject;
            if (obj != null)
            {
                obj.Deserialize(node as JSONClass, repository);
                return obj;
            }
            
            throw new Exception("Type must be of type IJsonObject" + clrTypeString);

        }
    }
}
