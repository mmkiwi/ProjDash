// This Source Code Form is subject to the terms of the Mozilla Public
// License, v.2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Collections.Immutable;

namespace MMKiwi.ProjDash.ViewModel.Model;

public sealed record Project
{
    public bool Equals(Project? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Title == other.Title && Subtitles.SequenceEqual(other.Subtitles) && Links.SequenceEqual(other.Links) &&
               Color == other.Color;
    }

    public override int GetHashCode()
    {
        HashCode hash = new();
        hash.Add(Title);
        foreach (var subtitle in Subtitles)
            hash.Add(subtitle);
        foreach (var link in Links)
            hash.Add(link);
        hash.Add(Color);
        return hash.ToHashCode();
    }

    public required string Title { get; init; }
    public ImmutableArray<string> Subtitles { get; init; } = [];
    public required ImmutableArray<ProjectLink> Links { get; init; }

    public string? Color { get; init; }
}