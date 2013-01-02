using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ITKE.WinLib
{
    public partial class DelayEvent : Component
    {
        public DelayEvent()
        {
            InitializeComponent();
        }

        public DelayEvent(IContainer container)
        {
            container.Add(this);

            InitializeComponent();
        }

        [DefaultValue(500)]
        public int Interval
        {
            get { return timer1.Interval; }
            set { timer1.Interval = value; }
        }

        public event Action ActualDo;

        public object[] Parameters;
        public void Do(params object[] parameters)
        {
            timer1.Stop();
            Parameters = parameters;
            timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Stop();
            if (ActualDo != null)
                ActualDo();
        }
    }
}
