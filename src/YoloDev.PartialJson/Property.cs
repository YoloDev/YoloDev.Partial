using System;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace YoloDev.PartialJson
{
  internal static class Property
  {
    private static ImmutableDictionary<PropertyInfo, Func<object, object>> _get = 
      ImmutableDictionary.Create<PropertyInfo, Func<object, object>>();

    public static object Get(PropertyInfo prop, object proxy)
    {
      var getter = ImmutableInterlocked.GetOrAdd(ref _get, prop, CreateGetter);
      return getter(proxy);
    }

    public static (PropertyInfo prop, bool isGetter) Find(Type type, MethodInfo methodInfo)
    {
      var nonPublic = !methodInfo.IsPublic;
      var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
      foreach (var prop in properties)
      {
        if (prop.GetGetMethod(nonPublic) == methodInfo)
        {
          return (prop, true);
        }

        if (prop.GetSetMethod(nonPublic) == methodInfo)
        {
          return (prop, false);
        }
      }

      return (null, false);
    }

    private static Func<object, object> CreateGetter(PropertyInfo prop)
    {
      var objParam = Expression.Parameter(typeof(object), "obj");
      var typedParam = Expression.Convert(objParam, prop.DeclaringType);
      var accessExpression = Expression.Property(typedParam, prop);
      var castedExpression = Expression.Convert(accessExpression, typeof(object));
      var lambdaExpression = Expression.Lambda<Func<object, object>>(castedExpression, objParam);

      return lambdaExpression.Compile();
    }
  }
}
