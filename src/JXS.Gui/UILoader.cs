using System.Collections.ObjectModel;
using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using JXS.AssetManagement;
using JXS.Graphics.Core;
using JXS.Graphics.Text;
using JXS.Graphics.Text.Assets;
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
	private readonly IGuiInputProvider guiInputProvider;

	private Dictionary<string, Style> styles;
	private Dictionary<string, string> values;

	private XElement? xStyles;

	static UILoader()
	{
		BuiltinComponents = new Dictionary<string, Type>();
		// Find all loaded subclasses of "Component" and map them.
		var subclasses = AppDomain.CurrentDomain.GetAssemblies()
			.SelectMany(assembly => assembly.GetTypes())
			.Where(type => type.IsSubclassOf(typeof(Component)));
		foreach (var subclass in subclasses)
		{
			BuiltinComponents.Add(subclass.Name, subclass);
		}

		Assets.AddAssetResolver(new FontAssetResolver());
	}

	public UILoader(IGraphicsProvider graphicsProvider, IGuiInputProvider guiInputProvider)
	{
		this.graphicsProvider = graphicsProvider;
		this.guiInputProvider = guiInputProvider;

		styles = new Dictionary<string, Style>();
		values = new Dictionary<string, string>();
	}

	public Scene LoadSceneFromXml(XDocument document)
	{
		var scene = new Scene(graphicsProvider, guiInputProvider);

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

		foreach (var xElement in xDraw.Elements())
		{
			ProcessComponent(xElement);
		}

		return scene;

		void ProcessComponent(XElement xElement, View? parent = null)
		{
			Logger.Trace($@"Processing component {xElement.Name.LocalName}");
			if (!BuiltinComponents.TryGetValue(xElement.Name.LocalName, out var componentType))
			{
				Logger.Error($@"Could not find component ""{xElement.Name.LocalName}"".");
				return;
			}

			var properties = xElement.Attributes().ToDictionary(
				xAttribute => xAttribute.Name.LocalName.ToLowerInvariant(),
				xAttribute => xAttribute.Value);
			var textValue = xElement.Nodes().OfType<XText>().FirstOrDefault();
			if (textValue != null && textValue.Value.Length > 0 && !properties.ContainsKey(VALUE_PROPERTY))
			{
				properties[VALUE_PROPERTY] = textValue.Value.Trim();
			}

			ConstructorInfo constructorInfo;
			try
			{
				// Find all constructors that can be initialized with the given properties and order them by:
				// First: The ones with the most given parameters
				// Second: The ones with the highest number of parameters in total
				var constructors = componentType
					.GetConstructors()
					.OrderByDescending(c => c.GetParameters()
						.Count(param => properties.ContainsKey(param.Name!.ToLowerInvariant())))
					.ThenByDescending(c => c.GetParameters().Length)
					.ToList();

				// Parameters that are not optional, not nullable, and not a value type must be initialized
				constructorInfo = constructors.FirstOrDefault(c => c.GetParameters()
					.Where(p => !p.IsOptional && !IsNullable(p) && !p.ParameterType.IsValueType)
					.All(p => properties.ContainsKey(p.Name!.ToLowerInvariant()))
				) ?? componentType.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException();
			}
			catch (InvalidOperationException)
			{
				Logger.Error(
					$"No constructor on type {componentType} matches given properties: {string.Join(", ", properties.Keys)}");
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
					{
						convertedParams[parameterInfo.Position] = Type.Missing;
					}
					else if (IsNullable(parameterInfo))
						// Default value for reference type
					{
						convertedParams[parameterInfo.Position] = null;
					}
					else
					{
						throw new InvalidOperationException(
							$"Parameter {parameterInfo.Name} of component {componentType.Name} is mandatory but no value was given");
					}

					continue;
				}

				convertedParams[parameterInfo.Position] = ConvertProperty(parameterInfo.ParameterType, property);

				Logger.Trace($@"Setting property {parameterInfo.Name} = {property}.");
			}

			var parameterNames = parameters.Select(p => p.Name!.ToLowerInvariant());
			var unusedProperties = properties.Select(p => p.Key).Where(p => !parameterNames.Contains(p)).ToList();
			if (unusedProperties.Count > 0)
			{
				Logger.Trace($"Properties ({string.Join(", ", unusedProperties)}) not used in constructor.");
			}

			var component = (Component)Activator.CreateInstance(componentType,
				BindingFlags.Default | BindingFlags.OptionalParamBinding, binder: null, convertedParams,
				CultureInfo.CurrentCulture)!;

			var writableProperties = componentType.GetProperties().Where(prop => prop.CanWrite).ToArray();
			for (var i = unusedProperties.Count - 1; i >= 0; i--)
			{
				var propertyName = unusedProperties[i];
				foreach (var propertyInfo in writableProperties)
				{
					if (!propertyInfo.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
					{
						continue;
					}

					Logger.Trace($"Wrote property {propertyName} to component.{propertyInfo.Name}.");
					var value = properties[propertyName];
					propertyInfo.SetValue(component, ConvertProperty(propertyInfo.PropertyType, value));
					unusedProperties.RemoveAt(i);
					break;
				}
			}

			if (unusedProperties.Count > 0)
			{
				Logger.Trace($"Properties ({string.Join(", ", unusedProperties)}) are unused.");
			}

			if (parent is null)
			{
				scene.AddComponent(component);
			}
			else
			{
				parent.AddChild(component);
			}

			foreach (var xChild in xElement.Elements())
			{
				ProcessComponent(xChild, (View)component);
			}
		}
	}

	private object ConvertProperty(Type type, string property)
	{
		if (type == typeof(Texture2D))
		{
			return Assets.Get<Texture2D>(property);
		}

		if (type == typeof(Font))
		{
			return Assets.Get<Font>(property);
		}

		if (type.IsSubclassOf(typeof(Style)) || type == typeof(Style))
		{
			return GetStyle(property, type) ?? (Style)Activator.CreateInstance(type)!;
		}

		return Convert.ChangeType(property, type);
	}

	private Style? GetStyle(string styleId, Type styleType)
	{
		if (styles.TryGetValue(styleId, out var style))
		{
			// Important! Create a shallow copy of the style.
			return style with { };
		}

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
			style = (Style)Activator.CreateInstance(styleType, xStyle, values)!;
			styles.Add(styleId, style);
			Logger.Trace($@"Created style ""{styleId}""");
		}
		catch (InvalidOperationException)
		{
			Logger.Error($"Style {styleId} is not defined in {STYLES_ELEMENT} element.");
			return null;
		}

		return style;
	}

	public static bool IsNullable(ParameterInfo parameter) =>
		IsNullableHelper(parameter.ParameterType, parameter.Member, parameter.CustomAttributes);

	private static bool IsNullableHelper(Type memberType, MemberInfo? declaringType,
		IEnumerable<CustomAttributeData> customAttributes)
	{
		if (memberType.IsValueType)
		{
			return Nullable.GetUnderlyingType(memberType) != null;
		}

		var nullable = customAttributes
			.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
		if (nullable != null && nullable.ConstructorArguments.Count == 1)
		{
			var attributeArgument = nullable.ConstructorArguments[0];
			if (attributeArgument.ArgumentType == typeof(byte[]))
			{
				var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
				if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
				{
					return (byte)args[0].Value! == 2;
				}
			}
			else if (attributeArgument.ArgumentType == typeof(byte))
			{
				return (byte)attributeArgument.Value! == 2;
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
			{
				return (byte)context.ConstructorArguments[0].Value! == 2;
			}
		}

		// Couldn't find a suitable attribute
		return false;
	}
}