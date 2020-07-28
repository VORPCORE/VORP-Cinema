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
        public static bool playing = false;
        public Functions()
        {
            Tick += isTimeCinema;
        }

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
                    string movieId = GetConfig.Config["Cinemas"][i]["Listings"][c]["MovieID"].ToString();
                    DateTime movieTime = new DateTime(now.Year, now.Month, now.Day, CineHour, CineMinute, 0);
                    string closeMinutes = GetConfig.Config["Cinemas"][i]["Listings"][c]["MinutesToClose"].ToString();

                    if (now.Hour == movieTime.Hour && now.Minute == movieTime.Minute && !vorp_cinema_init.CinemaTime[i])
                    {
                        vorp_cinema_init.CinemaTime[i] = true;
                        if (GetConfig.Config["Cinemas"][i]["Listings"][c]["Announce"].ToObject<bool>())
                        {
                            TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["AnnounceMovie"], cineName, movieName, closeMinutes), 5000);
                        }
                    }

                    movieTime = movieTime.AddMinutes(GetConfig.Config["Cinemas"][i]["Listings"][c]["MinutesToClose"].ToObject<int>());

                    if (now.Hour == movieTime.Hour && now.Minute == movieTime.Minute && vorp_cinema_init.CinemaTime[i])
                    {
                        vorp_cinema_init.CinemaTime[i] = false;
                        StartMovie(i, movieId);
                    }
                }
            }
        }

        private async Task StartMovie(int cine, string movieId)
        {
            int handle = CreateNamedRenderTargetForModel("bla_theater", -349278483);
            Function.Call((Hash)0xC6ED9D5092438D91, 0);
            Function.Call((Hash)0x593FAF7FC9401A56, -1);
            Function.Call((Hash)0x593FAF7FC9401A56, 2);
            playing = true;
            InputMovie(cine, handle);
            int channel_input = 2;
            string channel_name = movieId;
            bool playback_rp = false;
            Function.Call((Hash)0xDEC6B25F5DC8925B, channel_input, channel_name, playback_rp);
            Function.Call((Hash)0x593FAF7FC9401A56, channel_input);
        }

        private async Task InputMovie(int cine, int handle)
        {
            while (playing)
            {
                Function.Call((Hash)0x64437C98FCC5F291, false);
                Function.Call((Hash)0x40866A418EB8EFDE, vorp_cinema_init.CinemaScreens[cine]);
                Function.Call((Hash)0xE550CDE128D56757, handle);
                Function.Call((Hash)0xCFCC78391C8B3814, 4);
                Function.Call((Hash)0x906B86E6D7896B9E, true);
                Function.Call((Hash)0xC0A145540254A840, 0.5f, 0.5f, 1.1f, 1.1f, 0.0f, 255, 255, 255, 50);
                Function.Call((Hash)0xE550CDE128D56757, Function.Call<int>((Hash)0x66F35DD9D2B58579));
                Function.Call((Hash)0x906B86E6D7896B9E, false);
                await Delay(0);
            }
        }

        private int CreateNamedRenderTargetForModel(string name, int model)
        {
            int handle = 0;
            if (!API.IsNamedRendertargetRegistered(name))
            {
                API.RegisterNamedRendertarget(name, false);
            }
            if (!API.IsNamedRendertargetLinked((uint)model))
            {
                API.LinkNamedRendertarget((uint)model);
            }
            if (API.IsNamedRendertargetRegistered(name))
            {
                handle = API.GetNamedRendertargetRenderId(name);
            }
            return handle;
        }
    }
}
