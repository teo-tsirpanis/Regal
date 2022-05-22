// This file is part of Regal.
// Copyright © Theodore Tsirpanis
// Licensed under the MIT License.

namespace Regal;

internal static class Utilities
{
    public static T[] ToArrayPrefixed<T>(T first, ReadOnlySpan<T> rest)
    {
        T[] result = new T[rest.Length + 1];
        result[0] = first;
        rest.CopyTo(result.AsSpan(1));

        return result;
    }
}
