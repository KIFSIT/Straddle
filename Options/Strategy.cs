using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;

namespace Straddle
{
    public partial class Strategy : Form
    {
        List<string> _StrategyList;
        public Strategy()
        {
            InitializeComponent();
        }

        void AllowedStrategy()
        {
            string all_strategy = ArisApi_a._arisApi.SystemConfig.AllowStrategy.ToString();
            string[] strategy = all_strategy.Split(',');
            for (int i = 0; i < strategy.Count(); i++)
            {
                if (strategy[i] == "91")
                {
                    cmbRule.Items.Add("Single");
                }
                else if (strategy[i] == "111")
                {
                    cmbRule.Items.Add("Ratio1_1");
                }
                else if (strategy[i] == "211")
                {
                    cmbRule.Items.Add("Ratio1_2");
                }
                else if (strategy[i] == "311")
                {
                    cmbRule.Items.Add("RatioUserDefined");
                }
                else if (strategy[i] == "121")
                {
                    cmbRule.Items.Add("ButterFly");
                    cmbRule.Items.Add("BWB");
                }
                else if (strategy[i] == "1331")
                {
                    cmbRule.Items.Add("1331");
                }
                else if (strategy[i] == "1221")
                {
                    cmbRule.Items.Add("1221");
                }
                else if (strategy[i] == "2211")
                {
                    cmbRule.Items.Add("Strangle");
                    cmbRule.Items.Add("Straddle");
                }
                else if (strategy[i] == "888")
                {
                    cmbRule.Items.Add("Ladder");
                }
            }
            cmbRule.Items.Add("Empty");
        }

        private void Strategy_Load(object sender, EventArgs e)
        {
            cmbType.SelectedIndex = 0;
            


            cmbRule.Items.Clear();
            cmbStrategyName.Items.Clear();
            AllowedStrategy();
            _StrategyList = new List<string>();
            //if (AppGlobal.MarketWatch.Count() == 0)
            //{
            //    return;
            //}                       
            for (int index = 0; index < AppGlobal.MarketWatch.Count; index++)
            {   
                MarketWatch watch = AppGlobal.MarketWatch[index];
                string Strategy_name = Convert.ToString(watch.Strategy);
                if (!_StrategyList.Contains(Strategy_name))
                {
                    _StrategyList.Add(Strategy_name);
                    cmbStrategyName.Items.Add(Strategy_name);
                }
            }
            //if (_StrategyList.Count() == 0)
            //{
            //    _StrategyList.Add("Strategy_1");
            //    cmbStrategyName.Items.Add("Strategy_1");
            //}
            if (_StrategyList.Count() != 0)
                cmbStrategyName.SelectedIndex = 0;

            if (cmbType.Text == "New")
            {
                cmbStrategyName.Enabled = false;
            }
            else
            {
                cmbStrategyName.Enabled = true;
            }


        }

        private void button5_Click(object sender, EventArgs e)
        {
            AppGlobal.strategy_new_existing = false;
            string _type = Convert.ToString(cmbType.Text);
            if (_type == "New")
            {
                if (_StrategyList.Count() == 0)
                {
                    AppGlobal.Global_StrategyName = "Strategy_1";
                }
                else
                {                   
                    int count = _StrategyList.Count();
                    string strategy = _StrategyList[count - 1];
                    string[] strategyArray = strategy.Split('_');
                    int strategy_count = Convert.ToInt32(strategyArray[1]);

                    AppGlobal.Global_StrategyName = "Strategy_" + Convert.ToString(strategy_count + 1); 
                }
                AppGlobal.strategy_new_existing = true;
            }
            else if (_type == "Existing")
            {
                if (cmbStrategyName.Text != "")
                    AppGlobal.Global_StrategyName = cmbStrategyName.Text.ToString();
                else
                {
                    MessageBox.Show("Strategy Name can not be blank!!!!!!");
                    return;
                } 
            }
             if (cmbRule.Text == "Single")
            {
                if (AppGlobal.__singleLeg == null)
                {
                    AppGlobal.__singleLeg = new SingleLeg();
                    AppGlobal.__singleLeg.Show();
                }
                else
                {
                    AppGlobal.__singleLeg.Show();
                    AppGlobal.__singleLeg.Activate();
                }
            }
            
            else if (cmbRule.Text == "Strangle")
            {
                if (AppGlobal._Strangle == null)
                {
                    AppGlobal._Strangle = new Stragle();
                    AppGlobal._Strangle.Show();
                }
                else
                {
                    AppGlobal._Strangle.Show();
                    AppGlobal._Strangle.Activate();
                }
            }
            else if (cmbRule.Text == "Straddle")
            {
                if (AppGlobal._Stradder == null)
                {
                    AppGlobal._Stradder = new Stradder();
                    AppGlobal._Stradder.Show();
                }
                else
                {
                    AppGlobal._Stradder.Show();
                    AppGlobal._Stradder.Activate();
                }
            }
            else if (cmbRule.Text == "Empty")
            {

                MarketWatch watch = new MarketWatch();
                int selectindex = AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1;
                watch.RowData = AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex];
                string rulename = Convert.ToString(selectindex);
                watch.Ruleno = AppGlobal.RuleIndexNo;
                watch.RowData.Cells[WatchConst.Rule].Value = watch.Ruleno;
                watch.StrategyId = 0;
                watch.StrategyName = "Empty";
                watch.RowData.Cells[WatchConst.StrategyId].Value = watch.StrategyId;
                watch.Gui_id = AppGlobal.GUI_ID;
                watch.RowData.Cells[WatchConst.StrategyName].Value = watch.StrategyName;
                watch.IsStrikeReq = false;

                #region Row 1

                #region Leg1
                watch.Leg1 = new Straddle.AppClasses.Leg();
                watch.Leg1.ContractInfo.TokenNo = "0";
                watch.Leg1.Counter = 0;
                #endregion

                #region Leg2
                watch.Leg2 = new Straddle.AppClasses.Leg();
                watch.Leg2.ContractInfo.TokenNo = "0";
                watch.Leg2.Counter = 0;

                #endregion

                #region Leg3
                watch.Leg3 = new Straddle.AppClasses.Leg();
                watch.Leg3.ContractInfo.TokenNo = "0";
                watch.Leg3.Counter = 0;

                #endregion

                #region Leg4
                watch.Leg4 = new Straddle.AppClasses.Leg();
                watch.Leg4.ContractInfo.TokenNo = "0";
                watch.Leg4.Counter = 0;

                #endregion

                #region Unique ID

                watch.uniqueId = 0;
                watch.displayUniqueId = "0";
                watch.RowData.Cells[WatchConst.Unique].Value = watch.displayUniqueId;
                #endregion

                #region FutLeg
                watch.niftyLeg = new Straddle.AppClasses.Leg();
                watch.niftyLeg.ContractInfo.TokenNo = "0";
                watch.niftyLeg.Counter = 0;
                #endregion

                if (selectindex == AppGlobal.frmWatch.dgvMarketWatch.Rows.Count - 1)
                {
                    AppGlobal.frmWatch.dgvMarketWatch.Rows.Add();
                }
                else
                    AppGlobal.MarketWatch.RemoveAt(selectindex);
                AppGlobal.MarketWatch.Insert(selectindex, watch);
                AppGlobal.frmWatch.dgvMarketWatch.Rows[selectindex].DefaultCellStyle.BackColor = Color.LightSalmon;
                AppGlobal.RuleIndexNo++;
                #endregion

                MarketWatch.WriteXmlProfile(ref AppGlobal.MarketWatch);
            }
            

        }

        private void Strategy_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._strategy = null;
        }

        private void cmbType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cmbType.Text == "New")
            {
                cmbStrategyName.Enabled = false;
            }
            else
            {
                cmbStrategyName.Enabled = true;
            }
        }

      
    }
}
