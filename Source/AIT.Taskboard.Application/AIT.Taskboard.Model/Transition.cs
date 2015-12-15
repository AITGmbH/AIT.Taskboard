namespace AIT.Taskboard.Interface
{
    public struct Transition : ITransition
    {
        public string FromState;
        public string ToState;
        public string[] Reasons;
    }
}