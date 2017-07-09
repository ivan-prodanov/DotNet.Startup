using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace DotNet.Startup
{
    public class MethodLoader
    {
        public static bool TryGetMethodInfo<TDelegate>(Type type, string methodName, out MethodInfo result) where TDelegate : class
        {
            result = null;

            try
            {
                result = GetMethodInfo<TDelegate>(type, methodName);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static MethodInfo GetMethodInfo(Type type, string methodName, Type returnType)
        {
            var methodInfo = FindMethod(type, methodName, returnType, required: false)
               ?? FindMethod(type, methodName, typeof(void), required: true);

            return methodInfo;
        }

        public static MethodInfo GetMethodInfo<TReturnType>(Type type, string methodName)
        {
            return GetMethodInfo(type, methodName, typeof(TReturnType));
        }

        public static Func<TReturnType> GetMethod<TReturnType>(object instance, string methodName, params object[] arguments)
        {
            var executeMethod = GetMethodInfo<TReturnType>(instance.GetType(), methodName);

            var argumentsExpression = arguments.Select(a => Expression.Constant(a));

            var xRef = Expression.Constant(instance);
            var callRef = Expression.Call(xRef, executeMethod, argumentsExpression);
            var lambda = Expression.Lambda(callRef);
            
            
            return lambda.Compile() as Func<TReturnType>;
        }

        public static Action GetMethod(object instance, string methodName, params object[] arguments)
        {
            var executeMethod = GetMethodInfo(instance.GetType(), methodName, typeof(void));

            var argumentsExpression = arguments.Select(a => Expression.Constant(a));

            var xRef = Expression.Constant(instance);
            var callRef = Expression.Call(xRef, executeMethod, argumentsExpression);
            var lambda = Expression.Lambda(callRef);
            
            return lambda.Compile() as Action;
        }

        private static MethodInfo FindMethod(Type configuratorType, string methodName, Type returnType = null, bool required = true)
        {
            var methods = configuratorType.GetTypeInfo().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var selectedMethods = methods.Where(method => method.Name.Equals(methodName)).ToList();
            if (selectedMethods.Count > 1)
            {
                throw new InvalidOperationException($"Having multiple overloads of method '{methodName}' is not supported.");
            }

            var methodInfo = selectedMethods.FirstOrDefault();
            if (methodInfo == null)
            {
                if (required)
                {
                    throw new InvalidOperationException($"A public method named '{methodName}' could not be found in the '{configuratorType.FullName}' type.");
                }
                return null;
            }

            if (returnType != null && methodInfo.ReturnType != returnType)
            {
                if (required)
                {
                    throw new InvalidOperationException($"The '{methodInfo.Name}' method in the type '{configuratorType.FullName}' must have a return type of '{returnType.Name}'.");
                }
                return null;
            }

            return methodInfo;
        }
    }
}
