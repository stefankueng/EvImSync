using System;

namespace HtmlAgilityPack
{
    public class HtmlDocumentLoadCompleted : EventArgs
    {
        #region Fields

        public HtmlDocument Document;
        public Exception Error;

        #endregion

        #region C'tors

        public HtmlDocumentLoadCompleted(HtmlDocument doc)
        {
            Document = doc;
        }

        public HtmlDocumentLoadCompleted(Exception err)
        {
            Error = err;
        }

        #endregion
    }
}