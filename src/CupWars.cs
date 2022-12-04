using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Oxide.Core;
using System.Globalization;
using Newtonsoft.Json.Linq;
using ProtoBuf;

namespace Oxide.Plugins
{
    [Info("COINRUST implements CupWars", "Qbis, VerS7", "1.0.4.1")]
    public class CupWars : RustPlugin
    {
        #region [Vars]
        [PluginReference] Plugin ImageLibrary, Clans;
        private List<BuildingPrivlidge> Cups = new List<BuildingPrivlidge>();
        private List<ulong> CloseUI = new List<ulong>();
        public class cupData
        {
            public string OwnerName;
            public int lastCapture;
            public Vector3 position;
            public string Name;
        }

        private static CupWars plugin;
        #endregion

        #region [Config]
        private PluginConfig config;

        protected override void LoadDefaultConfig()
        {
            config = PluginConfig.DefaultConfig();
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            config = Config.ReadObject<PluginConfig>();

            if (config.PluginVersion < Version)
                UpdateConfigValues();

            Config.WriteObject(config, true);
        }

        private void UpdateConfigValues()
        {
            PluginConfig baseConfig = PluginConfig.DefaultConfig();
            if (config.PluginVersion < Version)
            {
                config.PluginVersion = Version;
                PrintWarning("Config checked completed!");
            }
            config.PluginVersion = Version;
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }

        private class PluginConfig
        {
            [JsonProperty("Настройки шкафа")]
            public Settings settings;

            [JsonProperty("Настройки маркера")]
            public MarkersSettings marker;

            [JsonProperty("Config version")]
            public VersionNumber PluginVersion = new VersionNumber();

            public static PluginConfig DefaultConfig()
            {
                return new PluginConfig()
                {
                    settings = new Settings()
                    {
                        skinID = 2679214470,
                        rewardRadius = 500,
                        captSeconds = 300,
                        captDelay = 14400,
                        gatherPrecent = 15
                    },
                    marker = new MarkersSettings()
                    {
                        markerRadius = 0.5f,
                        markerAlpha = 0.4f,
                        markerColorCanCapture = "#10c916",
                        markerColorCantCapture = "#ffb700",
                        markerColorCapture = "#ed0707"
                    },
                    PluginVersion = new VersionNumber()

                };
            }
        }

        public class Settings
        {
            [JsonProperty("skinID выдаваемоего шкафа")]
            public ulong skinID;

            [JsonProperty("Радиус в котором выдаются налоги (в метрах)")]
            public int rewardRadius;

            [JsonProperty("Сколько нужно секунд для захвата")]
            public int captSeconds;

            [JsonProperty("Откат между захватами в секундах")]
            public int captDelay;

            [JsonProperty("Процент налога")]
            public int gatherPrecent;
        }

        public class MarkersSettings
        {
            [JsonProperty("Радиус маркера")]
            public float markerRadius;

            [JsonProperty("Прозрачность маркера")]
            public float markerAlpha;

            [JsonProperty("Цвет маркера когда можно захватить")]
            public string markerColorCanCapture;

            [JsonProperty("Цвет маркера когда идет захват")]
            public string markerColorCapture;

            [JsonProperty("Цвет маркера когда нельзя захватить")]
            public string markerColorCantCapture;
        }
        #endregion

        #region [Localization⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠⁠]
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["CantCapt"] = "Захватить можно будет через {0}ч. {1}м. {2}с.",
                ["StartCapt"] = "Клан {0} начал захват территории {1}",
                ["AlreadyCapt"] = "Территория уже захвачена вашим кланом",
                ["OtherCapt"] = "Клан {0} перехватил вашу территорию",
                ["AlreadyCapturing"] = "Ваш клан уже захватывает территорию",
                ["StopCapt"] = "Клан {0} захватил территорию {1}",
                ["UI_rightPanelText"] = "До конца захвата:\n{0}мин {1}сек",
                ["UI_Header"] = "Захват зон",
                ["UI_Footer"] = "Налог составляет {0}%",
                ["UI_Name"] = "<size=24>Зона:\n {0}</size>",
                ["UI_Capt"] = "<size=20>Захвачена:</size>\n {0}",
                ["UI_Free"] = "<size=20>Свободна</size>",
                ["UI_CanCapt"] = "Можно\nзахватить",
                ["UI_NextCapt"] = "До повторного захвата\n {0}ч {1}мин",
            }, this);
        }
        string GetMsg(string key, BasePlayer player = null) => lang.GetMessage(key, this, player?.UserIDString);
        string GetMsg(string key) => lang.GetMessage(key, this);
        #endregion

        #region [Oxide]
        private void OnServerInitialized()
        {
            LoadCupboards();

            ImageLibrary?.Call("AddImage", "https://i.imgur.com/heVYeVD.png", "button_close_right");
            ImageLibrary?.Call("AddImage", "https://i.imgur.com/6xO5e95.png", "button_close");
        }

        private void Init()
        {
            plugin = this;
        }

        private void Unload()
        {
            SaveCups(true);
            foreach (var player in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(player, "CaptMenuMain");
                CuiHelper.DestroyUi(player, "CaptTableMain");
            }
        }

        private void OnEntitySpawned(BaseLock codeLock)
        {
            if (codeLock == null)
                return;

            var parent = codeLock.GetParentEntity();
            if (parent == null)
                return;

            var cup = parent as BuildingPrivlidge;
            if (cup == null)
                return;

            if (Cups.Contains(cup))
            {
                NextTick(() => {
                    codeLock.Kill();
                });
            }
        }

        private void OnEntityBuilt(Planner plan, GameObject go)
        {
            if (plan == null)
                return;

            var player = plan?.GetOwnerPlayer();
            if (player == null)
                return;

            var codelock = go.GetComponent<CodeLock>();
            if(codelock != null)
            {
                var parent = codelock.GetParentEntity();
                if (parent == null)
                    return;

                var cup = parent as BuildingPrivlidge;
                if (cup == null)
                    return;

                if (Cups.Contains(cup))
                {
                    NextTick(()=> {
                        if(codelock != null)
                            codelock.Kill();
                    });
                }
                return;
            }


            if (!player.IsAdmin)
                return;

            var priv = go.GetComponent<BuildingPrivlidge>();
            if (priv != null)
            {
                var activItem = player.GetActiveItem();

                if (activItem.skin == config.settings.skinID)
                {
                    var ent = priv as BaseEntity;
                    ent.OwnerID = 0;
                    ent.GetComponent<BaseCombatEntity>().lifestate = BaseCombatEntity.LifeState.Dead;
                    UnityEngine.Object.Destroy(ent.GetComponent<DestroyOnGroundMissing>());
                    UnityEngine.Object.Destroy(ent.GetComponent<GroundWatch>());
                    var comp = priv.gameObject.AddComponent<CupManager>();
                    comp.Initialize("-", Facepunch.Math.Epoch.Current - config.settings.captDelay, activItem.name, GetGrid(ent.transform.position));
                    Cups.Add(priv);
                    SaveCups();
                    player.ChatMessage($"Трерритория с именем {activItem.name} создана");
                }
            }
        }

        private object OnCupboardAuthorize(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (privilege == null || player == null)
                return null;

            if (Cups.Contains(privilege))
            {
                var clanName = Clans?.Call<string>("GetClanOf", player.userID);
                if (clanName == null)
                    return false;


                var comp = privilege.gameObject.GetComponent<CupManager>();
                if (comp == null)
                    return null;

                foreach(var cup in Cups)
                {
                    var comp2 = privilege.gameObject.GetComponent<CupManager>();
                    if (comp2 == null)
                        continue;

                    if(comp.ownerName == clanName)
                    {
                        return null;
                    }

                    if(comp.captClan == clanName)
                    {
                        player.ChatMessage(GetMsg("AlreadyCapturing", player));
                        return false;
                    }
                }

                var auth = comp.SetNewCaptClan(clanName, player);

                if (auth)
                {
                    privilege.authorizedPlayers.Clear();
                    privilege.inventory.Clear();
                }


                return false;
            }
            return null;
        }

        private object OnCupboardDeauthorize(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (privilege == null || player == null)
                return null;

            if (Cups.Contains(privilege))
            {
                return false;
            }
            return null;
        }

        private object OnCupboardClearList(BuildingPrivlidge privilege, BasePlayer player)
        {
            if (privilege == null || player == null)
                return null;

            if (Cups.Contains(privilege))
            {
                return false;
            }
            return null;
        }

        private void OnLootEntity(BasePlayer player, BaseEntity entity)
        {
            if (player == null || entity == null)
                return;

            var privilege = entity as BuildingPrivlidge;
            if (privilege == null)
                return;

            if (!Cups.Contains(privilege))
                return;

            var comp = privilege.GetComponent<CupManager>();
            if (comp == null)
                return;

            var clanName = Clans?.Call<string>("GetClanOf", player.userID);
            if (clanName == null)
            {
                PlayerNameID playerNameID = privilege.authorizedPlayers.FirstOrDefault(p => p.userid == player.userID);
                if (playerNameID != null)
                    privilege.authorizedPlayers.Remove(playerNameID);


                timer.Once(0.01f, player.EndLooting);
                return;
            }

            if (comp.ownerName != clanName)
            {
                PlayerNameID playerNameID = privilege.authorizedPlayers.FirstOrDefault(p => p.userid == player.userID);
                if (playerNameID != null)
                    privilege.authorizedPlayers.Remove(playerNameID);
                timer.Once(0.01f, player.EndLooting);
            }
        }

        private void OnDispenserGather(ResourceDispenser dispenser, BaseEntity entity, Item item)
        {
            if (entity.ToPlayer() == null) return;
            if (item == null) return;

            foreach (var cup in Cups)
            {
                if (Vector3.Distance(cup.transform.position, entity.ToPlayer().transform.position) < config.settings.rewardRadius)
                {
                    NextTick(() =>
                    {
                        var amount = Convert.ToInt32(item.amount * (config.settings.gatherPrecent / 100f));
                        if (amount <= 0)
                            return;

                        var bonusItem = ItemManager.CreateByName(item.info.shortname, amount);
                        if (bonusItem.amount <= 0)
                            return;

                        bonusItem.MoveToContainer(cup.inventory);

                    });
                }
            }

        }

        private void OnDispenserBonus(ResourceDispenser dispenser, BaseEntity entity, Item item) => OnDispenserGather(dispenser, entity, item);
        #endregion

        #region [Func]
        private static string GetGrid(Vector3 pos)
        {
            var letter = 'A';
            var x = Mathf.Floor((pos.x + ConVar.Server.worldsize / 2f) / 146.3f) % 26;
            var z = Mathf.Floor(ConVar.Server.worldsize / 146.3f) -
                    Mathf.Floor((pos.z + ConVar.Server.worldsize / 2f) / 146.3f);
            letter = (char)(letter + x);
            return $"{letter}{z}";
        }

        private void KillCups()
        {
            foreach(var cup in Cups)
            {
                if (cup == null)
                    continue;


                var comp = cup.gameObject.GetComponent<CupManager>();
                if (comp == null)
                    continue;

                UnityEngine.Object.Destroy(comp);
            }
        }
        #endregion

        #region [Data]
        private void LoadCupboards()
        {
            List<cupData> data = new List<cupData>();
            data = Interface.Oxide.DataFileSystem.ReadObject<List<cupData>>($"CupWars/data");


            foreach (var cup in data)
            {
                var cupboards = new List<BaseEntity>();
                Vis.Entities(cup.position, 2f, cupboards);

                foreach (var c in cupboards)
                {
                    if (!c.ShortPrefabName.Contains("cupboard.tool"))
                        continue;

                    var findCup = c;
                    findCup.OwnerID = 0;

                    var combat = findCup.GetComponent<BaseCombatEntity>();
                    if (combat != null)
                        combat.lifestate = BaseCombatEntity.LifeState.Dead;

                    var DGM = findCup.GetComponent<DestroyOnGroundMissing>();
                    if (DGM != null)
                        UnityEngine.Object.Destroy(DGM);

                    var GW = findCup.GetComponent<GroundWatch>();
                    if (GW != null)
                        UnityEngine.Object.Destroy(GW);
                    var bp = findCup as BuildingPrivlidge;

                    var comp = bp.gameObject.AddComponent<CupManager>();
                    comp.Initialize(cup.OwnerName, cup.lastCapture, cup.Name, GetGrid(findCup.transform.position));
                    Cups.Add(bp);
                    break;
                }
            }
        }

        private void SaveCups(bool destoyComp = false)
        {
            List<cupData> data = new List<cupData>();
            foreach(var cup in Cups)
            {
                if (cup == null)
                    continue;

                var comp = cup.gameObject.GetComponent<CupManager>();
                if (comp == null)
                    continue;

                cupData addData = new cupData();
                addData.OwnerName = comp.ownerName;
                addData.Name = comp.cupName;
                addData.lastCapture = comp.lastCapture;
                addData.position = cup.transform.position;
                data.Add(addData);
            }

            Interface.Oxide.DataFileSystem.WriteObject($"CupWars/data", data);

            if (destoyComp)
                KillCups();
        }

        void OnNewSave()
        {
            Cups.Clear();
            SaveCups();
            PrintWarning("Wipe detected! Data clearing");
        }
        #endregion

        #region [Comp]
        public class CupManager : MonoBehaviour
        {
            public string ownerName;
            public int lastCapture;
            public string cupName;

            private bool isCapture;
            public string captClan;
            public int capt;

            private MapMarkerGenericRadius mapMarker;
            private VendingMachineMapMarker vendingMarker;

            public void DestroyComp() => OnDestroy();
            private void OnDestroy()
            {
                RemoveMarker();
                Destroy(this);
            }

            public void Initialize(string owner, int last, string name, string grid)
            {
                ownerName = owner;
                lastCapture = last;
                cupName = name;

                isCapture = false;
                captClan = "";
                capt = 0;

                if (Facepunch.Math.Epoch.Current - lastCapture < plugin.config.settings.captDelay)
                    CreateMarker(plugin.config.marker.markerColorCantCapture);
                else
                    CreateMarker(plugin.config.marker.markerColorCanCapture);

                InvokeRepeating("Timer", 1f, 1f);
            }

            private void Timer()
            {
                if (isCapture)
                {
                    capt++;

                    if (capt >= plugin.config.settings.captSeconds)
                    {
                        StopCapture();
                    }
                    else
                    {
                        DrawUiCapture();
                    }

                }

                if (Facepunch.Math.Epoch.Current - lastCapture == plugin.config.settings.captDelay && !isCapture)
                    mapMarker.color1 = ConvertToColor(plugin.config.marker.markerColorCanCapture);

                UpdateMarker();
            }

            private void DrawUiCapture()
            {
                var clan = plugin.Clans?.Call<JObject>("GetClan", captClan);
                if (clan == null)
                    return;

                var members = clan.GetValue("members") as JArray;
                foreach(var member in members)
                {
                    var id = Convert.ToUInt64(member);
                    if (plugin.CloseUI.Contains(id))
                        continue;

                    var player = BasePlayer.FindByID(id);
                    if (player == null)
                        continue;

                    if (player.IsConnected)
                        plugin.CreateCaptureTable(player, this);
                }
            }

            public bool SetNewCaptClan(string clan, BasePlayer cPlayer)
            {
                if (clan == captClan)
                    return true;

                if (Facepunch.Math.Epoch.Current - lastCapture < plugin.config.settings.captDelay)
                {
                    var time = TimeSpan.FromSeconds(lastCapture + plugin.config.settings.captDelay - Facepunch.Math.Epoch.Current);
                    cPlayer.ChatMessage(String.Format(plugin.GetMsg("CantCapt", cPlayer), time.Hours, time.Minutes, time.Seconds));
                    return false;
                }

                if (isCapture)
                {
                    var Clan = plugin.Clans?.Call<JObject>("GetClan", captClan);
                    if (Clan == null)
                        return false;

                    var members = Clan.GetValue("members") as JArray;
                    foreach (var member in members)
                    {
                        var id = Convert.ToUInt64(member);
                        var player = BasePlayer.FindByID(id);
                        if (player == null)
                            continue;

                        if (player.IsConnected)
                        {
                            player.ChatMessage(String.Format(plugin.GetMsg("OtherCapt", player), clan));
                            CuiHelper.DestroyUi(player, "CaptTableMain");
                        }
                    }
                }
                else
                {
                    if(clan == ownerName)
                    {
                        cPlayer.ChatMessage(plugin.GetMsg("AlreadyCapt", cPlayer));
                        return false;
                    }

                    plugin.Server.Broadcast(String.Format(plugin.GetMsg("StartCapt"), clan, cupName));
                    mapMarker.color1 = ConvertToColor(plugin.config.marker.markerColorCapture);
                }

                captClan = clan;
                capt = 0;
                isCapture = true;
                DrawUiCapture();

                return true;
            }

            private void StopCapture()
            {
                capt = 0;
                isCapture = false;

                var Clan = plugin.Clans?.Call<JObject>("GetClan", captClan);
                if (Clan == null)
                {
                    captClan = "";
                    mapMarker.color1 = ConvertToColor(plugin.config.marker.markerColorCanCapture);
                    return;
                }

                var members = Clan.GetValue("members") as JArray;
                foreach (var member in members)
                {
                    var id = Convert.ToUInt64(member);
                    var player = BasePlayer.FindByID(id);
                    if (player == null)
                        continue;


                    if (player.IsConnected)
                        CuiHelper.DestroyUi(player, "CaptTableMain");
                }

                ownerName = captClan;
                vendingMarker.markerShopName = ownerName;
                vendingMarker.SendNetworkUpdate();

                lastCapture = Facepunch.Math.Epoch.Current;
                plugin.Clans?.Call("AddClanPoints", ownerName, 400);
                captClan = "";
                mapMarker.color1 = ConvertToColor(plugin.config.marker.markerColorCantCapture);
                plugin.Server.Broadcast(String.Format(plugin.GetMsg("StopCapt"), ownerName, cupName));
                plugin.SaveCups();
            }

            private void UpdateMarker()
            {
                mapMarker.SendUpdate();
            }

            private void RemoveMarker()
            {
                if (mapMarker != null && !mapMarker.IsDestroyed) mapMarker.Kill();
                if (vendingMarker != null && !vendingMarker.IsDestroyed) vendingMarker.Kill();
            }

            private void CreateMarker(string color)
            {
                RemoveMarker();

                mapMarker = GameManager.server.CreateEntity("assets/prefabs/tools/map/genericradiusmarker.prefab", transform.position).GetComponent<MapMarkerGenericRadius>();
                vendingMarker = GameManager.server.CreateEntity("assets/prefabs/deployable/vendingmachine/vending_mapmarker.prefab", transform.position).GetComponent<VendingMachineMapMarker>();

                mapMarker.radius = plugin.config.marker.markerRadius;
                mapMarker.color1 = ConvertToColor(color);
                var c = ConvertToColor(color);
                mapMarker.alpha = plugin.config.marker.markerAlpha;
                mapMarker.enabled = true;
                mapMarker.OwnerID = 0;
                mapMarker.Spawn();
                mapMarker.SendUpdate();

                vendingMarker.markerShopName = ownerName;
                vendingMarker.OwnerID = 0;
                vendingMarker.Spawn();
                vendingMarker.enabled = false;
            }

            private Color ConvertToColor(string color)
            {
                if (color.StartsWith("#")) color = color.Substring(1);
                int red = int.Parse(color.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                int green = int.Parse(color.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                int blue = int.Parse(color.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                return new Color((float)red / 255, (float)green / 255, (float)blue / 255);
            }
        }
        #endregion

        #region [Command]
        [ChatCommand("getCup")]
        private void cmd_GetCup(BasePlayer player, string c, string[] args)
        {
            if (!player.IsAdmin)
                return;

            if(args.Length < 1)
            {
                player.ChatMessage("/getCup НазваниеШкафа");
                return;
            }

            var item = ItemManager.CreateByName("cupboard.tool", 1, config.settings.skinID);
            item.name = args[0];
            player.GiveItem(item);
            player.ChatMessage($"Вам выдан шкаф {args[0]}");
        }

        [ChatCommand("remCup")]
        private void cmd_remCup(BasePlayer player, string c, string[] args)
        {
            if (!player.IsAdmin)
                return;

            if (args.Length < 1)
            {
                player.ChatMessage("/remCup НазваниеШкафа");
                return;
            }

            foreach(var cup in Cups)
            {
                var comp = cup.gameObject.GetComponent<CupManager>();
                if (comp == null) continue;

                if (comp.cupName.Contains(args[0]))
                {
                    Cups.Remove(cup);
                    UnityEngine.Object.Destroy(comp);
                    cup.Kill();
                    SaveCups();
                    player.ChatMessage($"Шкаф удален");
                    return;
                }
            }

            player.ChatMessage($"Шкаф {args[0]} не найден");
        }

        [ChatCommand("radCup")]
        private void cmd_radCup(BasePlayer player, string c, string[] args)
        {
            if (!player.IsAdmin)
                return;

            if (args.Length < 1)
            {
                player.ChatMessage("/radCup НазваниеШкафа");
                return;
            }

            foreach (var cup in Cups)
            {
                var comp = cup.gameObject.GetComponent<CupManager>();
                if (comp == null) continue;

                if (comp.cupName.Contains(args[0]))
                {
                    player.SendConsoleCommand("ddraw.box", 20f, Color.red, cup.transform.position, config.settings.rewardRadius);
                    return;
                }
            }

            player.ChatMessage($"Шкаф {args[0]} не найден");
        }

        [ChatCommand("cap")]
        private void cmd_ShowCups(BasePlayer player, string c, string[] a) => CreateCaptureMenu(player);

        #endregion

        #region [UI]
        private void CreateCaptureTable(BasePlayer player, CupManager comp)
        {
            CuiElementContainer container = new CuiElementContainer();
            container.Add(new CuiElement { Parent = "Overlay", Name = "CaptTableMain", Components = { new CuiImageComponent { Color = "0 0 0 0" }, new CuiRectTransformComponent { AnchorMin = "0.7554688 0.5097222", AnchorMax = "0.9906251 0.6486111" } } });
            CreateImage(ref container, "button_close_right", "CaptTableMain", "1 1 1 1", "button_close_right", "0.8704316 0", "1 1");
            UI.CreateButton(ref container, "button_close_right", "0 0 0 0", "", 0, "0 0", "1 1", "UI_CLOSE_TABLE");

            if (!CloseUI.Contains(player.userID))
            {
                UI.CreatePanel(ref container, "CaptTablePanel", "CaptTableMain", $"0.81 0.00 0.00 0.35", "0.15 0.1", "0.89 0.9");
                var time = TimeSpan.FromSeconds(config.settings.captSeconds - comp.capt);
                UI.CreateTextOutLine(ref container, "CaptTablePanel", String.Format(GetMsg("UI_rightPanelText", player), time.Minutes, time.Seconds, comp.cupName), "1 1 1 0.6", $"0 0", $"1 1", TextAnchor.MiddleCenter, 18);
            }


            CuiHelper.DestroyUi(player, "CaptTableMain");
            CuiHelper.AddUi(player, container);
        }

        private void CreateCaptureMenu(BasePlayer player)
        {
            CuiElementContainer container = new CuiElementContainer();

            container.Add(new CuiElement { Parent = "Overlay", Name = "CaptMenuMain", Components = { new CuiImageComponent { Color = "0 0 0 0.8", Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat" }, new CuiRectTransformComponent { AnchorMin = "0 0", AnchorMax = "1 1" }, new CuiNeedsCursorComponent() } });
            UI.CreateButton(ref container, "CaptMenuMain", "0 0 0 0", "", 0, "0 0", "1 1", "UI_CLOSE_MENU");

            UI.CreatePanel(ref container, "CaptMenuPanel", "CaptMenuMain", $"0.81 0.00 0.00 0.35", "0.1984375 0.2819445", "0.7429687 0.8222222");

            UI.CreateTextOutLine(ref container, "CaptMenuPanel", GetMsg("UI_Header", player), "1 1 1 1", "0.009821426 0.9006174", "0.9900553 0.9900553", TextAnchor.MiddleCenter, 28);

            int i = 0;
            foreach(var cup in Cups)
            {
                var comp = cup.GetComponent<CupManager>();
                if (comp == null) continue;

                UI.CreatePanel(ref container, "CaptCup" + i, "CaptMenuPanel", $"0.35 0.35 1.00 0.65", $"{0.02569436 + (i * 0.25) } 0.1028278", $"{0.2196413 + (i * 0.25)} 0.8971471");

                UI.CreateTextOutLine(ref container, "CaptCup" + i, String.Format(GetMsg("UI_Name", player), comp.cupName), "1 1 1 1", "0 0.7281784", "1 1", TextAnchor.UpperCenter, 20);


                if (comp.ownerName != "-")
                {
                    UI.CreateTextOutLine(ref container, "CaptCup" + i, String.Format(GetMsg("UI_Capt", player), comp.ownerName), "1 1 1 1", "0 0.4545436", "1 0.7087604", TextAnchor.MiddleCenter, 20);

                }
                else
                {
                    UI.CreateTextOutLine(ref container, "CaptCup" + i, GetMsg("UI_Free", player), "1 1 1 1", "0 0.4545436", "1 0.7087604", TextAnchor.MiddleCenter, 20);

                }

                if (Facepunch.Math.Epoch.Current - comp.lastCapture < config.settings.captDelay)
                {
                    var time = TimeSpan.FromSeconds(comp.lastCapture + plugin.config.settings.captDelay - Facepunch.Math.Epoch.Current);
                    UI.CreateTextOutLine(ref container, "CaptCup" + i, String.Format(GetMsg("UI_NextCapt", player), time.Hours, time.Minutes), "1 1 1 1", "0 0", "1 0.4559983", TextAnchor.LowerCenter, 18);
                }
                else
                {
                    UI.CreateTextOutLine(ref container, "CaptCup" + i, GetMsg("UI_CanCapt", player), "1 1 1 1", "0 0", "1 0.4059983", TextAnchor.LowerCenter, 18);

                }
                i++;
            }

            UI.CreateTextOutLine(ref container, "CaptMenuPanel", String.Format(GetMsg("UI_Footer", player), config.settings.gatherPrecent), "1 1 1 1", "0.04017222 0.01285341", "0.9784793 0.1131105", TextAnchor.LowerCenter, 18);

            CreateImage(ref container, "button_close", "CaptMenuPanel", "1 1 1 1", "button_close", "0.9350427 0.9169025", "0.9982907 0.998088");
            UI.CreateButton(ref container, "button_close", "0 0 0 0", "", 0, "0 0", "1 1", "UI_CLOSE_MENU");

            CuiHelper.DestroyUi(player, "CaptMenuMain");
            CuiHelper.AddUi(player, container);
        }

        [ConsoleCommand("UI_CLOSE_TABLE")]
        private void cmd_UI_CLOSE_TABLE(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null) return;
            if(CloseUI.Contains(player.userID))
            {
                CloseUI.Remove(player.userID);
            }
            else
            {
                CloseUI.Add(player.userID);
                CuiHelper.DestroyUi(player, "CaptTablePanel");
            }
        }

        [ConsoleCommand("UI_CLOSE_MENU")]
        private void cmd_UI_CLOSE_MENU(ConsoleSystem.Arg arg)
        {
            BasePlayer player = arg.Player();
            if (player == null) return;

            CuiHelper.DestroyUi(player, "CaptMenuMain");
        }
        #endregion

        #region [UI generator]
        public class UI
        {
            public static void CreateOutLines(ref CuiElementContainer container, string parent, string color)
            {
                CreatePanel(ref container, "Line", parent, color, "0 0", "0.001 1");
                CreatePanel(ref container, "Line", parent, color, "0 0", "1 0.001");
                CreatePanel(ref container, "Line", parent, color, "0.999 0", "1 1");
                CreatePanel(ref container, "Line", parent, color, "0 0.999", "1 1");
            }

            public static void CreateButton(ref CuiElementContainer container, string panel, string color, string text, int size, string aMin, string aMax, string command, TextAnchor align = TextAnchor.MiddleCenter, string name = "button", float FadeIn = 0f)
            {

                container.Add(new CuiButton
                {

                    Button = { Color = color, Command = command, FadeIn = FadeIn },
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Text = { Text = text, FontSize = size, Align = align }

                },
                panel, name);
            }

            public static void CreatePanel(ref CuiElementContainer container, string name, string parent, string color, string aMin, string aMax, float Fadeout = 0f, float Fadein = 0f)
            {

                container.Add(new CuiElement
                {
                    Name = name,
                    Parent = parent,
                    Components =
        {
            new CuiImageComponent { Color = color, FadeIn = Fadein },
            new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax}
        },
                    FadeOut = Fadeout
                });
            }

            public static void CreatePanelBlur(ref CuiElementContainer container, string name, string parent, string color, string aMin, string aMax, float Fadeout = 0f, float Fadein = 0f)
            {
                container.Add(new CuiPanel()
                {
                    CursorEnabled = true,
                    RectTransform = { AnchorMin = aMin, AnchorMax = aMax },
                    Image = { Color = color, Material = "assets/content/ui/uibackgroundblur-ingamemenu.mat", FadeIn = Fadein },
                    FadeOut = Fadeout
                }, parent, name);
            }

            public static void CreateText(ref CuiElementContainer container, string parent, string text, string color, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleLeft, int size = 14, string name = "name", float Fadein = 0f)
            {
                container.Add(new CuiElement
                {
                    Parent = parent,
                    Name = name,
                    Components =
        {
            new CuiTextComponent(){ Color = color, Text = text, FontSize = size, Align = align, FadeIn = Fadein },
            new CuiRectTransformComponent{ AnchorMin =  aMin ,AnchorMax = aMax }
        }
                });
            }

            public static void CreateTextOutLine(ref CuiElementContainer container, string parent, string text, string color, string aMin, string aMax, TextAnchor align = TextAnchor.MiddleLeft, int size = 14, string name = "name", float Fadein = 0f)
            {
                container.Add(new CuiElement
                {
                    Parent = parent,
                    Name = name,
                    Components =
        {
            new CuiTextComponent(){ Color = color, Text = text, FontSize = size, Align = align, FadeIn = Fadein },
            new CuiRectTransformComponent{ AnchorMin =  aMin ,AnchorMax = aMax },
            new CuiOutlineComponent{ Color = "0 0 0 1" }
        }
                });
            }
        }

        public void CreateImage(ref CuiElementContainer container, string name, string panel, string color, string image, string aMin, string aMax, float Fadeout = 0f, float Fadein = 0f, ulong skin = 0)
        {
            container.Add(new CuiElement
            {
                Name = name,
                Parent = panel,
                Components =
        {
            new CuiRawImageComponent { Color = color, Png = (string)ImageLibrary.Call("GetImage", image, skin), FadeIn = Fadein },
            new CuiRectTransformComponent { AnchorMin = aMin, AnchorMax = aMax },

        },
                FadeOut = Fadeout
            });
        }
        #endregion

        #region [COINRUST Command]
        [ChatCommand("checkbashes")]
        private void cmd_checkbashes(BasePlayer player) 
        {
            CreateCCbashesMenu(player);
        }
        [ChatCommand("CC_closeBashes")]
        private void CC_closeBashesMenu(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, "BashesUI");
        }
        #endregion

        #region [BashesUI]
        // Макет bashes интерфейса
        private string CUI_CCBashes = @"
[
  {
    ""name"": ""BashesUI"",
    ""parent"": ""Overlay"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Image"",
        ""material"": """",
        ""color"": ""0.1176471 0.1176471 0.08627451 0.9254902""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.1817708 0.2064814"",
        ""anchormax"": ""0.8067708 0.8203703"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""cupwarsplaceholder"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""ЗАХВАТ БАШЕН\n"",
        ""fontSize"": 60,
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""UnityEngine.UI.Outline"",
        ""color"": ""1 1 1 0.6117647"",
        ""distance"": ""1 -1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145825 0.7185186"",
        ""anchormax"": ""0.765625 0.8148151"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""cplaceholder"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""COIN\n\n"",
        ""fontSize"": 51,
        ""align"": ""UpperCenter"",
        ""color"": ""1 0.9003429 0.1596648 1""
      },
      {
        ""type"": ""UnityEngine.UI.Outline"",
        ""color"": ""0.7935846 0.9571212 0.2102455 1"",
        ""distance"": ""1 -1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2213542 0.1944444"",
        ""anchormax"": ""0.6557299 0.290741"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""rplaceholder"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""RUST\n"",
        ""fontSize"": 51,
        ""align"": ""UpperCenter"",
        ""color"": ""0.7333333 0.3490196 0.03921569 1""
      },
      {
        ""type"": ""UnityEngine.UI.Outline"",
        ""color"": ""0.7333333 0.3490196 0.03921569 1"",
        ""distance"": ""1 -1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3385417 0.1944444"",
        ""anchormax"": ""0.7729175 0.290741"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Dismiss"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Button"",
        ""command"": ""CCBashes_close"",
        ""sprite"": ""assets/icons/vote_down.png"",
        ""material"": ""assets/icons/iconmaterial.mat"",
        ""color"": ""0.5882316 0.5882316 0.5882316 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.7750002 0.7611113"",
        ""anchormax"": ""0.7994793 0.8055556"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""bashback3"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Image"",
        ""material"": """",
        ""color"": ""0.3536347 0.3536347 0.3536347 0.3477162""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.7458329 0.173454"",
        ""anchormax"": ""0.9483342 0.7616885"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Bashname"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{bashname3}"",
        ""fontSize"": 25,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.5938517 0.5929954 1 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.596296"",
        ""anchormax"": ""0.7755213 0.6703702"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""captplaceholder"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Захвачено кланом:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.5435185"",
        ""anchormax"": ""0.7755213 0.6175927"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Clan"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{clanName3}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.5064813"",
        ""anchormax"": ""0.7755213 0.5805559"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""taxplaceholder"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Налог:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.4703703"",
        ""anchormax"": ""0.7755213 0.5444443"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Tax"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{tax3}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.4425925"",
        ""anchormax"": ""0.7755213 0.5166664"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""timeplaceholder"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""До повторного\nзахвата:\n"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.3824074"",
        ""anchormax"": ""0.7755213 0.4787037"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Timer"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{cctime3}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.3314814"",
        ""anchormax"": ""0.7755213 0.4277777"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""coordsplaceholder"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Квадрат:\n"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.2944444"",
        ""anchormax"": ""0.7755213 0.3907408"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Coords"",
    ""parent"": ""bashback3"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{coord3}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.6494786 0.262037"",
        ""anchormax"": ""0.7755213 0.3583334"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""bashback1"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Image"",
        ""material"": """",
        ""color"": ""0.3536347 0.3536347 0.3536347 0.3477162""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3578124 0.3129631"",
        ""anchormax"": ""0.484375 0.6740742"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Bashname"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{bashname1}"",
        ""fontSize"": 25,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.5938517 0.5929954 1 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.5953704"",
        ""anchormax"": ""0.4854167 0.6694447"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""captplaceholder"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Захвачено кланом:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.5425924"",
        ""anchormax"": ""0.4854167 0.616667"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Clan"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{clanName1}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.5055555"",
        ""anchormax"": ""0.4854167 0.5796301"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""taxplaceholder"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Налог:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.4694445"",
        ""anchormax"": ""0.4854167 0.5435188"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Tax"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{tax1}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.01234595 0.3564101"",
        ""anchormax"": ""1.00823 0.5615385"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""timeplaceholder"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""До повторного\nзахвата:\n"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.3814816"",
        ""anchormax"": ""0.4854167 0.477778"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Timer"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{cctime1}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.3305556"",
        ""anchormax"": ""0.4854167 0.426852"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""coordsplaceholder"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Квадрат:\n"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.2935186"",
        ""anchormax"": ""0.4854167 0.389815"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Coords"",
    ""parent"": ""bashback1"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{coord1}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.3593749 0.2611112"",
        ""anchormax"": ""0.4854167 0.3574077"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""bashback2"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Image"",
        ""material"": """",
        ""color"": ""0.3536347 0.3536347 0.3536347 0.3477162""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5133331 0.1734543"",
        ""anchormax"": ""0.7158332 0.7616895"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Bashname"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{bashname2}"",
        ""fontSize"": 25,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.5938517 0.5929954 1 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.5953704"",
        ""anchormax"": ""0.6317707 0.6694447"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""captplaceholder"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Захвачено кланом:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.5425924"",
        ""anchormax"": ""0.6317707 0.616667"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Clan"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{clanName2}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.5055555"",
        ""anchormax"": ""0.6317707 0.5796301"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""taxplaceholder"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Налог:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.4694445"",
        ""anchormax"": ""0.6317707 0.5435188"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Tax"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{tax2}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.4416667"",
        ""anchormax"": ""0.6317707 0.5157409"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""timeplaceholder"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""До повторного\nзахвата:\n"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.3814816"",
        ""anchormax"": ""0.6317707 0.4777779"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Timer"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{cctime2}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.3305556"",
        ""anchormax"": ""0.6317707 0.4268519"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""coordsplaceholder"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Квадрат:\n"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.2935186"",
        ""anchormax"": ""0.6317707 0.3898149"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Coords"",
    ""parent"": ""bashback2"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{coord2}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.5057291 0.2611112"",
        ""anchormax"": ""0.6317707 0.3574076"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""bashback0"",
    ""parent"": ""BashesUI"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Image"",
        ""material"": """",
        ""color"": ""0.3536347 0.3536347 0.3536347 0.3477162""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.04416667 0.173454"",
        ""anchormax"": ""0.2466667 0.7616895"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Bashname"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{bashname0}"",
        ""fontSize"": 25,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.5938517 0.5929954 1 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.5962963"",
        ""anchormax"": ""0.340625 0.6703705"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""captplaceholder"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Захвачено кланом:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.5435183"",
        ""anchormax"": ""0.340625 0.6175928"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Clan"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{clanName0}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.5064814"",
        ""anchormax"": ""0.340625 0.5805559"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""taxplaceholder"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Налог:"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.4703704"",
        ""anchormax"": ""0.340625 0.5444446"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Tax"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{tax0}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.4425926"",
        ""anchormax"": ""0.340625 0.5166668"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""timeplaceholder"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""До повторного\nзахвата:\n"",
        ""fontSize"": 15,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.3824075"",
        ""anchormax"": ""0.340625 0.4787039"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Timer"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{cctime0}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.3314815"",
        ""anchormax"": ""0.340625 0.4277779"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""coordsplaceholder"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""Квадрат:\n"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter"",
        ""color"": ""0.8901961 1 0 1""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.2944445"",
        ""anchormax"": ""0.340625 0.3907409"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  },
  {
    ""name"": ""Coords"",
    ""parent"": ""bashback0"",
    ""components"": [
      {
        ""type"": ""UnityEngine.UI.Text"",
        ""text"": ""{coord0}"",
        ""fontSize"": 16,
        ""font"": ""robotocondensed-bold.ttf"",
        ""align"": ""UpperCenter""
      },
      {
        ""type"": ""RectTransform"",
        ""anchormin"": ""0.2145833 0.2620371"",
        ""anchormax"": ""0.340625 0.3583335"",
        ""offsetmin"": ""0 0"",
        ""offsetmax"": ""0 0""
      }
    ]
  }
]";
        private void CreateCCbashesMenu(BasePlayer player) 
        // Создаёт Bashes меню 
        {
            CuiHelper.AddUi(player, CUI_CCBashes);
        }
        #endregion
    }
}