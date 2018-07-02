namespace Ascension.Networking
{
    public delegate void CommandCallback(Command created, Command current);

    public struct CommandCallbackItem
    {
        public int End;
        public int Start;
        public Command Command;
        public CommandCallback Callback;
        public CommandCallbackModes Mode;
    }
}
