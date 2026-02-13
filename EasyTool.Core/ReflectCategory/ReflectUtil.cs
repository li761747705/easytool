using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 反射工具类，提供类型、属性、字段、方法的反射操作
    /// </summary>
    public static class ReflectUtil
    {
        #region 类型成员获取

        /// <summary>
        /// 获取类型的所有构造函数
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>构造函数数组</returns>
        public static ConstructorInfo[] GetConstructors(Type type)
        {
            return type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 获取类型的所有属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>属性数组</returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 获取类型的所有字段
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>字段数组</returns>
        public static FieldInfo[] GetFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 获取类型的所有方法
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>方法数组</returns>
        public static MethodInfo[] GetMethods(Type type)
        {
            return type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 获取类型的所有事件
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>事件数组</returns>
        public static EventInfo[] GetEvents(Type type)
        {
            return type.GetEvents(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        /// <summary>
        /// 获取类型的所有属性名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>属性名数组</returns>
        public static string[] GetPropertyNames(Type type)
        {
            return GetProperties(type).Select(p => p.Name).ToArray();
        }

        /// <summary>
        /// 获取类型的所有字段名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>字段名数组</returns>
        public static string[] GetFieldNames(Type type)
        {
            return GetFields(type).Select(f => f.Name).ToArray();
        }

        /// <summary>
        /// 获取类型的所有方法名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>方法名数组</returns>
        public static string[] GetMethodNames(Type type)
        {
            return GetMethods(type).Select(m => m.Name).ToArray();
        }

        /// <summary>
        /// 获取类型的所有事件名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>事件名数组</returns>
        public static string[] GetEventNames(Type type)
        {
            return GetEvents(type).Select(e => e.Name).ToArray();
        }

        /// <summary>
        /// 获取类型的所有接口名
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>接口名数组</returns>
        public static string[] GetInterfaceNames(Type type)
        {
            return type.GetInterfaces().Select(i => i.Name).ToArray();
        }

        #endregion

        #region 类型特性

        /// <summary>
        /// 获取指定类型的指定类型的特性数组
        /// </summary>
        /// <typeparam name="T">特性类型</typeparam>
        /// <param name="type">类型</param>
        /// <returns>特性数组</returns>
        public static T[] GetAttributes<T>(Type type) where T : Attribute
        {
            return type.GetCustomAttributes<T>().ToArray();
        }

        /// <summary>
        /// 判断类型是否实现了指定的接口
        /// </summary>
        /// <typeparam name="T">要判断的类型</typeparam>
        /// <typeparam name="TInterface">要判断的接口类型</typeparam>
        /// <returns>是否实现了指定的接口</returns>
        public static bool ImplementsInterface<T, TInterface>()
        {
            return typeof(T).GetInterfaces().Any(i => i == typeof(TInterface));
        }

        /// <summary>
        /// 获取类的继承层次结构
        /// </summary>
        /// <param name="type">要获取继承层次结构的类</param>
        /// <returns>类的继承层次结构</returns>
        public static Type[] GetClassHierarchy(Type type)
        {
            Type[] hierarchy = new Type[0];
            Type currentType = type;
            while (currentType != null)
            {
                Array.Resize(ref hierarchy, hierarchy.Length + 1);
                hierarchy[hierarchy.Length - 1] = currentType;
                currentType = currentType.BaseType;
            }
            return hierarchy;
        }

        /// <summary>
        /// 获取枚举类型的所有值
        /// </summary>
        /// <typeparam name="T">枚举类型</typeparam>
        /// <returns>枚举类型的所有值</returns>
        public static IEnumerable<T> GetEnumValues<T>()
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("Type is not an enum type");
            }

            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        #endregion

        #region 实例创建

        /// <summary>
        /// 创建类型的实例
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="args">构造函数参数</param>
        /// <returns>实例</returns>
        public static object CreateInstance(Type type, params object[] args)
        {
            Type[] parameterTypes = GetParameterTypes(args);
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null, parameterTypes, null);
            if (constructor == null)
            {
                throw new ArgumentException($"Type {type} does not have a constructor with specified arguments");
            }
            return constructor.Invoke(args);
        }

        /// <summary>
        /// 获取构造函数参数类型的数组
        /// </summary>
        /// <param name="parameters">要获取参数类型的参数数组</param>
        /// <returns>参数类型的数组</returns>
        private static Type[] GetParameterTypes(object[] parameters)
        {
            if (parameters == null)
            {
                return Type.EmptyTypes;
            }
            Type[] parameterTypes = new Type[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i] == null)
                {
                    parameterTypes[i] = typeof(object);
                }
                else
                {
                    parameterTypes[i] = parameters[i].GetType();
                }
            }
            return parameterTypes;
        }

        #endregion

        #region 方法调用

        /// <summary>
        /// 调用泛型方法
        /// </summary>
        /// <param name="obj">调用方法的对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="genericType">泛型参数类型</param>
        /// <param name="args">方法参数</param>
        /// <returns>方法返回值</returns>
        public static object InvokeGenericMethod(object obj, string methodName, Type genericType, params object[] args)
        {
            MethodInfo method = obj.GetType().GetMethod(methodName);
            MethodInfo genericMethod = method.MakeGenericMethod(genericType);
            return genericMethod.Invoke(obj, args);
        }

        /// <summary>
        /// 动态调用类的实例方法
        /// </summary>
        /// <param name="instance">要调用实例方法的类实例</param>
        /// <param name="methodName">要调用的实例方法的名称</param>
        /// <param name="arguments">要传递给实例方法的参数</param>
        /// <returns>实例方法的返回值</returns>
        public static object InvokeMethod(object instance, string methodName, params object[] arguments)
        {
            Type type = instance.GetType();
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
            return method.Invoke(instance, arguments);
        }

        /// <summary>
        /// 动态调用类的静态方法
        /// </summary>
        /// <param name="type">要调用静态方法的类</param>
        /// <param name="methodName">要调用的静态方法的名称</param>
        /// <param name="arguments">要传递给静态方法的参数</param>
        /// <returns>静态方法的返回值</returns>
        public static object InvokeStaticMethod(Type type, string methodName, params object[] arguments)
        {
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
            return method.Invoke(null, arguments);
        }

        #endregion

        #region 静态成员操作

        /// <summary>
        /// 获取类的静态属性的值
        /// </summary>
        /// <param name="type">要获取静态属性的类</param>
        /// <param name="propertyName">要获取的静态属性的名称</param>
        /// <returns>静态属性的值</returns>
        public static object GetStaticPropertyValue(Type type, string propertyName)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            return property.GetValue(null);
        }

        /// <summary>
        /// 设置类的静态属性的值
        /// </summary>
        /// <param name="type">要设置静态属性的类</param>
        /// <param name="propertyName">要设置的静态属性的名称</param>
        /// <param name="value">要设置的静态属性的值</param>
        public static void SetStaticPropertyValue(Type type, string propertyName, object value)
        {
            PropertyInfo property = type.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
            property.SetValue(null, value);
        }

        /// <summary>
        /// 获取类的静态字段的值
        /// </summary>
        /// <param name="type">要获取静态字段的类</param>
        /// <param name="fieldName">要获取的静态字段的名称</param>
        /// <returns>静态字段的值</returns>
        public static object GetStaticFieldValue(Type type, string fieldName)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
            return field.GetValue(null);
        }

        /// <summary>
        /// 设置类的静态字段的值
        /// </summary>
        /// <param name="type">要设置静态字段的类</param>
        /// <param name="fieldName">要设置的静态字段的名称</param>
        /// <param name="value">要设置的静态字段的值</param>
        public static void SetStaticFieldValue(Type type, string fieldName, object value)
        {
            FieldInfo field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.Public);
            field.SetValue(null, value);
        }

        #endregion
    }
}
