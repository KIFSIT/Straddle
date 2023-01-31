namespace Straddle
{
    partial class MARKETWATCH
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tradeBookDataGrid1 = new System.Windows.Forms.DataGridView();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.summary = new MTControls.MTGrid.MTDataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.tradeBookDataGrid1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.summary)).BeginInit();
            this.SuspendLayout();
            // 
            // tradeBookDataGrid1
            // 
            this.tradeBookDataGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.tradeBookDataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tradeBookDataGrid1.Location = new System.Drawing.Point(3, 3);
            this.tradeBookDataGrid1.Name = "tradeBookDataGrid1";
            this.tradeBookDataGrid1.RowHeadersVisible = false;
            this.tradeBookDataGrid1.Size = new System.Drawing.Size(521, 300);
            this.tradeBookDataGrid1.TabIndex = 4;
            this.tradeBookDataGrid1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.tradeBookDataGrid1_CellDoubleClick);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Controls.Add(this.summary, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tradeBookDataGrid1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 59.49612F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40.50388F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(527, 516);
            this.tableLayoutPanel1.TabIndex = 6;
            // 
            // summary
            // 
            this.summary.AllowUserToAddRows = false;
            this.summary.AllowUserToDeleteRows = false;
            this.summary.AllowUserToOrderColumns = true;
            this.summary.AllowUserToResizeRows = false;
            this.summary.BackgroundColor = System.Drawing.Color.Black;
            this.summary.BindSource = null;
            this.summary.BindSourceView = null;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.summary.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.summary.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.summary.CurGroupColIdx = -1;
            this.summary.CurMouseColIdx = 0;
            this.summary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.summary.EnableHeadersVisualStyles = false;
            this.summary.Location = new System.Drawing.Point(3, 309);
            this.summary.MultiSelect = false;
            this.summary.Name = "summary";
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.summary.RowHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.summary.RowHeadersVisible = false;
            this.summary.RowHeadersWidth = 11;
            this.summary.RowTemplate.Height = 24;
            this.summary.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.summary.SettingPath = "";
            this.summary.Size = new System.Drawing.Size(521, 204);
            this.summary.TabIndex = 8;
            this.summary.UniqueName = "";
            // 
            // MARKETWATCH
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(527, 516);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "MARKETWATCH";
            this.Text = "MARKETWATCH - Box_Trade";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MARKETWATCH_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MARKETWATCH_FormClosed);
            this.Load += new System.EventHandler(this.MARKETWATCH_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tradeBookDataGrid1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.summary)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView tradeBookDataGrid1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public MTControls.MTGrid.MTDataGridView summary;




    }
}