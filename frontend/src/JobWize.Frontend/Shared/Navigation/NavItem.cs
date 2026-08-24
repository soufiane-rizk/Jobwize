namespace JobWize.Frontend.Shared.Navigation
{
    /// <summary>
    /// A single entry in the application's main navigation.
    ///
    /// Shared/Layout renders whatever INavItem instances are registered in DI —
    /// it never references a specific module. Each module contributes its own
    /// entries from its own DependencyInjection.cs, the same way it registers
    /// its own services, so Shared stays ignorant of which modules exist.
    /// </summary>
    public interface INavItem
    {
        string Label { get; }

        string Href { get; }

        string Icon { get; }

        int Order { get; }
    }

    public sealed record NavItem(string Label, string Href, string Icon, int Order = 0) : INavItem;
}
