using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MagnatekControl.SystemModules.Adapters.ViewElements;
using MagnatekControl.SystemModules.Interfaces;

namespace OpenGLTutorial8
{
    public partial class MessageWindow : Form, IValueOutput<string>
    {
        private IValueOutput<string> log;
        public MessageWindow()
        {
            InitializeComponent();
            log = new TextBoxLogAdapter(textBox1);

        }

        public void write(string value)
        {
            log.write(value);
        }
    }
}
