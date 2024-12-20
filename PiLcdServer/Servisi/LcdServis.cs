using Iot.Device.Board;
using Iot.Device.CharacterLcd;
using Iot.Device.Imu;
using Iot.Device.Pcx857x;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using PiLcdServer.Modeli;
using System.Device.Gpio;
using System.Device.I2c;
using System.Globalization;
using System.Reflection;

namespace PiLcdServer.Servisi
{
    
    public class LcdServis 
    {

        private readonly AppSettings _appSettings;
        private readonly ILogger<LcdServis> _logger;
        
        private List<DisplayConfig> _displays = new List<DisplayConfig>();
        private readonly object _lock = new object(); // For thread safety

        public LcdServis(IOptions<AppSettings> options, ILogger<LcdServis> logger)
        {
            _appSettings = options.Value;
            _logger = logger;
            var a = BusQuery();
            if (!(a?.Valid ?? false))
                _logger.LogError($"BusQuery je pao sa porukom {a?.Msg ?? "Interni sex"}");
            else
                _logger.LogInformation($"Bus qery info {a.Msg} ");

            if (_displays.Count == 0)
                _logger.LogWarning("Nemamo niti i jedan zakačen display!!!");
            else
                _logger.LogInformation($"Na ploći imamo {_displays.Count()} i2c uređaja");

        }


        public List<DisplayConfig> GetDisplayConfigs()
        {
            lock (_lock)
            {
                // Return a copy to prevent external modification
                return _displays.ToList();
            }
        }


        public PocoResponse BusQuery()
        {
            lock (_lock)
            {

                try
                {

                    RaspberryPiBoard board = new RaspberryPiBoard();
                    var nesto = board.CreateOrGetI2cBus(board.GetDefaultI2cBusNumber());
                    var scan = nesto.PerformBusScan();
                    string adrese = string.Empty;
                    _displays.Clear();
                    if (scan != null && scan.Any())
                    {
                        foreach (var bus in scan)
                        {
                            DisplayConfig dsc = new DisplayConfig() { IntAdressaIc2Displaya = bus , ErrorState=false };
                            _displays.Add(dsc);
                            var pisi = new PisiModelRaw() { ClearBeforeWrite = true, IntAdresaUređaja = bus, Redovi =new List<string>(){ $"{bus} je moja adresa je {bus.ToString("X2")}", $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}" } };
                            var odgovor = this.PisiNaDisplay(pisi);
                            adrese += $"Uređaj na adresi  {bus.ToString("X2")} INT: {bus.ToString()} je u geršci {!odgovor.Valid} poruka {odgovor.Msg}\n ";
                        }
                        return new PocoResponse() { Msg = adrese, Valid = scan.Any(), StatusCode = 200 };
                    }
                    else
                    {
                        return new PocoResponse() { Msg = $"There is no IC2 devices on I2cBusNumber {board.GetDefaultI2cBusNumber()}", StatusCode = 404 };
                    }
                }
                catch (Exception ex)
                {
                    return new PocoResponse() { StatusCode = 500, Msg = $"Nismo uspijeli preskenirati I2C uređaje sa greškom {ex.Message} {ex.StackTrace} {ex.Source}" };

                }
            }
        }
        public DisplayConfig _dajMiDisplayConfig(int intAdresaDisplaya)
        {

            lock (_lock)
            {
                DisplayConfig? displayConfig = _displays?.Where(f => f.IntAdressaIc2Displaya == intAdresaDisplaya)?.FirstOrDefault() ?? null;
                if (displayConfig == null || displayConfig.ErrorState)
                {
                    var a = BusQuery();
                    displayConfig = _displays?.Where(f => f.IntAdressaIc2Displaya == intAdresaDisplaya)?.FirstOrDefault() ?? null;
                    if (displayConfig == null)
                    {
                        return new DisplayConfig() { ErrorMsg = $"Nismo našli uređaj sa int adresom {intAdresaDisplaya} nakačen na ploču sa greškom {a?.Msg ?? "Interni sex"}", ErrorState = true, IntAdressaIc2Displaya = intAdresaDisplaya };
                    }


                }
                //displayConfig = _displays?.Where(f => f.IntAdressaIc2Displaya == intAdresaDisplaya)?.FirstOrDefault();
                return displayConfig;
            }
        }
        public PocoResponse UgasiBacklgiht(int adresaDisplaya)
        {
            DisplayConfig displayConfig = new DisplayConfig() { ErrorMsg = "Nisam prošao query", ErrorState = true, IntAdressaIc2Displaya = adresaDisplaya };
            lock (_lock) {
                displayConfig = _dajMiDisplayConfig(adresaDisplaya);
                if (displayConfig.ErrorState)
                {
                    return new PocoResponse() { StatusCode = 400, Msg = $"Uređaj na adresi:{adresaDisplaya.ToString()} HEX: {adresaDisplaya.ToString("X2")} ima grešku {displayConfig.ErrorMsg} " };
                }
                                
                try
                {
                    using I2cDevice i2c1 = I2cDevice.Create(new I2cConnectionSettings(1, adresaDisplaya));


                    using var driver1 = new Pcf8574(i2c1);
                    using var lcd1 = new Lcd2004(registerSelectPin: 0,
                                            enablePin: 2,
                                            dataPins: new int[] { 4, 5, 6, 7 },
                                            backlightPin: 3,
                                            backlightBrightness: 0.1f,
                                            readWritePin: 1,
                                            controller: new GpioController(PinNumberingScheme.Logical, driver1)
                                            );
                    
                    lcd1.BacklightOn = false;
                    lcd1.Clear();
                    displayConfig.LastWrite = null;
                    displayConfig.ErrorState = false;
                    displayConfig.ErrorMsg = "OK";

                    return new PocoResponse() { StatusCode = 200, Valid = true, Msg = "OK" };
                }
                catch (Exception ex) {
                    displayConfig.ErrorState = true;
                    displayConfig.ErrorMsg=ex.Message;
                    displayConfig.LastWrite=null;
                    return new PocoResponse() { Msg = $"Sistmeka greška {ex.Message}", StatusCode = 500 };
                }
            }
        }
       
        public PocoResponse PisiNaDisplay(PisiModelRaw model)
        {
            lock (_lock)
            {
                DisplayConfig displayConfig = new DisplayConfig() { ErrorMsg = "Nisam prošao query", ErrorState = true, IntAdressaIc2Displaya = model?.IntAdresaUređaja ?? 0 };
                try
                {
                    if (model == null)
                    {
                        return new PocoResponse() { Msg = "Model ti je null", StatusCode = 400 };
                    }

                    displayConfig = _dajMiDisplayConfig(model.IntAdresaUređaja);

                    if (displayConfig.ErrorState)
                    {
                        return new PocoResponse() { Msg = $"Uređaj na adresi {model.IntAdresaUređaja} je u grešci {displayConfig.ErrorMsg} ", StatusCode = 404 };
                    }


                    using I2cDevice i2c1 = I2cDevice.Create(new I2cConnectionSettings(1, displayConfig.IntAdressaIc2Displaya));


                    using var driver1 = new Pcf8574(i2c1);
                    using var lcd1 = new Lcd2004(registerSelectPin: 0,
                                            enablePin: 2,
                                            dataPins: new int[] { 4, 5, 6, 7 },
                                            backlightPin: 3,
                                            backlightBrightness: 0.1f,
                                            readWritePin: 1,
                                            controller: new GpioController(PinNumberingScheme.Logical, driver1)
                                            );
                    if (model.ClearBeforeWrite)
                    {
                        lcd1.Clear();
                    }
                    lcd1.BacklightOn = true;

                    for (int i = 0; i < model.Redovi.Count; i++)
                    {
                        lcd1.SetCursorPosition(0, i);
                        lcd1.Write(model.Redovi[i]);
                    }

                    displayConfig.LastWrite = DateTime.Now;
                    displayConfig.ErrorMsg = "";
                    displayConfig.ErrorState = false;

                    return new PocoResponse() { Valid = true, Msg = "OK" };
                }
                catch (Exception ex)
                {
                    displayConfig.ErrorState = true;
                    displayConfig.ErrorMsg = ex.Message;
                    displayConfig.LastWrite = null;
                    return new PocoResponse() { Msg = ex.Message, StatusCode = 500 };
                }
            }
        }

      
    }
}
