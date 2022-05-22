// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

using Regal.AhoCorasick;

namespace Regal;

public sealed class StringMatcher
{
    private readonly AhoCorasickMatcher _matcher;

    public ReadOnlySpan<string> Words => _matcher.Words;

    public StringMatcher(ReadOnlySpan<string> words)
    {
        Init(words, out _matcher);
    }

    public StringMatcher(string firstWord, params string[] otherWords)
    {
        ArgumentException.ThrowIfNullOrEmpty(firstWord);
        ArgumentNullException.ThrowIfNull(otherWords);

        string[] words = Utilities.ToArrayPrefixed(firstWord, otherWords);
        Init(words, out _matcher);
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
    
    public int Count(ReadOnlySpan<char> text)
    {
        ReadOnlySpan<string> words = Words;
        int count = 0;
        while (true)
        {
            var findResult = Find(text);
            if (findResult.Index == -1)
            {
                break;
            }

            count++;
            text = text[(findResult.Index + words[findResult.StringNumber].Length)..];
        }

        return count;
    }
}
