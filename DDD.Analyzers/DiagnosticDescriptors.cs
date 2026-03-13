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

		public static readonly DiagnosticDescriptor DddTypeMustDeclareBoundedContext = new DiagnosticDescriptor(
			id: "DDD010",
			title: "Tipo DDD debe declarar su contexto",
			messageFormat: "La clase '{0}' está decorada con [{1}] pero no declara su contexto. Usa [BoundedContext(\"NombreBC\")] si pertenece a un Bounded Context específico, o [SharedKernel] si es un concepto compartido.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Warning,
			isEnabledByDefault: true,
			description: "Las clases decoradas con [Entity], [AggregateRoot] o [ValueObject] deben declarar su contexto: [BoundedContext(\"NombreBC\")] para tipos de un BC específico, o [SharedKernel] para conceptos compartidos entre múltiples BCs.");

		public static readonly DiagnosticDescriptor NoCrossContextDirectReference = new DiagnosticDescriptor(
			id: "DDD011",
			title: "No se pueden referenciar AggregateRoots de otro Bounded Context directamente",
			messageFormat: "La clase '{0}' (BC: '{1}') referencia directamente el AggregateRoot '{2}' (BC: '{3}'). Entre BCs solo se pueden intercambiar identidades — usa '{2}Id' (el Id del agregado) en su lugar.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Entre Bounded Contexts distintos solo se deben intercambiar identidades (Ids) de Aggregate Roots, nunca referencias directas. Reemplaza la propiedad por el Id del Aggregate Root referenciado.");

		public static readonly DiagnosticDescriptor NoCrossContextEntityReference = new DiagnosticDescriptor(
			id: "DDD011",
			title: "No se pueden referenciar Entidades internas de otro Bounded Context",
			messageFormat: "La clase '{0}' (BC: '{1}') referencia la entidad interna '{2}' (BC: '{3}'). Las entidades internas no deben exponerse fuera de su Bounded Context — accede solo a través del AggregateRoot de '{3}'.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Las entidades internas de un Bounded Context son un detalle de implementación y no deben ser visibles desde otros BCs. Accede siempre a través del Aggregate Root del BC propietario.");

		public static readonly DiagnosticDescriptor NoCrossContextValueObjectReference = new DiagnosticDescriptor(
			id: "DDD011",
			title: "No se pueden referenciar ValueObjects de otro Bounded Context",
			messageFormat: "La clase '{0}' (BC: '{1}') referencia el ValueObject '{2}' (BC: '{3}'). Los ValueObjects de otro BC deben copiarse o abstraerse en el BC propio — no referenciarlos directamente.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Error,
			isEnabledByDefault: true,
			description: "Los Value Objects pertenecen al BC que los define. Si necesitas su concepto en otro BC, crea tu propia representación local o muévelo a SharedKernel.");

		public static readonly DiagnosticDescriptor CrossContextInternalUsage = new DiagnosticDescriptor(
			id: "DDD012",
			title: "Uso interno de tipo de otro Bounded Context",
			messageFormat: "El campo privado '{0}' en '{1}' (BC: '{2}') usa el tipo '{3}' (BC: '{4}'). Considera reemplazarlo por el Id del tipo para reducir el acoplamiento entre BCs.",
			category: Category,
			defaultSeverity: DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Los campos privados que usan tipos de otros BCs generan acoplamiento implícito. Aunque no es un error, considera usar solo el Id del tipo referenciado.");
	}
}
