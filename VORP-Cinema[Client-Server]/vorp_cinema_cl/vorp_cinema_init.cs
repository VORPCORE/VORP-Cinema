using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vorp_cinema_cl
{
    public class vorp_cinema_init : BaseScript
    {
        public static List<int> CinemaBlips = new List<int>();
        public static List<int> CinemaPeds = new List<int>();
        public static uint KeyToEnter = 0;
        public vorp_cinema_init()
        {
            Tick += onCinema;
        }

        public static async Task InitCinema()
        {
            await Delay(15000);
            foreach (var cinema in GetConfig.Config["Cinemas"])
            {
                string ped = cinema["NPCModel"].ToString();
                uint pedHash = (uint)API.GetHashKey(ped);
                await LoadModel(pedHash);
                int blipIcon = cinema["BlipIcon"].ToObject<int>();
                float x = cinema["EnterCinema"][0].ToObject<float>();
                float y = cinema["EnterCinema"][1].ToObject<float>();
                float z = cinema["EnterCinema"][2].ToObject<float>();
                float Pedx = cinema["NPCCinema"][0].ToObject<float>();
                float Pedy = cinema["NPCCinema"][1].ToObject<float>();
                float Pedz = cinema["NPCCinema"][2].ToObject<float>();
                float Pedh = cinema["NPCCinema"][3].ToObject<float>();

                int _blip = Function.Call<int>((Hash)0x554D9D53F696D002, 1664425300, x, y, z);
                Function.Call((Hash)0x74F74D3207ED525C, _blip, blipIcon, 1);
                Function.Call((Hash)0x9CB1A1623062F402, _blip, cinema["Name"].ToString());
                CinemaBlips.Add(_blip);

                int _PedCinema = API.CreatePed(pedHash, Pedx, Pedy, Pedz, Pedh, false, true, true, true);
                Function.Call((Hash)0x283978A15512B2FE, _PedCinema, true);
                CinemaPeds.Add(_PedCinema);
                API.SetEntityNoCollisionEntity(API.PlayerPedId(), _PedCinema, false);
                API.SetEntityCanBeDamaged(_PedCinema, false);
                API.SetEntityInvincible(_PedCinema, true);
                API.SetBlockingOfNonTemporaryEvents(_PedCinema, true);
                API.SetPedCanBeTargetted(_PedCinema, false);
                await Delay(2000);
                API.FreezeEntityPosition(_PedCinema, true);
                API.SetModelAsNoLongerNeeded(pedHash);
            }
        }

        public static async Task<bool> LoadModel(uint hash)
        {
            if (Function.Call<bool>(Hash.IS_MODEL_VALID, hash))
            {
                Function.Call(Hash.REQUEST_MODEL, hash);
                while (!Function.Call<bool>(Hash.HAS_MODEL_LOADED, hash))
                {
                    Debug.WriteLine($"Waiting for model {hash} load!");
                    await Delay(100);
                }
                return true;
            }
            else
            {
                Debug.WriteLine($"Model {hash} is not valid!");
                return false;
            }
        }

        [Tick]
        private async Task onCinema()
        {
            if (CinemaPeds.Count() == 0) { return; }

            int pid = API.PlayerPedId();
            Vector3 pCoords = API.GetEntityCoords(pid, true, true);

            for (int i = 0; i < GetConfig.Config["Cinemas"].Count(); i++)
            {
                float x = GetConfig.Config["Cinemas"][i]["EnterCinema"][0].ToObject<float>();
                float y = GetConfig.Config["Cinemas"][i]["EnterCinema"][1].ToObject<float>();
                float z = GetConfig.Config["Cinemas"][i]["EnterCinema"][2].ToObject<float>();
                float radius = GetConfig.Config["Cinemas"][i]["EnterCinema"][3].ToObject<float>();

                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, x, y, z, true) <= radius)
                {
                    await DrawTxt(GetConfig.Langs["PressToAccess"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                    if (API.IsControlJustPressed(0, KeyToEnter))
                    {
                        Debug.WriteLine("Funciona");
                        await Delay(5000);
                    }
                }
            }
        }

        public async Task DrawTxt(string text, float x, float y, float fontscale, float fontsize, int r, int g, int b, int alpha, bool textcentred, bool shadow)
        {
            long str = Function.Call<long>(Hash._CREATE_VAR_STRING, 10, "LITERAL_STRING", text);
            Function.Call(Hash.SET_TEXT_SCALE, fontscale, fontsize);
            Function.Call(Hash._SET_TEXT_COLOR, r, g, b, alpha);
            Function.Call(Hash.SET_TEXT_CENTRE, textcentred);
            if (shadow) { Function.Call(Hash.SET_TEXT_DROPSHADOW, 1, 0, 0, 255); }
            Function.Call(Hash.SET_TEXT_FONT_FOR_CURRENT_COMMAND, 1);
            Function.Call(Hash._DISPLAY_TEXT, str, x, y);
        }
    }
}
