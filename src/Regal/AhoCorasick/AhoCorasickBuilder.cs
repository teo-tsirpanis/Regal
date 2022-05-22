// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

// Adapted from https://github.com/pgovind/runtime/blob/8f686549aa6014926ec244a32d961b090a72d9f8/src/libraries/System.Text.RegularExpressions/src/System/Text/RegularExpressions/RegexAhoCorasick.cs

namespace Regal.AhoCorasick;

internal static class AhoCorasickBuilder
{
    public const int RootNode = 0;
    
    public static void BuildTrieLinks(ReadOnlySpan<TrieNode> trie)
    {
        Queue<int> vertexQueue = new Queue<int>();
        vertexQueue.Enqueue(RootNode);
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
        if (vertex == RootNode)
        {
            node.SuffixLink = RootNode;
            node.DictionaryLink = RootNode;
            goto End;
        }

        // one character substrings
        if (node.Parent == RootNode)
        {
            node.SuffixLink = RootNode;
            node.DictionaryLink = node.IsLeaf ? vertex : trie[node.SuffixLink].DictionaryLink;
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
            if (curBetterVertex == RootNode)
            {
                node.SuffixLink = RootNode;
                break;
            }
            // Go up by suffixlink
            curBetterVertex = trie[curBetterVertex].SuffixLink;
        }

        node.DictionaryLink = node.IsLeaf ? vertex : trie[node.SuffixLink].DictionaryLink;

End:;
#if DEBUG
        node.SuffixLinkPath = trie[node.SuffixLink].Path;
        node.DictionaryLinkPath = trie[node.DictionaryLink].Path;
#endif
    }
}
