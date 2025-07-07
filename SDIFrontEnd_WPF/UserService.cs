using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ITCLib;

namespace SDIFrontEnd_WPF
{
    public class UserService
    {
        public ITCLib.UserPrefs User { get; private set; }

        public UserService(UserPrefs user)  
        {
            User = user;
        }
    }
}
