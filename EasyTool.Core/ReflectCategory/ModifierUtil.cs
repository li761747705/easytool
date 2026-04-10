using System;
using System.Reflection;

namespace EasyTool.ReflectCategory
{
    /// <summary>
    /// 修饰符工具类
    /// 对标 Hutool 的 ModifierUtil
    /// 提供类型、方法、字段修饰符的判断
    /// </summary>
    public static class ModifierUtil
    {
        #region 方法修饰符判断

        /// <summary>
        /// 判断方法是否是公开的
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否公开</returns>
        public static bool IsPublic(MethodInfo? method)
        {
            return method != null && method.IsPublic;
        }

        /// <summary>
        /// 判断方法是否是私有的
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否私有</returns>
        public static bool IsPrivate(MethodInfo? method)
        {
            return method != null && method.IsPrivate;
        }

        /// <summary>
        /// 判断方法是否是保护的
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否保护</returns>
        public static bool IsProtected(MethodInfo? method)
        {
            return method != null && method.IsFamily;
        }

        /// <summary>
        /// 判断方法是否是静态的
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否静态</returns>
        public static bool IsStatic(MethodInfo? method)
        {
            return method != null && method.IsStatic;
        }

        /// <summary>
        /// 判断方法是否是抽象的
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否抽象</returns>
        public static bool IsAbstract(MethodInfo? method)
        {
            return method != null && method.IsAbstract;
        }

        /// <summary>
        /// 判断方法是否是密封的（不可重写）
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否密封</returns>
        public static bool IsSealed(MethodInfo? method)
        {
            return method != null && method.IsFinal;
        }

        /// <summary>
        /// 判断方法是否是虚方法
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否虚方法</returns>
        public static bool IsVirtual(MethodInfo? method)
        {
            return method != null && method.IsVirtual && !method.IsFinal;
        }

        /// <summary>
        /// 判断方法是否是重写方法
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <returns>是否重写</returns>
        public static bool IsOverride(MethodInfo? method)
        {
            return method != null && method.IsVirtual && !method.IsAbstract
                && (method.Attributes & MethodAttributes.ReuseSlot) == 0;
        }

        #endregion

        #region 字段修饰符判断

        /// <summary>
        /// 判断字段是否是公开的
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否公开</returns>
        public static bool IsPublic(FieldInfo? field)
        {
            return field != null && field.IsPublic;
        }

        /// <summary>
        /// 判断字段是否是私有的
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否私有</returns>
        public static bool IsPrivate(FieldInfo? field)
        {
            return field != null && field.IsPrivate;
        }

        /// <summary>
        /// 判断字段是否是保护的
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否保护</returns>
        public static bool IsProtected(FieldInfo? field)
        {
            return field != null && field.IsFamily;
        }

        /// <summary>
        /// 判断字段是否是静态的
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否静态</returns>
        public static bool IsStatic(FieldInfo? field)
        {
            return field != null && field.IsStatic;
        }

        /// <summary>
        /// 判断字段是否是只读的
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否只读</returns>
        public static bool IsReadonly(FieldInfo? field)
        {
            return field != null && field.IsInitOnly;
        }

        /// <summary>
        /// 判断字段是否是常量
        /// </summary>
        /// <param name="field">字段信息</param>
        /// <returns>是否常量</returns>
        public static bool IsConstant(FieldInfo? field)
        {
            return field != null && field.IsLiteral;
        }

        #endregion

        #region 类型修饰符判断

        /// <summary>
        /// 判断类型是否是公开的
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否公开</returns>
        public static bool IsPublic(Type? type)
        {
            return type != null && type.IsPublic;
        }

        /// <summary>
        /// 判断类型是否是非公开的
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否非公开</returns>
        public static bool IsNotPublic(Type? type)
        {
            return type != null && type.IsNotPublic;
        }

        /// <summary>
        /// 判断类型是否是密封的
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否密封</returns>
        public static bool IsSealed(Type? type)
        {
            return type != null && type.IsSealed;
        }

        /// <summary>
        /// 判断类型是否是抽象的
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>是否抽象</returns>
        public static bool IsAbstract(Type? type)
        {
            return type != null && type.IsAbstract;
        }

        #endregion

        #region 属性修饰符判断

        /// <summary>
        /// 判断属性是否是静态的
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <returns>是否静态</returns>
        public static bool IsStatic(PropertyInfo? property)
        {
            if (property == null)
                return false;

            var getMethod = property.GetMethod;
            var setMethod = property.SetMethod;

            return (getMethod != null && getMethod.IsStatic) ||
                   (setMethod != null && setMethod.IsStatic);
        }

        /// <summary>
        /// 判断属性是否是只读的
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <returns>是否只读</returns>
        public static bool IsReadonly(PropertyInfo? property)
        {
            return property != null && property.CanRead && !property.CanWrite;
        }

        /// <summary>
        /// 判断属性是否是只写的
        /// </summary>
        /// <param name="property">属性信息</param>
        /// <returns>是否只写</returns>
        public static bool IsWriteOnly(PropertyInfo? property)
        {
            return property != null && !property.CanRead && property.CanWrite;
        }

        #endregion

        #region 构造函数修饰符判断

        /// <summary>
        /// 判断构造函数是否是公开的
        /// </summary>
        /// <param name="constructor">构造函数信息</param>
        /// <returns>是否公开</returns>
        public static bool IsPublic(ConstructorInfo? constructor)
        {
            return constructor != null && constructor.IsPublic;
        }

        /// <summary>
        /// 判断构造函数是否是私有的
        /// </summary>
        /// <param name="constructor">构造函数信息</param>
        /// <returns>是否私有</returns>
        public static bool IsPrivate(ConstructorInfo? constructor)
        {
            return constructor != null && constructor.IsPrivate;
        }

        /// <summary>
        /// 判断构造函数是否是静态的
        /// </summary>
        /// <param name="constructor">构造函数信息</param>
        /// <returns>是否静态</returns>
        public static bool IsStatic(ConstructorInfo? constructor)
        {
            return constructor != null && constructor.IsStatic;
        }

        #endregion

        #region 综合判断

        /// <summary>
        /// 判断成员是否具有指定修饰符
        /// </summary>
        /// <param name="member">成员信息</param>
        /// <param name="modifier">修饰符</param>
        /// <returns>是否具有</returns>
        public static bool HasModifier(MemberInfo member, Modifier modifier)
        {
            return member switch
            {
                MethodInfo method => HasMethodModifier(method, modifier),
                FieldInfo field => HasFieldModifier(field, modifier),
                Type type => HasTypeModifier(type, modifier),
                PropertyInfo property => HasPropertyModifier(property, modifier),
                ConstructorInfo constructor => HasConstructorModifier(constructor, modifier),
                _ => false
            };
        }

        private static bool HasMethodModifier(MethodInfo method, Modifier modifier)
        {
            return modifier switch
            {
                Modifier.Public => method.IsPublic,
                Modifier.Private => method.IsPrivate,
                Modifier.Protected => method.IsFamily,
                Modifier.Static => method.IsStatic,
                Modifier.Abstract => method.IsAbstract,
                Modifier.Sealed => method.IsFinal,
                Modifier.Virtual => method.IsVirtual && !method.IsFinal,
                _ => false
            };
        }

        private static bool HasFieldModifier(FieldInfo field, Modifier modifier)
        {
            return modifier switch
            {
                Modifier.Public => field.IsPublic,
                Modifier.Private => field.IsPrivate,
                Modifier.Protected => field.IsFamily,
                Modifier.Static => field.IsStatic,
                Modifier.Readonly => field.IsInitOnly,
                Modifier.Constant => field.IsLiteral,
                _ => false
            };
        }

        private static bool HasTypeModifier(Type type, Modifier modifier)
        {
            return modifier switch
            {
                Modifier.Public => type.IsPublic,
                Modifier.Private => type.IsNotPublic,
                Modifier.Sealed => type.IsSealed,
                Modifier.Abstract => type.IsAbstract,
                _ => false
            };
        }

        private static bool HasPropertyModifier(PropertyInfo property, Modifier modifier)
        {
            return modifier switch
            {
                Modifier.Static => IsStatic(property),
                Modifier.Readonly => IsReadonly(property),
                _ => false
            };
        }

        private static bool HasConstructorModifier(ConstructorInfo constructor, Modifier modifier)
        {
            return modifier switch
            {
                Modifier.Public => constructor.IsPublic,
                Modifier.Private => constructor.IsPrivate,
                Modifier.Static => constructor.IsStatic,
                _ => false
            };
        }

        #endregion
    }

    /// <summary>
    /// 修饰符枚举
    /// </summary>
    [Flags]
    public enum Modifier
    {
        /// <summary>
        /// 无修饰符
        /// </summary>
        None = 0,

        /// <summary>
        /// 公开的
        /// </summary>
        Public = 1,

        /// <summary>
        /// 私有的
        /// </summary>
        Private = 2,

        /// <summary>
        /// 保护的
        /// </summary>
        Protected = 4,

        /// <summary>
        /// 静态的
        /// </summary>
        Static = 8,

        /// <summary>
        /// 抽象的
        /// </summary>
        Abstract = 16,

        /// <summary>
        /// 密封的
        /// </summary>
        Sealed = 32,

        /// <summary>
        /// 虚方法
        /// </summary>
        Virtual = 64,

        /// <summary>
        /// 只读的
        /// </summary>
        Readonly = 128,

        /// <summary>
        /// 常量
        /// </summary>
        Constant = 256
    }
}