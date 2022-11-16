using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCPSLEnforcedRNG.Modules
{
    public abstract class BaseModule
    {
        //-------------------------------------------------------------------------------
        //Override
        //-------------------------------------------------------------------------------
        //Main
        //-------------------------------------------------------------------------------
        //Events
        public bool Enabled { get; set; } = true;
        public abstract string ModuleName { get; }

        public abstract void Activate(); // Is called once on the start of the server
        public virtual void SetUpRound() // Is called at the start of every round
        {
            return;
        }
    }
}
