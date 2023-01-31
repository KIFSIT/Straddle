using System.Collections.Generic;
using LogWriter;

using MTCommon;
using System;
using System.Data;
using ClientCommon;
using System.Net.Sockets;
using System.Timers;


namespace Straddle.AppClasses
{
    public class AppGlobal
    {
        public static MTEnums.GatewayId AllowedGatewayforStrategy;
        public static List<MarketWatch> MarketWatch;
        public static List<AnalysisWatch> AnalysisWatch;        
        public static double var_total_pnl = 0;
        public static double varexp_total_pnl = 0;
        public static string Strategy_Name = "";

        public static int EnterCount = 0;
        public static int ActiveScript = 0;
        public static int DeActiveScript = 0;
        public static List<int> monthint;
        public static Connection connection;
        public static int ManualCount;
        public static int AddCount;
        public static int enterCount;
        public static string Global_StrategyName = "";
        public static bool strategy_new_existing = false;
        public static bool Record = false;
        public static UInt64 RuleRecord = 0;
        public static int RuleIndexNo;
        public static int StrategyRuleIndexNo = 0;
        public static bool closingflg = true;
        public static UInt64 GUI_ID;
        public static double OverAllPnl;
        public static double DuePnl = 0;
        public static double Pnl = 0;
        public static double Delta = 0;
        public static double Vega = 0;
        public static double Theta = 0;
        public static double Gamma = 0;
        public static double Admin_Delta = 0;
        public static double Admin_Vega = 0;
        public static double Admin_Theta = 0;
        public static double Admin_Gamma = 0;
        public static double SpotNifty = 0;
        public static double SpotBankNifty = 0;
        public static double SpotFinNifty = 0;
        public static double upSideCallGamma = 0;
        public static double downSideCallGamma = 0;
        public static double upSidePutGamma = 0;
        public static double downSidePutGamma = 0;
        public static double LastSpotPrice = 0;
        public static double PutMTM = 0;
        public static double CallMTM = 0;
        public static double CallBuyMTM = 0;
        public static double CallSellMTM = 0;
        public static double PutBuyMTM = 0;
        public static double PutSellMTM = 0;
        public static UInt64 Token;
        public static UInt64 FutToken;
        public static UInt64 NiftyToken;
        public static UInt64 NiftyToken2;
        public static UInt64 BKToken;
        public static UInt64 BKToken2;

        public static UInt64 FinNiftyToken;

        public static double RemainDay;
        public static double URemainDay;
        public static double LastPnl = 0;
        public static string SelectedStrategy = "";
        public static Dictionary<long, RMSSendSocketHandler> R_clients =
          new Dictionary<long, RMSSendSocketHandler>();
        public static List<string> AllExpiry = new List<string>();


        public static double upperlimit = 10;
        public static double lowerlimit = 20;
        public static double Stocklimit = 1.50;



        #region Form Object
        public static OptionWatch frmWatch;
        public static MARKETWATCH frmMarketWatch;
        public static ManualTradeEntry _manualTrade;
        public static NetPositionMin_Max _NetMax_Min;
        public static SingleLeg __singleLeg;  
        public static Stragle _Strangle;
        public static Stradder _Stradder;
        public static Strategy _strategy;
        public static Analysis _Analysis;
        public static VARAnalysis _VARAnalysis;
        public static MainStraddle _MainStraddle;
        public static DD_BuyParameter _dd_BuyParameter;
        public static DD_SellParameter _dd_SellParameter;
        public static StraddleJodi _straddleJodi;
        public static RuleModifyJodi _ruleModifyJodi;
        public static BuyStopLoss _BuyStopLoss;
        public static SellStopLoss _SellStopLoss;
        public static StraddleSellStopLoss _straddleSellStopLoss;
        public static TLI_Strangle _TLI_Strangle;
        public static LSL_Strangle _LSL_Strangle;
        public static LSL_StrangleStopLoss _LSL_StrangleStopLoss;

        public static ImmediateWind _ImmediateWind;
        public static ImmediateUnwind _ImmediateUnWind;
        public static SqOffTime_Rule _sqoffTimeRule;
        public static StrategySqOff _strategySqOff;

        public static TLI_CE_Calender _TLI_Calender;
        public static TLI_PE_Calender _TLI_PE_Calender;

        public static Position_Action _PositionAction;
        public static Initial_Trailing _Initial_Trailing;
        public static ParameterInput _ParameterInput;

        public static StrategySelection _StrategySelection;
        public static GuiLevelPayoff _GuiLevelPayoff;
        public static RuleAction _RuleAction;

        public static LimitSet _limitset;
        public static BuyOrder _buyorder;
        public static SellOrder _sellorder;


        #endregion

        public static int currentHeartBeat = 0;
        public static int PreviousHeartBeat = 0;
        public static bool SQAllFlg = false;
        public static int HeartbeatCount = 0;

        public static UInt64 Unique;
        


        public static bool Flags = false;
        public static bool AnalysisFlags = false;
        public static Dictionary<string, AllDetailsStrategy> RuleMap = new Dictionary<string, AllDetailsStrategy>();
        public static Dictionary<string, AllDetailsStrategy> _RuleMap = new Dictionary<string, AllDetailsStrategy>();

        public static List<UInt64> uniqueNoMatch = new List<UInt64>();

        public static List<string> uniqueStrategyMatch = new List<string>();

        public static bool isStart = false;
        public const string Watch = "Straddle";
        public const string AnaWatch = "AnaWatch";
        public const string netWatch = "NetPosition";
        public const string Version = "1.0.9"; // modification rule
        public const string ReadContract = "D:\\nseContractFile\\";
        public static string logDirectory = MTClientEnvironment.SpecialFolder.CurrentDirectory;
        public static Dictionary<ushort, OrderRefrence> OrdStrategy = new Dictionary<ushort, OrderRefrence>();
        public static HashSet<string> g_EveryTradeLine = new HashSet<string>();
        public static DataTable NPNetPosition;
        public static string LogDir = MTClientEnvironment.SpecialFolder.CurrentDirectory;
        public static Dictionary<UInt64, int> TokenList = new Dictionary<ulong, int>();
        public static Dictionary<UInt64, List<int>> MapList = new Dictionary<UInt64, List<int>>();
        public static int Count_121 = 0;
        public static int Count_1331 = 0;
        public static int Count_3434 = 0;
        public static int Count_34 = 0;
        public static int Count_343 = 0;
        public static int Count_single;
        public static int Count_Ratio;
        public static int Count_Strangle;
        public static int Count_Straddle;
        public static int Count_Ladder;
        public static double niftyMargin = 50000;
        public static double bankniftyMargin = 37500;
        // Bank Nifty Margin utilisation figure
        public static double Bk1331Margin = 150000;
        public static double Bk121Margin = 75000;
        public static double BkboxMargin = 75000;
        // Bank Nifty Margin utilisation figure
        public static double N1331Margin = 200000;
        public static double N121Margin = 100000;
        public static double NboxMargin = 100000;
        public static int EnterLots;
        public static double OverallMarginUtilize;   
        public static double overallPremium;
        public static bool heartbeat = false;
        public static bool GotKeyDownFromEditing = false;
        public static bool GotEnterFromEditing = false;
        public static bool GotTabFromEditing = false;
        public static int TLI_Strangle = 0;
        public static int LSL_Strangle = 0;
        public static int TotalTrade = 0;
        public static Dictionary<UInt64, int> RuleTradeCount = new Dictionary<ulong, int>();
        public static List<string> SymbolFile = new List<string>();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        public delegate void MKTTerminal_MessageRecivedDel(Socket socket, byte[] message);
        public delegate void MKTTerminal_ConnectDel(Socket socket);
        public delegate void MKTTerminal_DisconnectDel(Socket socket);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="message"></param>
        public delegate void RMSTerminal_MessageRecivedDel(Socket socket, byte[] message);
        public delegate void RMSTerminal_ConnectDel(Socket socket);
        public delegate void RMSTerminal_DisconnectDel(Socket socket);

    }

    public class AllDetailsStrategy
    {
        public double RulePnl;
        public double RuleSqPnl;
        public double RuleDelta;
        public double RuleGamma;
        public double RuleVega;
        public double RuleTheta;
        public double UpGamma;
        public double DownGamma;
        public double avgTheta;
        public double Premium;
        public double LivePremium;
    } 
}
