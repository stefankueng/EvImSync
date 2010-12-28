using System.Dynamic;

namespace HtmlAgilityPack
{
    public partial class HtmlNode : DynamicObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            switch (GetBinderType(binder.Name))
            {
                case HtmlNodeType.Attribute:
                    var name = binder.Name.Substring(1);
                    result = Attributes[name];
                    //if (result == null)
                    //    result = Attributes.Add(name, string.Empty);
                    break;
                default:
                    result = ChildNodes[binder.Name];
                    //if (result == null)
                    //{
                    //    result = new HtmlNode(binder.Name, OwnerDocument);
                    //    ChildNodes.Add(result as HtmlNode);
                    //}
                    break;
            }

            return result != null;
        }

        private HtmlNodeType GetBinderType(string name)
        {
            return name.StartsWith("_") ? HtmlNodeType.Attribute : HtmlNodeType.Element;
        }
    }

}
