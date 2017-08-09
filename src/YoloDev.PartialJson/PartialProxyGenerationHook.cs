using Castle.DynamicProxy;
using System;
using System.Reflection;

namespace YoloDev.PartialJson
{
  internal class PartialProxyGenerationHook : IProxyGenerationHook
  {
    readonly Type _type;

    public PartialProxyGenerationHook(Type type)
    {
      _type = type;
    }

    public void MethodsInspected()
    {
    }

    public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
    {
    }

    public bool ShouldInterceptMethod(Type type, MethodInfo methodInfo)
    {
      if (type != _type) return false;

      var (prop, isGetter) = Property.Find(type, methodInfo);
      return prop != null && !isGetter;
    }
  }
}
