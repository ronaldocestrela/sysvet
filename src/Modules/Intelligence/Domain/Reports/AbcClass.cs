namespace Intelligence.Domain.Reports;

/// <summary>Pareto class for ABC curve ranking (roadmap 10.3).</summary>
public enum AbcClass
{
    /// <summary>Top contributors until cumulative share reaches 80%.</summary>
    A = 1,

    /// <summary>Contributors until cumulative share reaches 95%.</summary>
    B = 2,

    /// <summary>Long tail.</summary>
    C = 3
}
