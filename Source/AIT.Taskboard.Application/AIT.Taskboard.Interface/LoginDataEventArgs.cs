using System.ComponentModel;

namespace AIT.Taskboard.Interface
{
    public class LoginDataEventArgs : CancelEventArgs
    {
        public ILoginData LoginData { get; set; }
    }
}