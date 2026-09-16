namespace Core.Domain;

/// <summary>
/// Page of entities returned from repository search operations.
/// </summary>
/// <typeparam name="T">Entity type.</typeparam>
/// <param name="Items">Items in the current page.</param>
/// <param name="TotalCount">Total matching rows before paging.</param>
public sealed record PagedList<T>(IReadOnlyList<T> Items, int TotalCount);
