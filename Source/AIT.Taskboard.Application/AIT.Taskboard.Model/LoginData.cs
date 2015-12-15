using System;
using AIT.Taskboard.Interface;
using AIT.Taskboard.Model.Properties;

namespace AIT.Taskboard.Model
{
    public class LoginData : ILoginData
    {
        public LoginData()
        {
        }

        public Uri TeamProjectCollectionUri { get; set; }
        public string TeamProjectName { get; set; }
    }
}