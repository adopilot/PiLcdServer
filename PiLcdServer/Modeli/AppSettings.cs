namespace PiLcdServer.Modeli
{
    public class AppSettings
    {
        public string AllowedHosts { get; set; }
        public List<string> UseUrls { get; set; } = new List<string>();

        public bool AutoTurnOffDisplayEnabled { get; set; } = true;
        public int AutoTurnOffDisplayAfterSec { get; set; } = 10;
    }
}
