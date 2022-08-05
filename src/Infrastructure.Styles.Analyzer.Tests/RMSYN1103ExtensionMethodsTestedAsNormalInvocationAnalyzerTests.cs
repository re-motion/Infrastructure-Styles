// This file is part of the re-motion Framework (www.re-motion.org)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-motion Framework is free software; you can redistribute it
// and/or modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1 of the
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.

using Gu.Roslyn.Asserts;
using NUnit.Framework;

using StyleAnalyzer = Infrastructure.Styles.Analyzer.RMSYN1103ExtensionMethodsTestedAsNormalInvocationSyntaxAnalyzer;

[assembly: TransitiveMetadataReferences(typeof(NUnitAttribute))]

namespace Infrastructure.Styles.Analyzer.Tests
{
    [TestFixture]
    public class RMSYN1103ExtensionMethodsTestedAsNormalInvocationAnalyzerTests
    {
        private static readonly StyleAnalyzer Analyzer =
            new StyleAnalyzer();

        [Test]
        public void Analyze_WithExtensionMethodAndAnnotation_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Test]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithExtensionMethodAndAttributeAnnotation_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [TestAttribute]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }
        
        [Test]
        public void Analyze_ExtensionMethodInsideIf_ReportsDiagnostic ()
        {
          var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [TestAttribute]
    private void Main()
    {
      int a = 2;
      if(true)
        ↓a.ibimsafunktion();
    }
  }
}";
          RoslynAssert.Diagnostics(Analyzer,
            ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
            code);
        }
        
        [Test]
        public void Analyze_ExtensionMethodInsideBlock_ReportsDiagnostic ()
        {
          var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [TestAttribute]
    private void Main()
    {
      int a = 2;
      {
        ↓a.ibimsafunktion();
      }
    }
  }
}";
          RoslynAssert.Diagnostics(Analyzer,
            ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
            code);
        }

        [Test]
        public void Analyze_WithNormalInvocationExtensionMethodAndAnnotation_DoesNotReportDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Test]
    private void Main()
    {
      int a = 2;
      D.ibimsafunktion(a);
    }
  }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public void Analyze_WithOnlyExtensionMethod_DoesntReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    private void Main()
    {
      int a = 2;
      a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public void Analyze_WithTestAndOtherAttributeCombined_ExtensionMethod_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

[System.AttributeUsage(System.AttributeTargets.Method)]  
public class Attribute : System.Attribute  
{ 
}  

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Attribute, Test]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithTestQualifiedNameAndOtherAttributeCombined_ExtensionMethod_ReportsDiagnostic ()
        {
            var code = @"

[System.AttributeUsage(System.AttributeTargets.Method)]  
public class Attribute : System.Attribute  
{ 
}  

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Attribute, NUnit.Framework.Test]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithTestAndOtherAttributeSeparate_ExtensionMethod_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

[System.AttributeUsage(System.AttributeTargets.Method)]  
public class Attribute : System.Attribute  
{ 
}  

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Attribute]
    [Test]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithArgumentedTestAttribute_ExtensionMethod_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Test(Author=""ibims"")]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithArgumentedTestAndOtherAttribute_ExtensionMethod_ReportsDiagnostic ()
        {
            var code = @"
using NUnit.Framework;

[System.AttributeUsage(System.AttributeTargets.Method)]  
public class Attribute : System.Attribute  
{ 
}  

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Test(Author=""ibims""), Attribute]
    private void Main()
    {
      int a = 2;
      ↓a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Diagnostics(Analyzer,
                ExpectedDiagnostic.Create(
                    StyleAnalyzer.DiagnosticId,
                    StyleAnalyzer.Message),
                code);
        }

        [Test]
        public void Analyze_WithOtherAttributeAndExtensionMethod_DoesntReportsDiagnostic ()
        {
            var code = @"

[System.AttributeUsage(System.AttributeTargets.Method)]  
public class Attribute : System.Attribute  
{ 
}  

namespace N
{
  public static class D
  {
    public static void ibimsafunktion(this int a) 
    {
    }
  }
  
  class C
  {
    [Attribute]
    private void Main()
    {
      int a = 2;
      a.ibimsafunktion();
    }
  }
}";
            RoslynAssert.Valid(Analyzer, code);
        }

        [Test]
        public void Analyze_WithExtensionMethodsAsArguments_IgnoresArguments ()
        {
          var code = @"
using NUnit.Framework;

namespace N
{
    public static class D
    {
        public static int ibimsafunktion(this int a, int b = 0) { return a; }

    }
    
    class C
    {
        [Test]
        private void Main()
        {
            int a = 2;
            int b = 5;
            ↓a.ibimsafunktion(b.ibimsafunktion());
        }
    }
}";
          RoslynAssert.Diagnostics(Analyzer,
            ExpectedDiagnostic.Create(
              StyleAnalyzer.DiagnosticId,
              StyleAnalyzer.Message),
            code);
        }
        
        [Test]
        public void Analyze_WithCallToOtherMethodUsingExtensionMethod_Valid ()
        {
          var code = @"

namespace N
{
    public static class D
    {
        public static int ibimsafunktion(this int a, int b = 0) { return a; }
    }
public class E
    {
        public int doSomething (int a)
        {
            int b = a.ibimsafunktion(); 
            return b;
        }
    }
    
    
    class C
    {
        [NUnit.Framework.Test]
        private void Main()
        {
            int a = 2;
            var e = new E();
            var c = e.doSomething(a);
        }

    }
}";
          RoslynAssert.Valid(Analyzer, code);
        }
    }
}