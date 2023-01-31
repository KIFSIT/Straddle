using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MTCommon;
using Straddle.AppClasses;
using LogWriter;
using System.Diagnostics;

namespace Straddle
{
    public partial class Analysis : Form
    {
        List<string> _StrategyList = new List<string>();
        List<int> strategyCount = new List<int>();




        public Analysis()
        {
            InitializeComponent();
        }

        private void Analysis_Load(object sender, EventArgs e)
        {
            GenerateColumns();
            
            dgvMarketWatch1.LoadSaveSettings();
            AppGlobal._Analysis.dgvMarketWatch1.Rows.Clear();
           
            dgvMarketWatch1.Rows.Add();
            for (int i = 0; i < dgvMarketWatch1.Columns.Count - 1; i++)
            {
                dgvMarketWatch1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvMarketWatch1.Columns[i].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
            {
                MarketWatch watch = AppGlobal.MarketWatch[index];
                string Strategy_name = Convert.ToString(watch.Strategy);
                if (!_StrategyList.Contains(Strategy_name))
                {
                    _StrategyList.Add(Strategy_name);
                }
            }
           
            foreach (var _strategy in _StrategyList)
            {
                string[] strategyArray = _strategy.Split('_');
                int strategy_count = Convert.ToInt32(strategyArray[1]);
                if (!strategyCount.Contains(strategy_count))
                    strategyCount.Add(strategy_count);
            }
            strategyCount.Sort();


           
            AnalysisStrategy();
            AppGlobal.AnalysisFlags = false;

            ArisApi_a._arisApi.OnMarketDepthUpdate += new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdate);
        }

        public void AnalysisStrategy()
        {
            AppGlobal.AnalysisWatch = new List<AnalysisWatch>();
            foreach (var str in strategyCount)
            {
                AnalysisWatch watch = new AnalysisWatch();
                int selectindex = AppGlobal._Analysis.dgvMarketWatch1.Rows.Count - 1;
                watch.RowData = AppGlobal._Analysis.dgvMarketWatch1.Rows[selectindex];

                watch.Strategy = "Strategy_" + str;

                string strategyName = "";
                foreach (var _watch in AppGlobal.MarketWatch.Where(x => (Convert.ToString(x.Strategy) == watch.Strategy) && (Convert.ToInt32(x.StrategyId) == 0)))
                {

                    strategyName = _watch.StrategyName;
                }



                watch.RowData.Cells[AnalysisConst.Strategy].Value = watch.Strategy;
                watch.StrategyName = strategyName;
                watch.RowData.Cells[AnalysisConst.StrategyName].Value = watch.StrategyName;

                AppGlobal._Analysis.dgvMarketWatch1.Rows.Add();
               
                AppGlobal.AnalysisWatch.Insert(selectindex, watch);
            }
            AnalysisWatch.WriteXmlProfile(ref AppGlobal.AnalysisWatch);
        }

        void _arisApi_OnMarketDepthUpdate(MTApi.MTBCastPackets.MarketPicture _response)
        {
            if (InvokeRequired)
                BeginInvoke((MethodInvoker)(() => _arisApi_OnMarketDepthUpdate(_response)));
            else
            {
                try
                {
                    if (AppGlobal.NiftyToken == Convert.ToUInt64(_response.TokenNo))
                    {
                        if (AppGlobal.AnalysisFlags == false)
                            DisplayAnalysis();
                        return;

                    }
                }
                catch (Exception)
                {

                }
            }
        }

        public void DisplayAnalysis()
        {



            AppGlobal.AnalysisFlags = true;
            foreach (var kvp in AppGlobal.RuleMap.Keys)
            {
                MarketWatch watch = new MarketWatch();
                double totalPnl = 0;
                double totalDelta = 0;
                double totalVega = 0;
                double totalGamma = 0;
                double totalTheta = 0;
                double totalSqPnl = 0;


                double upCallGamma = 0;
                double upPutGamma = 0;
                double downCallGamma = 0;
                double downPutGamma = 0;


                totalPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.pnl).Sum();
                totalDelta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.sumDelta).Sum();
                totalGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.sumGamma).Sum();
                totalTheta = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.sumTheta).Sum();
                totalVega = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.sumVega).Sum();
                totalSqPnl = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Strategy == kvp).Select(x => x.Sqpnl).Sum();

                upCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE" && x.Strategy == kvp).Select(x => x.sumGamma).Sum();
                upPutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE" && x.Strategy == kvp).Select(x => x.sumGamma).Sum();


                downCallGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "CE" && x.Strategy == kvp).Select(x => x.sumGamma).Sum();
                downPutGamma = AppGlobal.MarketWatch.Where(x => x.Checked == true && x.StrategyId != 0 && x.Leg1.ContractInfo.Series == "PE" && x.Strategy == kvp).Select(x => x.sumGamma).Sum();




                AppGlobal.RuleMap[kvp].RulePnl = totalPnl;
                AppGlobal.RuleMap[kvp].RuleDelta = totalDelta;
                AppGlobal.RuleMap[kvp].RuleGamma = totalGamma;
                AppGlobal.RuleMap[kvp].RuleVega = totalVega;
                AppGlobal.RuleMap[kvp].RuleTheta = totalTheta;
                AppGlobal.RuleMap[kvp].RuleSqPnl = totalSqPnl;

                AppGlobal.RuleMap[kvp].UpGamma = (upCallGamma + (upPutGamma * -1));
                AppGlobal.RuleMap[kvp].DownGamma = ((downCallGamma * -1) + downPutGamma);

            }

           // AnalysisWatch _watch = new AnalysisWatch();
            for (int i = 0; i < AppGlobal.AnalysisWatch.Count; i++)
            {
                AnalysisWatch _watch = AppGlobal.AnalysisWatch[i];

                if (AppGlobal.RuleMap.ContainsKey(_watch.Strategy))
                {
                    _watch.RowData.Cells[AnalysisConst.Delta].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleDelta, 4);
                    _watch.RowData.Cells[AnalysisConst.Vega].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleVega, 4);
                    _watch.RowData.Cells[AnalysisConst.Theta].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleTheta, 4);
                    _watch.RowData.Cells[AnalysisConst.Gamma].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].RuleGamma, 4);

                    _watch.RowData.Cells[AnalysisConst.upGamma].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].UpGamma, 4);
                    _watch.RowData.Cells[AnalysisConst.downGamma].Value = Math.Round(AppGlobal.RuleMap[_watch.Strategy].DownGamma, 4);


                }
               
            }
            AppGlobal.AnalysisFlags = false;            
        }



        #region general functions

        private void GenerateColumn(string clName, MTEnums.FieldType fieldType, bool Editable)
        {
            dgvMarketWatch1.Columns.Add(clName, clName);
            dgvMarketWatch1.Columns[clName].ReadOnly = Editable;


            switch (fieldType)
            {
                case MTEnums.FieldType.None:
                    break;
                case MTEnums.FieldType.Date:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Format = MTConstant.DateFormatGrid;
                    break;
                case MTEnums.FieldType.Time:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Format = MTConstant.TimeFormatGrid;
                    break;
                case MTEnums.FieldType.Price:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Quantity:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Percentage:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Format = "0.00%";
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Indicator:
                    dgvMarketWatch1.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    break;
                case MTEnums.FieldType.DateTime:
                    break;
            }
        }

        private void GenerateColumns()
        {
            try
            {
                GenerateColumn(AnalysisConst.Strategy, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.StrategyName, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.Delta, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.Vega, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.Theta, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.Gamma, MTEnums.FieldType.None, true);


                GenerateColumn(AnalysisConst.upGamma, MTEnums.FieldType.None, true);
                GenerateColumn(AnalysisConst.downGamma, MTEnums.FieldType.None, true);

               
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "Column Creation... ")
                              , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
                StackTrace st = new StackTrace(ex, true);
            }
        }
        #endregion

        private void Analysis_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._Analysis = null;
            ArisApi_a._arisApi.OnMarketDepthUpdate -= new ArisApi_a.MarketDepthUpdateDelegate(_arisApi_OnMarketDepthUpdate);
        }

    }
}
