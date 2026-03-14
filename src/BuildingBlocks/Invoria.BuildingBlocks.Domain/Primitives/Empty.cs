namespace Invoria.BuildingBlocks.Domain.Primitives;

/// <summary>
/// Represents an empty value for use with Result{T} when no payload is needed.
/// Similar to MediatR's Unit.
/// </summary>
public readonly struct Empty
{
    public static readonly Empty Value = new();
}

