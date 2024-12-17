namespace PiLcdServer.Modeli
{
    public class DisplayConfig
    {
        public bool InUse { get; set; }
        public string AdressaIc2Displaya { get; set; } = string.Empty;
        public int BrojKaraktera { get; set; }
        public int BrojRedova { get; set; }
        public string AllowedHosts { get; set; } = "*";
    }
}
