using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rocket.API.Collections
{
    [Serializable]
    [XmlType(AnonymousType = false, IncludeInSchema = true, TypeName = "Translation")]
    public class TranslationListEntry
    {
        [XmlAttribute]
        public string Id;
        [XmlAttribute]
        public string Value;

        public TranslationListEntry(string id, string value)
        {
            Id = id;
            Value = value;
        }
        public TranslationListEntry() { }
    }

    public static class TranslationListExtension{
        public static void AddUnknownEntries(this TranslationList defaultTranslations, IAsset<TranslationList> translations)
        {
            bool hasChanged = false;
            foreach(TranslationListEntry entry in defaultTranslations)
            {
                if (translations.Instance[entry.Id] == null) {
                    translations.Instance.Add(entry);
                    hasChanged = true;
                }
            }
            if(hasChanged)
                translations.Save();
        }
    }

    [XmlRoot("Translations")]
    [XmlType(AnonymousType = false, IncludeInSchema = true, TypeName = "Translation")]
    [Serializable]
    public class TranslationList : IDefaultable, IEnumerable<TranslationListEntry>
    {
        public TranslationList() { }
        protected List<TranslationListEntry> translations = new List<TranslationListEntry>();

        public List<TranslationListEntry> TranslationsList => translations;

        public IEnumerator<TranslationListEntry> GetEnumerator()
        {
            return translations.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return translations.GetEnumerator();
        }

        public void Add(object o)
        {
            translations.Add((TranslationListEntry)o);
        }

        public void Add(string key, string value)
        {
            translations.Add(new TranslationListEntry(key, value));
        }

        public void AddRange(IEnumerable<TranslationListEntry> collection)
        {
            translations.AddRange(collection);
        }

        public void AddRange(TranslationList collection)
        {
            translations.AddRange(collection.translations);
        }

        public string this[string key]
        {
            get
            {
                foreach (var trans in translations)
                {
                    if (trans.Id.Equals(key))
                        return trans.Value;
                }
                return null;
            }
            set
            {
                foreach (var trans in translations)
                {
                    if (trans.Id.Equals(key))
                    {
                        trans.Value = value;
                        return;
                    }
                }
            }
        }

        public string Translate(string translationKey, params object[] placeholder)
        {
            string value = this[translationKey];
            if (string.IsNullOrEmpty(value)) return translationKey;
                
            if (value.Contains("{") && value.Contains("}") && placeholder != null && placeholder.Length != 0)
            {
                for (int i = 0; i < placeholder.Length; i++)
                {
                    if (placeholder[i] == null) placeholder[i] = "NULL";
                }
                value = string.Format(value, placeholder);
            }
            return value;
        }

        public virtual void LoadDefaults()
        {
            
        }
    }
}
