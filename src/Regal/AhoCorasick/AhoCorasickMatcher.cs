// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

namespace Regal.AhoCorasick;

internal class AhoCorasickMatcher
{
    private readonly List<TrieNode> _trie;
    private readonly string[] _words;

    public ReadOnlySpan<string> Words => _words;

    public AhoCorasickMatcher(ReadOnlySpan<string> words)
    {
        _trie = AhoCorasickBuilder.BuildTrie(words);
        _words = words.ToArray();
    }

    public (int Index, int StringNumber) Find(ReadOnlySpan<char> text)
    {
        var lastMatch = (Index: -1, StringNumber: -1);
        int currentState = TrieNode.Root;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            while (true)
            {
                if (_trie[currentState].Children.TryGetValue(c, out int nextState))
                {
                    currentState = nextState;
                    break;
                }
                // The algorithm effectively resets when we reach the root node.
                // If we had found a match before, we return it.
                if (currentState == TrieNode.Root)
                {
                    if (lastMatch.Index != -1)
                    {
                        return lastMatch;
                    }
                    break;
                }

                currentState = _trie[currentState].SuffixLink;
            }

            int dictLink = _trie[currentState].DictionaryLink;

            if (dictLink != TrieNode.Root)
            {
                // Found a match. We mark it and continue searching hoping it is getting bigger.
                int wordId = _trie[dictLink].WordId;
                int indexOfMatch = i + 1 - _words[wordId].Length;

                // We want to return the leftmost-longest match.
                // If this match starts later than the match we might have found before,
                // we cannot accept it because we want to return the leftmost match.
                // The match can start at the same position as the previous one, which
                // means that it is longer and we accept it.
                // The unsigned integer comparison will also always
                // succeed if the last match index is negative.
                if ((uint) indexOfMatch <= (uint) lastMatch.Index)
                {
                    lastMatch = (Index: indexOfMatch, StringNumber: wordId);
                }
            }
        }

        return lastMatch;
    }
}
