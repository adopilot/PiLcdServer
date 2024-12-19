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
            return StatusCode(odgovor?.StatusCode ?? 500, odgovor?.Msg ?? "Interni sex");
        
        }
        [HttpGet("UgasiDisplay")]
        public ActionResult UgasiDisplay(int intAdresaUredjaja)
        {
            var odgovor = _lcdServis.UgasiBacklgiht(intAdresaUredjaja);
            return StatusCode(odgovor?.StatusCode ?? 500, odgovor?.Msg ?? "Interni sex");

        }

        [HttpPost("Pisi")]
        public ActionResult Pisi(PisiModelRaw model)
        {
            if (model == null)
            {
                return BadRequest("Model ti je null");
            }
            var odgovor = _lcdServis.PisiNaDisplay(model);

            return StatusCode(odgovor?.StatusCode??500,odgovor?.Msg??"interni sex");

            
        }
        
        [HttpGet("GetDisplayConfigs")]
        public ActionResult<List<DisplayConfig>> GetDisplayConfigs() =>_lcdServis.GetDisplayConfigs();
        
    }
}
