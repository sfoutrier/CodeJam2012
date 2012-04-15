using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HallOfMirrors
{
    public partial class DumpForm : Form
    {
        public DumpForm()
        {
            InitializeComponent();
        }

        public void SetContent(string content)
        {
            dumpBox.Text = content;
            dumpBox.SelectionStart = 0;
            dumpBox.SelectionLength = 0;
        }
    }
}
