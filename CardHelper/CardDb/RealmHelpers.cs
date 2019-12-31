using Realms;
using System;
using System.Linq;

namespace CardHelper.CardDb
{

        public static Realm GetCardsRealm()
        {
            string realmPath = App.GetAppdataFilePath("cards.realm");
            RealmConfiguration config = new RealmConfiguration(realmPath)
            {
                ObjectClasses = new Type[] { typeof(Pack), typeof(WhiteCard), typeof(BlackCard) }
            };
            return Realm.GetInstance(config);
        }
    }
}
