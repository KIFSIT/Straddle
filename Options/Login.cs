using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ArisDev;

namespace Straddle
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
        }

        private void LoginBtn_Click(object sender, EventArgs e)
        {
            //ArisApi_a._arisApi.ConnectToExchange(exchange, MemberIdTxt.Text, branch, UserIdTxt.Text, PasswordTxt.Text, NewPasswordTxt.Text, nnf);
        }

        private void Login_Load(object sender, EventArgs e)
        {
          
        }

       
       

        private void button2_Click(object sender, EventArgs e)
        {
            PasswordTxt.Clear();
            NewPasswordTxt.Clear();
        }
    }
}
