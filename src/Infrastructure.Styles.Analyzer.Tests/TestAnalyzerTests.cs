using Gu.Roslyn.Asserts;
using NUnit.Framework;

[assembly: MetadataReference (typeof (object))]

namespace Infrastructure.Styles.Analyzer.Tests
{
  public class Tests
  {
    private static readonly TestAnalyzer Analyzer = new TestAnalyzer();

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
      var code = @"
namespace N
{
    class C
    {
    }
}";
      RoslynAssert.Valid (Analyzer, code);
    }
  }
}