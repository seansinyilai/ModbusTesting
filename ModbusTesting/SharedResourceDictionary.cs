using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModbusTesting
{
    public class SharedResourceDictionary : ResourceDictionary
    {
        public static Dictionary<Uri, ResourceDictionary> SharedDictionaries = new Dictionary<Uri, ResourceDictionary>();

        private Uri _Source;

        public new Uri Source
        {
            get => _Source;
            set
            {
                _Source = value;

                if (!SharedDictionaries.ContainsKey(value))
                {
                    base.Source = value;

                    SharedDictionaries.Add(value, this);
                }
                else
                {
                    MergedDictionaries.Add(SharedDictionaries[value]);
                }
            }
        }
    }
}
