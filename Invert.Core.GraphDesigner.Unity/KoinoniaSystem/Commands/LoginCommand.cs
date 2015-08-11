using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Invert.Core.GraphDesigner.Unity.KoinoniaSystem.Commands
{
    class LoginCommand : ICommand
    {
        public string Title
        {
            get { return "Login to Invert Empire"; }
        }

        public string Username { get; set; }
        public string Password { get; set; }

    }




}
