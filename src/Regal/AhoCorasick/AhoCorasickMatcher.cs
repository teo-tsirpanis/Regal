// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

using System.Runtime.InteropServices;

namespace Regal.AhoCorasick;

internal class AhoCorasickMatcher
{
    private readonly List<TrieNode> _trie;
    private readonly string[] _words;
    private int size;

    public ReadOnlySpan<string> Words => _words;

    public AhoCorasickMatcher(ReadOnlySpan<string> words)
    {
        TrieNode rootNode = new TrieNode();
#if DEBUG
        rootNode.Path = "<root>";
#endif
        _trie = new List<TrieNode>() { rootNode };
        _words = words.ToArray();
        size = 1;

        for (int i = 0; i < words.Length; i++)
        {
            AddString(words[i], i);
        }

        AhoCorasickBuilder.BuildTrieLinks(CollectionsMarshal.AsSpan(_trie));
    }

    public void AddString(string word, int wordID)
    {
        int currentVertex = TrieNode.Root;
        for (int i = 0; i < word.Length; i++)
        {
            char c = word[i];
            if (!_trie[currentVertex].Children.TryGetValue(c, out int nextVertex))
            {
                _trie.Add(new TrieNode()
                {
                    SuffixLink = -1, // If not - add vertex
                    Parent = currentVertex,
                    AccessingCharacter = c,
#if DEBUG
                    Path = word.AsSpan(0, i + 1).ToString()
#endif
                });
                _trie[currentVertex].Children[c] = nextVertex = size;
                size++;
            }
            currentVertex = nextVertex; // Move to the new vertex in the trie
        }
        // Mark the end of the word and store its ID
        _trie[currentVertex].IsLeaf = true;
        _trie[currentVertex].WordID = wordID;
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
                int wordId = _trie[dictLink].WordID;
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
