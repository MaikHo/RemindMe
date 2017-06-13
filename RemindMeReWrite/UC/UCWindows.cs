﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RemindMe
{
    public partial class UCWindows : UserControl
    {
        int alwaysOnTop = 1;
        public UCWindows()
        {
            InitializeComponent();
        }

        private void cbPopupType_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if(cbPopupType.SelectedItem.ToString() == "Always on top (Recommended)")
                alwaysOnTop = 1;            
            else
                alwaysOnTop = 0;

            Settings set = DLSettings.GetSettings();
            set.AlwaysOnTop = alwaysOnTop;

            DLSettings.UpdateSettings(set);            
        }

        private void UCWindows_Load(object sender, EventArgs e)
        {           
            if(DLSettings.GetSettings() == null)
            {
                Settings set = new Settings();
                set.AlwaysOnTop = alwaysOnTop;
                DLSettings.UpdateSettings(set);
            }
            //Since we're not going to change the contents of this combobox anyway, we're just going to do it like this
            if (DLSettings.IsAlwaysOnTop())
                cbPopupType.SelectedItem = cbPopupType.Items[0]; 
            else
                cbPopupType.SelectedItem = cbPopupType.Items[1];
        }
    }
}
