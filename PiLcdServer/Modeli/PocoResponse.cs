namespace PiLcdServer.Modeli
{
    public class PocoResponse
    {
        public bool Valid { get; set; } 
        public string Msg { get; set; }
        public int StatusCode { get; set; } = 200;
    }
}
