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
        public string AdreseIc2Uredjaja() { return _lcdServis.BusQuery(); }


        [HttpPost("Pisi")]
        public ActionResult Pisi(PisiModel model)
        {
            if (model == null)
            {
                return BadRequest("Model ti je null");
            }
            var odgovor = _lcdServis.PisiNaDisplay(model);

            return StatusCode(odgovor?.StatusCode??500,odgovor?.Msg??"interni sex");

            
        }
    }
}
