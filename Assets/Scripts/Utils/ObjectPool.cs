
namespace Utils
{
    public class ObjectPool<T>
    {

        public delegate T CreatorDelegate();
        public delegate void CleanerDelegate(T obj);

        private CreatorDelegate creator;
        private CleanerDelegate cleaner;
        private BetterList<T> freeList;

        public ObjectPool(CreatorDelegate creator, CleanerDelegate cleaner)
        {
            this.creator = creator;
            this.cleaner = cleaner;
            freeList = new BetterList<T>();
        }

        public T GetObject()
        {
            T newObject = default( T );
            if (freeList.size > 0)
            {
                newObject = freeList.Pop();
            }
            else
            {
                newObject = creator();
            }
            return newObject;
        }

        public void DisposeObject(T obj)
        {
            if (obj != null)
            {
                freeList.Add( obj );
            }
        }

        /// <summary>
        /// 清理空闲的物体
        /// 注意：这里不是清理所有生成的
        /// </summary>
        public void Clear()
        {
            if (cleaner != null)
            {
                for (int i = 0; i < freeList.size; ++i)
                {
                    cleaner( freeList[i] );
                }
                freeList.Clear();
                freeList = null;
            }
        }

        public int Length()
        {
            return freeList.size;
        }
    }
}
