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
    public partial class ParameterInput : Form
    {
        public ParameterInput()
        {
            InitializeComponent();
        }

        private void ParameterInput_Load(object sender, EventArgs e)
        {
            UniqueID.Text = Convert.ToString(AppGlobal.Unique);


            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == AppGlobal.Unique)))
            {
                for (int i = 0; i < watch.iterator; i++)
                {
                    ////Create label
                    //Label label = new Label();
                    //label.Text = String.Format("Level {0}", i);
                    ////Position label on screen
                    //label.Left = 30;
                    //label.Top = (i + 1) * 20;
                    ////Create textbox

                    TextBox textBox = new TextBox();
                    //Position textbox on screen
                    textBox.Name = Convert.ToString("Lots" + i);
                    textBox.Text = "0";
                    textBox.Left = 30;
                    textBox.Top = (i + 1) * 30;

                    TextBox textBox1 = new TextBox();
                    //Position textbox on screen
                    textBox1.Name = Convert.ToString("increament" + i);
                    textBox1.Text = "0";
                    textBox1.Left = 150;
                    textBox1.Top = (i + 1) * 30;

                    this.Controls.Add(textBox);
                    this.Controls.Add(textBox1);
                }
            }



            //    //TextBox textBox2 = new TextBox();
            //    ////Position textbox on screen
            //    //textBox2.Name = Convert.ToString("sqOff_Parameter" + i);
            //    //textBox2.Text = "0";
            //    //textBox2.Left = 250;
            //    //textBox2.Top = (i + 1) * 30;

            //    //Add controls to form
                
            //    this.Controls.Add(textBox);
            //    this.Controls.Add(textBox1);
            //    //this.Controls.Add(textBox2);
            //}



            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == AppGlobal.Unique)))
            {
                int i = watch.RowData.Index;
                if (watch._inputParameter == null)
                    return;
                for (int j = 0; j < watch._inputParameter.Count(); j++)
                {

                    ((TextBox)this.Controls["Lots" + (j).ToString()]).Text = watch._inputParameter[j].Lots.ToString();
                    ((TextBox)this.Controls["increament" + (j).ToString()]).Text = watch._inputParameter[j].Price.ToString();

                    if (watch._inputParameter[j].flg)
                    {
                        ((TextBox)this.Controls["Lots" + (j).ToString()]).Enabled = true;
                        ((TextBox)this.Controls["increament" + (j).ToString()]).Enabled = true;
                    }
                    else
                    {
                        ((TextBox)this.Controls["Lots" + (j).ToString()]).Enabled = true;
                        ((TextBox)this.Controls["increament" + (j).ToString()]).Enabled = true;
                    }
                }
            }

        }
           

        private void ParameterInput_FormClosing(object sender, FormClosingEventArgs e)
        {
            AppGlobal._ParameterInput = null;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UInt64 unique = AppGlobal.Unique;
            foreach (var watch in AppGlobal.MarketWatch.Where(x => (Convert.ToUInt64(x.uniqueId) == unique)))
            {
                int i = watch.RowData.Index;
                watch.iteratorflg = true;
                
                if(watch._inputParameter == null)
                    watch._inputParameter = new InputParameter[watch.iterator];
                if (watch._inputParameter.Count() == watch.iterator)
                {

                    for (int j = 0; j < watch.iterator; j++)
                    {
                        if (watch._inputParameter[j].flg != true)
                        {
                            watch._inputParameter[j].Lots = Convert.ToInt32(((TextBox)this.Controls["Lots" + (j).ToString()]).Text);
                            watch._inputParameter[j].Price = Convert.ToDouble(((TextBox)this.Controls["increament" + (j).ToString()]).Text);
                            watch._inputParameter[j].flg = false;
                        }
                    }
                }  
                //}
                //else
                //{
                //    MessageBox.Show("Previous interation size is not same!!!!");
                //}
                //if (watch.Leg1.TradeStack.Count() != 0)
                //{
                //    var tr = Array.FindIndex(watch.Leg1._inputParameter, tRow => tRow.SumLot >= Math.Abs(watch.posInt));
                //    if (tr != 0)
                //    {

                //        if (watch.Leg1.CurrentIteration == tr)
                //        {
                //            watch.Leg1.CurrentIteration = tr;
                //            watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].increament, 4);
                //            //   watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_trading_avg = Math.Round(watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].increament, 4);
                //        }
                //        else if (watch.Leg1.CurrentIteration < tr)
                //        {
                //            watch.Leg1.CurrentIteration = tr;
                //            if (Math.Abs(watch.posInt) != watch.Leg1.TotalTradecount_Iteration)
                //                watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration + 1].increament, 4);
                //            else
                //                watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].increament, 4);
                //        }
                //        else 
                //        {
                //            watch.Leg1.CurrentIteration = tr;
                //            watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration + 1].increament, 4);
                //        }
                //    }
                //    else
                //    {

                //        watch.Leg1.CurrentIteration = tr;
                //        watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average(), 4);
                //        if (Math.Abs(watch.posInt) == watch.Leg1._inputParameter[tr].SumLot)
                //        {
                //            watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg = Math.Round(watch.Leg1.TradeStack.Average() + watch.Leg1._inputParameter[watch.Leg1.CurrentIteration + 1].increament, 4);
                //        }
                //        // watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_trading_avg = Math.Round(watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_TradeStack.Average(), 4);
                //    }
                //    watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_trading_avg = Math.Round(watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_TradeStack.Average(), 4);
                //    watch.RowData.Cells[WatchConst.TradingAvg].Value = Math.Round(watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg, 4);
                //    watch.RowData.Cells[WatchConst.Iteration_TradingAvg].Value = Math.Round(watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_trading_avg, 4);
                //    watch.RowData.Cells[WatchConst.Itr_SqOff].Value = Math.Round((watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].increament * watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].sqOff_parameter) - watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].trading_avg, 4);
                //    watch.RowData.Cells[WatchConst.Iteration_Itr_SqOff].Value = Math.Round((watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].increament * watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].sqOff_parameter) - watch.Leg1._inputParameter[watch.Leg1.CurrentIteration].Iteration_trading_avg, 4);
                //    watch.RowData.Cells[WatchConst.Stack_avg].Value = Math.Round(watch.Leg1.TradeStack.Average(), 4);
                //    watch.Leg1.Save_tradeStack = watch.Leg1.TradeStack.Average();
                //}

            }
        }
    }
}
