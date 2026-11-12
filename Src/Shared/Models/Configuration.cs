namespace ZumenSearch.Models;

public enum WindowStateOption
{
    Restored,
    Maximized,
    Minimized
}

public enum ThemeOption
{
    Default,
    Light,
    Dark
}

public class Configuration
{
    public class WindowSettings
    {
        public int Width { get; set; } = 780;
        public int Height { get; set; } = 600;
        public int Left { get; set; } = 100;
        public int Top { get; set; } = 100;
        public WindowStateOption WindowState { get; set; } = WindowStateOption.Restored;

        public int RestoreWidth { get; set; } = 780;
        public int RestoreHeight { get; set; } = 600;
        public int RestoreLeft { get; set; } = 100;
        public int RestoreTop { get; set; } = 100;
    }

    public WindowSettings MainWindow { get; set; } = new WindowSettings();

    public WindowSettings RentResidentialEditorWindow { get; set; } = new WindowSettings();
    public WindowSettings RentResidentialListingEditorWindow { get; set; } = new WindowSettings();
    public WindowSettings RentCommercialEditorWindow { get; set; } = new WindowSettings();
    public WindowSettings RentCommercialListingEditorWindow { get; set; } = new WindowSettings();
    public WindowSettings LessorEditorWindow { get; set; } = new WindowSettings();
    public WindowSettings BrokerEditorWindow { get; set; } = new WindowSettings();

    public ThemeOption Theme { get; set; } = ThemeOption.Default;

    public SystemBackdropOption SystemBackdrop { get; set; } = SystemBackdropOption.Mica;

}
