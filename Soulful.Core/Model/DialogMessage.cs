using IntraMessaging;
using System;

namespace Soulful.Core.Model
{
    public class DialogMessage : Message
    {
        [Flags]
        public enum Button
        {
            Ok = 2,
            Yes = 4,
            No = 8,
            Cancel = 16
        }

        public string Title { get; set; }
        public string Content { get; set; }
        public Button Buttons { get; set; }
        public Action<Button> Callback { get; set; }
    }
}
