using Microsoft.AspNetCore.Mvc;
using PiLcdServer.Modeli;
using PiLcdServer.Servisi;

namespace PiLcdServer.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class LcdController : ControllerBase
    {
        private readonly LcdServis _lcdServis;

        public LcdController(LcdServis lcdServis)
        {
            _lcdServis = lcdServis;
        }
        [HttpGet("AdreseIc2Uredjaja")]
        public ActionResult AdreseIc2Uredjaja() {
            var odgovor = _lcdServis.BusQuery();
            return StatusCode(odgovor?.StatusCode ?? 500, odgovor?.Msg ?? "Interna greška");
        
        }
        [HttpGet("UgasiDisplay")]
        public ActionResult UgasiDisplay(int adresaUredjaja)
        {
            var odgovor = _lcdServis.UgasiBacklgiht(adresaUredjaja);
            return StatusCode(odgovor?.StatusCode ?? 500, odgovor?.Msg ?? "Interna greška");

        }

        [HttpPost("Pisi")]
        public ActionResult Pisi(PisiModelRaw model)
        {
            if (model == null)
            {
                return BadRequest("Model ti je null");
            }
            var odgovor = _lcdServis.PisiNaDisplay(model);

            return StatusCode(odgovor?.StatusCode??500,odgovor?.Msg?? "interna greška");

            
        }
        [HttpGet("BusQuery")]
        public ActionResult BusQuery()
        {
            var a = _lcdServis.BusQuery();
            return StatusCode(a?.StatusCode ?? 500, a?.Msg ?? "Interna greška");
        }

        [HttpGet("GetDisplayConfigs")]
        public ActionResult<List<DisplayConfig>> GetDisplayConfigs() =>_lcdServis.GetDisplayConfigs();

        [HttpGet("GetDisplayConfig")]
        public ActionResult<DisplayConfig> GetDisplayConfig([FromQuery]int adresaUredjaja) => _lcdServis._dajMiDisplayConfig(adresaUredjaja);
    }
}

