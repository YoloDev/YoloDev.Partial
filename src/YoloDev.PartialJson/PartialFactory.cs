using Castle.DynamicProxy;

namespace YoloDev.PartialJson
{
  internal class PartialFactory
  {
    public static PartialFactory Instance { get; } = new PartialFactory();

    private PartialFactory() { }

    private readonly ProxyGenerator _proxyGenerator = new ProxyGenerator();

    internal T CreateProxy<T>(Partial<T> owner) where T : class
    {
      var proxy = _proxyGenerator.CreateClassProxy(
        typeof(T), 
        new[] { typeof(IPartialProxy), typeof(IPartialProxy<T>) }, 
        new PartialInterceptor<T>(owner));

      return (T)proxy;
    }
  }
}
