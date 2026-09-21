using Xunit;

namespace API.IntegrationTests;

/// <summary>
/// Serializes WebApplicationFactory tests that share the Development SQLite file.
/// </summary>
[CollectionDefinition("IntegrationTests")]
public sealed class IntegrationTestsCollection;
