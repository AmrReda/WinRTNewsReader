using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace WinRTNewsReader.Common.Helpers
{
    public class ObservableVector<T> : IObservableVector<T>
    {
        private class VectorChangedEventArgs : IVectorChangedEventArgs
        {
            public VectorChangedEventArgs(CollectionChange change, uint index)
            {
                CollectionChange = change;
                Index = index;
            }
            public CollectionChange CollectionChange
            {
                get;
                private set;
            }

            public uint Index
            {
                get;
                private set;
            }
        }
        public event VectorChangedEventHandler<T> VectorChanged;

        private ObservableCollection<T> _adaptee;

        public ObservableVector()
            : this(new ObservableCollection<T>())
        { }

        public ObservableVector(ObservableCollection<T> adaptee)
        {
            _adaptee = adaptee;
            _adaptee.CollectionChanged += Adaptee_CollectionChanged;
        }

        protected void RaiseVectorChangedEvent(IVectorChangedEventArgs args)
        {
            var temp = VectorChanged;
            if (temp != null)
            {
                temp(this, args);
            }
        }


        private void Adaptee_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            VectorChangedEventArgs args;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    args = new VectorChangedEventArgs(CollectionChange.ItemInserted, (uint)e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Remove:
                    args = new VectorChangedEventArgs(CollectionChange.ItemRemoved, (uint)e.OldStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    args = new VectorChangedEventArgs(CollectionChange.ItemChanged, (uint)e.NewStartingIndex);
                    break;

                case NotifyCollectionChangedAction.Reset:
                case NotifyCollectionChangedAction.Move:
                    args = new VectorChangedEventArgs(CollectionChange.Reset, 0);
                    break;
                default:
                    return;
            }

            RaiseVectorChangedEvent(args);
        }

        public int IndexOf(T item)
        {
            return _adaptee.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            _adaptee.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _adaptee.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return _adaptee[index];
            }
            set
            {
                _adaptee[index] = value;
            }
        }

        public void Add(T item)
        {
            _adaptee.Add(item);
        }

        public void Clear()
        {
            _adaptee.Clear();
        }

        public bool Contains(T item)
        {
            return _adaptee.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _adaptee.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _adaptee.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            return _adaptee.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _adaptee.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _adaptee.GetEnumerator();
        }
    }
}
