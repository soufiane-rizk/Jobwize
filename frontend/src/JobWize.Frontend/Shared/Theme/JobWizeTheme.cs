using MudBlazor;

namespace JobWize.Frontend.Shared.Theme
{
    public static class JobWizeTheme
    {
        public static MudTheme Default { get; } = new()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#5C2D91",
                Secondary = "#336791",
                AppbarBackground = "#5C2D91",
                Background = "#F7F7F9",
                DrawerBackground = "#FFFFFF"
            },
            PaletteDark = new PaletteDark
            {
                Primary = "#B794F4",
                Secondary = "#7DA9D8"
            },
            LayoutProperties = new LayoutProperties
            {
                DefaultBorderRadius = "8px"
            }
        };
    }
}
