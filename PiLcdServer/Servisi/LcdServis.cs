using Iot.Device.Board;
using Iot.Device.CharacterLcd;
using Iot.Device.Imu;
using Iot.Device.Pcx857x;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PiLcdServer.Modeli;
using System.Device.Gpio;
using System.Device.I2c;

namespace PiLcdServer.Servisi
{
    
    public class LcdServis 
    {

        private readonly AppSettings _appSettings;
        private readonly ILogger<LcdServis> _logger;
        

        public LcdServis(IOptions<AppSettings> options, ILogger<LcdServis> logger)
        {
            _appSettings = options.Value;
            _logger = logger;
            
        }

       



        public string BusQuery()
        {
            

            RaspberryPiBoard board = new RaspberryPiBoard();
            var nesto =board.CreateOrGetI2cBus(board.GetDefaultI2cBusNumber());
            var scan = nesto.PerformBusScan();

            string adrese = string.Empty;

            if (scan != null  && scan.Any())
            {
                foreach (var bus in scan)
                {
                    adrese +=" "+ bus.ToString("X2");
                }
                return adrese;
            }
            else
            {
                return $"There is no IC2 devices on I2cBusNumber {board.GetDefaultI2cBusNumber()}";
            }

            
        }

        public PocoResponse PisiNaDisplay(PisiModel model)
        {
            try
            {
                if (model == null)
                {
                    return new PocoResponse() { Msg = "Model ti je null", StatusCode = 400 };
                }

                Modeli.DisplayConfig? displayConfig = model.LcdUlazIliIzlaz == "ULAZ" ? _appSettings.UlaziDisplay
                    : model.LcdUlazIliIzlaz == "IZLAZ" ? _appSettings.IzalzniDisplay : null;

                if (displayConfig == null) { return new PocoResponse() { Msg = $"Na koji display pišeš", StatusCode = 400 }; }

                if (!displayConfig.InUse)
                {
                    return new PocoResponse() { Msg = $"{model.LcdUlazIliIzlaz} se ne koristi ", StatusCode = 404 };
                }



                int deviceAdress = Convert.ToInt32(displayConfig.AdressaIc2Displaya, 16);

                

                using I2cDevice i2c1 = I2cDevice.Create(new I2cConnectionSettings(1, deviceAdress));


                using var driver1 = new Pcf8574(i2c1);
                using var lcd1 = new Lcd2004(registerSelectPin: 0,
                                        enablePin: 2,
                                        dataPins: new int[] { 4, 5, 6, 7 },
                                        backlightPin: 3,
                                        backlightBrightness: 0.1f,
                                        readWritePin: 1,
                                        controller: new GpioController(PinNumberingScheme.Logical, driver1)
                                        );
                lcd1.Clear();
                lcd1.BacklightOn = true;
                lcd1.SetCursorPosition(0, 0);
                lcd1.Write(model.PrviRed);
                lcd1.SetCursorPosition(0, 1);
                lcd1.Write(model.DrugiRed);


                return new PocoResponse() { Valid = true,Msg="OK" };
            }
            catch (Exception ex)
            {
                return new PocoResponse() { Msg = ex.Message, StatusCode = 500 };
            }
        }
        


    }
}
