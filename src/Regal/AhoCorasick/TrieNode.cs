// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

namespace Regal.AhoCorasick;

internal class TrieNode
{
    public const int Root = 0;
    
    public TrieNode()
    {
        Children = new Dictionary<char, int>();
        IsLeaf = false;
        Parent = -1;
        AccessingCharacter = char.MaxValue;
        SuffixLink = -1;
        WordID = -1;
        DictionaryLink = -1;
    }

    public Dictionary<char, int> Children;

    public bool IsLeaf;

    public int Parent;

    public char AccessingCharacter;

    public int SuffixLink;

    public int DictionaryLink;

    public int WordID;

#if DEBUG
    public string? Path;
    public string? SuffixLinkPath;
    public string? DictionaryLinkPath;

    public override string ToString() =>
        $"Path: {Path} Suffix Link: {SuffixLinkPath} Dictionary Link: {DictionaryLinkPath}";
#endif
}
