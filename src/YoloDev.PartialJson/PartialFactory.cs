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
      // TODO: Filter interceptors, only run on Set & add PartialProxyInterceptor
      var options = new ProxyGenerationOptions(new PartialProxyGenerationHook(typeof(T)));
      options.AddMixinInstance(new PartialProxy<T>(owner));

      var proxy = _proxyGenerator.CreateClassProxy(
        typeof(T), 
        options,
        new PartialInterceptor<T>(owner));

      return (T)proxy;
    }
  }
}
