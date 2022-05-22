// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

namespace Regal.AhoCorasick;

internal class TrieNode
{
    public TrieNode()
    {
        Children = new Dictionary<char, int>();
        IsLeaf = false;
        Parent = -1;
        ParentCharacter = char.MaxValue;
        SuffixLink = -1;
        WordID = -1;
        DictionaryLink = -1;
    }

    public Dictionary<char, int> Children;

    public bool IsLeaf;

    public int Parent;

    public char ParentCharacter;

    public int SuffixLink;

    public int DictionaryLink;

    public int WordID;
}

internal class AhoCorasickMatcher
{
    private readonly List<TrieNode> _trie;
    private readonly string[] _words;
    private int size;
    private const int RootNode = 0;

    public ReadOnlySpan<string> Words => _words;

    public AhoCorasickMatcher(ReadOnlySpan<string> words)
    {
        _trie = new List<TrieNode>() { new TrieNode() };
        _words = words.ToArray();
        size = 1;

        for (int i = 0; i < words.Length; i++)
        {
            AddString(words[i], i);
        }

        Initialize();
    }

    public void AddString(string word, int wordID)
    {
        int currentVertex = RootNode;
        foreach (char c in word)
        {
            int nextVertex;
            if (!_trie[currentVertex].Children.TryGetValue(c, out nextVertex))
            {
                _trie.Add(new TrieNode()
                {
                    SuffixLink = -1, // If not - add vertex
                    Parent = currentVertex,
                    ParentCharacter = c
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

    private void Initialize()
    {
        Queue<int> vertexQueue = new Queue<int>();
        vertexQueue.Enqueue(RootNode);
        while (vertexQueue.TryDequeue(out int currentVertex))
        {
            CalculateSuffixAndDictionaryLinks(currentVertex);

            foreach (int vertex in _trie[currentVertex].Children.Values)
            {
                vertexQueue.Enqueue(vertex);
            }
        }
    }

    private void CalculateSuffixAndDictionaryLinks(int vertex)
    {
        TrieNode node = _trie[vertex];
        if (vertex == RootNode)
        {
            node.SuffixLink = RootNode;
            node.DictionaryLink = RootNode;
            return;
        }

        // one character substrings
        if (node.Parent == RootNode)
        {
            node.SuffixLink = RootNode;
            node.DictionaryLink = node.IsLeaf ? vertex : _trie[node.SuffixLink].DictionaryLink;
            return;
        }

        // To calculate the suffix link for the current vertex, we need the suffix
        // link for the parent and the character that moved us to the
        // current vertex.
        int curBetterVertex = _trie[node.Parent].SuffixLink;
        char chVertex = node.ParentCharacter;
        while (true)
        {
            // If there is an edge with the needed char, update the suffix link
            // and leave the cycle
            if (_trie[curBetterVertex].Children.TryGetValue(chVertex, out int suffixLink))
            {
                node.SuffixLink = suffixLink;
                break;
            }
            // Jump by suffix links until we reach the root or find a better prefix for the current substring.
            if (curBetterVertex == RootNode)
            {
                node.SuffixLink = RootNode;
                break;
            }
            // Go up by suffixlink
            curBetterVertex = _trie[curBetterVertex].SuffixLink;
        }

        node.DictionaryLink = node.IsLeaf ? vertex : _trie[node.SuffixLink].DictionaryLink;
    }

    public (int Index, int StringNumber) Find(ReadOnlySpan<char> text)
    {
        var lastMatch = (Index: -1, StringNumber: -1);
        int currentState = RootNode;

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
                if (currentState == RootNode)
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

            if (dictLink != RootNode)
            {
                // Found a match. We mark it and continue searching hoping it is getting bigger.
                int wordId = _trie[dictLink].WordID;
                int indexOfMatch = i + 1 - _words[wordId].Length;

                // We want to return the leftmost-longest match.
                // If this match starts later than the match we might have found before,
                // we cannot accept it because we want to return the leftmost match.
                // The match can start at the same position as the previous one, which
                // means that it is longer and we accept it.
                if (lastMatch.Index == -1 || indexOfMatch <= lastMatch.Index)
                {
                    lastMatch = (Index: indexOfMatch, StringNumber: wordId);
                }
            }
        }

        return lastMatch;
    }
}
