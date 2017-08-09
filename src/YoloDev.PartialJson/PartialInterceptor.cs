using Castle.DynamicProxy;

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
      var (prop, isGetter) = Property.Find(type, method);

      if (!isGetter)
      {
        _owner.SetProperties.Add(prop);
      }

      invocation.Proceed();
    }
  }
}
