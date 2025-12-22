namespace SpkSnbp.Domain.Abstracts;

public abstract class JoinEntity : IEquatable<JoinEntity>
{
    protected abstract IEnumerable<object> GetJoinKeys();

    public bool Equals(JoinEntity? other) =>
        other is not null &&
        other.GetType() == GetType() &&
        other.GetJoinKeys().SequenceEqual(GetJoinKeys());

    public override bool Equals(object? obj) => obj is not null && obj is JoinEntity other && Equals(other);

    public override int GetHashCode() => GetJoinKeys().Aggregate(default(int), HashCode.Combine);

    public static bool operator ==(JoinEntity? left, JoinEntity? right) => left is not null && left.Equals(right);

    public static bool operator !=(JoinEntity? left, JoinEntity? right) => !(left == right);
}