using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MagnatekControl.SystemModules.Interfaces;

namespace MagnatekControl.SystemModules.Adapters.ViewElements
{
    class TextBoxLogAdapter : IValueIO<string>
    {
        public enum TBLogTimeFormat
        {
            SHORT,
            LONG,
            NONE
        };

        private TBLogTimeFormat logFormat = TBLogTimeFormat.NONE;

        delegate void writeLBCallback(string text);

        private TextBox _item;

        public TextBoxLogAdapter(TextBox item)
        {
            this._item = item;
            this.logFormat = TBLogTimeFormat.NONE;
        }

        public TextBoxLogAdapter(TextBox item, TBLogTimeFormat logFormat)
        {
            this._item = item;
            this.logFormat = logFormat;
        }

        public string read()
        {
            return _item.Text;
        }

        public void write(string value)
        {
            try
            {
                if (_item.InvokeRequired)
                {
                    writeLBCallback d = new writeLBCallback(write);
                    _item.Parent.Invoke(d, new object[] {value});
                }
                else
                {
                    string appstr = value + "\r\n";
                    string timestamp = "";
                    switch (logFormat)
                    {
                        case TBLogTimeFormat.SHORT:
                            timestamp = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() +
                                               ": ";
                            appstr = timestamp + appstr;
                            break;
                        case TBLogTimeFormat.LONG:
                            timestamp = DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLongTimeString() +
                                               ": "; 
                            appstr = timestamp + appstr;
                            break;
                        case TBLogTimeFormat.NONE:
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    _item.AppendText(appstr);
                    //_item.Text += value + "\r\n";
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }
}
