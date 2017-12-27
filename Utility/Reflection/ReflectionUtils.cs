using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using insp.Utility.Collections;

namespace insp.Utility.Reflection
{
    
    /// <summary>
    /// 反射工具
    /// </summary>
    public static class ReflectionUtils
    {
        #region Type
        /// <summary>
        /// 是否是基本数据类型，或日期时间类型，或者字符串
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsBasic(this Type type)
        {
            return type.IsPrimitive || type == typeof(DateTime) || type == typeof(String);
        }
        /// <summary>
        /// 在应用程序域查找类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static Type FindType(String typeName)
        {
            if (typeName == null || typeName == "") return null;
            if ("INT" == typeName.ToUpper()) return typeof(int);
            else if ("BYTE" == typeName.ToUpper()) return typeof(byte);
            else if ("CHAR" == typeName.ToUpper()) return typeof(char);
            else if ("FLOAT" == typeName.ToUpper()) return typeof(float);
            else if ("DOUBLE" == typeName.ToUpper()) return typeof(double);

            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            for(int i=0;i< assemblys.Length;i++)
            {
                Type t = assemblys[i].GetType(typeName);
                if (t != null) return t;
                t = assemblys[i].GetType("System."+typeName);
                if (t != null) return t;
            }
            return null;
        }
        /// <summary>
        /// 查找类型
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
        public static KeyValuePair<Assembly,Type> FindAssemblyAndType(String typeName)
        {
            if (typeName == null || typeName == "") return new KeyValuePair<Assembly, Type>();
            if ("INT" == typeName.ToUpper()) return new KeyValuePair<Assembly, Type>(Assembly.GetAssembly(typeof(int)),typeof(int));
            else if ("BYTE" == typeName.ToUpper()) return new KeyValuePair<Assembly, Type>(Assembly.GetAssembly(typeof(byte)), typeof(byte));
            else if ("CHAR" == typeName.ToUpper()) return new KeyValuePair<Assembly, Type>(Assembly.GetAssembly(typeof(char)), typeof(char));
            else if ("FLOAT" == typeName.ToUpper()) return new KeyValuePair<Assembly, Type>(Assembly.GetAssembly(typeof(float)), typeof(float));
            else if ("DOBULE" == typeName.ToUpper()) return new KeyValuePair<Assembly, Type>(Assembly.GetAssembly(typeof(double)), typeof(double));

            Assembly[] assemblys = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblys.Length; i++)
            {
                Type t = assemblys[i].GetType(typeName);
                if (t != null) return new KeyValuePair<Assembly, Type>(assemblys[i],t);
                t = assemblys[i].GetType("System." + typeName);
                if (t != null) return new KeyValuePair<Assembly, Type>(assemblys[i], t); ;
            }
            return new KeyValuePair<Assembly, Type>(null,null);
        }

        public static bool Equals(Type[] t1, Type[] t2, bool inhertied = false)
        {
            if (t1 == null && t2 == null) return true;
            if (t1 == null || t2 == null) return false;
            if (t1.Length != t2.Length) return false;
            for (int i = 0; i < t1.Length; i++)
            {
                if ((inhertied && t1[i] != t1[i]) || !inhertied && (t1[i] != t2[i] || t1[i].IsAssignableFrom(t2[i])))
                    return false;
            }
            return true;
        }
        #endregion
        #region Attribute
        public static T FindAttribute<T>(this Type type,bool inhertied=true) where T : Attribute
        {
            Object[] attrs = type.GetCustomAttributes(typeof(T), inhertied);
            return attrs == null || attrs.Length <= 0 ? null : (T)attrs[0];
        }
        #endregion

        #region MemberInfo
        public static bool HasName(this MemberInfo member,String name)
        {
            if (member.Name == name)
                return true;
            Text.TextAttribute attr = member.GetCustomAttribute<Text.TextAttribute>();
            if (attr == null) return false;
            return attr.HasName(name);
        }
        public static MemberInfo FindMember(this Type type, String membername)
        {
            MemberInfo[] mems = type.GetMembers();
            return mems.FirstOrDefault(x => x.Name == membername);
          
        }

        #endregion

        #region MemberValue
        public static Object FindMemberValue(this Object obj,String[] propNames,int begin=0)
        {
            for(int i = begin;i< propNames.Length;i++)
            {
                if(propNames[i] != null)
                    propNames[i] = propNames[i].Trim();
                obj = FindMemberValue(obj, propNames[i]);
                if (obj == null) return null;
            }
            return obj;
        }

        public static Object FindMemberValue(this Object obj,String membername)
        {
            Type type = obj.GetType();
            MemberInfo[] mems = type.GetMember(membername,BindingFlags.Default);
            if (mems == null || mems.Length <= 0)
                return null;
            return FindValue(mems[0], obj);
            
        }

        public static T FindMemberValue<T>(this Object obj, String membername)
        {
            Type type = obj.GetType();
            MemberInfo[] mems = type.GetMember(membername, BindingFlags.Default);
            if (mems == null || mems.Length <= 0)
                return default(T);
            return FindValue<T>(mems[0], obj);

        }

        public static Object FindValue(this MemberInfo member, Object owner)
        {
            if (member.MemberType == MemberTypes.Field)
                return ((FieldInfo)member).GetValue(owner);
            else if (member.MemberType == MemberTypes.Property)
                return ((PropertyInfo)member).GetValue(owner);

            return null;

        }

        public static T FindValue<T>(this MemberInfo member,Object owner)
        {
            if (member.MemberType == MemberTypes.Field)
                return (T)((FieldInfo)member).GetValue(owner);
            else if (member.MemberType == MemberTypes.Property)
                return (T)((PropertyInfo)member).GetValue(owner);

            return default(T);
        }
        public static void SetMemberValue(this Object obj,String memberName,Object value,bool throwException = false)
        {
            if(obj == null)
            {
                if (throwException)
                    throw new Exception("修改成员值失败:对象无效："+memberName);
                return;
            }
            MemberInfo member = FindMember(obj.GetType(),memberName);
            if (member == null)
            {
                if (throwException)
                    throw new Exception("修改成员值失败:找不到成员：" + memberName+":"+obj.ToString());
                return;
            }
            try
            {
                SetValue(member,obj,value);
            }
            catch(Exception e)
            {
                if (throwException)
                    throw new Exception("修改成员值失败:"+e.Message);
            }

        }
        public static void SetValue(this MemberInfo member, Object owner,Object value)
        {
            if (member.MemberType == MemberTypes.Field)
                ((FieldInfo)member).SetValue(owner, value);
            else if (member.MemberType == MemberTypes.Property)
            {
                if(((PropertyInfo)member).GetSetMethod() != null)
                    ((PropertyInfo)member).GetSetMethod().Invoke(owner, new Object[] { value });
            }
        }

        #endregion

        #region 泛型方法
        /// <summary>
        /// 查找泛型方法
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="genericTypes"></param>
        /// <returns></returns>
        public static MethodInfo FindGenericMethod(this Type type,String methodName,Type[] paramTypes,params Type[] genericTypes)
        {
            MethodInfo[] methods = type.GetMethods();
            foreach(MethodInfo method in methods)
            {
                if (method.Name != methodName)
                    continue;
                if (!method.IsGenericMethod)
                    continue;
                Type[] genericParamTypes = method.GetGenericArguments();
                if (genericParamTypes.Length != genericTypes.Length)
                    continue;

                MethodInfo methodInstance = method.MakeGenericMethod(genericTypes);

                if (paramTypes == null)
                    return methodInstance;
                ParameterInfo[] paramInfos = methodInstance.GetParameters();

                if (paramInfos == null || paramInfos.Length <= 0)
                    return methodInstance;
                if (paramTypes.Length != paramInfos.Length)
                    continue;
                
                bool paramTypeMatch = true;
                for(int i=0;i<paramInfos.Length;i++)
                {        
                    if(paramInfos[i].ParameterType != paramTypes[i] &&
                       !paramInfos[i].ParameterType.IsAssignableFrom(paramTypes[i]))
                    {
                        paramTypeMatch = false;
                        break;
                    }
                }
                if (!paramTypeMatch) continue;
                return methodInstance;

            }
            return null;
        }

        public static MethodInfo FindGenericMethod(this Type type, String methodName,  params Type[] genericTypes)
        {
            MethodInfo method = null;
            method = type.GetMethod(methodName);            
            if (method == null || !method.IsGenericMethod)
                return null;

            method = method.MakeGenericMethod(genericTypes);
            return method;
        }
        /// <summary>
        /// 调用范型方法 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="genericTypes"></param>
        /// <param name="owner"></param>
        /// <param name="param"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static Object CallGenericMethod(this Object owner, String methodName, Type[] genericTypes,Object[] param, Type[] paramTypes=null)
        {
            Type type = owner.GetType();
            MethodInfo method = FindGenericMethod(type, methodName, paramTypes, genericTypes);
            if (method == null) return null;
            return method.Invoke(owner, param);
        }

        /// <summary>
        /// 调用范型方法 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="genericTypes"></param>
        /// <param name="owner"></param>
        /// <param name="param"></param>
        /// <param name="paramTypes"></param>
        /// <returns></returns>
        public static Object CallGenericMethod(this Type type, Object owner, String methodName, Type[] genericTypes, Object[] param, Type[] paramTypes = null)
        {            
            MethodInfo method = FindGenericMethod(type, methodName, paramTypes, genericTypes);
            if (method == null) return null;
            return method.Invoke(owner, param);
        }

        #endregion
    }
}
