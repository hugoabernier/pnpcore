
# Extending the model for SharePoint REST

The PnP Core SDK model contains model, collection, and complex type classes which are populated via either Microsoft Graph and/or SharePoint REST. In this chapter you'll learn more on how to decorate your classes and their properties to interact with Microsoft 365 via the SharePoint REST API.

## Configuring model classes

### Public model (interface) decoration

For model classes that **are linq queriable** one needs to link the concrete (so the implementation) to the public interface via the `ConcreteType` class attribute:

```csharp
[ConcreteType(typeof(List))]
public interface IList : IDataModel<IList>, IDataModelUpdate, IDataModelDelete
{
    // Ommitted for brevity
}
```

### Class decoration

Each model class that uses SharePoint REST does need to have at least one `SharePointType` attribute which is defined on the coded model class (e.g. List.cs):

```csharp
[SharePointType("SP.List", Uri = "_api/Web/Lists(guid'{Id}')", Get = "_api/web/lists", Update = "_api/web/lists/getbyid(guid'{Id}')", LinqGet = "_api/web/lists")]
internal partial class List
{
    // Ommitted for brevity
}
```

When configuring the `SharePointType` attribute for SharePoint REST you need to set attribute properties:

Property | Required | Description
---------|----------|------------
Type | Yes | Defines the SharePoint REST type that maps with the model class. Each model that requires SharePoint REST requires this attribute, hence the type is requested via the attribute constructor.
Uri | Yes | Defines the URI that uniquely identifies this object. See [model tokens](model%20tokens.md) to learn more about the possible tokens you can use.
Target | No | A model can be used from multiple scope (e.g. the ContentTypeCollection is available for both Web and List model classes) and if so the `Target` property defines the scope of the `SharePointType` attribute.
Get | No | Overrides the Uri property for **get** operations.
LinqGet | No | Some model classes do support linq queries which are translated in corresponding server calls. If a class supports linq in this way, then it also needs to have the LinqGet attribute set.
Update | No | Overrides the Uri property for **update** operations.
Delete | No | Overrides the Uri property for **delete** operations.
OverflowProperty | No | Used when working with a dynamic property/value pair (e.g. fields in a SharePoint ListItem) whenever the SharePoint REST field containing these dynamic properties is not named `Values`.

#### Sample of using multiple SharePointType decorations

Below sample shows how a model can be decorated for multiple scopes:

```csharp
[SharePointType("SP.ContentType", Target = typeof(Web), Uri = "_api/Web/ContentTypes('{Id}')", Get = "_api/web/contenttypes", LinqGet = "_api/web/contenttypes")]
[SharePointType("SP.ContentType", Target = typeof(List), Uri = "_api/Web/Lists(guid'{Parent.Id}')/ContentTypes('{Id}')", Get = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes", LinqGet = "_api/Web/Lists(guid'{Parent.Id}')/contenttypes")]
internal partial class ContentType
{
    // Ommitted for brevity
}
```

### Property decoration

The property level decoration is done using the `SharePointProperty` and `KeyProperty` attributes. Each model instance does require to have a override of the `Key` property and that `Key` property **must** be decorated with the `KeyProperty` attribute, which specifies the actual fields in the model that must be selected as key. The key is for example used to ensure there are no duplicate model class instances in a single collection.

Whereas the `KeyProperty` attribute is always there once in each model class, the usage of the `SharePointProperty` attribute is only needed whenever it makes sense. For most properties you do not need to set this attribute, it's only required for special cases. Since the properties are defined in the generated model class (e.g. List.gen.cs) the decoration via attributes needs to happen in this class as well.

```csharp
// Configure the SharePoint REST field used to populate this model property
[SharePointProperty("DocumentTemplateUrl")]
public string DocumentTemplate { get => GetValue<string>(); set => SetValue(value); }

// Define a collection as expandable
[SharePointProperty("Items", Expandable = true)]
public IListItemCollection Items
{
    get
    {
        if (!HasValue(nameof(Items)))
        {
            var items = new ListItemCollection
            {
                PnPContext = this.PnPContext,
                Parent = this
            };
            SetValue(items);
        }
        return GetValue<IListItemCollection>();
    }
}

// Set the keyfield for this model class
[KeyProperty("Id")]
public override object Key { get => this.Id; set => this.Id = Guid.Parse(value.ToString()); }
```

You can set following properties on this attribute:

Property | Required | Description
---------|----------|------------
FieldName | Yes | Use this property when the SharePoint REST fieldname differs from the model property name, since the field name is required by the default constructor you always need to provide this value when you add this property.
JsonPath | No | When the information returned from SharePoint REST is a complex type and you only need a single value from it, then you can specify the JsonPath for that value. E.g. when you get sharePointIds.webId as response you tell the model that the fieldname is sharePointIds and the path to get there is webId. The path can be more complex, using a point to define property you need (e.g. property.child.childofchild).
Expandable | No | Defines that a collection is expandable, meaning it can be loaded via the $expand query parameter and used in the lambda expression in `Get` and `GetAsync` operations.
ExpandByDefault | No | When the model contains a collection of other model objects then setting this attribute to true will automatically result in the population of that collection. This can negatively impact performance, so only set this when the collection is almost always needed.
UseCustomMapping | No | Allows you to force a callout to the model's `MappingHandler` event handler whenever this property is populated. See the [Event Handlers](event%20handlers.md) article to learn more.

## Configuring complex type classes

Complex type classes are not used when the model is populated via SharePoint REST.

## Configuring collection classes

Collection classes **do not** have attribute based decoration.

## Implementing "Add" functionality

In contradiction with get, update, and delete which are fully handled by decorating classes and properties using attributes, you'll need to write actual code to implement add. Adding is implemented as follows:

- The public part (interface) is defined on the collection interface. Each functionality (like Add) is implemented via three methods:

  - An async method
  - A regular method
  - A regular method that allows to pass in a `Batch` as first method parameter

- Add methods defined on the interface are implemented in the collection classes as proxies that call into the respective add methods of the added model class.
- The implementation that performs the actual add is implemented as an `AddApiCallHandler` event handler in the model class. See the [Event Handlers](event%20handlers.md) page for more details.

Below code snippets show the above three concepts. First one shows the collection interface (e.g. IListCollection.cs) with the Add methods:

```csharp
/// <summary>
/// Public interface to define a collection of List objects of SharePoint Online
/// </summary>
public interface IListCollection : IQueryable<IList>, IDataModelCollection<IList>, ISupportPaging
{
    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public Task<IList> AddAsync(string title, int templateType);

    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="batch">Batch to use</param>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public IList Add(Batch batch, string title, int templateType);

    /// <summary>
    /// Adds a new list
    /// </summary>
    /// <param name="title">Title of the list</param>
    /// <param name="templateType">Template type</param>
    /// <returns>Newly added list</returns>
    public IList Add(string title, int templateType);
}
```

Implementation of the interface in the coded collection class (e.g. ListCollection.cs):

```csharp
internal partial class ListCollection
{
    public IList Add(string title, int templateType)
    {
        return Add(PnPContext.CurrentBatch, title, templateType);
    }

    public IList Add(Batch batch, string title, int templateType)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (templateType == 0)
        {
            throw new ArgumentException($"{nameof(templateType)} cannot be 0");
        }

        var newList = AddNewList();

        newList.Title = title;
        newList.TemplateType = templateType;

        return newList.Add(batch) as List;
    }

    public async Task<IList> AddAsync(string title, int templateType)
    {
        if (title == null)
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (templateType == 0)
        {
            throw new ArgumentException($"{nameof(templateType)} cannot be 0");
        }

        var newList = AddNewList();

        newList.Title = title;
        newList.TemplateType = templateType;

        return await newList.AddAsync().ConfigureAwait(false) as List;
    }
}
```

And finally you'll see the actual add logic being implemented in the coded model class (e.g. List.cs) via implementing the `AddApiCallHandler`:

```csharp
internal partial class List
{
    /// <summary>
    /// Class to model the Rest List Add request
    /// </summary>
    internal class ListAdd: RestBaseAdd<IList>
    {
        public int BaseTemplate { get; set; }

        public string Title { get; set; }

        internal ListAdd(BaseDataModel<IList> model, int templateType, string title) : base(model)
        {
            BaseTemplate = templateType;
            Title = title;
        }
    }

    internal List()
    {
        // Handler to construct the Add request for this list
        AddApiCallHandler = () =>
        {
            return new ApiCall($"_api/web/lists", ApiType.Rest, JsonSerializer.Serialize(new ListAdd(this, TemplateType, Title)));
        };
    }
}
```
