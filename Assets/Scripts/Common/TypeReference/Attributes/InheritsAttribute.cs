namespace TypeReferences
{
    using System;

    /// <summary>
    /// Constraint that allows selection of types that inherit a specific parent type or interface when
    /// selecting a <see cref="TypeReference"/> with the Unity inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class InheritsAttribute : TypeOptionsAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InheritsAttribute"/> class.
        /// </summary>
        /// <param name="baseType">Type that selectable types must inherit from.</param>
        public InheritsAttribute(Type baseType)
        {
			this.BaseType = baseType;
        }

        /// <summary>
        /// Gets the type that selectable types must derive from.
        /// </summary>
        public Type BaseType { get; private set; }

        /// <summary>
        /// Allows to choose the base type from the drop-down as well.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool IncludeBaseType { get; set; } = false;

        /// <summary>
        /// Allows abstract classes and interfaces to be selected from drop-down.
        /// Defaults to a value of <c>false</c> unless explicitly specified.
        /// </summary>
        public bool AllowAbstract { get; set; } = false;

        /// <inheritdoc/>
        public override bool MatchesRequirements(Type type)
        {
            if (type == this.BaseType && !this.IncludeBaseType)
            {
                return false;
            }

            // Include base type in the drop-down even if it is abstract.
            // If the user set IncludeBaseType to true, they probably want to include the base type in the dropdown
            // even though it is abstract.
            if (type == this.BaseType)
                return true;

            bool passesAbstractConstraint = this.AllowAbstract || !type.IsAbstract;

            return this.BaseType.IsAssignableFrom(type) && passesAbstractConstraint && base.MatchesRequirements(type);
        }
    }
}