// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

using System.Text.RegularExpressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Regal;

BenchmarkRunner.Run<RegalBenchmarks>(args: args);

public class RegalBenchmarks
{
    private readonly string _teaText = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "pg68148.txt"));
    private readonly string[] _1000MostCommonEnglishWords = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "english1000.txt"));

    private Regex _regex = null!;
    private Regex _compiledRegex = null!;
    private StringMatcher _regal = null!;

    [Params(10, 100, 500, 1000)]
    public int WordCount { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        var regexPattern = string.Join("|", _1000MostCommonEnglishWords.Take(WordCount));
        _regex = new Regex(regexPattern);
        _compiledRegex = new Regex(regexPattern, RegexOptions.Compiled);
        _regal = new StringMatcher(_1000MostCommonEnglishWords.AsSpan(0, WordCount));
    }

    [Benchmark(Baseline = true)]
    public int CountWordsRegex() => _regex.Count(_teaText);

    [Benchmark]
    public int CountWordsCompiledRegex() => _compiledRegex.Count(_teaText);

    [Benchmark]
    public int CountWordsRegal() => _regal.Count(_teaText);
}
