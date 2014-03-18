// Evernote2Onenote - imports Evernote notes to Onenote
// Copyright (C) 2014 - Stefan Kueng

// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.

// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml;
using Evernote2Onenote.Enums;
using HtmlAgilityPack;

namespace Evernote2Onenote
{
    class Note
    {
        public Note()
        {
            Title = string.Empty;
            ContentHash = string.Empty;
            Tags = new List<string>();
            Attachments = new List<Attachment>();
            NewTags = new List<string>();
            ObsoleteTags = new List<string>();
            Action = NoteAction.Nothing;
        }

        public void SetHtmlContent(string html)
        {
            StringWriter stream = new StringWriter();
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove potentially harmful elements 
            HtmlNodeCollection nc = doc.DocumentNode.SelectNodes("//script|//link|//iframe|//frameset|//frame|//applet|//object|//embed" +
            "|//head|//base|//basefont|//bgsound|//blink|//button|//dir|//fieldset|//form|//ilayer|//input|//isindex|//label" +
            "|//layer|//legend|//marquee|//menu|//meta|//noframes|//noscript|//optgroup|//option|//param|//plaintext|//select|//style|//textarea|//xml");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.ParentNode.RemoveChild(node, false);
                }
            }

            // remove hrefs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//a[starts-with(translate(@href, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("href", "#");
                }
            }

            // remove img with refs to java/j/vbscript URLs
            nc = doc.DocumentNode.SelectNodes("//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'javascript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'jscript')]|//img[starts-with(translate(@src, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'vbscript')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.SetAttributeValue("src", "#");
                }
            }

            // remove on<Event> handlers from all tags
            nc = doc.DocumentNode.SelectNodes("//*[@onclick or @onmouseover or @onfocus or @onblur or @onmouseout or @ondoubleclick or @onload or @onunload]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.Attributes.Remove("onFocus");
                    node.Attributes.Remove("onBlur");
                    node.Attributes.Remove("onClick");
                    node.Attributes.Remove("onMouseOver");
                    node.Attributes.Remove("onMouseOut");
                    node.Attributes.Remove("onDoubleClick");
                    node.Attributes.Remove("onLoad");
                    node.Attributes.Remove("onUnload");
                }
            }

            // remove any style attributes that contain the word expression (IE evaluates this as script)
            nc = doc.DocumentNode.SelectNodes("//*[contains(translate(@style, 'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz'), 'expression')]");
            if (nc != null)
            {
                foreach (HtmlNode node in nc)
                {
                    node.Attributes.Remove("style");
                }
            }

            doc.OptionOutputAsXml = true;
            doc.OptionWriteEmptyNodes = true;
            doc.OptionAutoCloseOnEnd = true;
            doc.OptionDefaultStreamEncoding = System.Text.Encoding.UTF8;
            doc.Save(stream);
            html = stream.GetStringBuilder().ToString();

            // now we have double-escaped sequences like "&amp;uuml;" instead of
            // a simple "&uuml;" - we have to fix those
            Regex rx = new Regex(@"&amp;([A-Za-z#0-9]{1,6}?;)", RegexOptions.IgnoreCase);
            html = rx.Replace(html, "&$1");
            rx = new Regex(@"<!doctype\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            rx = new Regex(@"<head\b[^>]*>(.*?)</head>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            rx = new Regex(@"<html\b[^>]*>(.*?)</html>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, "$1");
            rx = new Regex(@"<\?xml\b[^>]*\?>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            // remove office/word tags
            rx = new Regex(@"</?o:p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            rx = new Regex(@"</?_o3a_p>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            rx = new Regex(@"</?_st13a_city>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);
            // remove comments
            rx = new Regex(@"<!--.*?-->", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = rx.Replace(html, string.Empty);

            // ink notes must not have any content at all
            rx = new Regex(@"<body\b[^>]*>(.*?)</body>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            if (html.IndexOf("application/vnd.evernote.ink", StringComparison.InvariantCultureIgnoreCase) >= 0)
                Content = string.Empty;
            else
                Content = rx.Replace(html, "$1");
        }

        public void SetTextContent(string text)
        {
            // encode CDATA end tags
            text = text.Replace("]]>", "]]]]><![CDATA[>");
            // strip invalid xml chars
            text = XmlSanitizer.SanitizeXmlString(text).Replace("&", string.Empty);
            text = System.Security.SecurityElement.Escape(text);
            Content = "<pre>" + text + "</pre>";
        }

        public void AddAttachment(byte[] binaryData, string contentId, string contentType, string contentFileName)
        {
            byte[] hash = new MD5CryptoServiceProvider().ComputeHash(binaryData);
            string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
            string cid = string.Empty;
            if (contentId != null)
                cid = contentId.TrimStart('<', '"');
            cid = cid.TrimEnd('>', '"');
            if ((cid.Length > 0) && content.Contains(cid))
            {
                // convert the reference tag to a media tag
                int idIndex = content.IndexOf(cid);

                // go left until the '<' is found
                int bracketIndex = content.LastIndexOf('<', idIndex);
                int endBracket = content.IndexOf('>', bracketIndex);
                string refTag = content.Substring(bracketIndex, endBracket - bracketIndex + 1);
                int srcStart = refTag.ToLower().IndexOf("src=\"") + 4;
                int srcEnd = refTag.IndexOf('"', srcStart + 1);
                string srcString = refTag.Substring(srcStart, srcEnd - srcStart + 1);
                string mediaTag = refTag.Replace("src=", "hash=");
                mediaTag = mediaTag.Replace(srcString, '"' + hashHex + '"');
                int imgStart = mediaTag.IndexOf('<') + 1;
                int imgEnd = mediaTag.IndexOfAny(" \t".ToCharArray(), imgStart);
                mediaTag = mediaTag.Remove(imgStart, imgEnd - imgStart);
                mediaTag = mediaTag.Insert(imgStart, "en-media type=\"" + contentType.ToLower() + "\"");
                Content = content.Replace(refTag, mediaTag);
            }
            else
            {
                // just link the attachment to the content
                Content = content + "<en-media hash=\"" + hashHex + "\" type=\"" + contentType.ToLower() + "\"/>";
            }

            string attachmentString = Convert.ToBase64String(binaryData);
            attachmentString = "<data encoding=\"base64\">" + attachmentString + "</data><mime>" + contentType.ToLower() + "</mime>" +
                "<resource-attributes><file-name>" + XmlSanitizer.SanitizeXmlString(contentFileName).Replace("&", string.Empty) + "</file-name></resource-attributes>";
            Attachment at = new Attachment();
            at.Base64Data = attachmentString;
            at.ContentID = contentId;
            at.ContentType = contentType.ToLower();
            at.FileName = contentFileName;
            at.Hash = hashHex;
            Attachments.Add(at);
        }

        public void SaveEvernoteExportData(string path)
        {
            XmlDocument xmlDoc = new XmlDocument();

            // Write down the XML declaration
            XmlDeclaration xmlDeclaration = xmlDoc.CreateXmlDeclaration("1.0", "utf-8", null);

            // Create the root element
            XmlElement rootNode = xmlDoc.CreateElement("en-export");
            rootNode.SetAttribute("application", "Evernote/Windows");
            rootNode.SetAttribute("version", "3.5");
            xmlDoc.InsertBefore(xmlDeclaration, xmlDoc.DocumentElement);
            xmlDoc.AppendChild(rootNode);

            // Create a new <note> element and add it to the root node
            XmlElement parentNode = xmlDoc.CreateElement("note");
            xmlDoc.DocumentElement.PrependChild(parentNode);

            // Create the required nodes
            XmlElement titleNode = xmlDoc.CreateElement("title");
            XmlText titleText = xmlDoc.CreateTextNode(Title);
            titleNode.AppendChild(titleText);
            parentNode.AppendChild(titleNode);

            XmlElement contentNode = xmlDoc.CreateElement("content");
            contentNode.InnerXml = Content;
            parentNode.AppendChild(contentNode);

            foreach (string tag in Tags)
            {
                XmlElement tagNode = xmlDoc.CreateElement("tag");
                XmlText tagText = xmlDoc.CreateTextNode(tag);
                tagNode.AppendChild(tagText);
                parentNode.AppendChild(tagNode);
            }

            foreach (Attachment at in Attachments)
            {
                XmlElement resourceNode = xmlDoc.CreateElement("resource");
                resourceNode.InnerXml = at.Base64Data;
                parentNode.AppendChild(resourceNode);
            }

            if (Date != null)
            {
                XmlElement dateNode = xmlDoc.CreateElement("created");
                XmlText dateText = xmlDoc.CreateTextNode(Date.ToUniversalTime().ToString("yyyyMMddTHHmmssZ"));
                dateNode.AppendChild(dateText);
                parentNode.AppendChild(dateNode);
            }

            // Save to the XML file
            xmlDoc.Save(path);
        }

        public string Content
        {
            get
            {
                return content;
            }

            set
            {
                content = value.Replace("\r", string.Empty);
                byte[] hash = new MD5CryptoServiceProvider().ComputeHash(System.Text.Encoding.UTF8.GetBytes(content));
                string hashHex = BitConverter.ToString(hash).Replace("-", string.Empty).ToLower();
                ContentHash = hashHex;
            }
        }

        public string Title { get; set; }
        public string ContentHash { get; set; }
        public List<string> Tags { get; set; }
        public List<Attachment> Attachments { get; set; }
        public NoteAction Action { get; set; }
        public DateTime Date { get; set; }
        public string SourceUrl { get; set; }
        public List<string> NewTags { get; set; }
        public List<string> ObsoleteTags { get; set; }

        private string content;
    }
}
