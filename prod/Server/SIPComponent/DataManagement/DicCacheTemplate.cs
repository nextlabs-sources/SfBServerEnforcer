using SFBCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nextlabs.SFBServerEnforcer.PolicyHelper;

namespace Nextlabs.SFBServerEnforcer.SIPComponent.DataManagement
{
    class DicCacheTemplate<TKey,TValue>
    {
        #region Member
        private ReaderWriterLockSlim m_rwLockDicCache;
        private Dictionary<TKey, TValue> m_dicCache;
        #endregion

        #region Constructors
        public DicCacheTemplate() {
            m_dicCache = new Dictionary<TKey, TValue>();
            m_rwLockDicCache = new ReaderWriterLockSlim();
        }
        #endregion

        #region Public: set, get, delete, clear
        public void SetValue(TKey key, TValue value)
        {
            try
            {
                m_rwLockDicCache.EnterWriteLock();
                CommonHelper.AddKeyValuesToDir(m_dicCache, key, value);
            }
            finally
            {
                m_rwLockDicCache.ExitWriteLock();
            }
        }
        public TValue GetValue(TKey key, TValue valueDefault)
        {
            TValue valueReturn = valueDefault;
            try
            {
                m_rwLockDicCache.EnterReadLock();
                valueReturn = CommonHelper.GetValueByKeyFromDir(m_dicCache, key, valueDefault);
            }
            finally
            {
                m_rwLockDicCache.ExitReadLock();
            }
            return valueReturn;
        }
        public void Delete(TKey key)
        {
            try
            {
                m_rwLockDicCache.EnterWriteLock();
                CommonHelper.RemoveKeyValuesFromDir(m_dicCache, key);
            }
            finally
            {
                m_rwLockDicCache.ExitWriteLock();
            }
        }
        public void Clear()
        {
            try
            {
                m_rwLockDicCache.EnterWriteLock();
                m_dicCache.Clear();
            }
            finally
            {
                m_rwLockDicCache.ExitWriteLock();
            }
        }
        #endregion
    }
}
