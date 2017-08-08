using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace YoloDev.PartialJson
{
  public class PartialConverter : JsonConverter
  {
    static ImmutableDictionary<Type, Func<object>> _factories = 
      ImmutableDictionary.Create<Type, Func<object>>();

    public override bool CanConvert(Type objectType)
    {
      return objectType.IsAssignableFrom(typeof(IPartial));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      var proxyType = objectType.GetGenericArguments()[0];

      if (existingValue == null)
      {
        existingValue = CreateInstance(proxyType);
      }

      object proxy = ((IPartial)existingValue).Proxy;
      serializer.Populate(reader, proxy);
      return existingValue;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      throw new NotSupportedException("Cannot write partial objects to json");
    }

    private static object CreateInstance(Type type)
    {
      var factory = ImmutableInterlocked.GetOrAdd(ref _factories, type, CreateFactory);
      return factory();
    }

    private static Func<object> CreateFactory(Type type)
    {
      ConstructorInfo ci = typeof(Partial<>)
        .MakeGenericType(type)
        .GetConstructor(Type.EmptyTypes);

      var newExpression = Expression.New(ci);
      var castedExpression = Expression.Convert(newExpression, typeof(object));
      var lambdaExpression = Expression.Lambda<Func<object>>(castedExpression);
      return lambdaExpression.Compile();
    }
  }
}
