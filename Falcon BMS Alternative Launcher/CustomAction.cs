using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FalconBMS.Launcher
{
    [System.ComponentModel.RunInstaller(true)]
    public class CustomAction : System.Configuration.Install.Installer
    {
        public override void Install(IDictionary stateSaver)
        {
            base.Install(stateSaver);

            string auth = Context.Parameters["ARGS"];
        }
    }
}
