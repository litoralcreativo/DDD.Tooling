using Microsoft.CodeAnalysis;

namespace DDD.Analyzers
{
	/// <summary>
	/// Descriptores de diagnósticos para los analizadores DDD
	/// </summary>
	public static class DiagnosticDescriptors
	{
		private const string Category = "DDD.Design";

		public static readonly DiagnosticDescriptor EntityMustHaveEntityId = new DiagnosticDescriptor(
			id: "DDD001",
			title: "Entity debe tener una propiedad con EntityId",
			messageFormat: "La clase '{0}' está decorada con [Entity] pero no tiene ninguna propiedad decorada con [EntityId]",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Todas las entidades deben tener al menos una propiedad identificadora decorada con [EntityId].");

		public static readonly DiagnosticDescriptor AggregateRootMustHaveEntityId = new DiagnosticDescriptor(
			id: "DDD002",
			title: "AggregateRoot debe tener una propiedad con EntityId",
			messageFormat: "La clase '{0}' está decorada con [AggregateRoot] pero no tiene ninguna propiedad decorada con [EntityId]",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Todos los agregados raíz deben tener al menos una propiedad identificadora decorada con [EntityId].");

		public static readonly DiagnosticDescriptor EntityIdOnlyOnProperties = new DiagnosticDescriptor(
			id: "DDD003",
			title: "EntityId solo puede aplicarse a propiedades",
			messageFormat: "El atributo [EntityId] debe aplicarse solo a propiedades, no a '{0}'",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "El atributo EntityId debe aplicarse únicamente a propiedades.");

		public static readonly DiagnosticDescriptor ValueObjectShouldBeImmutable = new DiagnosticDescriptor(
			id: "DDD004",
			title: "ValueObject debe ser inmutable",
			messageFormat: "La propiedad '{0}' en el ValueObject '{1}' tiene un setter público. Los ValueObjects deben ser inmutables.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Los Value Objects deben ser inmutables y no tener setters públicos en sus propiedades.");

		public static readonly DiagnosticDescriptor CannotBeEntityAndValueObject = new DiagnosticDescriptor(
			id: "DDD005",
			title: "Una clase no puede ser Entity y ValueObject simultáneamente",
			messageFormat: "La clase '{0}' está decorada con [Entity] y [ValueObject]. Debe ser uno u otro, no ambos.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Una clase no puede ser Entity y ValueObject al mismo tiempo.");

		public static readonly DiagnosticDescriptor CannotBeAggregateRootAndValueObject = new DiagnosticDescriptor(
			id: "DDD006",
			title: "Una clase no puede ser AggregateRoot y ValueObject simultáneamente",
			messageFormat: "La clase '{0}' está decorada con [AggregateRoot] y [ValueObject]. Debe ser uno u otro, no ambos.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Una clase no puede ser AggregateRoot y ValueObject al mismo tiempo.");

		public static readonly DiagnosticDescriptor ValueObjectMustOverrideEquals = new DiagnosticDescriptor(
			id: "DDD007",
			title: "ValueObject debe sobrescribir Equals",
			messageFormat: "La clase '{0}' está decorada con [ValueObject] pero no sobrescribe el método Equals(object). Los ValueObjects deben compararse por valor.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Los Value Objects deben sobrescribir Equals para implementar igualdad por valor.");

		public static readonly DiagnosticDescriptor ValueObjectMustOverrideGetHashCode = new DiagnosticDescriptor(
			id: "DDD008",
			title: "ValueObject debe sobrescribir GetHashCode",
			messageFormat: "La clase '{0}' está decorada con [ValueObject] pero no sobrescribe el método GetHashCode(). Los ValueObjects deben implementar GetHashCode consistente con Equals.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Los Value Objects deben sobrescribir GetHashCode para mantener consistencia con Equals.");

		public static readonly DiagnosticDescriptor EntityShouldUseFactoryMethod = new DiagnosticDescriptor(
			id: "DDD009",
			title: "Entity debería usar Factory Method",
			messageFormat: "La clase '{0}' tiene constructor público. Considera usar un Factory Method estático (ej: Create) y hacer el constructor privado para mejor encapsulación.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Las Entities con constructores públicos pueden beneficiarse de Factory Methods para mejor control de creación y validación centralizada.");
	}
}
