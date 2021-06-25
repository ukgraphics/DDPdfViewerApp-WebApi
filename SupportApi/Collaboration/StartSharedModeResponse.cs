using SupportApi.Collaboration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace SupportApi.Collaboration
{
    public class StartSharedModeResponse
    {
        public StartSharedModeResponse(ModificationsState modifications, UserAccess userAccess, List<UserAccess> userAccessList)
        {
            this.modifications = modifications;
            this.userAccess = userAccess;
            this.userAccessList = userAccessList;
        }

        public ModificationsState modifications { get; set; }

        public UserAccess userAccess { get; set; }

        public List<UserAccess> userAccessList { get; set; }
    }
}
