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
using CodeFixProvider = Infrastructure.Styles.Analyzer.RMSYN1103ExtensionMethodsTestedAsNormalInvocationCodeFixProvider;

namespace Infrastructure.Styles.Analyzer.Tests
{
    public class RMSYN1103ExtensionMethodsAreTestedAsNormalInvocationCodeFixProviderTests
    {
        private static readonly StyleAnalyzer Analyzer =
            new StyleAnalyzer();

        private static readonly CodeFixProvider CodeFixProvider =
            new CodeFixProvider();

        [Test]
        public void CodeFix_BaseCase_FixesCode ()
        {
            var before = @"
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
            var after = @"
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
            RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
        }

        [Test]
        public void Codefix_WithArguments_FixesCode ()
        {
            var before = @"
using NUnit.Framework;

namespace N
{
    public static class D
    {
        public static void ibimsafunktion(this int a, int b = 0) 
        {
        }
    }
    
    class C
    {
        [Test]
        private void Main()
        {
            int a = 2;
            int b = 5;
            ↓a.ibimsafunktion(b);
        }
    }
}";
            var after = @"
using NUnit.Framework;

namespace N
{
    public static class D
    {
        public static void ibimsafunktion(this int a, int b = 0) 
        {
        }
    }
    
    class C
    {
        [Test]
        private void Main()
        {
            int a = 2;
            int b = 5;
            D.ibimsafunktion(a, b);
        }
    }
}";
            RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
        }

        [Test]
        public void Codefix_WithExtensionCallsInArguments_FixesCode ()
        {
            var before = @"
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
            var after = @"
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
            D.ibimsafunktion(a, b.ibimsafunktion());
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
        }

        [Test]
        public void Codefix_WithExtensionCallsInArguments_DoesntFixArguments ()
        {
            var before = @"
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
            ↓a.ibimsafunktion(b.ibimsafunktion(a.ibimsafunktion(b)));
        }
    }
}";
            var after = @"
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
            D.ibimsafunktion(a, b.ibimsafunktion(a.ibimsafunktion(b)));
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
        }
        [Test]
        public void CodeFix_WithNewlineWhitespaces_FixesCodeAndKeepsWhitespace()
        {
            var before = @"
using NUnit.Framework;

namespace N
{
    public static class D
    {
        public static int ibimsafunktion(this int a, int b = 0, int c = 0) { return a; }

    }
    
    class C
    {
        [Test]
        private void Main()
        {
            int a = 2;
            int b = 5;
            int c = 3;
            ↓a.ibimsafunktion(
                b,
                c);
        }
    }
}";
            var after = @"
using NUnit.Framework;

namespace N
{
    public static class D
    {
        public static int ibimsafunktion(this int a, int b = 0, int c = 0) { return a; }

    }
    
    class C
    {
        [Test]
        private void Main()
        {
            int a = 2;
            int b = 5;
            int c = 3;
            D.ibimsafunktion(
                a,
                b,
                c);
        }
    }
}";
            RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
        }
    }
}