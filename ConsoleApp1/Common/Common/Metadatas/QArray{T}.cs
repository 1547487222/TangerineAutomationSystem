using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QStandaedPlatform.Engine.Common.Common.Metadatas
{
    public class QArray<TData> : QData, ICollection<TData> where TData : QData
    {
        private readonly List<TData> _items;

        public QArray()
        {
            _items = [];
        }

        public QArray(IEnumerable<TData> collection)
        {
            _items = new List<TData>(collection ?? throw new ArgumentNullException(nameof(collection)));
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(TData item) => _items.Add(item ?? throw new ArgumentNullException(nameof(item)));

        public void AddRange(IEnumerable<TData> items) => _items.AddRange(items ?? throw new ArgumentNullException(nameof(items)));

        public void Clear() => _items.Clear();

        public bool Contains(TData item) => _items.Contains(item ?? throw new ArgumentNullException(nameof(item)));

        public void CopyTo(TData[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);

        public bool Remove(TData item) => _items.Remove(item ?? throw new ArgumentNullException(nameof(item)));


        public TData this[int index]
        {
            get => _items[index];
            set => _items[index] = value ?? throw new ArgumentNullException(nameof(value));
        }


        public IEnumerator<TData> GetEnumerator() => _items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        public void Insert(int index, TData item) => _items.Insert(index, item ?? throw new ArgumentNullException(nameof(item)));

        public void RemoveAt(int index) => _items.RemoveAt(index);

        public int IndexOf(TData item) => _items.IndexOf(item ?? throw new ArgumentNullException(nameof(item)));

    
        public Type DataType => typeof(TData);

   
        public List<TData> ToList() => new(_items);

        public TData[] ToArray() => [.. _items];

   
        public override string ToString()
        {
            return $"[QArray<{DataType.Name}>] Count = {Count}";
        }
     
        public static QArray<TData> Empty => [];


    public static implicit operator QArray<TData>(List<TData> list) => new(list);
    }
}
