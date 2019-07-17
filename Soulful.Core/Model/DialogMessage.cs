using IntraMessaging;
using System;

namespace Soulful.Core.Model
{
    public class DialogMessage : Message
    {
        public enum Button
        {
            OK, YesNo, OKCancel, YesNoCancel
        }

        public string Title { get; set; }
        public string Content { get; set; }
        public Button Buttons { get; set; }
        public Action<Button> Callback { get; set; }
    }
}
