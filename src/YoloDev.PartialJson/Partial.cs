using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Reflection;

namespace YoloDev.PartialJson
{
  [JsonConverter(typeof(PartialConverter))]
  public sealed class Partial<T> : IPartial<T>
    where T : class
  {
    private readonly IDictionary<PropertyInfo, object> _values;
    private readonly T _proxy;

    [JsonConstructor]
    public Partial()
    {
      _values = new Dictionary<PropertyInfo, object>();
      _proxy = PartialFactory.Instance.CreateProxy<T>(this);
    }

    internal IDictionary<PropertyInfo, object> Values => _values;

    object IPartial.Proxy => _proxy;
    public T Proxy => _proxy;

    public bool IsSet<R>(Expression<Func<T,R>> expr)
    {
      if (!(expr.Body is MemberExpression propExpr))
      {
        throw new ArgumentException("Needs member expression", nameof(expr));
      }

      var memberInfo = propExpr.Member;
      if (memberInfo.MemberType != MemberTypes.Property)
      {
        throw new ArgumentException("Member expression must be a property", nameof(expr));
      }

      return _values.ContainsKey((PropertyInfo)memberInfo);
    }

    public IImmutableDictionary<string, object> GetUpdates()
      => GetUpdates(DefaultNameConverter.Instance);

    public IImmutableDictionary<string, object> GetUpdates(INameConverter nameConverter) =>
      _values.ToImmutableDictionary(
        kvp => nameConverter.GetName(kvp.Key),
        kvp => kvp.Value);
  }
}
