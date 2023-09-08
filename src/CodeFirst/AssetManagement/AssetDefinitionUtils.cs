using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using CodeFirst.AssetManagement.Exceptions;
using CodeFirst.Utils.Logging;

namespace CodeFirst.AssetManagement;

public static class AssetDefinitionUtils
{
	private static readonly ILogger Logger = LoggingManager.Get(nameof(AssetDefinitionUtils));

	public static readonly string AssetDefinitionFileExtension = "xml";

	public static AssetDefinition? LoadAssetDefinition(Type assetDefinitionType, string assetPath)
	{
		using var _ = Logger.TraceScope($"Loading asset definition file for {assetPath}.");
		var assetDefinitionFilePath = $"{assetPath}.{AssetDefinitionFileExtension}";

		if (!File.Exists(assetDefinitionFilePath))
		{
			// No asset definition file, just use default definition.
			Logger.Debug($"No asset definition file for {assetPath} found.");
			return null;
		}

		try
		{
			using var fileStream = File.OpenRead(assetDefinitionFilePath);
			var xDocument = XDocument.Load(fileStream);
			var root = xDocument.Root;
			if (root == null)
			{
				Logger.Error($"Asset definition file {assetDefinitionFilePath} has invalid format: No root element.");
				return null;
			}

			var assetDefinition = CreateDefaultAssetDefinition(assetDefinitionType);
			if (assetDefinition == null)
			{
				Logger.Debug($"Could not create asset definition of type {assetDefinitionType}.");
				return null;
			}

			DeserializeAttributes(assetDefinition, root.Attributes());
			DeserializeElements(assetDefinition, root.Elements());

			Logger.Debug($"Parsed asset definition at {assetDefinitionFilePath} for asset {assetPath}.");
			return assetDefinition;
		}
		catch (Exception e)
		{
			// Could not load definition, just return null
			Logger.Error(e.ToString());
			return null;
		}
	}

	public static TAssetDefinition? LoadAssetDefinition<TAssetDefinition>(string assetPath)
		where TAssetDefinition : AssetDefinition =>
		(TAssetDefinition?)LoadAssetDefinition(typeof(TAssetDefinition), assetPath);

	private static void DeserializeAttributes(object obj, IEnumerable<XAttribute> attributes)
	{
		var type = obj.GetType();
		foreach (var xAttribute in attributes)
		{
			using var _ = Logger.TraceScope($"Reading xml attribute {xAttribute}");
			var attributeName = xAttribute.Name.LocalName;
			var propertyInfo = type.GetProperty(attributeName);
			if (propertyInfo == null)
			{
				var validProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Select(prop => $"({prop.Name}, {prop.PropertyType})");
				Logger.Error(
					$"No property {attributeName} exists on type {type}. Valid properties:\n{string.Join("\n    ", validProperties)}");
				continue;
			}

			var propertyType = propertyInfo.PropertyType;
			if (!TryDeserializeAttribute(propertyType, xAttribute, out var result))
			{
				Logger.Error($"Value {xAttribute.Value} is not convertable to expected type {propertyType}.");
				continue;
			}

			Logger.Trace($"Setting value to {result}.");
			propertyInfo.SetValue(obj, result);
		}
	}

	private static void DeserializeElements(object obj, IEnumerable<XElement> elements)
	{
		var type = obj.GetType();
		foreach (var xElement in elements)
		{
			using var _ = Logger.TraceScope($"Reading xml element {xElement}");
			var elementName = xElement.Name.LocalName;
			var propertyInfo = type.GetProperty(elementName);
			if (propertyInfo == null)
			{
				var validProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
					.Select(prop => $"({prop.Name}, {prop.PropertyType})");
				Logger.Error(
					$"No property {elementName} exists on type {type}. Valid properties:\n{string.Join("\n    ", validProperties)}");
				continue;
			}

			var propertyType = propertyInfo.PropertyType;
			if (!TryDeserializeElement(propertyType, xElement, out var result))
			{
				Logger.Error($"Element {xElement} could not be deserialized to type {propertyType}.");
				continue;
			}

			Logger.Trace($"Setting value to {result}.");
			propertyInfo.SetValue(obj, result);
		}
	}

	private static bool TryDeserializeElement(Type type, XElement xElement, [NotNullWhen(true)] out object? value)
	{
		using var _ = Logger.TraceScope($"Attempting to deserialize {xElement} to type {type}.");
		if (type == typeof(int))
		{
			if (!int.TryParse(xElement.Value, out var intResult))
			{
				value = default;
				return false;
			}

			value = intResult;
			return true;
		}

		if (type.IsEnum)
		{
			return Enum.TryParse(type, xElement.Value, ignoreCase: true, out value);
		}

		if (type.IsPrimitive)
		{
			Logger.Error($"No deserializer for {type}!");
			value = default;
			return false;
		}

		try
		{
			var instance = Activator.CreateInstance(type);
			if (instance == null)
			{
				Logger.Error(
					$"Could not create instance of {type}: {nameof(Activator.CreateInstance)} returned {null}.");
				value = default;
				return false;
			}

			DeserializeAttributes(instance, xElement.Attributes());
			DeserializeElements(instance, xElement.Elements());

			value = instance;
			return true;
		}
		catch (Exception e)
		{
			Logger.Error($"Could not create instance of {type}: {e}");
			value = default;
			return false;
		}
	}

	private static bool TryDeserializeAttribute(Type type, XAttribute xAttribute, [NotNullWhen(true)] out object? value)
	{
		using var _ = Logger.TraceScope($"Attempting to deserialize {xAttribute} to type {type}.");
		if (type == typeof(int))
		{
			if (!int.TryParse(xAttribute.Value, out var intResult))
			{
				value = default;
				return false;
			}

			value = intResult;
			return true;
		}

		if (type.IsEnum)
		{
			return Enum.TryParse(type, xAttribute.Value, ignoreCase: true, out value);
		}

		Logger.Error($"No deserializer for {type}!");
		value = default;
		return false;
	}

	private static AssetDefinition? CreateDefaultAssetDefinition(Type assetDefinitionType)
	{
		try
		{
			return Activator.CreateInstance(assetDefinitionType) as AssetDefinition;
		}
		catch (Exception e)
		{
			throw new InvalidAssetDefinitionFileException(assetDefinitionType, e);
		}
	}
}