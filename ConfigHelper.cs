using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace DynamicLottery
{
    public class ConfigHelper : ConfigurationSection
    {
        /// <summary>
        /// The value of the property here "Folders"
        /// needs to match that of the config file section
        /// </summary>
        [ConfigurationProperty("Lotte")]
        public LotteCollection HashKeys
        {
            get { return ((LotteCollection)(base["Lotte"])); }
        }
    }

    /// <summary>
    /// The collection class that will store the list of each element/item that
    /// is returned back from the configuration manager.
    /// </summary>
    [ConfigurationCollection(typeof(LotteElement))]
    public class LotteCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new LotteElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LotteElement)(element)).Id;
        }

        public LotteElement this[int idx]
        {
            get
            {
                return (LotteElement)BaseGet(idx);
            }
        }
    }

    /// <summary>
    /// The class that holds onto each element returned by the configuration manager.
    /// </summary>
    public class LotteElement : ConfigurationElement
    {
        [ConfigurationProperty("id", DefaultValue = "",
           IsKey = true, IsRequired = true)]
        public string Id
        {
            get
            {
                return ((string)(base["id"]));
            }
            set
            {
                base["id"] = value;
            }
        }
        [ConfigurationProperty("name", DefaultValue = "",
         IsKey = true, IsRequired = true)]
        public string Name
        {
            get
            {
                return ((string)(base["name"]));
            }
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("image",
          DefaultValue = "", IsKey = false, IsRequired = false)]
        public string Image
        {
            get
            {
                return ((string)(base["image"]));
            }
            set
            {
                base["image"] = value;
            }
        }
        [ConfigurationProperty("count",
          DefaultValue = 0, IsKey = false, IsRequired = false)]
        public int Count
        {
            get
            {
                return ((int)(base["count"]));
            }
            set
            {
                base["count"] = value;
            }
        }
    }
}
