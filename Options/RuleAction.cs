using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Straddle.AppClasses;
using MTCommon;
using System.Diagnostics;
using LogWriter;

namespace Straddle
{
    public partial class RuleAction : Form
    {
        public RuleAction()
        {
            InitializeComponent();
            KeyPress += new KeyPressEventHandler(RuleAction_KeyPress);
        }

        void RuleAction_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Escape)
            {
                AppGlobal._RuleAction = null;
                Close();
            }
        }

        

        private void RuleAction_Load(object sender, EventArgs e)
        {

            GenerateColumns();
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];

            lblSymbol.Text = watch.Leg1.ContractInfo.Symbol;
            lblStrike.Text = watch.Leg1.ContractInfo.StrikePrice.ToString();
            lblSeries.Text = watch.Leg1.ContractInfo.Series;
            lblUniqueId.Text = watch.uniqueId.ToString();

            dgvRuleAction.UniqueName = MTEnums.StrategyType.Liquidity.ToString();
            dgvRuleAction.LoadSaveSettings();


            RuleDisplay(watch);

            lblKeys.Text = watch.RuleActionNo.ToString();
        }

        #region general functions

        private void GenerateColumn(string clName, MTEnums.FieldType fieldType, bool Editable)
        {
            dgvRuleAction.Columns.Add(clName, clName);
            dgvRuleAction.Columns[clName].ReadOnly = Editable;


            switch (fieldType)
            {
                case MTEnums.FieldType.None:
                    break;
                case MTEnums.FieldType.Date:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Format = MTConstant.DateFormatGrid;
                    break;
                case MTEnums.FieldType.Time:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Format = MTConstant.TimeFormatGrid;
                    break;
                case MTEnums.FieldType.Price:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Quantity:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Percentage:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Format = "0.00%";
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                    break;
                case MTEnums.FieldType.Indicator:
                    dgvRuleAction.Columns[clName].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    break;
                case MTEnums.FieldType.DateTime:
                    break;
            }
        }

        private void GenerateColumns()
        {
            try
            {
                GenerateColumn(RuleActionWatch.RuleNo, MTEnums.FieldType.None, true);
                GenerateColumn(RuleActionWatch.TradePrice, MTEnums.FieldType.None, true);
                GenerateColumn(RuleActionWatch.TradeQty, MTEnums.FieldType.None, true);
                GenerateColumn(RuleActionWatch.TradeSide, MTEnums.FieldType.None, true);
                GenerateColumn(RuleActionWatch.Action, MTEnums.FieldType.None, true);               
            }
            catch (Exception ex)
            {
                Program._form.WriteToTransactionWatch(MTMethods.GetErrorMessage(ex, "Column Creation...")
                              , LogEnums.WriteOption.LogWindow_ErrorLogFile, color: AppLog.RedColor);
                StackTrace st = new StackTrace(ex, true);
            }
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            int iRow = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iRow];
            double Price = Convert.ToDouble(txtRuleActionPrice.Text);
            int Qty = Convert.ToInt32(txtRuleActionQty.Text);
            string Side = Convert.ToString(cmbRuleActionSide.Text);

            if (watch.uniqueId == Convert.ToUInt64(lblUniqueId.Text))
            {
                if (!watch.RuleAction.ContainsKey(watch.RuleActionNo))
                {
                    watch.RuleAction.Add(watch.RuleActionNo, new RuleParameter());
                    watch.RuleAction[watch.RuleActionNo].Price = Price;
                    watch.RuleAction[watch.RuleActionNo].Lots = Qty;
                    watch.RuleAction[watch.RuleActionNo].Side = Side;
                    watch.RuleAction[watch.RuleActionNo].Preform = false;
                    watch.RuleActionNo++;
                }
            }
            RuleDisplay(watch);

            lblKeys.Text = watch.RuleActionNo.ToString();
        }

        public void RuleDisplay(MarketWatch watch)
        {
            dgvRuleAction.Rows.Clear();
            if (watch.RuleAction.Count() == 0)
                return;
            else
            {
                foreach (var kvp in watch.RuleAction.Keys)
                {
                    dgvRuleAction.Rows.Add();
                    int i = dgvRuleAction.Rows.Count - 1;
                    dgvRuleAction.Rows[i].Cells[RuleActionWatch.RuleNo].Value = Convert.ToString(kvp);
                    dgvRuleAction.Rows[i].Cells[RuleActionWatch.TradePrice].Value = watch.RuleAction[Convert.ToInt32(kvp)].Price;
                    dgvRuleAction.Rows[i].Cells[RuleActionWatch.TradeQty].Value = watch.RuleAction[Convert.ToInt32(kvp)].Lots;
                    dgvRuleAction.Rows[i].Cells[RuleActionWatch.TradeSide].Value = watch.RuleAction[Convert.ToInt32(kvp)].Side;
                    if(watch.RuleAction[Convert.ToInt32(kvp)].Preform == true)
                        dgvRuleAction.Rows[i].Cells[RuleActionWatch.Action].Value = "YES";
                    else
                        dgvRuleAction.Rows[i].Cells[RuleActionWatch.Action].Value = "NO";

                }
            }
        }

        private void RuleAction_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._RuleAction = null;
        }

        private void dgvRuleAction_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            int iRow = dgvRuleAction.CurrentCell.RowIndex;

            int iwatch = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;

            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iwatch];
            int keys = Convert.ToInt32(dgvRuleAction.Rows[iRow].Cells[RuleActionWatch.RuleNo].Value);

            lblKeys.Text = keys.ToString();

            txtRuleActionPrice.Text = watch.RuleAction[keys].Price.ToString();
            txtRuleActionQty.Text = watch.RuleAction[keys].Lots.ToString();

            cmbRuleActionSide.Text = watch.RuleAction[keys].Side.ToString();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int iRow = dgvRuleAction.CurrentCell.RowIndex;

            int iwatch = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iwatch];
            int keys = Convert.ToInt32(dgvRuleAction.Rows[iRow].Cells[RuleActionWatch.RuleNo].Value);

            if (keys == Convert.ToInt32(lblKeys.Text))
            {
                watch.RuleAction.Remove(keys);
                RuleDisplay(watch);
            }
            else
            {
                MessageBox.Show("Please select Proper Keys Index");
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            int iRow = dgvRuleAction.CurrentCell.RowIndex;

            int iwatch = AppGlobal.frmWatch.dgvMarketWatch.CurrentRow.Index;
            MarketWatch watch = new MarketWatch();
            watch = AppGlobal.MarketWatch[iwatch];
            int keys = Convert.ToInt32(dgvRuleAction.Rows[iRow].Cells[RuleActionWatch.RuleNo].Value);

            if (keys == Convert.ToInt32(lblKeys.Text))
            {
                if (!watch.RuleAction[keys].Preform)
                {
                    double TradePrice = Convert.ToDouble(txtRuleActionPrice.Text);
                    int TradeQty = Convert.ToInt32(txtRuleActionQty.Text);

                    watch.RuleAction[keys].Price = TradePrice;
                    watch.RuleAction[keys].Lots = TradeQty;
                    RuleDisplay(watch);
                }
                else
                {
                    MessageBox.Show("Rule no " + keys + " is already Traded. U cant be modify");
                }
            }
            else
            {
                MessageBox.Show("Please select Proper Keys Index");
            }

        }


    }
}
