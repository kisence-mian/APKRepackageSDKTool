using APKRepackageSDKTool;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class KeyValueList : List<KeyValue>, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public void Change()
    {
        NotifyCollectionChangedEventArgs e = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
        CollectionChanged?.Invoke(this, e);
    }

    public bool ContainsKey(string key)
    {
        for (int i = 0; i < Count; i++)
        {
            KeyValue item = this[i];

            if(item.key == key)
            {
                return true;
            }
        }

        return false;
    }
}

