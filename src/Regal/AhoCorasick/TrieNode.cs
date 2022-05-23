// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

namespace Regal.AhoCorasick;

internal sealed class TrieNode
{
    public const int Root = 0;

    public TrieNode(int parent, string word, int wordId, int charIndex)
    {
        Parent = parent;
        AccessingCharacter = word[charIndex];
        WordId = charIndex == word.Length -1 ? wordId : -1;
#if DEBUG
            Path = word.AsSpan(0, charIndex + 1).ToString();
#endif
    }

    public TrieNode()
    {
        Parent = -1;
        AccessingCharacter = '\0';
        WordId = -1;
#if DEBUG
            Path = "<root>";
#endif
    }

    public Dictionary<char, int> Children = new();

    public bool IsLeaf => WordId != -1;

    public readonly int Parent;

    public readonly char AccessingCharacter;

    public readonly int WordId = -1;

    public int SuffixLink = -1;

    public int DictionaryLink = -1;

#if DEBUG
    public readonly string Path;
    public string? SuffixLinkPath;
    public string? DictionaryLinkPath;

    public override string ToString() =>
        $"Path: {Path} Suffix Link: {SuffixLinkPath} Dictionary Link: {DictionaryLinkPath}";
#endif
}
