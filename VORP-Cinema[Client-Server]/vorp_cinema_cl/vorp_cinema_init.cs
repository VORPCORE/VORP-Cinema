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
        public static List<int> CinemaScreens = new List<int>();
        public static Dictionary<int, int> Doorlist = new Dictionary<int, int>();
        public static Dictionary<int, bool> CinemaTime = new Dictionary<int, bool>();
        public static uint KeyToEnter = 0;
        public vorp_cinema_init()
        {
            Tick += onCinema;
            Tick += doorCinema;
        }

        public static async Task InitCinema()
        {
            await Delay(15000);
            int index = 0;
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
                float cinemaX = cinema["CinemaScreen"]["Coords"][0].ToObject<float>();
                float cinemaY = cinema["CinemaScreen"]["Coords"][1].ToObject<float>();
                float cinemaZ = cinema["CinemaScreen"]["Coords"][2].ToObject<float>();
                float cinemaRX = cinema["CinemaScreen"]["Rotation"][0].ToObject<float>();
                float cinemaRY = cinema["CinemaScreen"]["Rotation"][1].ToObject<float>();
                float cinemaRZ = cinema["CinemaScreen"]["Rotation"][2].ToObject<float>();

                int _object = API.CreateObjectNoOffset(unchecked((uint)(-349278483)), cinemaX, cinemaY, cinemaZ, false, true, false, true);
                API.SetEntityRotation(_object, cinemaRX, cinemaRY, cinemaRZ, 2, true);
                API.SetEntityVisible(_object, true);
                API.SetEntityDynamic(_object, true);
                API.SetEntityProofs(_object, 31, true);
                API.FreezeEntityPosition(_object, true);
                CinemaScreens.Add(_object);

                int _blip = Function.Call<int>((Hash)0x554D9D53F696D002, 1664425300, x, y, z);
                Function.Call((Hash)0x74F74D3207ED525C, _blip, blipIcon, 1);
                Function.Call((Hash)0x9CB1A1623062F402, _blip, cinema["Name"].ToString());
                CinemaBlips.Add(_blip);

                int _PedCinema = API.CreatePed(pedHash, Pedx, Pedy, Pedz, Pedh, false, true, true, true);
                Function.Call((Hash)0x283978A15512B2FE, _PedCinema, true);
                CinemaPeds.Add(_PedCinema);
                CinemaTime.Add(index, false);
                API.SetEntityNoCollisionEntity(API.PlayerPedId(), _PedCinema, false);
                API.SetEntityCanBeDamaged(_PedCinema, false);
                API.SetEntityInvincible(_PedCinema, true);
                API.SetBlockingOfNonTemporaryEvents(_PedCinema, true);
                API.SetPedCanBeTargetted(_PedCinema, false);
                await Delay(2000);
                API.FreezeEntityPosition(_PedCinema, true);
                API.SetModelAsNoLongerNeeded(pedHash);
                index += 1;
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
            if (!GetConfig.configLoaded) return;
            if (CinemaPeds.Count() == 0) return;

            int pid = API.PlayerPedId();
            Vector3 pCoords = API.GetEntityCoords(pid, true, true);

            for (int i = 0; i < GetConfig.Config["Cinemas"].Count(); i++)
            {
                float xEnter = GetConfig.Config["Cinemas"][i]["EnterCinema"][0].ToObject<float>();
                float yEnter = GetConfig.Config["Cinemas"][i]["EnterCinema"][1].ToObject<float>();
                float zEnter = GetConfig.Config["Cinemas"][i]["EnterCinema"][2].ToObject<float>();
                float radiusEnter = GetConfig.Config["Cinemas"][i]["EnterCinema"][3].ToObject<float>();
                float xExit = GetConfig.Config["Cinemas"][i]["ExitCinema"][0].ToObject<float>();
                float yExit = GetConfig.Config["Cinemas"][i]["ExitCinema"][1].ToObject<float>();
                float zExit = GetConfig.Config["Cinemas"][i]["ExitCinema"][2].ToObject<float>();
                float radiusExit = GetConfig.Config["Cinemas"][i]["ExitCinema"][3].ToObject<float>();
                float animCoordX = GetConfig.Config["Cinemas"][i]["AnimationCoord"][0].ToObject<float>();
                float animCoordY = GetConfig.Config["Cinemas"][i]["AnimationCoord"][1].ToObject<float>();
                float animCoordZ = GetConfig.Config["Cinemas"][i]["AnimationCoord"][2].ToObject<float>();
                string price = GetConfig.Config["Cinemas"][i]["Price"].ToString();
                string cinemaName = GetConfig.Config["Cinemas"][i]["Name"].ToString();
                float cinemaX = GetConfig.Config["Cinemas"][i]["CinemaScreen"]["Coords"][0].ToObject<float>();
                float cinemaY = GetConfig.Config["Cinemas"][i]["CinemaScreen"]["Coords"][1].ToObject<float>();
                float cinemaZ = GetConfig.Config["Cinemas"][i]["CinemaScreen"]["Coords"][2].ToObject<float>();

                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, xEnter, yEnter, zEnter, true) <= radiusEnter && CinemaTime[i])
                {
                    await DrawTxt(GetConfig.Langs["PressToAccess"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                    if (API.IsControlJustPressed(0, KeyToEnter))
                    {
                        TriggerEvent("vorp:ExecuteServerCallBack", "getMoneyCinema", new Action<bool>(async (haveMoney) =>
                        {
                            if (haveMoney)
                            {
                                API.DoScreenFadeOut(800);
                                await Delay(500);
                                API.SetEntityCoords(pid, xExit, yExit, zExit, false, false, false, false);
                                await Delay(1000);
                                API.DoScreenFadeIn(1000);
                                TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["Welcome"], cinemaName), 4000);
                                Function.Call((Hash)0x6FC9B065229C0787, true);
                            }
                            else
                            {
                                TriggerEvent("vorp:Tip", string.Format(GetConfig.Langs["NoMoney"], price), 4000);
                            }
                        }), i);
                        await Delay(5000);
                    }
                }
                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, xExit, yExit, zExit, true) <= radiusExit)
                {
                    await DrawTxt(GetConfig.Langs["PressToExit"], 0.5f, 0.9f, 0.7f, 0.7f, 255, 255, 255, 255, true, true);
                    if (API.IsControlJustPressed(0, KeyToEnter))
                    {
                        Functions.Functions.playing = false;
                        API.SetTvChannel(-1);
                        Function.Call((Hash)0xE550CDE128D56757, 0);
                        API.DoScreenFadeOut(800);
                        await Delay(1000);
                        API.SetEntityCoords(pid, xEnter, yEnter, zEnter, false, false, false, false);
                        await Delay(600);
                        API.DoScreenFadeIn(1500);
                        API.TaskGoToCoordAnyMeans(pid, animCoordX, animCoordY, animCoordZ, 0.5f, 0, false, 524419, -1f);
                        Function.Call((Hash)0x6FC9B065229C0787, true);
                        await Delay(5000);
                    }
                }
            }
        }

        [Tick]
        private async Task doorCinema()
        {
            await Delay(50);
            if (!GetConfig.configLoaded) return;

            if (!GetConfig.Config["CloseDoors"].ToObject<bool>()) return;

            Vector3 pCoords = API.GetEntityCoords(API.PlayerPedId(), true, true);
            for (int i = 0; i < GetConfig.Config["Doors"].Count(); i++)
            {
                float doorX = GetConfig.Config["Doors"][i][0].ToObject<float>();
                float doorY = GetConfig.Config["Doors"][i][1].ToObject<float>();
                float doorZ = GetConfig.Config["Doors"][i][2].ToObject<float>();
                float doorH = GetConfig.Config["Doors"][i][3].ToObject<float>();
                if (API.GetDistanceBetweenCoords(pCoords.X, pCoords.Y, pCoords.Z, doorX, doorY, doorZ, true) < 20.0f)
                {
                    int shapeTest = Function.Call<int>((Hash)0xFE466162C4401D18, doorX, doorY, doorZ, 1.0f, 1.0f, 1.0f, 0.0f, 0.0f, 0.0f, true, 16);
                    bool hit = false;
                    Vector3 endCoords = new Vector3();
                    Vector3 surfaceNormal = new Vector3();
                    int entity = 0;
                    int result = API.GetShapeTestResult(shapeTest, ref hit, ref endCoords, ref surfaceNormal, ref entity);
                    Doorlist[i] = entity;
                    API.SetEntityHeading(entity, doorH);
                    API.FreezeEntityPosition(entity, true);
                    API.DoorSystemSetDoorState(entity, 1);
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
