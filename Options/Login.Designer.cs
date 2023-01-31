namespace Straddle
{
    partial class Login
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.MemberIdTxt = new System.Windows.Forms.TextBox();
            this.UserIdTxt = new System.Windows.Forms.TextBox();
            this.PasswordTxt = new System.Windows.Forms.TextBox();
            this.NewPasswordTxt = new System.Windows.Forms.TextBox();
            this.LoginBtn = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label1.Location = new System.Drawing.Point(22, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "Member Id";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label2.Location = new System.Drawing.Point(33, 49);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 17);
            this.label2.TabIndex = 1;
            this.label2.Text = "User Id ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label3.Location = new System.Drawing.Point(22, 78);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(69, 17);
            this.label3.TabIndex = 2;
            this.label3.Text = "Password";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.label4.Location = new System.Drawing.Point(5, 107);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(96, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "NewPassword";
            // 
            // MemberIdTxt
            // 
            this.MemberIdTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.MemberIdTxt.Location = new System.Drawing.Point(106, 17);
            this.MemberIdTxt.Name = "MemberIdTxt";
            this.MemberIdTxt.ReadOnly = true;
            this.MemberIdTxt.Size = new System.Drawing.Size(100, 23);
            this.MemberIdTxt.TabIndex = 4;
            this.MemberIdTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // UserIdTxt
            // 
            this.UserIdTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.UserIdTxt.Location = new System.Drawing.Point(106, 46);
            this.UserIdTxt.Name = "UserIdTxt";
            this.UserIdTxt.ReadOnly = true;
            this.UserIdTxt.Size = new System.Drawing.Size(100, 23);
            this.UserIdTxt.TabIndex = 5;
            this.UserIdTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // PasswordTxt
            // 
            this.PasswordTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.PasswordTxt.Location = new System.Drawing.Point(106, 75);
            this.PasswordTxt.Name = "PasswordTxt";
            this.PasswordTxt.PasswordChar = '*';
            this.PasswordTxt.Size = new System.Drawing.Size(100, 23);
            this.PasswordTxt.TabIndex = 0;
            this.PasswordTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // NewPasswordTxt
            // 
            this.NewPasswordTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.NewPasswordTxt.Location = new System.Drawing.Point(106, 104);
            this.NewPasswordTxt.Name = "NewPasswordTxt";
            this.NewPasswordTxt.PasswordChar = '*';
            this.NewPasswordTxt.Size = new System.Drawing.Size(100, 23);
            this.NewPasswordTxt.TabIndex = 1;
            this.NewPasswordTxt.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LoginBtn
            // 
            this.LoginBtn.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.LoginBtn.Location = new System.Drawing.Point(113, 146);
            this.LoginBtn.Name = "LoginBtn";
            this.LoginBtn.Size = new System.Drawing.Size(85, 22);
            this.LoginBtn.TabIndex = 6;
            this.LoginBtn.Text = "Login";
            this.LoginBtn.UseVisualStyleBackColor = true;
            this.LoginBtn.Click += new System.EventHandler(this.LoginBtn_Click);
            // 
            // button2
            // 
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button2.Location = new System.Drawing.Point(22, 146);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(85, 22);
            this.button2.TabIndex = 7;
            this.button2.Text = "Reset";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Login
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(223, 189);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.LoginBtn);
            this.Controls.Add(this.NewPasswordTxt);
            this.Controls.Add(this.PasswordTxt);
            this.Controls.Add(this.UserIdTxt);
            this.Controls.Add(this.MemberIdTxt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "Login";
            this.Text = "Login";
            this.Load += new System.EventHandler(this.Login_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox MemberIdTxt;
        private System.Windows.Forms.TextBox UserIdTxt;
        private System.Windows.Forms.TextBox PasswordTxt;
        private System.Windows.Forms.TextBox NewPasswordTxt;
        private System.Windows.Forms.Button LoginBtn;
        private System.Windows.Forms.Button button2;
    }
}