using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Xml;
using Svg.Transforms;

namespace Svg
{
    /// <summary>
    /// The base class of which all SVG elements are derived from.
    /// </summary>
    public abstract partial class SvgElement : ISvgElement, ISvgTransformable, ICloneable, ISvgNode
    {
        internal const int StyleSpecificity_PresAttribute = 0;
        internal const int StyleSpecificity_InlineStyle = 1 << 16;

        //optimization
        protected class PropertyAttributeTuple
        {
            public PropertyDescriptor Property;
            public SvgAttributeAttribute Attribute;
        }

        protected class EventAttributeTuple
        {
            public FieldInfo Event;
            public SvgAttributeAttribute Attribute;
        }

        //reflection cache
        private IEnumerable<PropertyAttributeTuple> _svgPropertyAttributes;
        private IEnumerable<EventAttributeTuple> _svgEventAttributes;

        internal SvgElement _parent;
        private string _elementName;
        private SvgAttributeCollection _attributes;
        private EventHandlerList _eventHandlers;
        private SvgElementCollection _children;
        private static readonly object _loadEventKey = new object();
        private Matrix3x2 _graphicsTransform;
        private SvgCustomAttributeCollection _customAttributes;
        private List<ISvgNode> _nodes = new List<ISvgNode>();

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        protected internal string ElementName
        {
            get
            {
                if (string.IsNullOrEmpty(this._elementName))
                {
                    var attr = TypeDescriptor.GetAttributes(this).OfType<SvgElementAttribute>().SingleOrDefault();

                    if (attr != null)
                    {
                        this._elementName = attr.ElementName;
                    }
                }

                return this._elementName;
            }
            internal set { this._elementName = value; }
        }

        /// <summary>
        /// Gets or sets the color <see cref="SvgPaintServer"/> of this element which drives the currentColor property.
        /// </summary>
        [SvgAttribute("color")]
        public virtual SvgPaintServer Color
        {
            get { return GetAttribute("color", true, SvgPaintServer.NotSet); }
            set { Attributes["color"] = value; }
        }

        /// <summary>
        /// Gets or sets the content of the element.
        /// </summary>
        private string _content;
        public virtual string Content
        {
            get
            {
                return _content;
            }
            set
            {
                if (_content != null)
                {
                    var oldVal = _content;
                    _content = value;
                    if (_content != oldVal)
                        OnContentChanged(new ContentEventArgs { Content = value });
                }
                else
                {
                    _content = value;
                    OnContentChanged(new ContentEventArgs { Content = value });
                }
            }
        }

        /// <summary>
        /// Gets an <see cref="EventHandlerList"/> of all events belonging to the element.
        /// </summary>
        protected virtual EventHandlerList Events
        {
            get { return this._eventHandlers; }
        }

        /// <summary>
        /// Occurs when the element is loaded.
        /// </summary>
        public event EventHandler Load
        {
            add { this.Events.AddHandler(_loadEventKey, value); }
            remove { this.Events.RemoveHandler(_loadEventKey, value); }
        }

        /// <summary>
        /// Gets a collection of all child <see cref="SvgElement"/> objects.
        /// </summary>
        public virtual SvgElementCollection Children
        {
            get { return this._children; }
        }

        public IList<ISvgNode> Nodes
        {
            get { return this._nodes; }
        }

        public IEnumerable<SvgElement> Descendants()
        {
            return this.AsEnumerable().Descendants();
        }
        private IEnumerable<SvgElement> AsEnumerable()
        {
            yield return this;
        }

        /// <summary>
        /// Gets a value to determine whether the element has children.
        /// </summary>
        public virtual bool HasChildren()
        {
            return (this.Children.Count > 0);
        }

        /// <summary>
        /// Gets the parent <see cref="SvgElement"/>.
        /// </summary>
        /// <value>An <see cref="SvgElement"/> if one exists; otherwise null.</value>
        public virtual SvgElement Parent
        {
            get { return this._parent; }
        }

        public IEnumerable<SvgElement> Parents
        {
            get
            {
                var curr = this;
                while (curr.Parent != null)
                {
                    curr = curr.Parent;
                    yield return curr;
                }
            }
        }
        public IEnumerable<SvgElement> ParentsAndSelf
        {
            get
            {
                var curr = this;
                yield return curr;
                while (curr.Parent != null)
                {
                    curr = curr.Parent;
                    yield return curr;
                }
            }
        }

        /// <summary>
        /// Gets the owner <see cref="SvgDocument"/>.
        /// </summary>
        public virtual SvgDocument OwnerDocument
        {
            get
            {
                if (this is SvgDocument)
                {
                    return this as SvgDocument;
                }
                else
                {
                    if (this.Parent != null)
                        return Parent.OwnerDocument;
                    else
                        return null;
                }
            }
        }

        /// <summary>
        /// Gets a collection of element attributes.
        /// </summary>
        protected internal virtual SvgAttributeCollection Attributes
        {
            get
            {
                if (this._attributes == null)
                {
                    this._attributes = new SvgAttributeCollection(this);
                }

                return this._attributes;
            }
        }

        protected bool Writing { get; set; }

        protected internal TAttributeType GetAttribute<TAttributeType>(string attributeName, bool inherited, TAttributeType defaultValue = default(TAttributeType))
        {
            if (Writing)
                return Attributes.GetAttribute(attributeName, defaultValue);
            else
                return Attributes.GetInheritedAttribute(attributeName, inherited, defaultValue);
        }

        /// <summary>
        /// Gets a collection of custom attributes
        /// </summary>
        public SvgCustomAttributeCollection CustomAttributes
        {
            get { return this._customAttributes; }
        }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        protected internal virtual bool PushTransforms(ISvgRenderer renderer)
        {
            _graphicsTransform = renderer.Transform;

            var transforms = Transforms;
            // Return if there are no transforms
            if (transforms == null || transforms.Count == 0)
                return true;

            var transformMatrix = transforms.GetMatrix();
            if (transformMatrix.IsIdentity)
                return false;

            renderer.Transform = Matrix3x2.Multiply(transformMatrix, _graphicsTransform);

            return true;
        }

        /// <summary>
        /// Removes any previously applied transforms from the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that should have transforms removed.</param>
        protected internal virtual void PopTransforms(ISvgRenderer renderer)
        {
            renderer.Transform = _graphicsTransform;
        }

        /// <summary>
        /// Applies the required transforms to <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to be transformed.</param>
        void ISvgTransformable.PushTransforms(ISvgRenderer renderer)
        {
            PushTransforms(renderer);
        }

        /// <summary>
        /// Removes any previously applied transforms from the specified <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that should have transforms removed.</param>
        void ISvgTransformable.PopTransforms(ISvgRenderer renderer)
        {
            PopTransforms(renderer);
        }

        /// <summary>
        /// Gets or sets the element transforms.
        /// </summary>
        /// <value>The transforms.</value>
        [SvgAttribute("transform")]
        public SvgTransformCollection Transforms
        {
            get { return GetAttribute<SvgTransformCollection>("transform", false); }
            set
            {
                var old = Transforms;
                if (old != null)
                    old.TransformChanged -= Attributes_AttributeChanged;
                value.TransformChanged += Attributes_AttributeChanged;
                Attributes["transform"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the ID of the element.
        /// </summary>
        /// <exception cref="SvgException">The ID is already used within the <see cref="SvgDocument"/>.</exception>
        [SvgAttribute("id")]
        public string ID
        {
            get { return GetAttribute<string>("id", false); }
            set { SetAndForceUniqueID(value, false); }
        }

        /// <summary>
        /// Gets or sets the space handling.
        /// </summary>
        /// <value>The space handling.</value>
        [SvgAttribute("space", SvgAttributeAttribute.XmlNamespace)]
        public virtual XmlSpaceHandling SpaceHandling
        {
            get { return GetAttribute("space", true, XmlSpaceHandling.inherit); }
            set { Attributes["space"] = value; }
        }

        public void SetAndForceUniqueID(string value, bool autoForceUniqueID = true, Action<SvgElement, string, string> logElementOldIDNewID = null)
        {
            // Don't do anything if it hasn't changed
            if (string.Compare(ID, value) == 0)
            {
                return;
            }

            if (OwnerDocument != null)
            {
                OwnerDocument.IdManager.Remove(this);
            }

            Attributes["id"] = value;

            if (OwnerDocument != null)
            {
                OwnerDocument.IdManager.AddAndForceUniqueID(this, null, autoForceUniqueID, logElementOldIDNewID);
            }
        }

        /// <summary>
        /// Only used by the ID Manager
        /// </summary>
        /// <param name="newID"></param>
        internal void ForceUniqueID(string newID)
        {
            Attributes["id"] = newID;
        }

        /// <summary>
        /// Called by the underlying <see cref="SvgElement"/> when an element has been added to the
        /// <see cref="Children"/> collection.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been added.</param>
        /// <param name="index">An <see cref="int"/> representing the index where the element was added to the collection.</param>
        protected virtual void AddElement(SvgElement child, int index)
        {
        }

        /// <summary>
        /// Fired when an Element was added to the children of this Element
        /// </summary>
        public event EventHandler<ChildAddedEventArgs> ChildAdded;

        /// <summary>
        /// Calls the <see cref="AddElement"/> method with the specified parameters.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been added.</param>
        /// <param name="index">An <see cref="int"/> representing the index where the element was added to the collection.</param>
        internal void OnElementAdded(SvgElement child, int index)
        {
            this.AddElement(child, index);
            SvgElement sibling = null;
            if (index < (Children.Count - 1))
            {
                sibling = Children[index + 1];
            }
            var handler = ChildAdded;
            if (handler != null)
            {
                handler(this, new ChildAddedEventArgs { NewChild = child, BeforeSibling = sibling });
            }
        }

        /// <summary>
        /// Called by the underlying <see cref="SvgElement"/> when an element has been removed from the
        /// <see cref="Children"/> collection.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been removed.</param>
        protected virtual void RemoveElement(SvgElement child)
        {
        }

        /// <summary>
        /// Calls the <see cref="RemoveElement"/> method with the specified <see cref="SvgElement"/> as the parameter.
        /// </summary>
        /// <param name="child">The <see cref="SvgElement"/> that has been removed.</param>
        internal void OnElementRemoved(SvgElement child)
        {
            this.RemoveElement(child);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SvgElement"/> class.
        /// </summary>
        public SvgElement()
        {
            this._children = new SvgElementCollection(this);
            this._eventHandlers = new EventHandlerList();
            this._elementName = string.Empty;
            this._customAttributes = new SvgCustomAttributeCollection(this);

            //subscribe to attribute events
            Attributes.AttributeChanged += Attributes_AttributeChanged;
            CustomAttributes.AttributeChanged += Attributes_AttributeChanged;

            //find svg attribute descriptions
            _svgPropertyAttributes = from PropertyDescriptor a in TypeDescriptor.GetProperties(this)
                                     let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
                                     where attribute != null
                                     select new PropertyAttributeTuple { Property = a, Attribute = attribute };

            _svgEventAttributes = from EventDescriptor a in TypeDescriptor.GetEvents(this)
                                  let attribute = a.Attributes[typeof(SvgAttributeAttribute)] as SvgAttributeAttribute
                                  where attribute != null
                                  select new EventAttributeTuple { Event = a.ComponentType.GetField(a.Name, BindingFlags.Instance | BindingFlags.NonPublic), Attribute = attribute };

        }

        //dispatch attribute event
        void Attributes_AttributeChanged(object sender, AttributeEventArgs e)
        {
            OnAttributeChanged(e);
        }

        public virtual void InitialiseFromXML(XmlTextReader reader, SvgDocument document)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Renders this element to the <see cref="ISvgRenderer"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> that the element should use to render itself.</param>
        public void RenderElement(ISvgRenderer renderer)
        {
            Render(renderer);
        }

        /// <summary>Derrived classes may decide that the element should not be written. For example, the text element shouldn't be written if it's empty.</summary>
        public virtual bool ShouldWriteElement()
        {
            //Write any element who has a name.
            return (this.ElementName != String.Empty);
        }

        protected virtual void WriteStartElement(XmlTextWriter writer)
        {
            if (this.ElementName != String.Empty)
            {
                writer.WriteStartElement(this.ElementName);
            }

            this.WriteAttributes(writer);
        }

        protected virtual void WriteEndElement(XmlTextWriter writer)
        {
            if (this.ElementName != String.Empty)
            {
                writer.WriteEndElement();
            }
        }
        protected virtual void WriteAttributes(XmlTextWriter writer)
        {
            //properties
            WritePropertyAttributes(writer);

            //events
            if (AutoPublishEvents)
            {
                foreach (var attr in _svgEventAttributes)
                {
                    var evt = attr.Event.GetValue(this);

                    //if someone has registered publish the attribute
                    if (evt != null && !string.IsNullOrEmpty(this.ID))
                    {
                        writer.WriteAttributeString(attr.Attribute.Name, this.ID + "/" + attr.Attribute.Name);
                    }
                }
            }

            //add the custom attributes
            var additionalStyleValue = string.Empty;
            foreach (var item in this._customAttributes)
            {
                writer.WriteAttributeString(item.Key, item.Value);
            }
        }

        private void WritePropertyAttributes(XmlTextWriter writer)
        {
            var opacityAttributes = new List<PropertyAttributeTuple>();
            var opacityValues = new Dictionary<string, float>();

            try
            {
                Writing = true;

                foreach (var attr in _svgPropertyAttributes)
                {
                    if (attr.Property.Converter.CanConvertTo(typeof(string)))
                    {
                        if (attr.Attribute.Name == "fill-opacity" || attr.Attribute.Name == "stroke-opacity")
                        {
                            opacityAttributes.Add(attr);
                            continue;
                        }

                        if (Attributes.ContainsKey(attr.Attribute.Name))
                        {
                            var propertyValue = attr.Property.GetValue(this);

                            var forceWrite = false;
                            var writeStyle = attr.Attribute.Name == "fill" || attr.Attribute.Name == "stroke";

                            if (Parent != null)
                            {
                                if (writeStyle && propertyValue == SvgPaintServer.NotSet)
                                    continue;

                                object parentValue;
                                if (TryResolveParentAttributeValue(attr.Attribute.Name, out parentValue))
                                {
                                    if ((parentValue == propertyValue)
                                        || ((parentValue != null) && parentValue.Equals(propertyValue)))
                                    {
                                        if (writeStyle)
                                            continue;
                                    }
                                    else
                                        forceWrite = true;
                                }
                            }

                            var hasOpacity = writeStyle;
                            if (hasOpacity)
                            {
                                if (propertyValue is SvgColourServer && ((SvgColourServer)propertyValue).Colour.A < 255)
                                {
                                    var opacity = ((SvgColourServer)propertyValue).Colour.A / 255f;
                                    opacityValues.Add(attr.Attribute.Name + "-opacity", opacity);
                                }
                            }

                            // dotnetcore throws exception if input is null
                            var value = propertyValue == null ? null : (string)attr.Property.Converter.ConvertTo(propertyValue, typeof(string));

                            if (propertyValue != null)
                            {
                                //Only write the attribute's value if it is not the default value, not null/empty, or we're forcing the write.
                                if (forceWrite || !string.IsNullOrEmpty(value))
                                {
                                    if (writeStyle)
                                    {
                                        //styles[attr.Attribute.Name] = value;
                                    }
                                    else
                                    {
                                        writer.WriteAttributeString(attr.Attribute.NamespaceAndName, value);
                                    }
                                }
                            }
                            else if (attr.Attribute.Name == "fill") //if fill equals null, write 'none'
                            {
                                if (writeStyle)
                                {
                                    //styles[attr.Attribute.Name] = value;
                                }
                                else
                                {
                                    writer.WriteAttributeString(attr.Attribute.NamespaceAndName, value);
                                }
                            }
                        }
                    }
                }

                foreach (var attr in opacityAttributes)
                {
                    var opacity = 1f;
                    var write = false;

                    var key = attr.Attribute.Name;
                    if (opacityValues.ContainsKey(key))
                    {
                        opacity = opacityValues[key];
                        write = true;
                    }
                    if (Attributes.ContainsKey(key))
                    {
                        opacity *= (float)attr.Property.GetValue(this);
                        write = true;
                    }
                    if (write)
                    {
                        opacity = (float)Math.Round(opacity, 2, MidpointRounding.AwayFromZero);
                        var value = (string)attr.Property.Converter.ConvertTo(opacity, typeof(string));
                        if (!string.IsNullOrEmpty(value))
                            writer.WriteAttributeString(attr.Attribute.NamespaceAndName, value);
                    }
                }
            }
            finally
            {
                Writing = false;
            }
        }

        public bool AutoPublishEvents = true;

        private bool TryResolveParentAttributeValue(string attributeKey, out object parentAttributeValue)
        {
            parentAttributeValue = null;

            //attributeKey = char.ToUpper(attributeKey[0]) + attributeKey.Substring(1);

            var currentParent = Parent;
            var resolved = false;
            while (currentParent != null)
            {
                if (currentParent.Attributes.ContainsKey(attributeKey))
                {
                    resolved = true;
                    parentAttributeValue = currentParent.Attributes[attributeKey];
                    if (parentAttributeValue != null)
                        break;
                }
                currentParent = currentParent.Parent;
            }

            return resolved;
        }

        public virtual void Write(XmlTextWriter writer)
        {
            if (ShouldWriteElement())
            {
                this.WriteStartElement(writer);
                this.WriteChildren(writer);
                this.WriteEndElement(writer);
            }
        }

        protected virtual void WriteChildren(XmlTextWriter writer)
        {
            if (this.Nodes.Any())
            {
                SvgContentNode content;
                foreach (var node in this.Nodes)
                {
                    content = node as SvgContentNode;
                    if (content == null)
                    {
                        ((SvgElement)node).Write(writer);
                    }
                    else if (!string.IsNullOrEmpty(content.Content))
                    {
                        writer.WriteString(content.Content);
                    }
                }
            }
            else
            {
                //write the content
                if (!String.IsNullOrEmpty(this.Content))
                    writer.WriteString(this.Content);

                //write all children
                foreach (SvgElement child in this.Children)
                {
                    child.Write(writer);
                }
            }
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        protected virtual void Render(ISvgRenderer renderer)
        {
            try
            {
                PushTransforms(renderer);
                RenderChildren(renderer);
            }
            finally
            {
                PopTransforms(renderer);
            }
        }

        /// <summary>
        /// Renders the children of this <see cref="SvgElement"/>.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> to render the child <see cref="SvgElement"/>s to.</param>
        protected virtual void RenderChildren(ISvgRenderer renderer)
        {
            foreach (var element in Children)
                element.Render(renderer);
        }

        /// <summary>
        /// Renders the <see cref="SvgElement"/> and contents to the specified <see cref="ISvgRenderer"/> object.
        /// </summary>
        /// <param name="renderer">The <see cref="ISvgRenderer"/> object to render to.</param>
        void ISvgElement.Render(ISvgRenderer renderer)
        {
            Render(renderer);
        }

        /// <summary>
        /// Recursive method to add up the paths of all children
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="path"></param>
        protected void AddPaths(SvgElement elem, GraphicsPath path)
        {
            foreach (var child in elem.Children)
            {
                // Skip to avoid double calculate Symbol element
                // symbol element is only referenced by use element 
                // So here we need to skip when it is directly considered
                if (child is Svg.Document_Structure.SvgSymbol)
                    continue;

                if (child is SvgVisualElement)
                {
                    if (!(child is SvgGroup))
                    {
                        var childPath = ((SvgVisualElement)child).Path(null)?.Clone();
                        if (child.Transforms != null)
                        {
                            childPath.Transform(child.Transforms.GetMatrix());
                        }

                        if (!childPath.IsEmpty)
                        {
                            path.AddPath(childPath);
                        }
                    }
                }

                if (!(child is SvgPaintServer)) AddPaths(child, path);
            }
        }

        /// <summary>
        /// Recursive method to add up the paths of all children
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="renderer"></param>
        protected GraphicsPath GetPaths(SvgElement elem, ISvgRenderer renderer)
        {
            var ret = new GraphicsPath();

            foreach (var child in elem.Children)
            {
                if (child is SvgVisualElement)
                {
                    if (child is SvgGroup)
                    {
                        var childPath = GetPaths(child, renderer);
                        if (!childPath.IsEmpty)
                        {
                            if (child.Transforms != null)
                            {
                                childPath.Transform(child.Transforms.GetMatrix());
                            }

                            ret.AddPath(childPath);
                        }
                    }
                    else
                    {
                        var childPath = ((SvgVisualElement)child).Path(renderer);
                        childPath = childPath != null ? (GraphicsPath)childPath.Clone() : new GraphicsPath();

                        // Non-group element can have child element which we have to consider. i.e tspan in text element
                        if (child.Children.Count > 0)
                        {
                            var descendantPath = GetPaths(child, renderer);
                            if (!descendantPath.IsEmpty)
                                childPath.AddPath(descendantPath);
                        }

                        if (!childPath.IsEmpty)
                        {
                            if (child.Transforms != null)
                            {
                                childPath.Transform(child.Transforms.GetMatrix());
                            }

                            ret.AddPath(childPath);
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// Creates a new object that is a copy of the current instance.
        /// </summary>
        /// <returns>
        /// A new object that is a copy of this instance.
        /// </returns>
        public virtual object Clone()
        {
            return DeepCopy();
        }

        public abstract SvgElement DeepCopy();

        ISvgNode ISvgNode.DeepCopy()
        {
            return DeepCopy();
        }

        public virtual SvgElement DeepCopy<T>() where T : SvgElement, new()
        {
            var newObj = new T
            {
                Content = Content,
                ElementName = ElementName
            };

            //if (this.Parent != null)
            //    this.Parent.Children.Add(newObj);

            foreach (var attribute in Attributes)
            {
                var value = attribute.Value is ICloneable ? ((ICloneable)attribute.Value).Clone() : attribute.Value;
                newObj.Attributes.Add(attribute.Key, value);
            }

            foreach (var child in Children)
                newObj.Children.Add(child.DeepCopy());

            foreach (var attr in _svgEventAttributes)
            {
                var evt = attr.Event.GetValue(this);

                // if someone has registered also register here
                if (evt != null)
                {
                    if (attr.Event.Name == "MouseDown")
                        newObj.MouseDown += delegate { };
                    else if (attr.Event.Name == "MouseUp")
                        newObj.MouseUp += delegate { };
                    else if (attr.Event.Name == "MouseOver")
                        newObj.MouseOver += delegate { };
                    else if (attr.Event.Name == "MouseOut")
                        newObj.MouseOut += delegate { };
                    else if (attr.Event.Name == "MouseMove")
                        newObj.MouseMove += delegate { };
                    else if (attr.Event.Name == "MouseScroll")
                        newObj.MouseScroll += delegate { };
                    else if (attr.Event.Name == "Click")
                        newObj.Click += delegate { };
                    else if (attr.Event.Name == "Change") // text element
                        (newObj as SvgText).Change += delegate { };
                }
            }

            foreach (var attribute in CustomAttributes)
                newObj.CustomAttributes.Add(attribute.Key, attribute.Value);

            foreach (var node in Nodes)
                newObj.Nodes.Add(node.DeepCopy());

            return newObj;
        }

        /// <summary>
        /// Fired when an Atrribute of this Element has changed
        /// </summary>
        public event EventHandler<AttributeEventArgs> AttributeChanged;

        protected void OnAttributeChanged(AttributeEventArgs args)
        {
            var handler = AttributeChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        /// <summary>
        /// Fired when an Atrribute of this Element has changed
        /// </summary>
        public event EventHandler<ContentEventArgs> ContentChanged;

        protected void OnContentChanged(ContentEventArgs args)
        {
            var handler = ContentChanged;
            if (handler != null)
            {
                handler(this, args);
            }
        }

        #region graphical EVENTS

        /*
            onfocusin = "<anything>"
            onfocusout = "<anything>"
            onactivate = "<anything>"
            onclick = "<anything>"
            onmousedown = "<anything>"
            onmouseup = "<anything>"
            onmouseover = "<anything>"
            onmousemove = "<anything>"
            onmouseout = "<anything>" 
         */


        [SvgAttribute("onclick")]
        public event EventHandler<MouseArg> Click;

        [SvgAttribute("onmousedown")]
        public event EventHandler<MouseArg> MouseDown;

        [SvgAttribute("onmouseup")]
        public event EventHandler<MouseArg> MouseUp;

        [SvgAttribute("onmousemove")]
        public event EventHandler<MouseArg> MouseMove;

        [SvgAttribute("onmousescroll")]
        public event EventHandler<MouseScrollArg> MouseScroll;

        [SvgAttribute("onmouseover")]
        public event EventHandler<MouseArg> MouseOver;

        [SvgAttribute("onmouseout")]
        public event EventHandler<MouseArg> MouseOut;

        //click
        protected void RaiseMouseClick(object sender, MouseArg e)
        {
            var handler = Click;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        //down
        protected void RaiseMouseDown(object sender, MouseArg e)
        {
            var handler = MouseDown;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        //up
        protected void RaiseMouseUp(object sender, MouseArg e)
        {
            var handler = MouseUp;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        protected void RaiseMouseMove(object sender, MouseArg e)
        {
            var handler = MouseMove;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        //over
        protected void RaiseMouseOver(object sender, MouseArg args)
        {
            var handler = MouseOver;
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        //out
        protected void RaiseMouseOut(object sender, MouseArg args)
        {
            var handler = MouseOut;
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        //scroll
        protected void OnMouseScroll(int scroll, bool ctrlKey, bool shiftKey, bool altKey, string sessionID)
        {
            RaiseMouseScroll(this, new MouseScrollArg { Scroll = scroll, AltKey = altKey, ShiftKey = shiftKey, CtrlKey = ctrlKey, SessionID = sessionID });
        }

        protected void RaiseMouseScroll(object sender, MouseScrollArg e)
        {
            var handler = MouseScroll;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        #endregion graphical EVENTS
    }

    public class SVGArg : EventArgs
    {
        public string SessionID;
    }

    /// <summary>
    /// Describes the Attribute which was set
    /// </summary>
    public class AttributeEventArgs : SVGArg
    {
        public string Attribute;
        public object Value;
    }

    /// <summary>
    /// Content of this whas was set
    /// </summary>
    public class ContentEventArgs : SVGArg
    {
        public string Content;
    }

    /// <summary>
    /// Describes the Attribute which was set
    /// </summary>
    public class ChildAddedEventArgs : SVGArg
    {
        public SvgElement NewChild;
        public SvgElement BeforeSibling;
    }

    /// <summary>
    /// Represents the state of the mouse at the moment the event occured.
    /// </summary>
    public class MouseArg : SVGArg
    {
        public float x;
        public float y;

        /// <summary>
        /// 1 = left, 2 = middle, 3 = right
        /// </summary>
        public int Button;

        /// <summary>
        /// Amount of mouse clicks, e.g. 2 for double click
        /// </summary>
        public int ClickCount = -1;

        /// <summary>
        /// Alt modifier key pressed
        /// </summary>
        public bool AltKey;

        /// <summary>
        /// Shift modifier key pressed
        /// </summary>
        public bool ShiftKey;

        /// <summary>
        /// Control modifier key pressed
        /// </summary>
        public bool CtrlKey;
    }

    /// <summary>
    /// Represents a string argument
    /// </summary>
    public class StringArg : SVGArg
    {
        public string s;
    }

    public class MouseScrollArg : SVGArg
    {
        public int Scroll;

        /// <summary>
        /// Alt modifier key pressed
        /// </summary>
        public bool AltKey;

        /// <summary>
        /// Shift modifier key pressed
        /// </summary>
        public bool ShiftKey;

        /// <summary>
        /// Control modifier key pressed
        /// </summary>
        public bool CtrlKey;
    }

    public interface ISvgNode
    {
        string Content { get; }

        /// <summary>
        /// Create a deep copy of this <see cref="ISvgNode"/>.
        /// </summary>
        /// <returns>A deep copy of this <see cref="ISvgNode"/></returns>
        ISvgNode DeepCopy();
    }

    /// <summary>This interface mostly indicates that a node is not to be drawn when rendering the SVG.</summary>
    public interface ISvgDescriptiveElement
    {
    }

    internal interface ISvgElement
    {
        SvgElement Parent { get; }
        SvgElementCollection Children { get; }
        IList<ISvgNode> Nodes { get; }

        void Render(ISvgRenderer renderer);
    }
}
