using BedrockService.Shared.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BedrockService.Client.Forms {
    public partial class ExpertOptionsForm : Form {
        public List<Property> PropertiesToList = new List<Property>();

        public ExpertOptionsForm() {
            InitializeComponent();
        }

        public bool PopulateOptions() {
            int index = 0;
            foreach (Property p in PropertiesToList) {
                bool result;
                if(!bool.TryParse(p.Value, out result)) {
                    return false;
                }
                OptionsChecklist.Items.Add(p.KeyName);
                if (result) {
                    OptionsChecklist.SetItemChecked(index, true);
                }
                index++;
            }
            return true;
        }

        private void cancelBtn_Click(object sender, EventArgs e) {
            OptionsChecklist.Items.Clear();
            DialogResult = DialogResult.Cancel;
        }

        private void saveBtn_Click(object sender, EventArgs e) {

            for (int i = 0; i < OptionsChecklist.Items.Count; i++) {
                Property property = PropertiesToList.Where(x => x.KeyName == (string)OptionsChecklist.Items[i]).First();
                property.Value = OptionsChecklist.GetItemChecked(i).ToString();
            }
            DialogResult = DialogResult.OK;
        }
    }
}
