// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

namespace Regal.Tests;

public class StringMatcherTests
{
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
    public void Test1(string text, int index, int stringNumber)
    {
        var result = _simpleMatcher.Find(text);
        Assert.Equal((index, stringNumber), result);
    }
}
