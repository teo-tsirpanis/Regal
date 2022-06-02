// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

using System.Runtime.InteropServices;

namespace Regal.AhoCorasick;

internal static class AhoCorasickBuilder
{
    public static List<TrieNode> BuildTrie(ReadOnlySpan<string> words)
    {
        List<TrieNode> trie = new() { new TrieNode() };

        for (int i = 0; i < words.Length; i++)
        {
            AddStringToTrie(trie, words[i], i);
        }

        // That gives us the assurance that no new trie nodes will be created.
        BuildTrieLinks(CollectionsMarshal.AsSpan(trie));

        return trie;
    }

    private static void AddStringToTrie(List<TrieNode> trie, string word, int wordID)
    {
        int currentVertex = TrieNode.Root;
        for (int i = 0; i < word.Length; i++)
        {
            TrieNode currentNode = trie[currentVertex];
            char c = word[i];
            if (!currentNode.Children.TryGetValue(c, out int nextVertex))
            {
                TrieNode newNode = new TrieNode(currentVertex, word, wordID, i);
                currentNode.Children[c] = nextVertex = trie.Count;
                trie.Add(newNode);
            }
            currentVertex = nextVertex; // Move to the new vertex in the trie
        }
    }

    private static void BuildTrieLinks(ReadOnlySpan<TrieNode> trie)
    {
        Queue<int> vertexQueue = new Queue<int>();
        vertexQueue.Enqueue(TrieNode.Root);
        while (vertexQueue.TryDequeue(out int currentVertex))
        {
            CalculateSuffixAndDictionaryLinks(trie, currentVertex);

            foreach (int vertex in trie[currentVertex].Children.Values)
            {
                vertexQueue.Enqueue(vertex);
            }
        }
    }

    private static void CalculateSuffixAndDictionaryLinks(ReadOnlySpan<TrieNode> trie, int vertex)
    {
        TrieNode node = trie[vertex];
        if (vertex == TrieNode.Root)
        {
            node.SuffixLink = TrieNode.Root;
            node.DictionaryLink = TrieNode.Root;
            goto End;
        }

        // one character substrings
        if (node.Parent == TrieNode.Root)
        {
            node.SuffixLink = TrieNode.Root;
            node.DictionaryLink = node.IsMatch ? vertex : trie[node.SuffixLink].DictionaryLink;
            goto End;
        }

        // To calculate the suffix link for the current vertex, we need the suffix
        // link for the parent and the character that moved us to the
        // current vertex.
        int curBetterVertex = trie[node.Parent].SuffixLink;
        char chVertex = node.AccessingCharacter;
        while (true)
        {
            // If there is an edge with the needed char, update the suffix link
            // and leave the cycle
            if (trie[curBetterVertex].Children.TryGetValue(chVertex, out int suffixLink))
            {
                node.SuffixLink = suffixLink;
                break;
            }
            // Jump by suffix links until we reach the root or find a better prefix for the current substring.
            if (curBetterVertex == TrieNode.Root)
            {
                node.SuffixLink = TrieNode.Root;
                break;
            }
            // Go up by suffixlink
            curBetterVertex = trie[curBetterVertex].SuffixLink;
        }

        node.DictionaryLink = node.IsMatch ? vertex : trie[node.SuffixLink].DictionaryLink;

End:;
#if DEBUG
        node.SuffixLinkPath = trie[node.SuffixLink].Path;
        node.DictionaryLinkPath = trie[node.DictionaryLink].Path;
#endif
    }
}
