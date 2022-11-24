using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using JXS.Graphics.Core;
using JXS.Gui.Components;
using JXS.Utils.Logging;

namespace JXS.Gui;

public class UILoader
{
    private const string STYLES_ELEMENT = "Styles";
    private const string DRAW_ELEMENT = "Draw";
    private const string VALUE_ELEMENT = "Value";
    private const string STYLE_ATTRIBUTE = "Style";
    private const string ID_ATTRIBUTE = "Id";
    private const string VALUE_PROPERTY = "value"; // All properties are treated as lower case
    private static readonly ILogger Logger = LoggingManager.Get<UILoader>();

    private static readonly IDictionary<string, Type> BuiltinComponents;

    private readonly IGraphicsProvider graphicsProvider;
    private readonly IInputProvider inputProvider;
    private readonly IResourceProvider resourceProvider;

    private Dictionary<string, Style>? styles;
    private Dictionary<string, string>? values;

    private XElement? xStyles;

    static UILoader()
    {
        BuiltinComponents = new Dictionary<string, Type>();
        // Find all loaded subclasses of "Component" and map them.
        var subclasses = from assembly in AppDomain.CurrentDomain.GetAssemblies()
            from type in assembly.GetTypes()
            where type.IsSubclassOf(typeof(Component))
            select type;
        foreach (var subclass in subclasses)
            BuiltinComponents.Add(subclass.Name, subclass);
    }

    public UILoader(IGraphicsProvider graphicsProvider, IInputProvider inputProvider, IResourceProvider resourceProvider)
    {
        this.graphicsProvider = graphicsProvider;
        this.inputProvider = inputProvider;
        this.resourceProvider = resourceProvider;
    }

    public Scene LoadSceneFromXml(XDocument document)
    {
        var scene = new Scene(graphicsProvider, inputProvider);

        var xRoot = document.Root;
        if (xRoot is null)
        {
            Logger.Error($"Invalid Xml document {document}");
            return scene;
        }

        xStyles = xRoot.Element(STYLES_ELEMENT);
        styles = new Dictionary<string, Style>();
        values = new Dictionary<string, string>();

        if (xStyles != null)
        {
            Logger.Trace("Processing values");
            foreach (var xValue in xStyles.Elements(VALUE_ELEMENT))
            {
                if (!xValue.StringAttribute(ID_ATTRIBUTE, out var id))
                {
                    Logger.Error($@"Value does not have an ""{ID_ATTRIBUTE}""!");
                    continue;
                }

                if (!xValue.StringAttribute(VALUE_PROPERTY, out var value))
                {
                    Logger.Error($@"Value {id} does not have a ""{VALUE_PROPERTY}""!");
                    continue;
                }

                values.Add(id, value);
            }
        }

        var xDraw = xRoot.Element(DRAW_ELEMENT);
        if (xDraw is null)
        {
            Logger.Error($"Scene must contain the {DRAW_ELEMENT} element.");
            return scene;
        }

        void ProcessComponent(XElement xElement, View? parent = null)
        {
            Logger.Trace($@"Processing component {xElement.Name.LocalName}");
            if (!BuiltinComponents.TryGetValue(xElement.Name.LocalName, out var componentType))
            {
                Logger.Error($@"Could not find component ""{xElement.Name.LocalName}"".");
                return;
            }

            var properties = xElement.Attributes().ToDictionary(
                keySelector: xAttribute => xAttribute.Name.LocalName.ToLowerInvariant(),
                elementSelector: xAttribute => xAttribute.Value);
            var textValue = xElement.Nodes().OfType<XText>().FirstOrDefault();
            if (textValue != null && textValue.Value.Length > 0 && !properties.ContainsKey(VALUE_PROPERTY))
                properties[VALUE_PROPERTY] = textValue.Value.Trim();

            ConstructorInfo constructorInfo;
            try
            {
                // Find all constructors that can be initialized with the given properties and order them by:
                // First: The ones with the most given parameters
                // Second: The ones with the highest number of parameters in total
                var constructors = componentType.GetConstructors().OrderByDescending(c =>
                        c.GetParameters().Count(param => properties.ContainsKey(param.Name!.ToLowerInvariant())))
                    .ThenByDescending(c => c.GetParameters().Length).ToList();

                // Parameters that are not optional, not nullable, and not a value type must be initialized
                constructorInfo = constructors.First(c =>
                    c.GetParameters().Where(p => !p.IsOptional && !IsNullable(p) && !p.ParameterType.IsValueType)
                        .All(p => properties.ContainsKey(p.Name!.ToLowerInvariant())));
            }
            catch (InvalidOperationException)
            {
                Logger.Error(
                    $"No constructor on type {componentType} matches given properties: {string.Join(separator: ", ", properties.Keys)}");
                return;
            }

            var parameters = constructorInfo.GetParameters();
            var convertedParams = new object?[parameters.Length];
            foreach (var parameterInfo in parameters)
            {
                if (!properties.TryGetValue(parameterInfo.Name!.ToLowerInvariant(), out var property))
                {
                    if (parameterInfo.IsOptional)
                        // Optional parameter
                        convertedParams[parameterInfo.Position] = Type.Missing;
                    else if (IsNullable(parameterInfo))
                        // Default value for reference type
                        convertedParams[parameterInfo.Position] = null;
                    else
                        throw new InvalidOperationException(
                            $"Parameter {parameterInfo.Name} of component {componentType.Name} is mandatory but no value was given");
                    continue;
                }

                var type = parameterInfo.ParameterType;
                if (type == typeof(Texture2D))
                {
                    var texture = resourceProvider.Load<Texture2D>(property);
                    convertedParams[parameterInfo.Position] = texture;
                }
                else if (type.IsSubclassOf(typeof(Style)) || type == typeof(Style))
                {
                    var style = GetStyle(property, type) ?? (Style) Activator.CreateInstance(type)!;
                    convertedParams[parameterInfo.Position] = style;
                }
                else
                {
                    var conv = Convert.ChangeType(property, type);
                    convertedParams[parameterInfo.Position] = conv;
                }

                Logger.Trace($@"Setting property {parameterInfo.Name} = {property}.");
            }

            var parameterNames = parameters.Select(p => p.Name!.ToLowerInvariant());
            var unusedProperties = properties.Select(p => p.Key).Where(p => !parameterNames.Contains(p)).ToList();
            if (unusedProperties.Count > 0)
                Logger.Warn($"Properties ({string.Join(separator: ", ", unusedProperties)}) are unused.");

            var component = (Component) Activator.CreateInstance(componentType,
                BindingFlags.Default | BindingFlags.OptionalParamBinding, binder: null, convertedParams,
                CultureInfo.CurrentCulture)!;

            if (parent is null)
                scene.AddComponent(component);
            else
                parent.AddChild(component);

            foreach (var xChild in xElement.Elements()) ProcessComponent(xChild, (View) component);
        }

        foreach (var xElement in xDraw.Elements())
            ProcessComponent(xElement);
        return scene;
    }

    private Style? GetStyle(string styleId, Type styleType)
    {
        if (!styles!.TryGetValue(styleId, out var style))
        {
            if (xStyles is null)
            {
                Logger.Error(
                    $@"Can not find style ""{styleId}"": Scene does not contain a ""{STYLES_ELEMENT}"" element.");
                return null;
            }

            try
            {
                var xStyle = xStyles.Elements().First(elem =>
                    elem.StringAttribute(ID_ATTRIBUTE, out var id) && id == styleId);
                style = (Style) Activator.CreateInstance(styleType, xStyle, values)!;
                styles.Add(styleId, style);
                Logger.Trace($@"Created style ""{styleId}""");
            }
            catch (InvalidOperationException)
            {
                Logger.Error($"Style {styleId} is not defined in {STYLES_ELEMENT} element.");
                return null;
            }
        }
        else
        {
            // Important! Create a shallow copy of the style.
            style = style with { };
        }

        return style;
    }

    public static bool IsNullable(ParameterInfo parameter) =>
        IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

    private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType,
        IEnumerable<CustomAttributeData> customAttributes)
    {
        if (memberType.IsValueType)
            return Nullable.GetUnderlyingType(memberType) != null;

        var nullable = customAttributes
            .FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        if (nullable != null && nullable.ConstructorArguments.Count == 1)
        {
            var attributeArgument = nullable.ConstructorArguments[0];
            if (attributeArgument.ArgumentType == typeof(byte[]))
            {
                var args = (ReadOnlyCollection<CustomAttributeTypedArgument>) attributeArgument.Value!;
                if (args.Count > 0 && args[0].ArgumentType == typeof(byte)) return (byte) args[0].Value! == 2;
            }
            else if (attributeArgument.ArgumentType == typeof(byte))
            {
                return (byte) attributeArgument.Value! == 2;
            }
        }

        for (var type = declaringType; type != null; type = type.DeclaringType)
        {
            var context = type.CustomAttributes
                .FirstOrDefault(x =>
                    x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");
            if (context != null &&
                context.ConstructorArguments.Count == 1 &&
                context.ConstructorArguments[0].ArgumentType == typeof(byte))
                return (byte) context.ConstructorArguments[0].Value! == 2;
        }

        // Couldn't find a suitable attribute
        return false;
    }
}