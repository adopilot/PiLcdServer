namespace PiLcdServer.Modeli
{
    public class PisiModel
    {
        
        public string LcdUlazIliIzlaz { get; set; } = "ULAZ";
        public string PrviRed { get; set; }
        public string DrugiRed { get; set; }

    }

    public class PisiModelRaw
    {

        public int IntAdresaUređaja { get; set; }
        public List<string> Redovi { get; set; } = new List<string>();
        public bool ClearBeforeWrite { get; set; }

    }

}
