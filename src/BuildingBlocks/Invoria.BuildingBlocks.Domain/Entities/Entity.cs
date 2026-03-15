using System.Diagnostics.CodeAnalysis;

namespace Invoria.BuildingBlocks.Domain.Entities;

public abstract class Entity<TId> : IEntity<TId>, IEquatable<Entity<TId>>
{
    public TId Id { get; protected set; } = default!;

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Equals(right);
    }

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is not Entity<TId> other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (EqualityComparer<TId>.Default.Equals(Id, default!))
        {
            return false;
        }

        return EqualityComparer<TId>.Default.Equals(Id, other.Id) &&
               GetType() == other.GetType();
    }

    public bool Equals(Entity<TId>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Equals((object)other);
    }

    public override int GetHashCode()
    {
        if (EqualityComparer<TId>.Default.Equals(Id, default!))
        {
            return base.GetHashCode();
        }

        return HashCode.Combine(GetType(), Id);
    }
}

public class Entity : Entity<string> , IEntity { }


