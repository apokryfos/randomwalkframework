using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;


namespace GraphSerializationFramework
{
	
    public static class PrefferedGenericDataTypes<TKey, TValue>
    {
        public static Type PrefferedDictionaryType = typeof(Dictionary<TKey, TValue>);
        public static Type PrefferedGenericCollectionType = typeof(List<TKey>);
        
        #region Generic Dictionary
        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance()
        {
            return Activator.CreateInstance(PrefferedDictionaryType) as IDictionary<TKey, TValue>;
        }

        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance(int capacity)
        {
            return Activator.CreateInstance(PrefferedDictionaryType, capacity) as IDictionary<TKey, TValue>;
        }
        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance(IEqualityComparer<TKey> comparer)
        {
            return Activator.CreateInstance(PrefferedDictionaryType, comparer) as IDictionary<TKey, TValue>;
        }
        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance(IDictionary<TKey, TValue> copyFrom)
        {
            return Activator.CreateInstance(PrefferedDictionaryType, copyFrom) as IDictionary<TKey, TValue>;
        }
        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance(IDictionary<TKey, TValue> copyFrom, IEqualityComparer<TKey> comparer)
        {
            return Activator.CreateInstance(PrefferedDictionaryType, copyFrom, copyFrom) as IDictionary<TKey, TValue>;
        }
        public static IDictionary<TKey, TValue> GetGenericDictionaryInstance(int capacity, IEqualityComparer<TKey> comparer)
        {
            return Activator.CreateInstance(PrefferedDictionaryType, capacity, comparer) as IDictionary<TKey, TValue>;
        }
        #endregion

        #region Generic Collection
        public static ICollection<TKey> GetPrefferedGenericCollection()
        {
            return (ICollection<TKey>)Activator.CreateInstance(PrefferedGenericCollectionType);
        }
        public static ICollection<TKey> GetPrefferedGenericCollection(IEnumerable<TKey> enumerable)
        {
            return (ICollection<TKey>)Activator.CreateInstance(PrefferedGenericCollectionType, enumerable);
        }
        
        #endregion
    }

    public static class PrefferedDataTypes
    {

        public static Type PrefferedCollectionType = typeof(List<int>);
        public static Type PrefferedAdjListType = typeof(Dictionary<int, ICollection<int>>);		
        public static Type PrefferedGraphType = typeof(BidirectionalGraph<int, Edge<int>>);
     
        #region Collection
        public static ICollection<int> GetCollectionInstance()
        {
            return Activator.CreateInstance(PrefferedCollectionType) as ICollection<int>;
        }

        public static ICollection<int> GetCollectionInstance(int capacity)
        {
            if (PrefferedCollectionType.GetConstructor(new Type[] { typeof(int) }) != null)
                return (ICollection<int>)Activator.CreateInstance(PrefferedCollectionType, capacity);
            else
                return GetCollectionInstance();
        }

        public static ICollection<int> GetCollectionInstance(IEnumerable<int> collection)
        {
            return Activator.CreateInstance(PrefferedCollectionType,collection) as ICollection<int>;
        }
        #endregion


        #region AdjecencyList
        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance()
        {
            return Activator.CreateInstance(PrefferedAdjListType) as IDictionary<int, ICollection<int>>;
        }

        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance(int capacity)
        {
            return Activator.CreateInstance(PrefferedAdjListType, capacity) as IDictionary<int, ICollection<int>>;
        }
        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance(IEqualityComparer<int> comparer)
        {
            return Activator.CreateInstance(PrefferedAdjListType, comparer) as IDictionary<int, ICollection<int>>;
        }
        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance(IDictionary<int, ICollection<int>> copyFrom)
        {
            return Activator.CreateInstance(PrefferedAdjListType, copyFrom) as IDictionary<int, ICollection<int>>;
        }
        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance(IDictionary<int, ICollection<int>> copyFrom, IEqualityComparer<int> comparer)
        {
            return Activator.CreateInstance(PrefferedAdjListType, copyFrom, copyFrom) as IDictionary<int, ICollection<int>>;
        }
        public static IDictionary<int, ICollection<int>> GetAdjecencyListInstance(int capacity, IEqualityComparer<int> comparer)
        {
            return Activator.CreateInstance(PrefferedAdjListType, capacity, comparer) as IDictionary<int, ICollection<int>>;
        }
        #endregion

        

        #region Graph
        public static IMutableBidirectionalGraph<int, Edge<int>> GetPrefferedGraphInstance()
        {
            return Activator.CreateInstance(PrefferedGraphType) as IMutableBidirectionalGraph<int, Edge<int>>;

        }
        public static IMutableBidirectionalGraph<int, Edge<int>> GetPrefferedGraphInstance(bool allowParallelEdges)
        {
            return Activator.CreateInstance(PrefferedGraphType, allowParallelEdges) as IMutableBidirectionalGraph<int, Edge<int>>;

        }
        public static IMutableBidirectionalGraph<int, Edge<int>> GetPrefferedGraphInstance(bool allowParallelEdges, int vertexCapacity)
        {
            return Activator.CreateInstance(PrefferedGraphType, allowParallelEdges, vertexCapacity) as IMutableBidirectionalGraph<int, Edge<int>>;

        }
        #endregion


    }
}
