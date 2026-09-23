using Core.Domain;

namespace Intelligence.Domain;

/// <summary>Stable error codes for the Intelligence module.</summary>
public static class ErrorCodes
{
    /// <summary>Dashboard layout validation and persistence errors.</summary>
    public static class DashboardLayout
    {
        public static readonly Error NotFound = new("Intelligence.DashboardLayout.NotFound", "Dashboard layout not found.");
        public static readonly Error UnknownWidget = new("Intelligence.DashboardLayout.UnknownWidget", "Unknown dashboard widget key.");
        public static readonly Error DuplicateWidget = new("Intelligence.DashboardLayout.DuplicateWidget", "Duplicate widget key in layout.");
        public static readonly Error EmptyLayout = new("Intelligence.DashboardLayout.EmptyLayout", "Layout must contain at least one widget.");
        public static readonly Error ProfileNotFound = new("Intelligence.DashboardLayout.ProfileNotFound", "Access profile not found.");
    }
}
