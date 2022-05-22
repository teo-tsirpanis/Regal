// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

namespace Regal.Tests;

public class StringMatcherTests
{
    // Matches the strings from the Wikipedia example.
    private readonly StringMatcher _simpleMatcher =
        new StringMatcher("a", "ab", "bab", "bc", "bca", "c", "caa");

    [Theory]
    [InlineData("a", 0, 0)]
    [InlineData("ab", 0, 1)]
    [InlineData("bab", 0, 2)]
    [InlineData("bc", 0, 3)]
    [InlineData("bca", 0, 4)]
    [InlineData("c", 0, 5)]
    [InlineData("caa", 0, 6)]
    [InlineData("caaa", 0, 6)]
    public void TestSimpleMatcher(string text, int index, int stringNumber)
    {
        var result = _simpleMatcher.Find(text);
        Assert.Equal((index, stringNumber), result);
    }

    [Theory]
    [InlineData("a", 1)]
    [InlineData("aa", 2)]
    [InlineData("caa", 1)]
    [InlineData("caaa", 2)]
    public void TestCount(string text, int count)
    {
        var result = _simpleMatcher.Count(text);
        Assert.Equal(count, result);
    }
}
