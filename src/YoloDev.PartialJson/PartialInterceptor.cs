using Castle.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YoloDev.PartialJson
{
  internal class PartialInterceptor<T> : IInterceptor
    where T : class
  {
    private readonly Partial<T> _owner;

    public PartialInterceptor(Partial<T> owner)
    {
      _owner = owner;
    }

    public void Intercept(IInvocation invocation)
    {
      var method = invocation.GetConcreteMethod() ?? invocation.Method;
      var type = method.DeclaringType;
      var nonPublic = !method.IsPublic;
      var prop = type
        .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Where(p => p.GetSetMethod(nonPublic) == method || p.GetGetMethod(nonPublic) == method)
        .FirstOrDefault();

      if (prop.Name == nameof(IPartialProxy.Partial) 
        && (type == typeof(IPartialProxy) 
        || type.GetTypeInfo().IsGenericType 
        && type.GetGenericTypeDefinition() == typeof(IPartialProxy<>)))
      {
        invocation.ReturnValue = _owner;
        return;
      }

      if (prop == null)
      {
        invocation.Proceed();
        return;
      }

      if (method == prop.GetGetMethod(nonPublic))
      {
        invocation.Proceed();
        return;
      }

      _owner.Values[prop] = invocation.GetArgumentValue(0);
      invocation.Proceed();
    }
  }
}
