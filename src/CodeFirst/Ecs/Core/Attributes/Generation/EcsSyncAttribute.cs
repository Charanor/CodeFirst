using System.Diagnostics.CodeAnalysis;

namespace CodeFirst.Ecs.Core.Attributes.Generation;

/// <summary>
///     An attribute that marks the given property to be synchronized over the network. The property must be marked as
///     <c>partial</c> and must be of a serializable type:
///     <list type="bullet">
///         <item>
///             <c>(s)byte</c>
///         </item>
///         <item>
///             <c>(u)short</c>
///         </item>
///         <item>
///             <c>(u)int</c>
///         </item>
///         <item>
///             <c>float</c>
///         </item>
///         <item>
///             <c>double</c>
///         </item>
///         <item>
///             <c>Vector2</c>
///         </item>
///         <item>
///             <c>Vector3</c>
///         </item>
///     </list>
///     Custom types can be serialized but needs a serializer to manually be added to the <see cref="World" /> first.
/// </summary>
/// <remarks>
///     Only works when the <c>CodeFirst.Generators</c> package is installed. If that package is not installed, this
///     attribute does nothing.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
[Experimental("CF_ECS_0001")]
public class EcsSyncAttribute : Attribute;