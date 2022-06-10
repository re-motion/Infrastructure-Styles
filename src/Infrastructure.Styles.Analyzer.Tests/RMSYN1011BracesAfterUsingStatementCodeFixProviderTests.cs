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

namespace Infrastructure.Styles.Analyzer.Tests
{
  [TestFixture]
  public class RMSYN1011BracesAfterUsingStatementCodeFixProviderTests
  {
    private static readonly RMSYN1101BracesAfterUsingStatementAnalyzer Analyzer = new();
    private static readonly RMSYN1011BracesAfterUsingStatementCodeFixProvider CodeFixProvider = new();

    [Test]
    public void CodeFix_WithOneUsingStatement ()
    {
      var before = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var d = new D())
                ↓d.ibimsafunktion();
        }
    }
}";
      ;
      var after = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var d = new D())
            {
                d.ibimsafunktion();
            }
        }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithNestedUsingStatements ()
    {
      var before = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var o = new D())
            using (var d = new D())
                ↓d.ibimsafunktion();
        }
    }
}";
      var after = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var o = new D())
            using (var d = new D())
            {
                d.ibimsafunktion();
            }
        }
    }
}";
      RoslynAssert.CodeFix(Analyzer, CodeFixProvider, before, after);
    }

    [Test]
    public void CodeFix_WithMultipleUsingStatementsAndNesting ()
    {
      var before = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var o = new D())
            {
                o.ibimsafunktion();
                using (var d = new D())
                    ↓d.ibimsafunktion();
            }
            using (var d = new D())
                ↓d.ibimsafunktion();
        }
    }
}";
      var after = @"
using System;

namespace N
{
    class D: IDisposable
    {
        public void ibimsafunktion() {}

        public void Dispose() {}
    }

    class C
    {
        private void Main()
        {
            using (var o = new D())
            {
                o.ibimsafunktion();
                using (var d = new D())
                {
                    d.ibimsafunktion();
                }
            }
            using (var d = new D())
            {
                d.ibimsafunktion();
            }
        }
    }
}";
      RoslynAssert.FixAll(Analyzer, CodeFixProvider, before, after);
    }
  }
}