using System;

namespace AIT.Taskboard.Interface
{
    public interface ILoginData
    {
        Uri TeamProjectCollectionUri { get; set; }
        string TeamProjectName { get; set; }
    }
}