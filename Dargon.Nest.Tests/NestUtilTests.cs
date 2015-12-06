using Dargon.Nest.Internals;
using Xunit;
using static NMockito.NMockitoStatic;

namespace Dargon.Nest.Tests {
   public class NestUtilTests {
      [Fact]
      public void CombineTests() {
         AssertEquals("http://dargon.io/derp", IoUtilities.CombinePath("http://dargon.io", "derp"));
         AssertEquals("http://dargon.io/derp/lerp", IoUtilities.CombinePath("http://dargon.io/derp", "lerp"));
         AssertEquals("http://dargon.io/derp/lerp", IoUtilities.CombinePath("http://dargon.io/", "derp/lerp"));
      }
   }
}
