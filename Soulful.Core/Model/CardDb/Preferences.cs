using Realms;

namespace Soulful.Core.Model.CardDb
{
    public class Preferences : RealmObject
    {
        /// <summary>
        /// Gets or sets the user's last username
        /// </summary>
        public string LastUserName { get; set; }
    }
}
