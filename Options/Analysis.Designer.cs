namespace Straddle
{
    partial class Analysis
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.dgvMarketWatch1 = new MTControls.MTGrid.MTDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketWatch1)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvMarketWatch1
            // 
            this.dgvMarketWatch1.AllowUserToAddRows = false;
            this.dgvMarketWatch1.AllowUserToDeleteRows = false;
            this.dgvMarketWatch1.AllowUserToOrderColumns = true;
            this.dgvMarketWatch1.AllowUserToResizeRows = false;
            this.dgvMarketWatch1.BackgroundColor = System.Drawing.Color.Black;
            this.dgvMarketWatch1.BindSource = null;
            this.dgvMarketWatch1.BindSourceView = null;
            this.dgvMarketWatch1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dgvMarketWatch1.CurGroupColIdx = -1;
            this.dgvMarketWatch1.CurMouseColIdx = 0;
            this.dgvMarketWatch1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvMarketWatch1.EnableHeadersVisualStyles = false;
            this.dgvMarketWatch1.Location = new System.Drawing.Point(0, 0);
            this.dgvMarketWatch1.MultiSelect = false;
            this.dgvMarketWatch1.Name = "dgvMarketWatch1";
            this.dgvMarketWatch1.RowHeadersVisible = false;
            this.dgvMarketWatch1.RowHeadersWidth = 11;
            this.dgvMarketWatch1.RowTemplate.Height = 24;
            this.dgvMarketWatch1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvMarketWatch1.SettingPath = "";
            this.dgvMarketWatch1.Size = new System.Drawing.Size(489, 435);
            this.dgvMarketWatch1.TabIndex = 4;
            this.dgvMarketWatch1.UniqueName = "";
            // 
            // Analysis
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(489, 435);
            this.Controls.Add(this.dgvMarketWatch1);
            this.Name = "Analysis";
            this.Text = "Analysis";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Analysis_FormClosing);
            this.Load += new System.EventHandler(this.Analysis_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMarketWatch1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        public MTControls.MTGrid.MTDataGridView dgvMarketWatch1;
    }
}