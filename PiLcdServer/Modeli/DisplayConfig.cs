namespace PiLcdServer.Modeli
{
    public class DisplayConfig
    {
        public int IntAdressaIc2Displaya { get; set; } = 0;
        public bool ErrorState { get; set; }
        public string ErrorMsg { get; set; }
        public DateTime? LastWrite { get; set; }

    }
}
