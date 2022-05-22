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
        Leaf = false;
        Parent = -1;
        ParentCharacter = char.MaxValue;
        SuffixLink = -1;
        WordID = -1;
        DictionaryLink = -1;
    }

    public Dictionary<char, int> Children;

    public bool Leaf;

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
        _trie[currentVertex].Leaf = true;
        _trie[currentVertex].WordID = wordID;
    }

    private void Initialize()
    {
        Queue<int> vertexQueue = new Queue<int>();
        vertexQueue.Enqueue(RootNode);
        while (vertexQueue.Count > 0)
        {
            int currentVertex = vertexQueue.Dequeue();
            CalculateSuffixAndDictionaryLinks(currentVertex);

            foreach (char key in _trie[currentVertex].Children.Keys)
            {
                vertexQueue.Enqueue(_trie[currentVertex].Children[key]);
            }
        }
    }

    private void CalculateSuffixAndDictionaryLinks(int vertex)
    {
        if (vertex == RootNode)
        {
            _trie[vertex].SuffixLink = RootNode;
            _trie[vertex].DictionaryLink = RootNode;
            return;
        }

        // one character substrings
        if (_trie[vertex].Parent == RootNode)
        {
            _trie[vertex].SuffixLink = RootNode;
            if (_trie[vertex].Leaf) _trie[vertex].DictionaryLink = vertex;
            else _trie[vertex].DictionaryLink = _trie[_trie[vertex].SuffixLink].DictionaryLink;
            return;
        }

        // To calculate the suffix link for the current vertex, we need the suffix
        // link for the parent and the character that moved us to the
        // current vertex.
        int curBetterVertex = _trie[_trie[vertex].Parent].SuffixLink;
        char chVertex = _trie[vertex].ParentCharacter;
        while (true)
        {
            // If there is an edge with the needed char, update the suffix link
            // and leave the cycle
            if (_trie[curBetterVertex].Children.TryGetValue(chVertex, out int suffixLink))
            {
                _trie[vertex].SuffixLink = suffixLink;
                break;
            }
            // Jump by suffix links until we reach the root or find a better prefix for the current substring.
            if (curBetterVertex == RootNode)
            {
                _trie[vertex].SuffixLink = RootNode;
                break;
            }
            // Go up by suffixlink
            curBetterVertex = _trie[curBetterVertex].SuffixLink;
        }

        if (_trie[vertex].Leaf)
        {
            _trie[vertex].DictionaryLink = vertex;
        }
        else
        {
            _trie[vertex].DictionaryLink = _trie[_trie[vertex].SuffixLink].DictionaryLink;
        }
    }

    public (int Index, int StringNumber) Find(ReadOnlySpan<char> text)
    {
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
                if (currentState == RootNode)
                {
                    break;
                }

                currentState = _trie[currentState].SuffixLink;
            }

            int checkState = currentState;

            // Trying to find all possible words from this prefix
            while (true)
            {
                checkState = _trie[checkState].DictionaryLink;

                if (checkState == RootNode) break;

                // Found a match
                int wordId = _trie[checkState].WordID;
                int indexOfMatch = i + 1 - _words[wordId].Length;
                return (indexOfMatch, wordId);
            }
        }

        return (-1, -1);
    }
}
