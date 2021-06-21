using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Eleon.Modding;
using YamlDotNet.Serialization;


namespace TicketSystem
{
    public class MyEmpyrionMod : IMod, ModInterface
    {
        internal static string ModShortName = "TicketSystem";
        public static string ModVersion = ModShortName + " v1.0.15 made by Xango2000 (Tested: v1.2 build 3243)";
        public static string ModPath = "..\\Content\\Mods\\" + ModShortName + "\\";
        internal static IModApi modApi;
        internal static bool debug = false;
        internal static Dictionary<int, Storage.StorableData> SeqNrStorage = new Dictionary<int, Storage.StorableData> { };
        public int thisSeqNr = 2000;
        internal static SetupYaml.Root SetupYamlData = new SetupYaml.Root { };
        //public ItemStack[] blankItemStack = new ItemStack[] { };
        //########################################################################################################################################################
        internal Dictionary<int, PlayerInfo> OnlineAdmins = new Dictionary<int, PlayerInfo> { };
        //internal Dictionary<int, TicketYaml.Ticket> OpenTickets = new Dictionary<int, TicketYaml.Ticket> { };
        //internal Dictionary<int, AdminSavedLocation> AdminSavedLocations = new Dictionary<int, AdminSavedLocation> { };
        internal Dictionary<int, AdminSavedLocation> PlayerLocation = new Dictionary<int, AdminSavedLocation> { };
        internal Dictionary<int, int> Client = new Dictionary<int, int> { };
        internal List<int> Online = new List<int> { };
        internal Dictionary<int, Dictionary<string, SaveLoc.Location>> SavedLocations = new Dictionary<int, Dictionary<string, SaveLoc.Location>> { };
        internal Dictionary<int, List<int>> ListPlayers = new Dictionary<int, List<int>> { };
        internal Dictionary<int, FactionInfo> Factions = new Dictionary<int, FactionInfo> { };
        public class AdminSavedLocation
        {
            public string Playfield { get; set; }
            public int coordx { get; set; }
            public int coordy { get; set; }
            public int coordz { get; set; }
        }
        internal static ApplicationMode AppMode;
        internal static bool Disable = false;
        internal static string SaveGameName = "";
        public string Throwable = "";

        //########################################################################################################################################################
        //################################################ This is where the actual Empyrion Modding API stuff Begins ############################################
        //########################################################################################################################################################
        public void Game_Start(ModGameAPI gameAPI)
        {
            Storage.GameAPI = gameAPI;
            /*
            if (File.Exists(ModPath + "ERROR.txt")) { File.Delete(ModPath + "ERROR.txt"); }
            if (File.Exists(ModPath + "Debug.txt")) { File.Delete(ModPath + "Debug.txt"); }
            
            //CommonFunctions.LogFile("Debug.txt", "ModPath = " + ModPath);
            if (Directory.GetCurrentDirectory().EndsWith("DedicatedServer"))
            {
                ModPath = "..\\Content\\Mods\\" + ModShortName + "\\";
            }
            else if (ModPath.EndsWith("empyrion\\"))
            {
                ModPath = "Content\\Mods\\TicketSystem\\";
            }
            //CommonFunctions.LogFile("Debug.txt", "ModPath = " + ModPath);
            //CommonFunctions.LogFile("Debug.txt", Directory.GetCurrentDirectory());

            SetupYamlData = SetupYaml.Setup();
            */
        }

        public void Game_Event(CmdId cmdId, ushort seqNr, object data)
        {
            try
            {
                switch (cmdId)
                {
                    case CmdId.Event_ChatMessage:
                        //Triggered when player says something in-game
                        ChatInfo Received_ChatInfo = (ChatInfo)data;
                        Throwable = "ChatMessage";
                        if (!Disable)
                        {
                            Throwable = "ChatMessage => NotDisabled";
                            string msg = Received_ChatInfo.msg.ToLower();
                            if (msg == "/mods" || msg == "!mods")
                            {
                                API.ServerTell(Received_ChatInfo.playerId, ModShortName, ModVersion, true);
                            }
                            else if (msg.ToLower() == SetupYamlData.ReinitializeCommand.ToLower())
                            {
                                SetupYamlData = SetupYaml.Setup();
                                API.ServerTell(Received_ChatInfo.playerId, ModShortName, "Reinitialized", true);
                            }
                            else if (msg.ToLower() == SetupYamlData.ListAdminsCommand.ToLower())
                            {
                                Throwable = "ChatMessage => ListAdmins";
                                //API.Chat("Player", Received_ChatInfo.playerId, "[c][ffff00]Online Admins List: [-][/c]");
                                string BodyText = "";
                                if (OnlineAdmins.Values.Count() > 0)
                                {
                                    Throwable = "ChatMessage => ListAdmins => OnlineAdminCount = 0";
                                    foreach (PlayerInfo Admin in OnlineAdmins.Values)
                                    {

                                        //API.Chat("Player", Received_ChatInfo.playerId, Admin.playerName);
                                        BodyText = BodyText + Admin.playerName + "\r\n";
                                    }
                                    Throwable = "ChatMessage => ListAdmins => DialogConfig";
                                    DialogConfig newDialog = new DialogConfig
                                    {
                                        TitleText = "Online Admins",
                                        BodyText = BodyText,
                                        ButtonTexts = new string[1] { "Close" },
                                    };
                                    Throwable = "ChatMessage => ListAdmins => DialogActionHandler";
                                    DialogActionHandler DialogHandler = new DialogActionHandler(OnlineAdmins_DialogReturn);
                                    Throwable = "ChatMessage => ListAdmins => modApi.Application.ShowDialogBox()";
                                    modApi.Application.ShowDialogBox(Received_ChatInfo.playerId, newDialog, DialogHandler, 10);
                                }
                                else
                                {
                                    API.ServerTell(Received_ChatInfo.playerId, ModShortName, "No Admins Online.", true);
                                }
                            }
                            else if (msg.StartsWith(SetupYamlData.AdminTicketsCommand.ToLower()))
                            {
                                //Admin Tickets
                                try
                                {
                                    Storage.StorableData function = new Storage.StorableData
                                    {
                                        function = "AdminTickets",
                                        Match = Convert.ToString(Received_ChatInfo.playerId),
                                        Requested = "PlayerInfo",
                                        ChatInfo = Received_ChatInfo
                                    };
                                    API.PlayerInfo(Received_ChatInfo.playerId, function);
                                }
                                catch
                                {
                                    CommonFunctions.ERROR("ERROR: in Chat section of AdminListTickets. " + msg);
                                }
                            }
                            else if (msg.StartsWith(SetupYamlData.AdminPlayerInfoCommand.ToLower()))
                            {
                                //Admin Tickets
                                try
                                {
                                    Storage.StorableData function = new Storage.StorableData
                                    {
                                        function = "AdminPlayers",
                                        Match = Convert.ToString(Received_ChatInfo.playerId),
                                        Requested = "PlayerInfo",
                                        ChatInfo = Received_ChatInfo
                                    };
                                    API.PlayerInfo(Received_ChatInfo.playerId, function);
                                }
                                catch
                                {
                                    CommonFunctions.ERROR("ERROR: Chat AdminListPlayers " + msg);
                                }
                            }
                            else if (msg.StartsWith(SetupYamlData.PlayerTicketCommand.ToLower()))
                            {
                                //**** Player Tickets
                                try
                                {
                                    Storage.StorableData function = new Storage.StorableData
                                    {
                                        function = "PlayerTicket",
                                        Match = Convert.ToString(Received_ChatInfo.playerId),
                                        Requested = "PlayerInfo",
                                        ChatInfo = Received_ChatInfo
                                    };
                                    API.PlayerInfo(Received_ChatInfo.playerId, function);
                                }
                                catch
                                {
                                    CommonFunctions.ERROR("ERROR: Chat PlayerTicket " + msg);
                                }
                            }
                            else if (msg.StartsWith(SetupYamlData.AdminTeleportTo + " "))
                            {
                                //Admin Teleport to Player
                                try
                                {
                                    Storage.StorableData function = new Storage.StorableData
                                    {
                                        function = "Teleport",
                                        Match = Convert.ToString(Received_ChatInfo.playerId),
                                        Requested = "PlayerInfo",
                                        ChatInfo = Received_ChatInfo
                                    };
                                    API.PlayerInfo(Received_ChatInfo.playerId, function);
                                }
                                catch
                                {
                                    CommonFunctions.ERROR("ERROR: Chat PlayerTicket " + msg);
                                }
                            }
                            else if (msg == SetupYamlData.AdminTeleportTo)
                            {
                                //API.Chat("player", Received_ChatInfo.playerId, "TT: You must Specify a target PlayerID. Usage: /tt [PlayerID]");
                                API.ServerTell(Received_ChatInfo.playerId, ModShortName, "You must Specify a target PlayerID. Usage: /tt [PlayerID]", true);
                            }
                            Throwable = "ChatMessage => End";
                        }
                        break;


                    case CmdId.Event_Player_Connected:
                        //Triggered when a player logs on
                        Id Received_PlayerConnected = (Id)data;
                        Throwable = "PlayerConnected";
                        try
                        {
                            Storage.StorableData function = new Storage.StorableData
                            {
                                function = "LogOn",
                                Match = Convert.ToString(Received_PlayerConnected.id),
                                Requested = "PlayerInfo"
                            };
                            API.PlayerInfo(Received_PlayerConnected.id, function);
                        }
                        catch
                        {
                            CommonFunctions.ERROR("ERROR: in Player Logon");
                        }
                        break;


                    case CmdId.Event_Player_Disconnected:
                        //Triggered when a player logs off
                        Id Received_PlayerDisconnected = (Id)data;
                        Throwable = "PlayerDisconnected";
                        try
                        {
                            if ( OnlineAdmins.Keys.Contains(Received_PlayerDisconnected.id))
                            {
                                API.ServerSay(ModShortName, "Admin " + modApi.Application.GetPlayerDataFor(Received_PlayerDisconnected.id).Value.PlayerName + " Logged Off or got disconnected.", false);
                                OnlineAdmins.Remove(Received_PlayerDisconnected.id);
                            }
                            Online.Remove(Received_PlayerDisconnected.id);
                            Client.Remove(Received_PlayerDisconnected.id);
                            //PlayerLocation.Remove(Received_PlayerDisconnected.id);
                        }
                        catch
                        {
                            CommonFunctions.ERROR("ERROR: in player logoff");
                        }
                        break;
                    #region NotUsed

                    case CmdId.Event_Player_ChangedPlayfield:
                        //Triggered when a player changes playfield
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ChangePlayfield, (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [PlayerID], [Playfield Name], [PVector3 position], [PVector3 Rotation] ));
                        IdPlayfield Received_PlayerChangedPlayfield = (IdPlayfield)data;
                        break;


                    case CmdId.Event_Playfield_Loaded:
                        //Triggered when a player goes to a playfield that isnt currently loaded in memory
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Load_Playfield, (ushort)CurrentSeqNr, new PlayfieldLoad( [float nSecs], [string nPlayfield], [int nProcessId] ));
                        PlayfieldLoad Received_PlayfieldLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Playfield_Unloaded:
                        //Triggered when there are no players left in a playfield
                        PlayfieldLoad Received_PlayfieldUnLoaded = (PlayfieldLoad)data;
                        break;


                    case CmdId.Event_Faction_Changed:
                        //Triggered when an Entity (player too?) changes faction
                        FactionChangeInfo Received_FactionChange = (FactionChangeInfo)data;
                        break;


                    case CmdId.Event_Statistics:
                        //Triggered on various game events like: Player Death, Entity Power on/off, Remove/Add Core
                        StatisticsParam Received_EventStatistics = (StatisticsParam)data;
                        break;


                    case CmdId.Event_Player_DisconnectedWaiting:
                        //Triggered When a player is having trouble logging into the server
                        Id Received_PlayerDisconnectedWaiting = (Id)data;
                        break;


                    case CmdId.Event_TraderNPCItemSold:
                        //Triggered when a player buys an item from a trader
                        TraderNPCItemSoldInfo Received_TraderNPCItemSold = (TraderNPCItemSoldInfo)data;
                        break;


                    case CmdId.Event_Player_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_List, (ushort)CurrentSeqNr, null));
                        IdList Received_PlayerList = (IdList)data;
                        break;
                    #endregion

                    case CmdId.Event_Player_Info:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Info, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        PlayerInfo Received_PlayerInfo = (PlayerInfo)data;
                        Throwable = "PlayerInfo";
                        AdminSavedLocation PlayerLoc = new AdminSavedLocation
                        {
                            Playfield = Received_PlayerInfo.playfield,
                            coordx = Convert.ToInt32(Received_PlayerInfo.pos.x),
                            coordy = Convert.ToInt32(Received_PlayerInfo.pos.y),
                            coordz = Convert.ToInt32(Received_PlayerInfo.pos.z)
                        };
                        Throwable = "PlayerInfo => SavePlayerLoc";
                        PlayerLocation[Received_PlayerInfo.entityId] = PlayerLoc;
                        Throwable = "PlayerInfo => SaveClientID";
                        Client[Received_PlayerInfo.entityId] = Received_PlayerInfo.clientId;
                        Throwable = "PlayerInfo => If Admin";
                        if (Received_PlayerInfo.permission > 0)
                        {
                            try
                            {
                                OnlineAdmins[Received_PlayerInfo.entityId] = Received_PlayerInfo;
                            }
                            catch
                            {
                                CommonFunctions.ERROR("ERROR: in PlayerInfo section adding admin to OnlineAdmins list");
                            }
                        }

                        Throwable = "PlayerInfo => if seqNr stored";
                        if (SeqNrStorage.Keys.Contains(seqNr))
                        {
                            Storage.StorableData RetrievedData = SeqNrStorage[seqNr];
                            if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "LogOn" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                Throwable = "PlayerInfo => LogOn => add Online player";
                                Online.Add(Received_PlayerInfo.entityId);
                                try
                                {
                                    CommonFunctions.Debug("Logged on " + Received_PlayerInfo.entityId + " " + Received_PlayerInfo.playerName);
                                }
                                catch
                                {
                                    CommonFunctions.Debug("Logged on " + Received_PlayerInfo.entityId);
                                }
                                Throwable = "PlayerInfo => LogOn => if permission 0 and not a hidden admin";

                                if (Received_PlayerInfo.permission > 0 && !SetupYamlData.DontBarkTheseAdmins.Contains(Received_PlayerInfo.steamId))
                                {
                                    try
                                    {
                                        //API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]TicketSystem: There are currently [b]" + SetupYamlData.OpenTickets.Count() + "[/b] Open tickets.[-][/c]");
                                        API.ServerTell(Received_PlayerInfo.entityId, ModShortName, "[c][ffff00]There are currently [b]" + SetupYaml.DictOpenTickets.Keys.Count() + "[/b] Open tickets.[-][/c]", true);
                                    } catch
                                    {
                                        CommonFunctions.ERROR("non-error: API failure in telling player current number of tickets");
                                    }
                                    //API.Chat("Global", 0, "[c][ffff00]Admin Logged on:[-][/c] " + Received_PlayerInfo.playerName);
                                    Throwable = "PlayerInfo => LogOn => API.ServerSay()";
                                    API.ServerSay(ModShortName, "[c][ffff00]Admin Logged on:[-][/c] " + Received_PlayerInfo.playerName, false);
                                    if (!OnlineAdmins.Keys.Contains(Received_PlayerInfo.entityId))
                                    {
                                        OnlineAdmins.Add(Received_PlayerInfo.entityId, Received_PlayerInfo);
                                    }
                                    Throwable = "PlayerInfo => LogOn => CommonFunctions.Timestamp()";
                                    string Timestamp = CommonFunctions.TimeStamp();

                                    Throwable = "PlayerInfo => LogOn => LogOn Log";
                                    if (Received_PlayerInfo.permission == 9)
                                    {
                                        CommonFunctions.Log(Timestamp + " Admin LogOn: " + Received_PlayerInfo.playerName);
                                    }
                                    else if (Received_PlayerInfo.permission == 6)
                                    {
                                        CommonFunctions.Log(Timestamp + " Moderator LogOn: " + Received_PlayerInfo.playerName);
                                    }
                                    else if (Received_PlayerInfo.permission == 3)
                                    {
                                        CommonFunctions.Log(Timestamp + " GameMaster LogOn: " + Received_PlayerInfo.playerName);
                                    }
                                    else
                                    {
                                        CommonFunctions.Log(Timestamp + " Player LogOn: " + Received_PlayerInfo.playerName);
                                    }
                                }
                                Throwable = "PlayerInfo => LogOn => if SaveLoc file exists";
                                if ( File.Exists(ModPath + "SaveLoc\\" + Received_PlayerInfo.steamId + ".yaml"))
                                {
                                    SaveLoc.Root ReadSaveLocData = SaveLoc.ReadYaml(ModPath + "SaveLoc\\" + Received_PlayerInfo.steamId + ".yaml");
                                    Dictionary<string, SaveLoc.Location> SaveLocation = new Dictionary<string, SaveLoc.Location> { };
                                    foreach (SaveLoc.Location Location in ReadSaveLocData.LocationData)
                                    {
                                        SaveLocation[Location.LocName] = Location;
                                    }
                                    SavedLocations[Received_PlayerInfo.entityId] = SaveLocation;
                                }
                                Throwable = "PlayerInfo => LogOn => End";
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "AdminTickets" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                //CommonFunctions.Debug("Permission = " + Received_PlayerInfo.permission);
                                Throwable = "PlayerInfo => AdminTickets => is Admin";

                                if (Received_PlayerInfo.permission > 0 || Received_PlayerInfo.steamId == "76561198117632903")
                                {
                                    if (RetrievedData.ChatInfo.msg.ToLower() == SetupYamlData.AdminTicketsCommand.ToLower())
                                    {
                                        DisplayAdmin_TicketsList(Received_PlayerInfo.entityId);
                                    }
                                    else if (RetrievedData.ChatInfo.msg.Contains(' ') && RetrievedData.ChatInfo.msg.ToLower().StartsWith(SetupYamlData.AdminTicketsCommand.ToLower()))
                                    {
                                        try
                                        {
                                            string TicketID = RetrievedData.ChatInfo.msg.Split(' ')[1];
                                            DisplayAdmin_TicketInfo(Received_PlayerInfo.entityId, Int32.Parse(TicketID));
                                        }
                                        catch
                                        {
                                            DisplayAdmin_TicketsList(Received_PlayerInfo.entityId);
                                        }
                                    }
                                }
                                else
                                {
                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Admin command received but you aren't an admin. [-][/c]" + RetrievedData.ChatInfo.msg);
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "AdminPlayers" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                CommonFunctions.Debug("Permission = " + Received_PlayerInfo.permission);
                                if (Received_PlayerInfo.permission > 0)
                                {
                                    if (RetrievedData.ChatInfo.msg == SetupYamlData.AdminPlayerInfoCommand + "s")
                                    {
                                        DisplayAdmin_PlayersList(Received_PlayerInfo.entityId, 1);
                                    }
                                    else if (RetrievedData.ChatInfo.msg.Contains(' '))
                                    {
                                        try
                                        {
                                            DisplayAdmin_PlayerInfo(Received_PlayerInfo.entityId, "player " + RetrievedData.ChatInfo.msg.Split(' ')[1], Int32.Parse(RetrievedData.ChatInfo.msg.Split(' ')[1]));
                                        }
                                        catch
                                        {
                                            DisplayAdmin_PlayersList(Received_PlayerInfo.entityId, 1);
                                        }
                                    }
                                }
                                else
                                {
                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Admin command received but you aren't an admin. [-][/c]" + RetrievedData.ChatInfo.msg);
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "PlayerTicket" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                if (RetrievedData.ChatInfo.msg == SetupYamlData.PlayerTicketCommand.ToLower())
                                {
                                    //****
                                    int PlayerTicketCount = 0;
                                    string BodyText = "Enter text below and click \"Create Ticket\"\r\nor type in a ticket number and click \"View Ticket\"\r\n\r\nYour currently open tickets.\r\n";
                                    foreach ( int ticket in SetupYaml.DictOpenTickets.Keys)
                                    {
                                        if (SetupYaml.DictOpenTickets[ticket].PlayerID == Received_PlayerInfo.entityId)
                                        {
                                            PlayerTicketCount = PlayerTicketCount + 1;
                                            BodyText = BodyText + ticket + "  "+ SetupYaml.DictOpenTickets[ticket].Bug.Last().PlayerName + ": " + SetupYaml.DictOpenTickets[ticket].Bug.Last().ChatMessage + "\r\n";
                                        }
                                    }

                                    if (PlayerTicketCount == 0)
                                    {
                                        DialogConfig newDialog = new DialogConfig
                                        {
                                            TitleText = "Ticket System",
                                            BodyText = BodyText,
                                            ButtonIdxForEnter = 3,
                                            ButtonIdxForEsc = 4,
                                            ButtonTexts = new string[2] { "Create Ticket", "Close Window" },
                                            Placeholder = "Enter new ticket text here",
                                            MaxChars = 500,
                                            InitialContent = ""
                                        };
                                        DialogActionHandler DialogHandler = new DialogActionHandler(OnClosePlayer_Generic);
                                        modApi.Application.ShowDialogBox(Received_PlayerInfo.entityId, newDialog, DialogHandler, 0);
                                    }
                                    else
                                    {
                                        DialogConfig newDialog = new DialogConfig
                                        {
                                            TitleText = "Ticket System",
                                            BodyText = BodyText,
                                            ButtonIdxForEnter = 3,
                                            ButtonIdxForEsc = 4,
                                            ButtonTexts = new string[3] { "Create Ticket", "View Ticket", "Close Window" },
                                            Placeholder = "Enter new ticket text here",
                                            MaxChars = 500,
                                            InitialContent = ""
                                        };
                                        DialogActionHandler DialogHandler = new DialogActionHandler(OnClosePlayer_Generic);
                                        modApi.Application.ShowDialogBox(Received_PlayerInfo.entityId, newDialog, DialogHandler, 1);
                                    }
                                }
                                else
                                {
                                    //try pull up all text in ticket
                                    if (RetrievedData.ChatInfo.msg.StartsWith(SetupYamlData.AdminTicketsCommand)) { }
                                    else
                                    {
                                        try
                                        {
                                            int TicketID = Int32.Parse(RetrievedData.ChatInfo.msg.Split(' ')[1]);
                                            if (SetupYaml.DictOpenTickets.Keys.Contains(TicketID))
                                            {
                                                TicketYaml.Ticket ArchivableTicket = SetupYaml.DictOpenTickets[TicketID];
                                                API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Message Log for Ticket " + TicketID + ":[-][/c]");
                                                foreach (TicketYaml.MessageLog message in ArchivableTicket.Bug)
                                                {
                                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]" + message.PlayerName + "[-][/c]: " + message.ChatMessage);
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Something went wrong attempting to list all text in ticket, check syntax.[-][/c] " + RetrievedData.ChatInfo.msg);
                                        }
                                    }
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "Teleport" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                if( Received_PlayerInfo.permission > 2)
                                {
                                try
                                {
                                    int TargetPlayer = Int32.Parse(RetrievedData.ChatInfo.msg.Split(' ')[1]);
                                    try
                                    {
                                        Storage.StorableData function = new Storage.StorableData
                                        {
                                            function = "Teleport2",
                                            Match = Convert.ToString(TargetPlayer),
                                            Requested = "PlayerInfo",
                                            TriggerPlayer = Received_PlayerInfo
                                        };
                                        API.PlayerInfo(TargetPlayer, function);
                                    }
                                    catch
                                    {
                                        CommonFunctions.ERROR("ERROR: in PlayerInfo section of Teleport To " + RetrievedData.ChatInfo.msg);
                                    }
                                }
                                catch
                                {
                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Error:[-][/c] Player " + Int32.Parse(RetrievedData.ChatInfo.msg.Split(' ')[1]) + " Not online.");
                                }

                                }
                                else
                                {
                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Error:[-][/c] Admin Only");
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "Teleport2" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                try
                                {
                                    if (RetrievedData.TriggerPlayer.playfield == Received_PlayerInfo.playfield)
                                    {
                                        API.TeleportEntity(RetrievedData.TriggerPlayer.entityId, Received_PlayerInfo.pos);
                                    }
                                    else
                                    {
                                        API.TeleportPlayer(RetrievedData.TriggerPlayer.entityId, Received_PlayerInfo.playfield, Received_PlayerInfo.pos);
                                    }
                                }
                                catch
                                {
                                    API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Error:[-][/c] Player " + Int32.Parse(RetrievedData.ChatInfo.msg.Split(' ')[1]) + " Not online.");
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "SaveLoc" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                Dictionary<string, SaveLoc.Location> LocDict = new Dictionary<string, SaveLoc.Location>
                                {

                                };
                                string newLocName = RetrievedData.API2DialogBoxData.inputContent.Split(' ')[1].ToLower();
                                SaveLoc.Location NewLoc = new SaveLoc.Location
                                {
                                    Playfield = Received_PlayerInfo.playfield,
                                    LocName = newLocName,
                                    Coords = new List<int> { Convert.ToInt32(Received_PlayerInfo.pos.x), Convert.ToInt32(Received_PlayerInfo.pos.y), Convert.ToInt32(Received_PlayerInfo.pos.z) },
                                    Facing = new List<int> { Convert.ToInt32(Received_PlayerInfo.rot.x), Convert.ToInt32(Received_PlayerInfo.rot.y), Convert.ToInt32(Received_PlayerInfo.rot.z) }
                                };
                                if ( SavedLocations.Keys.Contains(Received_PlayerInfo.entityId))
                                {
                                    LocDict = SavedLocations[Received_PlayerInfo.entityId];
                                    LocDict[newLocName] = NewLoc;
                                }
                                else
                                {
                                    LocDict[newLocName] = NewLoc;
                                    SavedLocations[Received_PlayerInfo.entityId] = LocDict;
                                }
                                //Convert to Yaml
                                List<SaveLoc.Location> Locations = new List<SaveLoc.Location> { };
                                foreach (SaveLoc.Location Location in SavedLocations[Received_PlayerInfo.entityId].Values)
                                {
                                    Locations.Add(Location);
                                }
                                SaveLoc.Root SaveLocYamlData = new SaveLoc.Root
                                {
                                    PlayerID = Received_PlayerInfo.entityId,
                                    PlayerName = Received_PlayerInfo.playerName,
                                    LocationData = Locations
                                };
                                //Write Yaml
                                SaveLoc.WriteYaml(ModPath + "SaveLoc\\" + Received_PlayerInfo.steamId + ".yaml", SaveLocYamlData);
                                API.ServerTell(Received_PlayerInfo.entityId, ModShortName, "Location saved as " + RetrievedData.API2DialogBoxData.inputContent.Split(' ')[1], true);
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "TT" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                try
                                {
                                    if (RetrievedData.API2DialogBoxData.inputContent.ToLower() == "tt")
                                    {
                                        // teleport to ticket
                                        TicketYaml.Ticket TicketData = TicketYaml.ReadOpenTicketYaml(RetrievedData.API2DialogBoxData.CustomValue);
                                        PVector3 Pos = new PVector3
                                        {
                                            x = TicketData.Coordx,
                                            y = TicketData.Coordy,
                                            z = TicketData.Coordz
                                        };
                                        if (Received_PlayerInfo.playfield == TicketData.Playfield)
                                        {
                                            API.TeleportPlayer(Received_PlayerInfo.entityId, Received_PlayerInfo.playfield, Pos);
                                        }
                                        else
                                        {
                                            API.TeleportEntity(Received_PlayerInfo.entityId, Pos);
                                        }
                                    }
                                    else if (RetrievedData.API2DialogBoxData.inputContent.ToLower().Contains("tt"))
                                    {
                                        string destination = RetrievedData.API2DialogBoxData.inputContent.Split(' ')[1].ToLower();
                                        if (SavedLocations[Received_PlayerInfo.entityId].ContainsKey(destination))
                                        {
                                            // teleport to LocName
                                            SaveLoc.Location Loc = SavedLocations[Received_PlayerInfo.entityId][destination];
                                            PVector3 Pos = new PVector3
                                            {
                                                x = Loc.Coords[0],
                                                y = Loc.Coords[1],
                                                z = Loc.Coords[2],
                                            };
                                            PVector3 Rot = new PVector3
                                            {
                                                x = Loc.Coords[0],
                                                y = Loc.Coords[1],
                                                z = Loc.Coords[2],
                                            };
                                            if (Received_PlayerInfo.playfield == Loc.Playfield)
                                            {
                                                API.TeleportPlayer(Received_PlayerInfo.entityId, Received_PlayerInfo.playfield, Pos, Rot);
                                            }
                                            else
                                            {
                                                API.TeleportEntity(Received_PlayerInfo.entityId, Pos, Rot);
                                            }
                                        }
                                        else
                                        {
                                            //try teleport to PlayerID

                                        }
                                    }
                                }
                                catch
                                {
                                    API.ServerTell(Received_PlayerInfo.entityId, ModShortName, "Unable to teleport.", true);
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "PlayerInfoDisplayBox" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);

                                string TextBox =
                                "Playfield = " + Received_PlayerInfo.playfield + "\r\n" +
                                "Location = " + Received_PlayerInfo.pos.x + ", " + Received_PlayerInfo.pos.y + ", " + Received_PlayerInfo.pos.z + "\r\n" +
                                "Credits = " + Received_PlayerInfo.credits + "\r\n";
                                try { TextBox = TextBox + "Faction = " + Factions[Received_PlayerInfo.factionId].name + "\r\n"; } catch { TextBox = TextBox + "Faction = NoFaction\r\n"; }
                                TextBox = TextBox + "\r\n" +
                                "Health = "  + Received_PlayerInfo.health + "\r\n" + 
                                "Food = " + Received_PlayerInfo.food + "\r\n" +
                                "Stamina = " + Received_PlayerInfo.stamina + "\r\n" +
                                "Oxygen = " + Received_PlayerInfo.oxygen
                                ;
                                DialogConfig newDialog = new DialogConfig
                                {
                                    TitleText = "Player " + Received_PlayerInfo.playerName,
                                    BodyText = TextBox,
                                    ButtonIdxForEnter = 3,
                                    ButtonIdxForEsc = 4,
                                    ButtonTexts = new string[2] { "Run Command", "Close Window" },
                                    Placeholder = "Summon, Credit, TT, Kick, Ban, Heal, Heal+",
                                    MaxChars = 500,
                                    InitialContent = ""
                                };
                                DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_PlayerInfo);
                                modApi.Application.ShowDialogBox(RetrievedData.API2DialogBoxData.PlayerID, newDialog, DialogHandler, Received_PlayerInfo.entityId);

                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "Summon" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                if ( OnlineAdmins.Keys.Contains(RetrievedData.API2DialogBoxData.PlayerID))
                                {
                                    if (OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].playfield == Received_PlayerInfo.playfield)
                                    {
                                        //playfield Match
                                        CommonFunctions.Debug("Attempt Teleport Same Playfield  1");
                                        API.TeleportEntity(Received_PlayerInfo.entityId, OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].pos);
                                    }
                                    else
                                    {
                                        //No Playfield match
                                        API.TeleportPlayer(Received_PlayerInfo.entityId, OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].playfield, OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].pos);
                                    }
                                }
                                else
                                {
                                    CommonFunctions.Debug("Admin Not Listed");
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "TT from PlayerInfo" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                CommonFunctions.Debug("TT from PlayerInfo");
                                if (OnlineAdmins.Keys.Contains(RetrievedData.API2DialogBoxData.PlayerID))
                                {
                                    if (OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].playfield == Received_PlayerInfo.playfield)
                                    {
                                        //playfield Match
                                        CommonFunctions.Debug("Attempt Teleport Same Playfield  2");
                                        API.TeleportEntity(RetrievedData.API2DialogBoxData.PlayerID, Received_PlayerInfo.pos, Received_PlayerInfo.rot);
                                    }
                                    else
                                    {
                                        //No Playfield match
                                        API.TeleportPlayer(RetrievedData.API2DialogBoxData.PlayerID, Received_PlayerInfo.playfield, Received_PlayerInfo.pos, Received_PlayerInfo.rot);
                                    }
                                }
                            }
                            else if (RetrievedData.Requested == "PlayerInfo" && RetrievedData.function == "TT from General" && Convert.ToString(Received_PlayerInfo.entityId) == RetrievedData.Match)
                            {
                                SeqNrStorage.Remove(seqNr);
                                CommonFunctions.Debug("TT from General");
                                if (OnlineAdmins.Keys.Contains(RetrievedData.API2DialogBoxData.PlayerID))
                                {
                                    if (OnlineAdmins[RetrievedData.API2DialogBoxData.PlayerID].playfield == Received_PlayerInfo.playfield)
                                    {
                                        //playfield Match
                                        CommonFunctions.Debug("Attempt Teleport Same Playfield  3");
                                        API.TeleportEntity(RetrievedData.API2DialogBoxData.PlayerID, Received_PlayerInfo.pos, Received_PlayerInfo.rot);
                                    }
                                    else
                                    {
                                        //No Playfield match
                                        API.TeleportPlayer(RetrievedData.API2DialogBoxData.PlayerID, Received_PlayerInfo.playfield, Received_PlayerInfo.pos, Received_PlayerInfo.rot);
                                    }
                                }
                            }
                        }
                        break;
                    #region API1

                    case CmdId.Event_Player_Inventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerInventory = (Inventory)data;
                        break;
                    

                    case CmdId.Event_Player_ItemExchange:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_ItemExchange, (ushort)CurrentSeqNr, new ItemExchangeInfo( [id], [title], [description], [buttontext], [ItemStack[]] ));
                        ItemExchangeInfo Received_ItemExhcangeInfo = (ItemExchangeInfo)data;
                        break;


                    case CmdId.Event_DialogButtonIndex:
                        //All of This is a Guess
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ShowDialog_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //DialogBoxData Received_DialoxButtonIndex = (DialogBoxData)data;
                        break;


                    case CmdId.Event_Player_Credits:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_Credits, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        IdCredits Received_PlayerCredits = (IdCredits)data;
                        break;


                    case CmdId.Event_Player_GetAndRemoveInventory:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_GetAndRemoveInventory, (ushort)CurrentSeqNr, new Id( [playerID] ));
                        Inventory Received_PlayerGetRemoveInventory = (Inventory)data;
                        break;


                    case CmdId.Event_Playfield_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_List, (ushort)CurrentSeqNr, null));
                        PlayfieldList Received_PlayfieldList = (PlayfieldList)data;
                        break;


                    case CmdId.Event_Playfield_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Stats, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldStats Received_PlayfieldStats = (PlayfieldStats)data;
                        break;


                    case CmdId.Event_Playfield_Entity_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Playfield_Entity_List, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        PlayfieldEntityList Received_PlayfieldEntityList = (PlayfieldEntityList)data;
                        break;


                    case CmdId.Event_Dedi_Stats:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Dedi_Stats, (ushort)CurrentSeqNr, null));
                        DediStats Received_DediStats = (DediStats)data;
                        break;


                    case CmdId.Event_GlobalStructure_List:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_List, (ushort)CurrentSeqNr, null));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GlobalStructure_Update, (ushort)CurrentSeqNr, new PString( [Playfield Name] ));
                        GlobalStructureList Received_GlobalStructureList = (GlobalStructureList)data;
                        break;


                    case CmdId.Event_Entity_PosAndRot:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_PosAndRot, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdPositionRotation Received_EntityPosRot = (IdPositionRotation)data;
                        break;


                    case CmdId.Event_Get_Factions:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Get_Factions, (ushort)CurrentSeqNr, new Id( [int] )); //Requests all factions from a certain Id onwards. If you want all factions use Id 1.
                        FactionInfoList Received_FactionInfoList = (FactionInfoList)data;
                        Throwable = "GetFactions";
                        int FacID = 0;
                        try
                        {
                            foreach (FactionInfo faction in Received_FactionInfoList.factions)
                            {
                                FacID = faction.factionId;
                                //CommonFunctions.Debug("Faction: " + faction.factionId + " = " + faction.name + " AKA: " + faction.abbrev);
                                Factions[faction.factionId] = faction;
                            }
                        }
                        catch
                        {
                            CommonFunctions.ERROR("Error: Bad Faction name or abbreviation: " + FacID);
                        }
                        break;


                    case CmdId.Event_NewEntityId:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_NewEntityId, (ushort)CurrentSeqNr, null));
                        Id Request_NewEntityId = (Id)data;
                        break;


                    case CmdId.Event_Structure_BlockStatistics:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_BlockStatistics, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        IdStructureBlockInfo Received_StructureBlockStatistics = (IdStructureBlockInfo)data;
                        break;


                    case CmdId.Event_AlliancesAll:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesAll, (ushort)CurrentSeqNr, null));
                        AlliancesTable Received_AlliancesAll = (AlliancesTable)data;
                        break;


                    case CmdId.Event_AlliancesFaction:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_AlliancesFaction, (ushort)CurrentSeqNr, new AlliancesFaction( [int nFaction1Id], [int nFaction2Id], [bool nIsAllied] ));
                        AlliancesFaction Received_AlliancesFaction = (AlliancesFaction)data;
                        break;


                    case CmdId.Event_BannedPlayers:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_GetBannedPlayers, (ushort)CurrentSeqNr, null ));
                        BannedPlayerData Received_BannedPlayers = (BannedPlayerData)data;
                        break;


                    case CmdId.Event_GameEvent:
                        //Triggered by PDA Events
                        GameEventData Received_GameEvent = (GameEventData)data;
                        break;


                    case CmdId.Event_Ok:
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetInventory, (ushort)CurrentSeqNr, new Inventory(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddItem, (ushort)CurrentSeqNr, new IdItemStack(){ [changes to be made] });
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_SetCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Player_AddCredits, (ushort)CurrentSeqNr, new IdCredits( [PlayerID], [+/- Double] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Finish, (ushort)CurrentSeqNr, new Id( [PlayerID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Blueprint_Resources, (ushort)CurrentSeqNr, new BlueprintResources( [PlayerID], [List<ItemStack>], [bool ReplaceExisting?] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Teleport, (ushort)CurrentSeqNr, new IdPositionRotation( [EntityId OR PlayerID], [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_ChangePlayfield , (ushort)CurrentSeqNr, new IdPlayfieldPositionRotation( [EntityId OR PlayerID], [Playfield],  [Pvector3 Position], [Pvector3 Rotation] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Destroy2, (ushort)CurrentSeqNr, new IdPlayfield( [EntityID], [Playfield] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_SetName, (ushort)CurrentSeqNr, new Id( [EntityID] )); Wait, what? This one doesn't make sense. This is what the Wiki says though.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Entity_Spawn, (ushort)CurrentSeqNr, new EntitySpawnInfo()); Doesn't make sense to me.
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_Structure_Touch, (ushort)CurrentSeqNr, new Id( [EntityID] ));
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_SinglePlayer, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_Faction, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_InGameMessage_AllPlayers, (ushort)CurrentSeqNr, new IdMsgPrio( [int nId], [string nMsg], [byte nPrio], [float nTime] )); //for Prio: 0=Red, 1=Yellow, 2=Blue
                        //Triggered by API mod request GameAPI.Game_Request(CmdId.Request_ConsoleCommand, (ushort)CurrentSeqNr, new PString( [Telnet Command] ));

                        //uh? Not Listed in Wiki... Received_ = ()data;
                        break;


                    case CmdId.Event_Error:
                        //Triggered when there is an error coming from the API
                        ErrorInfo Received_ErrorInfo = (ErrorInfo)data;
                        break;


                    case CmdId.Event_PdaStateChange:
                        //Triggered by PDA: chapter activated/deactivated/completed
                        PdaStateInfo Received_PdaStateChange = (PdaStateInfo)data;
                        break;


                    case CmdId.Event_ConsoleCommand:
                        //Triggered when a player uses a Console Command in-game
                        ConsoleCommandInfo Received_ConsoleCommandInfo = (ConsoleCommandInfo)data;
                        CommonFunctions.Log("ConsoleCommand: " + Received_ConsoleCommandInfo.playerEntityId + " used " + Received_ConsoleCommandInfo.command + "and allowed = " + Received_ConsoleCommandInfo.allowed);
                        break;


                    default:
                        break;
                        
                }
            }
            catch (Exception ex)
            {
                CommonFunctions.ERROR("\r\nException:");
                CommonFunctions.ERROR("Source: " + ex.Source);
                CommonFunctions.ERROR("Section: " + Throwable);
                CommonFunctions.ERROR("Message: " + ex.Message);
                CommonFunctions.ERROR("Data: " + ex.Data);
                CommonFunctions.ERROR("HelpLink: " + ex.HelpLink);
                CommonFunctions.ERROR("InnerException: " + ex.InnerException);
                CommonFunctions.ERROR("StackTrace: " + ex.StackTrace);
                CommonFunctions.ERROR("TargetSite: " + ex.TargetSite + "\r\n");
            }

        }
        public void Game_Update()
        {
            //Triggered whenever Empyrion experiences "Downtime", roughly 75-100 times per second
        }

        public void Game_Exit()
        {
            //Triggered when the server is Shutting down. Does NOT pause the shutdown.
        }

        public void Init(IModApi modAPI)
        {
            Throwable = "Init()";
            MyEmpyrionMod.modApi = modAPI;
            AppMode = modApi.Application.Mode;
            ModPath = modApi.Application.GetPathFor(AppFolder.Mod) + "\\" + ModShortName + "\\";
            if (File.Exists(ModPath + "ERROR.txt")) { File.Delete(ModPath + "ERROR.txt"); }
            if (File.Exists(ModPath + "Debug.txt")) { File.Delete(ModPath + "Debug.txt"); }
            CommonFunctions.LogFile("Debug.txt", "ModPath = " + ModPath);

            string SaveGamePath = modApi.Application.GetPathFor(AppFolder.SaveGame);
            string[] SaveGameArray = SaveGamePath.Split('/');
            SaveGameName = SaveGameArray.Last();
            try
            {
                SetupYamlData = SetupYaml.Setup();
            }
            catch
            {
                CommonFunctions.ERROR("ERROR: running SetupYaml.Setup() while Initializing failed");
            }
            CommonFunctions.Log("--------------------" + " Server Start " + CommonFunctions.TimeStamp() + "----------------------------");
            API.FactionList(1);


            //modApi.Application.OnPlayfieldLoaded += Application_OnPlayfieldLoaded;
            //modApi.Application.Update += Application_Update;
            //Eleon.Pda.PdaData
            //modAPI.PDA.SpawnEntityAtPosition();
            //throw new NotImplementedException();
        }

        public void Shutdown()
        {
            //throw new NotImplementedException();
        }
        #endregion
        public void OnlineAdmins_DialogReturn(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            Throwable = "OnCloseAdmin_DialogReturn()";
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
        }

        public void OnClosePlayer_Generic(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            Throwable = "OnCloseAdmin_Generic()";
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (( buttonIdx == 0 && inputContent != null) || buttonIdx == 3)
            {
                //Create Ticket
                string SteamID = API.SteamID(PlayerID);
                string PlayerName = API.PlayerName(PlayerID);
                TicketYaml.MessageLog NewMessage = new TicketYaml.MessageLog
                {
                    PlayerName = PlayerName,
                    PlayerID = PlayerID,
                    ChatMessage = inputContent
                };
                TicketYaml.Ticket NewTicket = new TicketYaml.Ticket
                {
                    PlayerName = PlayerName,
                    PlayerID = PlayerID,
                    Timestamp = CommonFunctions.TimeStamp(),
                    UnixTimestamp = CommonFunctions.UnixTimeStamp(),
                    Playfield = PlayerLocation[PlayerID].Playfield,
                    Coordx = PlayerLocation[PlayerID].coordx,
                    Coordy = PlayerLocation[PlayerID].coordy,
                    Coordz = PlayerLocation[PlayerID].coordz,
                    Bug = new List<TicketYaml.MessageLog> { NewMessage }
                };
                int ThisTicket = SetupYamlData.TicketIDstartNumber++;
                //SetupYamlData.OpenTickets.Add(ThisTicket);
                SetupYaml.DictOpenTickets[ThisTicket] = NewTicket;
                SetupYaml.WriteYaml(ModPath + "Setup.yaml", SetupYamlData);
                TicketYaml.WriteOpenTicket(ThisTicket, NewTicket);
                API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + ThisTicket + " Created.[-][/c]", true);
                foreach (int admin in OnlineAdmins.Keys)
                {
                    API.ServerTell(OnlineAdmins[admin].entityId, ModShortName, "[c][ffff00]Ticket " + ThisTicket + " Created.[-][/c]", false);
                }
            }
            else if (buttonIdx == 1 && CustomValue != 0)
            {
                //view open ticket
                string BodyText = "";
                try
                {
                    int TicketID = 0;
                    try { TicketID = Int32.Parse(inputContent); } catch { TicketID = CustomValue; }
                    TicketYaml.Ticket TicketData = TicketYaml.ReadOpenTicketYaml(TicketID);
                    BodyText = TicketData.Timestamp + "\r\n";
                    foreach( TicketYaml.MessageLog line in TicketData.Bug)
                    {
                        BodyText = BodyText + line.PlayerName + ": " + line.ChatMessage + "\r\n";
                    }
                    DialogConfig newDialog = new DialogConfig
                    {
                        TitleText = "Ticket System",
                        BodyText = BodyText,
                        ButtonIdxForEnter = 3,
                        ButtonIdxForEsc = 4,
                        ButtonTexts = new string[3] { "Append to ticket", "Archive Ticket",  "Close Window" },
                        Placeholder = "Add additional text here",
                        MaxChars = 500,
                        InitialContent = ""
                    };
                    DialogActionHandler DialogHandler = new DialogActionHandler(OnClosePlayer_TicketView);
                    modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, TicketID);
                }
                catch
                {
                    API.ServerTell(PlayerID, ModShortName, "Ticket "+ inputContent + " not found.", true);
                }
            }
            else if (buttonIdx == 2)
            {
                //Do nothing
            }
        }

        public void OnClosePlayer_TicketView(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            Throwable = "OnCloseAdmin_TicketView()";
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            //Note: CustomeValue has been set to TicketID
            if (buttonIdx == 0)
            {
                //Append
                try
                {
                    if (SetupYaml.DictOpenTickets.Keys.Contains(CustomValue))
                    {
                        TicketYaml.Ticket TheTicket = SetupYaml.DictOpenTickets[CustomValue];
                        if (Convert.ToString(TheTicket.PlayerID) == Convert.ToString(PlayerID))
                        {
                            string PlayerName = API.PlayerName(PlayerID);
                            TicketYaml.MessageLog NewMessage = new TicketYaml.MessageLog
                            {
                                PlayerName = PlayerName,
                                PlayerID = PlayerID,
                                ChatMessage = inputContent
                            };
                            TheTicket.Bug.Add(NewMessage);
                            SetupYaml.DictOpenTickets[CustomValue] = TheTicket;
                            TicketYaml.WriteOpenTicket(CustomValue, TheTicket);
                            API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + CustomValue + " appended:[-][/c] " + inputContent, true);
                            foreach (int admin in OnlineAdmins.Keys)
                            {
                                API.ServerTell(admin, ModShortName, "[c][ffff00]Ticket " + CustomValue + " appended[-][-/c] " + inputContent, true);
                            }
                            TicketYaml.Ticket TicketData = TicketYaml.ReadOpenTicketYaml(CustomValue);
                            string BodyText = TicketData.Timestamp + "\r\n";
                            foreach (TicketYaml.MessageLog line in TicketData.Bug)
                            {
                                BodyText = BodyText + line.PlayerName + ": " + line.ChatMessage + "\r\n";
                            }
                            DialogConfig newDialog = new DialogConfig
                            {
                                TitleText = "Ticket System",
                                BodyText = BodyText,
                                ButtonIdxForEnter = 3,
                                ButtonIdxForEsc = 4,
                                ButtonTexts = new string[3] { "Append to ticket", "Archive Ticket", "Close Window" },
                                Placeholder = "Add additional text here",
                                MaxChars = 500,
                                InitialContent = ""
                            };
                            DialogActionHandler DialogHandler = new DialogActionHandler(OnClosePlayer_TicketView);
                            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, CustomValue);
                        }
                    }
                    else
                    {
                        API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket [-][/c] " + CustomValue + " no longer exists", true);
                    }
                }
                catch
                {
                    API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Something went wrong appending [-][/c] " + inputContent + " to ticket " + CustomValue, true);
                }
            }
            else if (buttonIdx == 1)
            {
                //Archive
                try
                {
                    int TicketID = CustomValue;
                    if (SetupYaml.DictOpenTickets.Keys.Contains(TicketID))
                    {
                        TicketYaml.Ticket ArchivableTicket = SetupYaml.DictOpenTickets[TicketID];
                        if ( ArchivableTicket.PlayerID == PlayerID)
                        {
                            string PlayerName = API.PlayerName(PlayerID);
                            string SteamID = API.SteamID(PlayerID);
                            TicketYaml.MessageLog NewMessage = new TicketYaml.MessageLog
                            {
                                PlayerName = PlayerName,
                                PlayerID = PlayerID,
                                ChatMessage = "Sent to Archive"
                            };
                            ArchivableTicket.Bug.Add(NewMessage);
                            TicketYaml.WriteArchive(TicketID, ArchivableTicket);
                            SetupYaml.DictOpenTickets.Remove(TicketID);
                            //SetupYamlData.OpenTickets.Remove(TicketID);
                            SetupYaml.WriteYaml(ModPath + "Setup.yaml", SetupYamlData);
                            File.Delete(ModPath + "OpenTickets\\" + TicketID + ".yaml");
                            //API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Ticket " + TicketID + " Archived.[-][/c]");
                            API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + TicketID + " Archived.[-][/c]", true);
                            CommonFunctions.Log(CommonFunctions.TimeStamp() + "  " + PlayerID + " Archived ticket " + TicketID);
                            foreach (int admin in OnlineAdmins.Keys)
                            {
                                //API.Chat("Player", OnlineAdmins[admin].entityId, "[c][ffff00]Ticket " + TicketID + " Archived[-][-/c]");
                                API.ServerTell(admin, ModShortName, "[c][ffff00]Ticket " + TicketID + " Archived.[-][/c]", false);
                            }
                            DisplayAdmin_TicketsList(PlayerID);
                        }
                        else
                        {
                            API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + CustomValue + "[-][/c] Does not belong to you.", true);
                        }
                    }
                }
                catch
                {
                    //API.Chat("Player", Received_PlayerInfo.entityId, "[c][ffff00]Something went wrong with Archive function, check syntax.[-][/c] " + RetrievedData.ChatInfo.msg);
                    API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Something went wrong with Archive function, check syntax.[-][/c] Ticket = " + CustomValue, true);
                }

            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }

        public void OnCloseAdmin_Generic(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            Throwable = "OnCloseAdmin_Generic()";
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                //Run Command
                AdminCommands(PlayerID, inputContent, CustomValue);
            }
            else if (buttonIdx == 1)
            {
                //Close Window
            }
            else if (buttonIdx == 2)
            {
                //Not Used
            }
        }

        public void OnCloseAdmin_TicketsList(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            //On Close Tickets List
            if (buttonIdx == 0)
            {
                //Run Command: Menu, Help, View [TicketID]
                bool isTicketID = false;
                try
                {
                    isTicketID = SetupYaml.DictOpenTickets.Keys.Contains(Int32.Parse(inputContent));
                }
                catch { }
                CommonFunctions.Debug("InputContent = " + inputContent);
                if (isTicketID)
                {
                    CommonFunctions.Debug("Try Display TicketInfo");
                    DisplayAdmin_TicketInfo(PlayerID, Int32.Parse(inputContent));
                    CommonFunctions.Debug("Displaying TicketInfo");
                }
                else
                {
                    CommonFunctions.Debug("Running Admin Commands");
                    AdminCommands(PlayerID, inputContent, CustomValue);
                    CommonFunctions.Debug("Ran Admin Commands");
                }
            }
            else if (buttonIdx == 1)
            {
                //**** view next segment of the list
                if (SetupYaml.DictOpenTickets.Keys.Count() > 15)
                {
                    DisplayAdmin_TicketsList(PlayerID);
                }
            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }

        public void OnCloseAdmin_TicketInfo(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                //Append
                try
                {
                    if (SetupYaml.DictOpenTickets.Keys.Contains(CustomValue))
                    {
                        TicketYaml.Ticket TheTicket = SetupYaml.DictOpenTickets[CustomValue];
                        string PlayerName = API.PlayerName(PlayerID);
                        TicketYaml.MessageLog NewMessage = new TicketYaml.MessageLog
                        {
                            PlayerName = PlayerName,
                            PlayerID = PlayerID,
                            ChatMessage = inputContent
                        };
                        TheTicket.Bug.Add(NewMessage);
                        SetupYaml.DictOpenTickets[CustomValue] = TheTicket;
                        TicketYaml.WriteOpenTicket(CustomValue, TheTicket);
                        API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + CustomValue + " appended:[-][/c] " + inputContent, true);
                        DisplayAdmin_TicketInfo(PlayerID, CustomValue);
                    }
                    else
                    {
                        API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket [-][/c] " + CustomValue + " no longer exists", true);
                    }
                }
                catch
                {
                    API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Something went wrong appending [-][/c] " + inputContent + " to ticket " + CustomValue, true);
                }
                DisplayAdmin_TicketInfo(PlayerID, CustomValue);
            }
            else if (buttonIdx == 1)
            {
                //Run Command: Archive, TT (null, PlayerID or LocID), Help
                if ( inputContent.ToLower() == "archive")
                {
                    try
                    {
                        int TicketID = CustomValue;
                        if (SetupYaml.DictOpenTickets.Keys.Contains(TicketID))
                        {
                            TicketYaml.Ticket ArchivableTicket = SetupYaml.DictOpenTickets[TicketID];
                            string PlayerName = API.PlayerName(PlayerID);
                            string SteamID = API.SteamID(PlayerID);
                            TicketYaml.MessageLog NewMessage = new TicketYaml.MessageLog
                            {
                                PlayerName = PlayerName,
                                PlayerID = PlayerID,
                                ChatMessage = "Sent to Archive"
                            };
                            ArchivableTicket.Bug.Add(NewMessage);
                            TicketYaml.WriteArchive(TicketID, ArchivableTicket);
                            SetupYaml.DictOpenTickets.Remove(TicketID);
                            //SetupYamlData.OpenTickets.Remove(TicketID);
                            SetupYaml.WriteYaml(ModPath + "Setup.yaml", SetupYamlData);
                            File.Delete(ModPath + "OpenTickets\\" + TicketID + ".yaml");
                            API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Ticket " + TicketID + " Archived.[-][/c]", true);
                            CommonFunctions.Log(CommonFunctions.TimeStamp() + "  " + PlayerID + " Archived ticket " + TicketID);
                            foreach (int admin in OnlineAdmins.Keys)
                            {
                                API.ServerTell(admin, ModShortName, "[c][ffff00]Ticket " + TicketID + " Archived.[-][/c]", false);
                            }
                            // **** add in call to open tickets list here
                        }
                    }
                    catch
                    {
                        API.ServerTell(PlayerID, ModShortName, "[c][ffff00]Something went wrong with Archive function, check syntax.[-][/c] Ticket = " + CustomValue, true);
                    }

                }
                else if (inputContent.ToLower() == "help")
                {
                    DisplayAdmin_TicketInfoMenu(PlayerID, CustomValue);
                }
                else if (inputContent.ToLower() == "tt")
                {
                    //****
                    if ( OnlineAdmins[PlayerID].playfield == SetupYaml.DictOpenTickets[CustomValue].Playfield)
                    {
                        //same playfield

                        API.TeleportEntity(PlayerID, SetupYaml.DictOpenTickets[CustomValue].Coordx, SetupYaml.DictOpenTickets[CustomValue].Coordy, SetupYaml.DictOpenTickets[CustomValue].Coordz);
                    }
                    else
                    {
                        //different playfield
                        API.TeleportPlayer(PlayerID, SetupYaml.DictOpenTickets[CustomValue].Playfield, SetupYaml.DictOpenTickets[CustomValue].Coordx, SetupYaml.DictOpenTickets[CustomValue].Coordy, SetupYaml.DictOpenTickets[CustomValue].Coordz);
                    }
                }
                else
                {
                    //****
                    AdminCommands(PlayerID, inputContent, CustomValue);
                }
            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }

        public void OnCloseAdmin_TicketInfoMenu(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                //Back to Ticket
                DisplayAdmin_TicketInfo(PlayerID, CustomValue);
            }
            else if (buttonIdx == 1)
            {
                //close
            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }

        public void OnCloseAdmin_PlayersList(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                //run command
                bool isPlayerInfo = false;
                try
                {
                    isPlayerInfo = Online.Contains(Int32.Parse(inputContent));
                }
                catch { }
                if (isPlayerInfo)
                {
                    Storage.API2DialogBoxData APIThing = new Storage.API2DialogBoxData
                    {
                        PlayerID = PlayerID,
                        inputContent = inputContent,
                        CustomValue = CustomValue
                    };
                    Storage.StorableData Storable = new Storage.StorableData
                    {
                        Match = inputContent,
                        Requested = "PlayerInfo",
                        function = "PlayerInfoDisplayBox",
                        API2DialogBoxData = APIThing
                    };
                    API.PlayerInfo(Int32.Parse(inputContent), Storable);
                }
                else
                {
                    AdminCommands(PlayerID, inputContent, CustomValue);
                }
            }
            else if (buttonIdx == 1)
            {
                //Close Window
            }
            else if (buttonIdx == 2)
            {
                //Close Window
            }
        }

        public void OnCloseAdmin_PlayerInfo(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                //command
                if (inputContent.ToLower() == "tt")
                {
                    //teleport admin to player
                    Storage.API2DialogBoxData DialogData = new Storage.API2DialogBoxData
                    {
                        CustomValue = CustomValue,
                        inputContent = inputContent,
                        PlayerID = PlayerID
                    };
                    Storage.StorableData Storable = new Storage.StorableData
                    {
                        API2DialogBoxData = DialogData,
                        function = "TT from PlayerInfo",
                        Match = Convert.ToString(CustomValue),
                        Requested = "PlayerInfo"
                    };
                    API.PlayerInfo(CustomValue, Storable);
                    CommonFunctions.Debug("Requesting Playerinfo for " + CustomValue);
                }
                else if (inputContent.ToLower().StartsWith("credit"))
                {
                    //change player's credit balance
                    if (inputContent.Contains(' '))
                    {
                        try
                        {
                            API.Credits(CustomValue, Int32.Parse(inputContent.Split(' ')[1]));
                        }
                        catch
                        {
                            API.ServerTell(PlayerID, ModShortName, "Syntax error running command 'Credit [CreditChange]'", true);
                        }
                    }
                    else
                    {
                        API.ServerTell(PlayerID, ModShortName, "Syntax error running command 'Credit [CreditChange]', missing [CreditChange] amount", true);
                    }
                }
                else if (inputContent.ToLower().StartsWith("ban"))
                {
                    //ban player
                    try
                    {
                        if (inputContent.ToLower().Contains(' '))
                        {
                            API.ConsoleCommand("ban " + API.SteamID(CustomValue) + " " + Int32.Parse(inputContent.Split(' ')[1]) + "h");
                            API.ServerTell(PlayerID, ModShortName, "Player "+ API.PlayerName(CustomValue) + " Has Been Banned for " + Int32.Parse(inputContent.Split(' ')[1]) + " Hours", true);
                        }
                        else
                        {
                            API.ServerTell(PlayerID, ModShortName, "Syntax error running command 'Ban [Duration]', missing [Duration]", true);
                        }
                    }
                    catch
                    {
                        API.ServerTell(PlayerID, ModShortName, "Error: Duration must be a number. 'Ban [Duration]'", true);

                    }
                }
                else if (inputContent.ToLower().StartsWith("kick"))
                {
                    //Kick player
                    if (inputContent.ToLower().Contains(' '))
                    {
                        string Message = CommonFunctions.ChatmessageHandler(inputContent.Split(' '), "1*");
                        API.ConsoleCommand("kick " + API.SteamID(CustomValue) + " '" + Message + "'");
                        API.ServerTell(PlayerID, ModShortName, "Player " + API.PlayerName(CustomValue) + " Has Been Kicked", true);
                    }
                    else
                    {
                        API.ServerTell(PlayerID, ModShortName, "Syntax error running command 'Kick [Reason]', missing [Reason]", true);
                    }

                }
                else if (inputContent.ToLower() == "summon")
                {
                    //Summon player to admin
                    Storage.API2DialogBoxData DialogData = new Storage.API2DialogBoxData
                    {
                        CustomValue = CustomValue,
                        inputContent = inputContent,
                        PlayerID = PlayerID
                    };
                    Storage.StorableData Storable = new Storage.StorableData
                    {
                        API2DialogBoxData = DialogData,
                        function = "Summon",
                        Match = CustomValue.ToString(),
                        Requested = "PlayerInfo"
                    };
                    API.PlayerInfo(CustomValue, Storable);
                }
                else if (inputContent.ToLower() == "heal" || inputContent.ToLower() == "heal+")
                {
                    // heal player
                    if (inputContent.ToLower().Contains("+")) API.PlayerRemoveStatusEffects(Client[CustomValue]);
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'se NeutralBodyTemp'");
                    PlayerInfoSet PI2Set = new PlayerInfoSet
                    {
                        health = 1000,
                        oxygen = 1000,
                        oxygenMax = 1000,
                        food = 1000,
                        radiation = 0,
                        stamina = 1000,
                        entityId = CustomValue
                    };
                    API.PlayerInfoSet(PI2Set);
                }
                else if (inputContent.ToLower() == "help")
                {
                    DisplayAdmin_PlayerInfoMenu(PlayerID, CustomValue);
                }
                else if (inputContent.ToLower().StartsWith("ex"))
                {
                    if (inputContent.Contains(' '))
                    {
                        string Command = CommonFunctions.ChatmessageHandler(inputContent.Split(' '), "1*");
                        try
                        {
                            API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " '" + Command + "'");
                            //API.Credits(CustomValue, Int32.Parse(inputContent.Split(' ')[1]));
                        }
                        catch
                        {
                            API.ServerTell(PlayerID, ModShortName, "Syntax error running Execute command '" + Command + "'", true);
                        }
                    }
                    else
                    {
                        API.ServerTell(PlayerID, ModShortName, "Syntax error running Execute command. See Console commands that use 'Local Player' as their target.", true);
                    }

                }
                else if (inputContent.ToLower() =="detach")
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'detach'");
                }
                else if (inputContent.ToLower() == "fbp")
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'finishbp'");
                }
                else if (inputContent.ToLower() == "sbp")
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'sbp'");
                }
                else if (inputContent.ToLower() == "di")
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'di'");
                }
                else if (inputContent.ToLower().StartsWith("gm"))
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " '" + inputContent + "'");
                }
                else if (inputContent.ToLower().StartsWith("wp"))
                {
                    string mark = CommonFunctions.ChatmessageHandler(inputContent.Split(' '), "1*");
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " 'marker add " + mark + "'");
                }
                else if (inputContent.ToLower().StartsWith("se"))
                {
                    API.ConsoleCommand("remoteex cl=" + Client[CustomValue] + " '" + inputContent + "'");
                }
                else
                {
                    AdminCommands(PlayerID, inputContent, CustomValue);
                }
                DisplayAdmin_PlayerInfo(PlayerID, inputContent, CustomValue);
            }
            else if (buttonIdx == 1)
            {
                //Run Command
                //DisplayAdmin_PlayersList(PlayerID, CustomValue + 1);
            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }

        public void OnCloseAdmin_PlayerInfoMenu(int buttonIdx, string linkID, string inputContent, int PlayerID, int CustomValue)
        {
            API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se MedikitApplied -r'");
            if (buttonIdx == 0)
            {
                Storage.API2DialogBoxData API2Data = new Storage.API2DialogBoxData
                {
                    PlayerID = PlayerID,
                    CustomValue = CustomValue,
                    inputContent = inputContent
                };
                Storage.StorableData Storable = new Storage.StorableData
                {
                    API2DialogBoxData = API2Data,
                    Match = Convert.ToString(CustomValue),
                    Requested = "PlayerInfo",
                    function = "PlayerInfoDisplayBox"
                };
                API.PlayerInfo(CustomValue, Storable);

            }
            else if (buttonIdx == 1)
            {
            }
            else if (buttonIdx == 2)
            {
                //Do nothing, just close
            }
        }


        internal void DisplayAdmin_MainMenu(int PlayerID)
        {
            Throwable = "DisplayAdmin_MainMenu()";
            //Open Window to View Main Menu
            List<string> BodyTextList = new List<string>
                                        {
                                            "Commands you can run from this window to:",
                                            "List Tickets: 'Tickets'",
                                            "Display the data in a particular ticket: 'Ticket [TicketID]'",
                                            "List online players: 'Players'",
                                            "Display PlayerInfo: 'Player [PlayerID]'",
                                            "Teleport to a player: 'tt [PlayerID]",
                                            "Teleport to a recorded Location: 'goto [LocID]'",
                                            "Heal Yourself: 'HealMe'",
                                            "Heal a player: 'Heal [PlayerID]'",
                                            "\r\nOn any other window you can use the command 'Help' to display the list of commands available in that window."
                                        };
            string BodyText = "";
            foreach (string line in BodyTextList)
            {
                BodyText = BodyText + line + "\r\n";
            }
            DialogConfig newDialog = new DialogConfig
            {
                TitleText = "Admin Ticket System",
                BodyText = BodyText,
                ButtonIdxForEnter = 3,
                ButtonIdxForEsc = 4,
                ButtonTexts = new string[2] { "Run Command", "Close Window" },
                Placeholder = "Type command here",
                MaxChars = 500,
                InitialContent = ""
            };
            DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_Generic);
            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, 0);
        }

        internal void DisplayAdmin_TicketInfo(int PlayerID, int TicketID)
        {
            Throwable = "DisplayAdmin_TicketInfo()";
            //Open window to View A Ticket
            TicketYaml.Ticket TicketData = SetupYaml.DictOpenTickets[TicketID];
            string BodyText = TicketData.Timestamp + "\r\n";
            foreach (TicketYaml.MessageLog line in TicketData.Bug)
            {
                BodyText = BodyText + line.PlayerName + ": " + line.ChatMessage + "\r\n";
            }

            DialogConfig newDialog = new DialogConfig
            {
                TitleText = "Ticket " + TicketID,
                BodyText = BodyText,
                ButtonIdxForEnter = 3,
                ButtonIdxForEsc = 4,
                ButtonTexts = new string[3] { "Append", "Run Command", "Close Window" },
                Placeholder = "Text to append goes here",
                MaxChars = 500,
                InitialContent = ""
            };
            DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketInfo);
            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, TicketID);
        }

        internal void DisplayAdmin_TicketInfoMenu(int PlayerID, int TicketID)
        {
            Throwable = "DisplayAdmin_TicketInfoMenu()";
            List<string> BodyTextList = new List<string>
                                        {
                                            "Commands you can run from the View Ticket window:",
                                            "Archive: Archive",
                                            "Teleport to Ticket location: TT",
                                            "Teleport to PlayerID: TT [PlayerID]",
                                            "Teleport to LocName: TT [LocName] (unique to you)",
                                            "Create LocName: SaveLoc [LocName]",
                                            "View players list: Players",
                                            "Heal Yourself: HealMe",
                                            "Heal Yourself and remove Debuffs: HealMe+"
                                        };
            string BodyText = "";
            foreach (string line in BodyTextList)
            {
                BodyText = BodyText + line + "\r\n";
            }
            DialogConfig newDialog = new DialogConfig
            {
                TitleText = "TicketInfo Panel Help ",
                BodyText = BodyText,
                ButtonIdxForEnter = 0,
                ButtonIdxForEsc = 1,
                ButtonTexts = new string[2] { "Ticket " + TicketID, "Close Window" }            };
            DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketInfoMenu);
            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, TicketID);

        }

        internal void DisplayAdmin_PlayerInfoMenu(int PlayerID, int PlayerInfoID){
            Throwable = "DisplayAdmin_PlayerInfoMenu()";
            List<string> BodyTextList = new List<string>
                                        {
                                            "Commands you can run from the PlayerInfo window:",
                                            "Teleport to Player (Shortcut): TT",
                                            "Change player's Credits: Credit [credits]",
                                            "Ban (Hours): Ban [Duration]",
                                            "Kick: Kick [Reason]",
                                            "Teleport player to You: Summon",
                                            "Heal: Heal",
                                            "Heal and remove Status effects: Heal+"
                                        };
            string BodyText = "";
            foreach (string line in BodyTextList)
            {
                BodyText = BodyText + line + "\r\n";
            }
            DialogConfig newDialog = new DialogConfig
            {
                TitleText = "PlayerInfo Panel Commands",
                BodyText = BodyText,
                ButtonIdxForEnter = 0,
                ButtonIdxForEsc = 1,

                ButtonTexts = new string[2] { "PlayerInfo " + PlayerInfoID, "Close Window" }
            };
            DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_PlayerInfo);
            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, PlayerInfoID);
        }

        internal void DisplayAdmin_TicketsList(int PlayerID)
        {
            Throwable = "DisplayAdmin_TicektsList";
            //int TicketCount = 0;
            //int TicketStart = (Page - 1) * 15;
            //int TicketEnd = 14;
            /*
            if (SetupYaml.DictOpenTickets.Keys.Count() > TicketStart + 15)
            {
                TicketEnd = TicketStart + 14;
            }
            else
            {
                TicketEnd = SetupYaml.DictOpenTickets.Keys.Count();
            }
            */

            //CommonFunctions.Debug("TicketStart = " + TicketStart);
            //CommonFunctions.Debug("TicketEnd = " + TicketEnd);
            //CommonFunctions.Debug("TicketCount = " + TicketCount);

            string BodyText = "TicketID  PlayerName:   LastMessage\r\n";
            foreach (int i in SetupYaml.DictOpenTickets.Keys)
            {
                try
                {
                    CommonFunctions.Debug("Value of i = " + i);
                    //CommonFunctions.Debug("TicketID = " + SetupYamlData.OpenTickets[i]);
                    BodyText = BodyText + i + "   "
                        + SetupYaml.DictOpenTickets[i].Bug.Last().PlayerName + ":  "
                        + SetupYaml.DictOpenTickets[i].Bug.Last().ChatMessage + "\r\n";
                    //TicketCount++;
                }
                catch { }
            }
            /*
            if (SetupYaml.DictOpenTickets.Keys.Count() < 15)
            {*/
                DialogConfig newDialog = new DialogConfig
                {
                    TitleText = "Open Tickets",
                    BodyText = BodyText,
                    ButtonIdxForEnter = 3,
                    ButtonIdxForEsc = 4,
                    ButtonTexts = new string[2] { "Run Command", "Close Window" },
                    Placeholder = "TicketID",
                    MaxChars = 500,
                    InitialContent = ""
                };
                DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketsList);
                modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, 0);
            /*}
            else
            {
                DialogConfig newDialog = new DialogConfig
                {
                    TitleText = "Open Tickets",
                    BodyText = BodyText,
                    ButtonIdxForEnter = 3,
                    ButtonIdxForEsc = 4,
                    ButtonTexts = new string[3] { "Run Command", "Next Page", "Close Window" },
                    Placeholder = "TicketID",
                    MaxChars = 500,
                    InitialContent = ""
                };
                DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketsList);
                modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, Page +1);
            }*/

        }

        internal void DisplayAdmin_PlayerInfo(int PlayerID, string inputContent, int CustomValue)
        {
            Throwable = "DisplayAdmin_PlayerInfo()";
            API.FactionList(1);
            Storage.API2DialogBoxData API2Data = new Storage.API2DialogBoxData
            {
                PlayerID = PlayerID,
                CustomValue = CustomValue,
                inputContent = inputContent
            };
            Storage.StorableData Storable = new Storage.StorableData
            {
                API2DialogBoxData = API2Data,
                Match = Convert.ToString(CustomValue),
                Requested = "PlayerInfo",
                function = "PlayerInfoDisplayBox"
            };
            API.PlayerInfo(CustomValue, Storable);
        }

        internal void DisplayAdmin_PlayersList(int PlayerID, int Page)
        {
            Throwable = "DisplayAdmin_PlayersList()";
            API.FactionList(1);
            string TextBox = "";
            ListPlayers[PlayerID] = Online;
            int OnlineCount = ListPlayers[PlayerID].Count();
            int IndexLastDisplayed = 0;
            foreach (int Player in Online)
            {
                try { TextBox = TextBox + Player + "   " + API.PlayerName(Player) + "\r\n"; } catch { }
            }
            /*
            if (OnlineCount > 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    TextBox = TextBox + ListPlayers[PlayerID][i] + "   " + API.PlayerName(ListPlayers[PlayerID][i]) + "\r\n";
                    IndexLastDisplayed = i;
                }
            }
            else
            {
                for (int i = 0; i < OnlineCount; i++)
                {
                    IndexLastDisplayed = i;
                }
                
            }
            */
            DialogConfig newDialog = new DialogConfig
            {
                TitleText = "Player list",
                BodyText = TextBox,
                ButtonIdxForEnter = 3,
                ButtonIdxForEsc = 4,
                ButtonTexts = new string[2] { "Run Command", "Close Window" },
                Placeholder = "Basic Commands",
                MaxChars = 500,
                InitialContent = ""
            };
            DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_PlayersList);
            modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, IndexLastDisplayed);

        }


        internal void AdminCommands(int PlayerID, string inputContent, int CustomValue)
        {
            Throwable = "AdminCommands()";
            CommonFunctions.Debug("Start of AdminCommands()");
            if (inputContent.ToLower().StartsWith("tt"))
            {
                // **** TT commands
                if ( inputContent.Contains(' '))
                {
                    if (SavedLocations.ContainsKey(PlayerID))
                    {
                        string SavedLoc = inputContent.Split(' ')[1];
                        if (SavedLocations[PlayerID].Keys.Contains(SavedLoc))
                        {
                            if (SavedLocations[PlayerID][SavedLoc].Playfield == OnlineAdmins[PlayerID].playfield)
                            {
                                API.TeleportEntity(PlayerID, SavedLocations[PlayerID][SavedLoc].Coords[0], SavedLocations[PlayerID][SavedLoc].Coords[1], SavedLocations[PlayerID][SavedLoc].Coords[2]);
                            }
                            else
                            {
                                API.TeleportPlayer(PlayerID, SavedLocations[PlayerID][SavedLoc].Playfield, SavedLocations[PlayerID][SavedLoc].Coords[0], SavedLocations[PlayerID][SavedLoc].Coords[1], SavedLocations[PlayerID][SavedLoc].Coords[2]);
                            }
                        }
                    }
                    int TargetPlayer = Int32.Parse(inputContent.Split(' ')[1]);
                    Storage.API2DialogBoxData A2DBD = new Storage.API2DialogBoxData
                    {
                        PlayerID = PlayerID,
                        inputContent = inputContent,
                        CustomValue = CustomValue
                    };
                    Storage.StorableData Storable = new Storage.StorableData
                    {
                        function = "TT from General",
                        Requested = "PlayerInfo",
                        Match = Convert.ToString(TargetPlayer),
                        API2DialogBoxData = A2DBD
                    };
                    API.PlayerInfo(TargetPlayer, Storable);
                }
            }
            else if (inputContent.ToLower().StartsWith("saveloc "))
            {
                // **** SaveLoc command
                Storage.API2DialogBoxData A2DBD = new Storage.API2DialogBoxData
                {
                    PlayerID = PlayerID,
                    inputContent = inputContent,
                    CustomValue = CustomValue
                };
                Storage.StorableData Storable = new Storage.StorableData
                {
                    function = "SaveLoc",
                    Requested = "PlayerInfo",
                    Match = Convert.ToString(PlayerID),
                    API2DialogBoxData = A2DBD
                };
                API.PlayerInfo(PlayerID, Storable);
            }
            else if (inputContent.ToLower().StartsWith("healme"))
            {
                // **** HealMe command
                try
                {
                    if (inputContent.ToLower().Contains("+")) API.PlayerRemoveStatusEffects(Client[PlayerID]);
                    API.ConsoleCommand("remoteex cl=" + Client[PlayerID] + " 'se NeutralBodyTemp'");
                    PlayerInfoSet PI2Set = new PlayerInfoSet
                    {
                        health = 1000,
                        oxygen = 1000,
                        oxygenMax = 1000,
                        food = 1000,
                        radiation = 0,
                        stamina = 1000,
                        entityId = PlayerID
                    };
                    API.PlayerInfoSet(PI2Set);
                }
                catch
                {
                    API.ServerTell(PlayerID, ModShortName, "I dont know you.", true);
                }
            }
            else if (inputContent.ToLower().StartsWith("heal"))
            {
                // **** HealMe command
                if (inputContent.Contains(' '))
                {
                    try
                    {
                        int TargetPlayer = Int32.Parse(inputContent.Split(' ')[1]);
                        if (inputContent.ToLower().Contains("+")) API.PlayerRemoveStatusEffects(Client[TargetPlayer]);
                        API.ConsoleCommand("remoteex cl=" + Client[TargetPlayer] + " 'se NeutralBodyTemp'");
                        PlayerInfoSet PI2Set = new PlayerInfoSet
                        {
                            health = 1000,
                            oxygen = 1000,
                            oxygenMax = 1000,
                            food = 1000,
                            radiation = 0,
                            stamina = 1000,
                            entityId = TargetPlayer
                        };
                        API.PlayerInfoSet(PI2Set);
                    }
                    catch
                    {
                        API.ServerTell(PlayerID, ModShortName, "Isn't " + inputContent.Split(' ')[1] + " that supposed to be a player ID?  Heal [PlayerID]", true);
                    }
                }
            }
            else if (inputContent.ToLower().StartsWith("player"))
            {
                //Player Info Command
                if (inputContent.ToLower() == "players")
                {
                    //"List online players: 'Players'",
                    API.FactionList(1);
                    DisplayAdmin_PlayersList(PlayerID, 1);
                    /*
                    string TextBox = "Showing 10 per page:\r\n";
                    ListPlayers[PlayerID] = Online;
                    int OnlineCount = ListPlayers[PlayerID].Count();
                    int IndexLastDisplayed = 0;
                    if ( OnlineCount > 10)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            TextBox = TextBox + ListPlayers[PlayerID][i] + "   " + API.PlayerName(ListPlayers[PlayerID][i]) + "\r\n";
                            IndexLastDisplayed = i;
                        }
                    }
                    else
                    {
                        for (int i = 0; i < OnlineCount; i++)
                        {
                            TextBox = TextBox + ListPlayers[PlayerID][i] + "   " + API.PlayerName(ListPlayers[PlayerID][i]) + "\r\n";
                            IndexLastDisplayed = i;
                        }
                    }
                    DialogConfig newDialog = new DialogConfig
                    {
                        TitleText = "Players " + 1 + "-" + (IndexLastDisplayed + 1),
                        BodyText = TextBox,
                        ButtonIdxForEnter = 3,
                        ButtonIdxForEsc = 4,
                        ButtonTexts = new string[2] { "Run Command", "Close Window" },
                        Placeholder = "Basic Commands",
                        MaxChars = 500,
                        InitialContent = ""
                    };
                    DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_Generic);
                    modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, IndexLastDisplayed);
                    */
                }
                else
                {
                    // **** Show Specific PlayerInfo
                    //Request PlayerInfo then display it
                    Storage.API2DialogBoxData API2Data = new Storage.API2DialogBoxData
                    {
                        PlayerID = PlayerID,
                        CustomValue = CustomValue,
                        inputContent = inputContent
                    };
                    Storage.StorableData Storable = new Storage.StorableData
                    {
                        API2DialogBoxData = API2Data,
                        Match = inputContent.Split(' ')[1],
                        Requested = "PlayerInfo",
                        function = "PlayerInfoDisplayBox"
                    };
                    API.PlayerInfo(Int32.Parse(inputContent.Split(' ')[1]), Storable);
                }
            }
            else if (inputContent.ToLower().StartsWith("ticket"))
            {
                if (inputContent.Contains(' '))
                {
                    //**** Show Specific Ticket
                    try
                    {
                        DisplayAdmin_TicketInfo(PlayerID, Int32.Parse(inputContent.Split(' ')[1]));
                    }
                    catch
                    {
                        API.ServerTell(PlayerID, ModShortName, "Unable to view ticket " + inputContent.Split(' ')[1], true);
                    }
                }

                else
                {
                    int TicketCount = 0;
                    string BodyText = "TicketID  PlayerName:   LastMessage\r\n";
                    foreach (int ticket in SetupYaml.DictOpenTickets.Keys)
                    {
                        BodyText = BodyText + ticket + "  " 
                            + SetupYaml.DictOpenTickets[ticket].Bug.Last().PlayerName + ": " 
                            + SetupYaml.DictOpenTickets[ticket].Bug.Last().ChatMessage + "\r\n";
                    }

                    if (TicketCount < 15)
                    {
                        DialogConfig newDialog = new DialogConfig
                        {
                            TitleText = "Open Tickets",
                            BodyText = BodyText,
                            ButtonIdxForEnter = 3,
                            ButtonIdxForEsc = 4,
                            ButtonTexts = new string[2] { "Run Command", "Close Window" },
                            Placeholder = "TicketNumber",
                            MaxChars = 500,
                            InitialContent = ""
                        };
                        DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketsList);
                        modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, 1);
                    }
                    else
                    {
                        DialogConfig newDialog = new DialogConfig
                        {
                            TitleText = "Open Tickets",
                            BodyText = BodyText,
                            ButtonIdxForEnter = 3,
                            ButtonIdxForEsc = 4,
                            ButtonTexts = new string[3] { "Run Command", "Next Page", "Close Window" },
                            Placeholder = "TicketNumber",
                            MaxChars = 500,
                            InitialContent = ""
                        };
                        DialogActionHandler DialogHandler = new DialogActionHandler(OnCloseAdmin_TicketsList);
                        modApi.Application.ShowDialogBox(PlayerID, newDialog, DialogHandler, 1);
                    }

                }
            }
            else if (inputContent.ToLower() == "help")
            {
                DisplayAdmin_MainMenu(PlayerID);
            }
            else
            {
                //Anything else? Command not Listed?
            }
        }
    }
}

