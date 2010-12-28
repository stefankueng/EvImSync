/********************************************************************************************
 * InterIMAP
 * Copyright (C) 2008-2009 Jason Miesionczek
 * Original Author: Rohit Joshi
 * Based on this article on codeproject.com:
 * IMAP Client library using C#
 * http://www.codeproject.com/KB/IP/imaplibrary.aspx?msg=2498332
 * Posted: August 16th 2004
 * 
 * ZipStorer code written by Jaime Olivares
 * http://www.codeproject.com/KB/recipes/ZipStorer.aspx
 * 
 * InterIMAP is free software; you can redistribute it and/or modify it under the terms
 * of the GNU Lesser General Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 * 
 * InterIMAP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 * without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License along with
 * InterIMAP. If not, see http://www.gnu.org/licenses/.
 * 
 * *****************************************************************************************/

using System.Collections.Generic;
using InterIMAP.Common.BatchRequests;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="percentComplete"></param>
    public delegate void AsyncBatchProgressCallback(double percentComplete);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="batch"></param>
    public delegate void AsyncBatchCompletedCallback(IBatchRequest batch);
    
    /// <summary>
    /// 
    /// </summary>
    public class AsyncBatchRequest : BaseBatchRequest
    {
        #region Private Fields

        private int requestCompletedCount;
        private readonly int requestCount;
        private event AsyncBatchProgressCallback ProgressUpdate;
        private event AsyncBatchCompletedCallback BatchCompleted;
        #endregion

        private void InternalRequestCompleted(IRequest req)
        {
            System.Threading.Interlocked.Increment(ref requestCompletedCount);

            if (ProgressUpdate != null)
            {
                double percent = requestCompletedCount > 0 ? (requestCount / requestCompletedCount) : 0.00;
                ProgressUpdate(percent);
            }

            if (requestCompletedCount == requestCount)
                if (BatchCompleted != null)
                    BatchCompleted(this);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="requests"></param>
        /// <param name="progCallback"></param>
        /// <param name="completeCallback"></param>
        public AsyncBatchRequest(ICollection<IRequest> requests, AsyncBatchProgressCallback progCallback, AsyncBatchCompletedCallback completeCallback)
        {
            requestCompletedCount = 0;
            requestCount = requests.Count;

            ProgressUpdate += progCallback;
            BatchCompleted += completeCallback;
            
            foreach (IRequest req in requests)
                ((BaseRequest) req).RequestCompleted += InternalRequestCompleted;

            Requests.AddRange(requests);
        }
    }
}
