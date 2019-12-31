using Realms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Soulful.Core.Model.CardDb
{
    public static class RealmHelpers
    {
        private static readonly Dictionary<Type, int> _currentIds = new Dictionary<Type, int>();

        public static Realm GetRealmInstance()
        {
            string realmPath = App.GetAppdataFilePath("Soulful.realm");
            RealmConfiguration config = new RealmConfiguration(realmPath);
            return Realm.GetInstance(config);
        }

        public static int GetNextId<T>(Realm realm = null) where T : RealmObject, ICard
        {
            if (realm == null)
                realm = GetRealmInstance();

            if (!_currentIds.ContainsKey(typeof(T)))
            {
                int max = 0;
                foreach (T element in realm.All<T>())
                {
                    if (element.Id > max)
                        max = element.Id;
                }
                _currentIds.Add(typeof(T), max);
            }

            return ++_currentIds[typeof(T)];
        }

        public static void ClearNextIds() => _currentIds.Clear();

        public static Preferences GetUserPreferences(Realm instance = null)
        {
            if (instance == null)
                instance = GetRealmInstance();
            IQueryable<Preferences> preferences = instance.All<Preferences>();

            if (preferences.Count() == 0)
            {
                Preferences p = new Preferences();
                instance.Write(() => instance.Add(p));
                return p;
            }
            else
            {
                return preferences.First();
            }
        }
    }
}
