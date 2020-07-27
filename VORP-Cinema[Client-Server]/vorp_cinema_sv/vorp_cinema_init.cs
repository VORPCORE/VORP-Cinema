using CitizenFX.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_cinema_sv
{
    public class vorp_cinema_init : BaseScript
    {
        public vorp_cinema_init()
        {
            TriggerEvent("vorp:addNewCallBack", "getMoneyCinema", new Action<int, CallbackDelegate, int>((source, cb, cinema) =>
            {
                double price = LoadConfig.Config["Cinemas"][cinema]["Price"].ToObject<double>();
                TriggerEvent("vorp:getCharacter", source, new Action<dynamic>((user) =>
                {
                    double money = user.money;
                    if (money >= price)
                    {
                        TriggerEvent("vorp:removeMoney", source, 0, price);
                        cb(true);
                    }
                    else
                    {
                        cb(false);
                    }
                }));
            }));
        }
    }
}
