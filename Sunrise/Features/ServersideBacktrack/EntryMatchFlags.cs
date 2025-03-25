using System;

namespace Sunrise.Features.ServersideBacktrack;

[Flags]
internal enum EntryMatchFlags : byte
{
    Position = 1 << 0,
    Rotation = 1 << 1,

    None = 0,
    All = Position | Rotation,
}