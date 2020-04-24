using IntraMessaging;
using Soulful.Core.Resources;
using System;

namespace Soulful.Core.Model
{
    public class DialogMessage : Message
    {
        public string Title { get; set; } = AppStrings.DialogTitle_Default;
        public string Message { get; set; }
        public string OkayButtonContent { get; set; } = AppStrings.DialogButton_Okay;
        public string CancelButtonContent { get; set; }
        public string HelpUrl { get; set; } = HelpUrls.Default;
        public Action<bool> Callback { get; set; }
    }
}
