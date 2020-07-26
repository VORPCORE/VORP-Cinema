using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_cinema_cl.Functions
{
    class Functions : BaseScript
    {
        public Functions()
        {
            Tick += isTimeCinema;
        }

        [Tick]
        private async Task isTimeCinema()
        {
            await Delay(100);
            if (!GetConfig.configLoaded) return;

            DateTime now = DateTime.UtcNow;

            for (int i = 0; i < GetConfig.Config["Cinemas"].Count(); i++)
            {
                for (int c = 0; c < GetConfig.Config["Cinemas"][i]["Listings"].Count(); c++)
                {
                    int CineHour = GetConfig.Config["Cinemas"][i]["Listings"][c]["HourOfDay"].ToObject<int>();
                    int CineMinute = GetConfig.Config["Cinemas"][i]["Listings"][c]["MinuteOfDay"].ToObject<int>();
                    string cineName = GetConfig.Config["Cinemas"][i]["Name"].ToString();
                    string movieName = GetConfig.Config["Cinemas"][i]["Listings"][c]["Name"].ToString();
                    DateTime movieTime = new DateTime(now.Year, now.Month, now.Day, CineHour, CineMinute, 0);

                    if (now.Hour == movieTime.Hour && now.Minute == movieTime.Minute && !vorp_cinema_init.CinemaTime[i])
                    {
                        vorp_cinema_init.CinemaTime[i] = true;
                        TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["AnnounceMovie"], cineName, movieName), 5000);
                    }

                    movieTime = movieTime.AddMinutes(GetConfig.Config["Cinemas"][i]["Listings"][c]["MinutesToClose"].ToObject<int>());

                    if (now.Hour == movieTime.Hour && now.Minute == movieTime.Minute && vorp_cinema_init.CinemaTime[i])
                    {
                        vorp_cinema_init.CinemaTime[i] = false;
                    }
                }
            }
        }
    }
}
