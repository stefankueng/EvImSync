using System;
using System.Collections.Generic;
using System.Text;

namespace InterIMAP.Synchronous
{
    /// <summary>
    /// Simple class to store the quota information for a folder
    /// </summary>
    [Serializable]
    public class IMAPFolderQuota
    {
        #region Private Fields
        private int _currentSize;
        private int _maxSize;
        #endregion

        #region Public Properties
        /// <summary>
        /// The current size of this folder
        /// </summary>
        public int CurrentSize
        {
            get { return _currentSize; }
            set { _currentSize = value; }
        }

        /// <summary>
        /// The maximum size of this folder
        /// </summary>
        public int MaxSize
        {
            get { return _maxSize; }
            set { _maxSize = value; }
        }
        #endregion

        #region CTOR
        /// <summary>
        /// Default Constructor
        /// </summary>
        public IMAPFolderQuota()
        {
            _currentSize = 0;
            _maxSize = 0;
        }
        #endregion

        #region Override
        /// <summary>
        /// Simple override to show the quota data
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return _currentSize < 0 || _maxSize < 0 ? "Unlimited" : String.Format("{0} of {1} used", _currentSize, _maxSize);
        }
        #endregion
    }
}
