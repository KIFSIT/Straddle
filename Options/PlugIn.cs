using System;
using ClientCommon;
using Straddle.AppClasses;
using LogWriter;

using MTCommon;
using MTControls.MTGrid;
using WeifenLuo.WinFormsUI.Docking;

namespace Straddle
{
    public class PlugIn : IPlugin
    {
        private IPluginHost m_Host;

        public PlugIn()
        {
            Name = "Straddle Trade";
        }

        #region IPlugin Members
        public IPluginHost Host
        {
            get { return m_Host; }
            set
            {
                m_Host = value;
                m_Host.Register(this);
            }
        }
        public string Name { get; set; }
        public object MainDock { get; set; }

        public bool IsAutoTradingStart
        {
            get { return AppGlobal.isStart; }
            set { AppGlobal.isStart = value; }
        }

        public MTDataGridView GridWatch { get; set; }
        public Type WatchType { get; set; }

        public void Show(MTEnums.GatewayId allowedgateway)
        {
            AppGlobal.AllowedGatewayforStrategy = allowedgateway;
            if (AppGlobal.frmWatch == null)
            {
                AppGlobal.frmWatch = new OptionWatch();
                AppGlobal.frmWatch.ShowInTaskbar = false;
                AppGlobal.frmWatch.Show((DockPanel)MainDock, DockState.Document);
            }
            else
            {
                AppGlobal.frmWatch.Activate();
            }

            WatchType = typeof(OptionWatch);
            GridWatch = AppGlobal.frmWatch.dgvMarketWatch;
        }

        public void Show(string WindowName)
        {
        }
        public void Show(string WindowName, MTEnums.GatewayId gateway)
        {
        }
        public object GetInstance(string formName = "")
        {
            //IDockContent
            if (AppGlobal.frmWatch == null)
            {
                return AppGlobal.frmWatch = new OptionWatch();
            }
            return AppGlobal.frmWatch;
        }

        public string PersistString(MTEnums.GatewayId allowedgateway, string formName = "")
        {
            AppGlobal.AllowedGatewayforStrategy = allowedgateway;
            return typeof(OptionWatch).ToString();
        }

        public void Initialize( AppErrorLog logger, object inter, object bcast )
        {
            //AppGlobal.Logger = logger;
            //AppGlobal.IntInterface = inter as Interactive;
            //AppGlobal.BCastInterface = bcast as BroadCast;
        }
        #endregion
    }
}
