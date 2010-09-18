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

using System;
using System.Collections.Generic;
using InterIMAP.Asynchronous.Client;
using InterIMAP.Common.Interfaces;

namespace InterIMAP.Common.Requests
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="totalTime"></param>
    public delegate void FullMessageCompleteCallback(IMessage msg, TimeSpan totalTime);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="bytesReceived"></param>
    /// <param name="totalBytes"></param>
    public delegate void FullMessageProgressCallback(IMessage msg, long bytesReceived, long totalBytes);
    
    /// <summary>
    /// Asynchronously retrieve all parts of a message
    /// </summary>
    public class FullMessageRequest
    {
        #region Private Fields
        private readonly IMessage _msg;
        private readonly IMAPAsyncClient _client;
        private int completedParts;
        private List<MessagePartRequest> partRequests;
        private long totalMessageSize;
        private long totalBytesReceived;
        private readonly Dictionary<ICommand, long> bytesReceivedPerCommand = new Dictionary<ICommand, long>();
        private double secondsToComplete;
        #endregion

        #region Public Events
        public event FullMessageCompleteCallback MessageComplete;
        public event FullMessageProgressCallback MessageProgress;
        #endregion

        #region CTOR
        /// <summary>
        /// Create new full message request
        /// </summary>
        /// <param name="client"></param>
        /// <param name="msg"></param>
        public FullMessageRequest(IMAPAsyncClient client, IMessage msg)
        {
            _msg = msg;
            _client = client;


        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Begin retrieving the message
        /// </summary>
        public void Start()
        {
            MessageFlagRequest mfr = new MessageFlagRequest(_msg, delegate
                                                                      {
                                                                          if (_msg.HeaderLoaded)
                                                                          {
                                                                              GetMessageStructure();
                                                                          }
                                                                          else
                                                                          {
                                                                              MessageHeaderRequest mhr = new MessageHeaderRequest(_msg, delegate
                                                                              {
                                                                                  GetMessageStructure();
                                                                              });
                                                                              _client.RequestManager.SubmitRequest(mhr, true);
                                                                          }
                                                                      });
            _client.RequestManager.SubmitRequest(mfr, false);

        }

        public void SubmitAndWait()
        {
            MessageFlagRequest mfr = new MessageFlagRequest(_msg, delegate
            {
                if (_msg.HeaderLoaded)
                {
                    GetMessageStructure();
                }
                else
                {
                    MessageHeaderRequest mhr = new MessageHeaderRequest(_msg, delegate
                    {
                        GetMessageStructure();
                    });
                    _client.RequestManager.SubmitAndWait(mhr, true);
                }
            });
            bool msgComplete = false;
            // IMessage msg, TimeSpan totalTime
            MessageComplete += delegate(IMessage msg, TimeSpan totalTime) { msgComplete = true; };
            _client.RequestManager.SubmitAndWait(mfr, false);

            while (!msgComplete) { System.Threading.Thread.Sleep(10); }
        }

        #endregion

        #region Private Methods
        private void GetMessageStructure()
        {
            MessageStructureRequest msr = new MessageStructureRequest(_msg, delegate
                                                                                {
                                                                                    GetMessageContent();
                                                                                });
            _client.RequestManager.SubmitRequest(msr, true);
        }

        private void GetMessageContent()
        {
            
            partRequests = new List<MessagePartRequest>();
            foreach (IMessageContent part in _msg.MessageContent)
            {
                partRequests.Add(new MessagePartRequest(part, PartComplete, PartProgress));
                totalMessageSize += part.ContentSize;
            }

            foreach (IRequest req in partRequests)
                _client.RequestManager.SubmitRequest(req, true);
        }

        private void PartProgress(ICommand cmd, long bytesReceived, long bytesTotal)
        {
            lock (bytesReceivedPerCommand)
            {
                if (!bytesReceivedPerCommand.ContainsKey(cmd))
                    bytesReceivedPerCommand.Add(cmd, bytesReceived);
                else
                {
                    bytesReceivedPerCommand[cmd] = bytesReceived;
                }

                totalBytesReceived = CountBytesReceived();
            }
        

            
            if (MessageProgress != null)
                MessageProgress(_msg, totalBytesReceived, totalMessageSize);
        }

        private long CountBytesReceived()
        {
            long totalbytes = 0;
            foreach (ICommand c in bytesReceivedPerCommand.Keys)
            {
                totalbytes += bytesReceivedPerCommand[c];
            }
            return totalbytes;
        }
        
        private void PartComplete(IRequest req)
        {
            completedParts++;
            secondsToComplete += req.Result.ElapsedTime.TotalSeconds;
            if (completedParts == partRequests.Count)
                if (MessageComplete != null)
                    MessageComplete(_msg, TimeSpan.FromSeconds(secondsToComplete));

        }
        #endregion


    }
}
