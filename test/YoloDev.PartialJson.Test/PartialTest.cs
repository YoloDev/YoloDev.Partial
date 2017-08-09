using Newtonsoft.Json;
using System;
using Xunit;

namespace YoloDev.PartialJson.Test
{
  public class PartialTest
  {
    public class TestData
    {
      public virtual string Foo { get; set; }
      public virtual int Bar { get; set; }
      public virtual string NotUsed { get; set; }
    }

    private static T AssertIs<T>(object obj)
    {
      Assert.IsAssignableFrom(typeof(T), obj);
      return (T)obj;
    }

    [Fact]
    public void Test()
    {
      var json = "{\"Foo\":\"testing\",\"Bar\":42}";
      var partialResponse = JsonConvert.DeserializeObject<IPartial<TestData>>(json);

      var proxy = AssertIs<IPartialProxy>(partialResponse.Proxy);
      var partial = proxy.Partial;
      Assert.Same(partialResponse, partial);

      var typedProxy = AssertIs<IPartialProxy<TestData>>(partialResponse.Proxy);
      var typedPartial = typedProxy.Partial;
      Assert.Same(partialResponse, typedPartial);

      Assert.True(partialResponse.IsSet(x => x.Foo));
      Assert.True(partialResponse.IsSet(x => x.Bar));
      Assert.False(partialResponse.IsSet(x => x.NotUsed));

      var updates = partialResponse.GetUpdates();
      Assert.Contains("Foo", updates.Keys);
      Assert.Contains("Bar", updates.Keys);
      Assert.DoesNotContain("NotUsed", updates.Keys);
      Assert.Equal("testing", updates["Foo"]);
      Assert.Equal(42, updates["Bar"]);

      Assert.Equal("testing", partialResponse.Proxy.Foo);
      Assert.Equal(42, partialResponse.Proxy.Bar);

      var target = new TestData();
      partialResponse.Populate(target);

      Assert.Equal("testing", target.Foo);
      Assert.Equal(42, target.Bar);
    }
  }
}
