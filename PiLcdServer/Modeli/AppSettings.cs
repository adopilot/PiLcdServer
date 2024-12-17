namespace PiLcdServer.Modeli
{
    public class AppSettings
    {
        public DisplayConfig UlaziDisplay { get; set; } = new DisplayConfig();
        public DisplayConfig IzalzniDisplay { get; set; } = new DisplayConfig();
        public List<string> UseUrls { get; set; } = new List<string>();
    }
}
