using Cognex.VisionPro.ToolBlock;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YDC_Inspection
{
    public partial class FrmJob1 : Form
    {
        private CogToolBlock cogtoolblock;
        public FrmJob1(CogToolBlock cogtb)
        {
            cogtoolblock = cogtb;
            InitializeComponent();
        }

        private void cogToolBlockEditV21_Load(object sender, EventArgs e)
        {
           
        }

        private void FrmJob1_Load(object sender, EventArgs e)
        {
            cogToolBlockEditV21.Subject = cogtoolblock;
        }

        private void FrmJob1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cogToolBlockEditV21.Subject = null;
        }
    }
}
