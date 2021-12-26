using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using BedrockService.Shared.Classes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class RemoveServerControl : Form {
        public NetworkMessageFlags SelectedFlag;
        public RemoveServerControl() {
            InitializeComponent();
        }

        private void saveBtn_Click(object sender, EventArgs e) {
            if (checkedListBox1.CheckedItems.Count == 0) {
                return;
            }
            foreach(var item in checkedListBox1.CheckedItems) { 
                if(item.ToString() == "Remove server files") {
                    SelectedFlag = NetworkMessageFlags.RemoveSrv;
                }
                if(item.ToString() == "Remove server's backups") {
                    if (SelectedFlag == NetworkMessageFlags.RemoveSrv) {
                        SelectedFlag = NetworkMessageFlags.RemoveBckSrv;
                    } else {
                        SelectedFlag = NetworkMessageFlags.RemoveBackups;
                    }
                }
                if(item.ToString() == "Remove player record files") {
                    if (SelectedFlag == NetworkMessageFlags.RemoveSrv) {
                        SelectedFlag = NetworkMessageFlags.RemovePlySrv;
                    }
                    if (SelectedFlag == NetworkMessageFlags.RemoveBckSrv) {
                        SelectedFlag = NetworkMessageFlags.RemoveAll;
                    }
                    if (SelectedFlag == NetworkMessageFlags.RemoveBackups) {
                        SelectedFlag = NetworkMessageFlags.RemoveBckPly;
                    }
                }
            }
            DialogResult = DialogResult.OK;
        }
    }
}
