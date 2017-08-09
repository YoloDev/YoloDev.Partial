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
    private readonly ISet<PropertyInfo> _set;
    private readonly T _proxy;

    [JsonConstructor]
    public Partial()
    {
      _set = new HashSet<PropertyInfo>();
      _proxy = PartialFactory.Instance.CreateProxy<T>(this);
    }

    internal ISet<PropertyInfo> SetProperties => _set;

    object IPartial.Proxy => _proxy;
    public T Proxy => _proxy;

    public bool IsSet(PropertyInfo propertyInfo) => _set.Contains(propertyInfo);

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

      return IsSet((PropertyInfo)memberInfo);
    }

    public IImmutableDictionary<string, object> GetUpdates()
      => GetUpdates(DefaultNameConverter.Instance);

    public IImmutableDictionary<string, object> GetUpdates(Func<PropertyInfo, string> nameConverter) =>
      GetUpdates(new FuncNameConverter(nameConverter));

    public IImmutableDictionary<string, object> GetUpdates(INameConverter nameConverter) =>
      _set.ToImmutableDictionary(
        prop => nameConverter.GetName(prop),
        prop => Property.Get(prop, _proxy));

    void IPartial.Populate(object target)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(target));
      }

      if (!(target is T typedTarget))
      {
        throw new ArgumentException($"Cannot populate object of type {target.GetType()}", nameof(target));
      }

      Populate(typedTarget);
    }

    void IPartial.Populate(object target, Predicate<PropertyInfo> propertyFilter)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(propertyFilter));
      }

      if (!(target is T typedTarget))
      {
        throw new ArgumentException($"Cannot populate object of type {target.GetType()}", nameof(target));
      }

      Populate(typedTarget, propertyFilter);
    }

    void IPartial.Populate(object target, IPropertyFilter propertyFilter)
    {
      if (target == null)
      {
        throw new ArgumentNullException(nameof(propertyFilter));
      }

      if (!(target is T typedTarget))
      {
        throw new ArgumentException($"Cannot populate object of type {target.GetType()}", nameof(target));
      }

      Populate(typedTarget, propertyFilter);
    }

    public void Populate(T target)
    {
      Populate(target, DefaultPropertyFilter.Instance);
    }

    public void Populate(T target, Predicate<PropertyInfo> propertyFilter)
    {
      Populate(target, new PredicatePropertyFilter(propertyFilter));
    }

    public void Populate(T target, IPropertyFilter propertyFilter)
    {
      foreach (var prop in _set)
      {
        if (propertyFilter.IncludeProperty(prop))
        {
          Property.Copy(prop, _proxy, target);
        }
      }
    }
  }
}
