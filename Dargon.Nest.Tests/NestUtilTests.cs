using Xunit;
using static NMockito.NMockitoStatic;

namespace Dargon.Nest.Tests {
   public class NestUtilTests {
      [Fact]
      public void CombineTests() {
         AssertEquals("http://dargon.io/derp", NestUtil.CombineUrl("http://dargon.io", "derp"));
         AssertEquals("http://dargon.io/derp/lerp", NestUtil.CombineUrl("http://dargon.io/derp", "lerp"));
         AssertEquals("http://dargon.io/derp/lerp", NestUtil.CombineUrl("http://dargon.io/", "derp/lerp"));
      }
   }
}
