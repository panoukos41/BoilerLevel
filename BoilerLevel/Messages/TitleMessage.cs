namespace BoilerLevel.Messages
{
    public class TitleMessage
    {
        public TitleMessage() { }

        public TitleMessage(string title)
        {
            Title = title;
        }

        public string Title { get; set; }
    }
}