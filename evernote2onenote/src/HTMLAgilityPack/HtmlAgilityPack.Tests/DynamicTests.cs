using System.Collections.Generic;
using NUnit.Framework;

namespace HtmlAgilityPack.Tests
{
    [TestFixture]
    public class DynamicTests
    {
        [Test]
        public void TestCallingExistingFunction()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body class=\"asdfasd\"><p>asdf asdf sdf</p></body></html>");
            dynamic docElement = doc.DocumentNode;
            var item = docElement.Descendants();
            Assert.IsInstanceOf<IEnumerable<HtmlNode>>(item);
        }

        [Test]
        public void TestCallingExistingMember()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body class=\"asdfasd\"><p>asdf asdf sdf</p></body></html>");
            dynamic docElement = doc.DocumentNode;
            var item = docElement.Closed;
            Assert.IsInstanceOf<bool>(item);
        }

        [Test]
        public void TestGetAttribute()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body class=\"asdfasd\"><p>asdf asdf sdf</p></body></html>");
            dynamic docElement = doc.DocumentNode;
            var item = docElement.Html.Body._Class;
            Assert.IsNotNull(item);
            Assert.IsInstanceOf<HtmlAttribute>(item);
        }

        [Test]
        public void TestGetMember()
        {
            var doc = new HtmlDocument();
            doc.LoadHtml("<html><body><p>asdf asdf sdf</p></body></html>");
            dynamic docElement = doc.DocumentNode;
            var item = docElement.Html.Body;
            Assert.IsNotNull(item);
            Assert.IsInstanceOf<HtmlNode>(item);
        }
    }
}