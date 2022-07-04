using StructureMargins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VMS.TPS.Common.Model.API;

[assembly: ESAPIScript(IsWriteable = true)]

namespace VMS.TPS
{
    public class Script
    {
        public ScriptContext scriptcontext;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void Execute(ScriptContext scriptcontext)
        {
            if (scriptcontext.StructureSet == null)
            {
                MessageBox.Show("Structure set is not active.", "Error");
                return;
            }

            try
            {
                scriptcontext.Patient.BeginModifications();
                MarginsWindow marginsWindow = new MarginsWindow(scriptcontext);
                marginsWindow.ShowDialog();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }
    }
}


