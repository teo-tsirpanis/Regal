// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

using Regal.AhoCorasick;

namespace Regal;

public sealed class StringMatcher
{
    private readonly AhoCorasickMatcher _matcher;

    private readonly string[] _words;

    public ReadOnlySpan<string> Words => _words;

    public StringMatcher(ReadOnlySpan<string> words)
    {
        _words = words.ToArray();
        Init(words, out _matcher);
    }

    public StringMatcher(string firstWord, params string[] otherWords)
    {
        ArgumentException.ThrowIfNullOrEmpty(firstWord);
        ArgumentNullException.ThrowIfNull(otherWords);

        _words = Utilities.ToArrayPrefixed(firstWord, otherWords);
        Init(_words, out _matcher);
    }

    private static void ValidateWords(ReadOnlySpan<string> words)
    {
        if (words.IsEmpty)
        {
            throw new ArgumentException("Word list must not be empty.", nameof(words));
        }

        foreach (string s in words)
        {
            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentException("Word must not be null or empty.", nameof(words));
            }
        }
    }

    private static void Init(ReadOnlySpan<string> words, out AhoCorasickMatcher matcher)
    {
        ValidateWords(words);

        matcher = new AhoCorasickMatcher(words);
    }

    public (int Index, int StringNumber) Find(ReadOnlySpan<char> text)
    {
        return _matcher.Find(text);
    }
}
