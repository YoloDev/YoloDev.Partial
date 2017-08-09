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

    private static ImmutableDictionary<PropertyInfo, Action<object, object>> _set =
      ImmutableDictionary.Create<PropertyInfo, Action<object, object>>();

    private static ImmutableDictionary<PropertyInfo, Action<object, object>> _copy =
      ImmutableDictionary.Create<PropertyInfo, Action<object, object>>();

    public static object Get(PropertyInfo prop, object proxy)
    {
      var getter = ImmutableInterlocked.GetOrAdd(ref _get, prop, CreateGetter);
      return getter(proxy);
    }

    public static void Set(PropertyInfo prop, object proxy, object value)
    {
      var setter = ImmutableInterlocked.GetOrAdd(ref _set, prop, CreateSetter);
      setter(proxy, value);
    }

    public static void Copy(PropertyInfo prop, object source, object target)
    {
      var copy = ImmutableInterlocked.GetOrAdd(ref _copy, prop, CreateCopy);
      copy(source, target);
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

    private static Action<object, object> CreateSetter(PropertyInfo prop)
    {
      var objParam = Expression.Parameter(typeof(object), "obj");
      var valParam = Expression.Parameter(typeof(object), "val");
      var typedParam = Expression.Convert(objParam, prop.DeclaringType);
      var typedVal = Expression.Convert(valParam, prop.PropertyType);
      var accessExpression = Expression.Property(typedParam, prop);
      var assignExpression = Expression.Assign(accessExpression, typedVal);
      var lambdaExpression = Expression.Lambda<Action<object, object>>(assignExpression, objParam, valParam);

      return lambdaExpression.Compile();
    }

    private static Action<object, object> CreateCopy(PropertyInfo prop)
    {
      var sourceParam = Expression.Parameter(typeof(object), "source");
      var targetParam = Expression.Parameter(typeof(object), "target");
      var typedSource = Expression.Convert(sourceParam, prop.DeclaringType);
      var typedTarget = Expression.Convert(targetParam, prop.DeclaringType);
      var sourceAccess = Expression.Property(typedSource, prop);
      var targetAccess = Expression.Property(typedTarget, prop);
      var assignExpression = Expression.Assign(targetAccess, sourceAccess);
      var lambdaExpression = Expression.Lambda<Action<object, object>>(assignExpression, sourceParam, targetParam);

      return lambdaExpression.Compile();
    }
  }
}
