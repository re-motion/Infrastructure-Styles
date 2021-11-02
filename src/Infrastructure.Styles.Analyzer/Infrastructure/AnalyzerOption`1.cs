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

using Microsoft.CodeAnalysis.Diagnostics;

namespace Infrastructure.Styles.Analyzer.Infrastructure
{
  /// <summary>
  /// Contains the names of the analyzer settings that can be override using, for example, editorconfig.
  /// </summary>
  internal abstract class AnalyzerOption<T> : IAnalyzerOptionValueProvider<T>
  {
    public string Key { get; }

    public T Default { get; }

    protected AnalyzerOption (string key, T @default)
    {
      Key = key;
      Default = @default;
    }

    public abstract bool TryConvertValue (string value, out T result);
    
    /// <inheritdoc />
    public T GetOptionValue (SyntaxTreeAnalysisContext context)
    {
      var config = context.Options.AnalyzerConfigOptionsProvider.GetOptions(context.Tree);
      return config.TryGetValue(Key, out var stringValue) && TryConvertValue(stringValue, out var tValue)
        ? tValue
        : Default;
    }
  }
}