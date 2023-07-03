using System.Data;

namespace OutboxFlow.Storage;

/// <summary>
/// Creates <see cref="IDbConnection" /> objects.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates <see cref="IDbConnection" /> objects.
    /// </summary>
    IDbConnection Create();
}