using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using static CitizenFX.Core.Native.API;

namespace vorp_cinema_cl
{
    class ClearCaches : BaseScript
    {
        public ClearCaches()
        {
            EventHandlers["onResourceStop"] += new Action<string>(OnResourceStop);
        }

        private void OnResourceStop(string resourceName)
        {
            if (GetCurrentResourceName() != resourceName) return;

            Debug.WriteLine($"{resourceName} cleared blips and NPC's.");

            foreach (int blip in vorp_cinema_init.CinemaBlips)
            {
                int _blip = blip;
                RemoveBlip(ref _blip);
            }

            foreach (int npc in vorp_cinema_init.CinemaPeds)
            {
                int _ped = npc;
                DeletePed(ref _ped);
            }

            foreach (int screen in vorp_cinema_init.CinemaScreens)
            {
                int _screen = screen;
                DeleteObject(ref _screen);
            }
        }
    }
}
