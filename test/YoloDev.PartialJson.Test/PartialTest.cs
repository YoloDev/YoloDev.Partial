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

    [Fact]
    public void Test()
    {
      var json = "{\"Foo\":\"testing\",\"Bar\":42}";
      var partialResponse = JsonConvert.DeserializeObject<Partial<TestData>>(json);
      if (partialResponse.Proxy is IPartialProxy proxy)
      {
        var partial = proxy.Partial;
        Assert.Same(partialResponse, partial);
      }

      if (partialResponse.Proxy is IPartialProxy<TestData> proxy2)
      {
        var partial = proxy2.Partial;
        Assert.Same(partialResponse, partial);
      }

      Assert.True(partialResponse.IsSet(x => x.Foo));
      Assert.True(partialResponse.IsSet(x => x.Bar));
      Assert.False(partialResponse.IsSet(x => x.NotUsed));

      var updates = partialResponse.GetUpdates();
      Assert.Contains("Foo", updates.Keys);
      Assert.Contains("Bar", updates.Keys);
      Assert.DoesNotContain("NotUsed", updates.Keys);
      Assert.Equal("testing", updates["Foo"]);
      Assert.Equal(42, updates["Bar"]);
    }
  }
}
