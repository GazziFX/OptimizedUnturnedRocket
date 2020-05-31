namespace Rocket.API
{
    public class ConsolePlayer : IRocketPlayer
    {
        public string Id
        {
            get
            {
                return "Console";
            }
        }

        public string DisplayName
        {
            get
            {
                return "Console";
            }
        }

        public bool IsAdmin
        {
            get
            {
                return true;
            }
        }

        public int CompareTo(object obj)
        {
            return Id.CompareTo(obj);
        }

        public override bool Equals(object p)
        {
            return p is ConsolePlayer;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
