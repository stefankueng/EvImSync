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
using System.Threading;
using InterIMAP.Common.Interfaces;
using InterIMAP.Common.Requests;

namespace InterIMAP.Asynchronous.Client
{
    /// <summary>
    /// Manages the requests to be processed by the worker threads
    /// </summary>
    public class IMAPRequestManager
    {
        #region Private Fields

        private readonly IMAPAsyncClient _client;
        private readonly List<IRequest> _pendingRequests;
        private readonly List<IBatchRequest> _pendingBatchRequests;
        private readonly List<IRequest> _activeRequests;
        private int _requestCount;
        #endregion

        #region CTOR
        /// <summary>
        /// Create a new request manager for the specified client
        /// </summary>
        /// <param name="client"></param>
        public IMAPRequestManager(IMAPAsyncClient client)
        {
            _client = client;
            _pendingRequests = new List<IRequest>();
            _pendingBatchRequests = new List<IBatchRequest>();
            _activeRequests = new List<IRequest>();
            _requestCount = 0;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Indicates if there is at least one request pending
        /// </summary>
        public bool RequestPending
        {
            get
            {
                lock(_pendingRequests)
                {
                    if (_pendingRequests.Count > 0)
                        return true;                                            
                }

                return false;
            }
        }

        /// <summary>
        /// Indicates if there is at least one batch request pending
        /// </summary>
        public bool BatchRequestPending
        {
            get
            {
                lock(_pendingBatchRequests)
                {
                    if (_pendingBatchRequests.Count > 0)
                        return true;
                }

                return false;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the next pending request, adding it to the active requests list.
        /// Returns null if there are no pending requests.
        /// </summary>
        /// <returns></returns>
        public IRequest GetNextRequest()
        {
            IRequest req = null;
            lock(_pendingRequests)
            {
                if (_pendingRequests.Count == 0) return req;
                req = _pendingRequests[0];
                _pendingRequests.RemoveAt(0);
                lock(_activeRequests)
                {
                    _activeRequests.Add(req);
                }
            }

            return req;
        }

        /// <summary>
        /// Gets the next pending batch request.
        /// </summary>
        /// <returns></returns>
        public IBatchRequest GetNextBatchRequest()
        {
            IBatchRequest req = null;
            lock(_pendingBatchRequests)
            {
                if (_pendingBatchRequests.Count == 0) return req;
                req = _pendingBatchRequests[0];
                _pendingBatchRequests.RemoveAt(0);
                lock(_activeRequests)
                {
                    foreach (IRequest r in req.Requests)
                        _activeRequests.Add(r);
                }
            }

            return req;
        }

        
        /// <summary>
        /// Removes the specified request from the active requests list
        /// </summary>
        /// <param name="req">The request to remove</param>
        public void RequestCompleted(IRequest req)
        {
            lock(_activeRequests)
            {
                int idx = _activeRequests.IndexOf(req);
                if (idx >= 0)
                    _activeRequests.RemoveAt(idx);
            }
        }

        /// <summary>
        /// Inserts a new request and the end of the pending requests list
        /// </summary>
        /// <param name="req">The request to add</param>
        /// <param name="urgent">If True, request will be added to beginning to pending requests.</param>
        public void SubmitRequest(IRequest req, bool urgent)
        {
            if (req.Command == null)
                return;

            InjectClient(req);
            
            System.Threading.Interlocked.Increment(ref _requestCount);
            
            lock(_pendingRequests)
            {
                req.RequestID = _requestCount;
                
                if (urgent)
                    _pendingRequests.Insert(0, req);
                else
                    _pendingRequests.Add(req);
            }
        }

        /// <summary>
        /// Inserts a new batch request to the end of the pending requests list
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="urgent"></param>
        public void SubmitBatchRequest(IBatchRequest batch, bool urgent)
        {
            InjectClient(batch.Requests.ToArray());
            
            lock (_pendingBatchRequests)
            {
                if (urgent)
                    _pendingBatchRequests.Insert(0, batch);
                else
                    _pendingBatchRequests.Add(batch);
            }
        }

        /// <summary>
        /// Submits a batch of asynchronous requests
        /// </summary>
        /// <param name="batch"></param>
        /// <param name="urgent"></param>
        public void SubmitAsyncBatchRequest(IBatchRequest batch, bool urgent)
        {
            lock(_pendingRequests)
            {
                foreach (IRequest req in batch.Requests)
                {
                    InjectClient(req);
                    
                    if (urgent)
                        _pendingRequests.Insert(0, req);
                    else
                        _pendingRequests.Add(req);
                        
                    
                }
            }
        }

        public void SubmitBatchAndWait(IBatchRequest batch, bool urgent)
        {
            int completed = 0;
            Dictionary<IRequest, BaseRequest.RequestCompletedCallback> callbacks = new Dictionary<IRequest, BaseRequest.RequestCompletedCallback>();
            ManualResetEvent mre = new ManualResetEvent(false);
            foreach (IRequest req in batch.Requests)
            {
                BaseRequest.RequestCompletedCallback _callback = req.RequestCompleteCallback;
                callbacks.Add(req, _callback);

                req.RequestCompleteCallback = delegate(IRequest r)
                                                  {
                                                      completed++;
                                                      callbacks[r](r);
                                                      if (completed >= batch.Requests.Count)
                                                          mre.Set();
                                                  };

                SubmitRequest(req, false);
            }
            mre.WaitOne();
        }

        /// <summary>
        /// Submits the specified request and then blocks the calling thread until it completes.
        /// </summary>
        /// <param name="req"></param>
        /// <param name="urgent"></param>
        public void SubmitAndWait(IRequest req, bool urgent)
        {
            BaseRequest.RequestCompletedCallback _callback = req.RequestCompleteCallback;

            bool requestComplete = false;
            req.RequestCompleteCallback = delegate(IRequest r)
                                              {
                                                  requestComplete = true;
                                              };
            SubmitRequest(req, urgent);
            while (!requestComplete) { Thread.Sleep(1);}
            if (_callback != null)
                _callback(req);
        }

        private void InjectClient(IRequest req)
        {
            req.Client = _client;
        }

        private void InjectClient(IEnumerable<IRequest> reqs)
        {
            foreach (IRequest req in reqs)
                req.Client = _client;
        }
        #endregion
    }
}