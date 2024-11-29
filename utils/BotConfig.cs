namespace QQBotCodePlugin.utils
{
    public class BotConfig
    {
        public string Name { get; set; }
        public bool Plugin { get; set; }
        public string Description { get; set; }
        public string IP { get; set; }
        public int ServerPort { get; set; }
        public int EventPort { get; set; }
        public Abilities Abilities { get; set; }
    }

    public class Abilities
    {
        public Meme Meme { get; set; }
        public bool AIChat { get; set; }
        public bool AIChatPrivate { get; set; }
        public bool Help { get; set; }
        public bool KudosMe { get; set; }
        public bool NumberBoom { get; set; }
        public bool Onset { get; set; }
        public bool Ping { get; set; }
        public bool RunWindowsCommand { get; set; }
        public bool Sky { get; set; }
        public bool Wife { get; set; }
    }

    public class Meme
    {
        public bool Ba { get; set; }
        public bool Cat { get; set; }
        public bool Dragon { get; set; }
        public bool Eat { get; set; }
        public bool Play { get; set; }
    }
}
