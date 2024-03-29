﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CodeFirst.Generators.Graphics.Utils;

namespace CodeFirst.Generators.Graphics.Parsing;

public class ShaderVisitor : GLSLBaseVisitor<IEnumerable<GLSLDefinition>>
{
	private const int NUM_BRACKETS_PER_ARRAY_RANK = 2;

	private static readonly ImmutableArray<string> ValidStorageQualifiers = new[]
	{
		"in",
		"out",
		"uniform"
	}.ToImmutableArray();

	public ShaderVisitor(ShaderType shaderVariant)
	{
		ShaderVariant = shaderVariant;
	}

	protected override IEnumerable<GLSLDefinition> DefaultResult => Enumerable.Empty<GLSLDefinition>();
	private static IEnumerable<GLSLDefinition> Empty => Enumerable.Empty<GLSLDefinition>();

	private ShaderType ShaderVariant { get; }

	public override IEnumerable<GLSLDefinition> VisitBasic_interface_block(
		GLSLParser.Basic_interface_blockContext context)
	{
		var storageCtx = context.interface_qualifier()
			.FirstOrDefault(qualifier => ValidStorageQualifiers.Contains(qualifier.GetText()));
		var storageType = GLSLUtils.StringToGLSLStorageType(storageCtx?.GetText());

		var type = context.IDENTIFIER().GetText();
		var identifier = context.instance_name()?.IDENTIFIER().GetText();
		var members = context.member_list().Accept(this).OfType<GLSLMember>();
		return Single(new GLSLInterface(ShaderVariant, storageType, type, identifier, members, ArrayRanks: 0,
			Array.Empty<string>(), Enumerable.Empty<string>()));
	}

	public override IEnumerable<GLSLDefinition> VisitSingle_declaration(GLSLParser.Single_declarationContext context)
	{
		var identifier = context.IDENTIFIER()?.GetText();
		var glslBrackets = context.array_specifier()?.GetText() ?? "";
		var identifierBrackets = GLSLUtils.GLSLToCSArrayBrackets(glslBrackets);
		var glslConstants = GLSLUtils.GLSLArraySizeConstants(glslBrackets);
		return Single(
			CreateMemberFromFullySpecifiedType(context.fully_specified_type(), identifier, identifierBrackets,
				glslConstants));
	}

	public override IEnumerable<GLSLDefinition> VisitVersion_statement(GLSLParser.Version_statementContext context)
	{
		// CAN BE NULL! If default version is specified!
		var number = context.INTCONSTANT()?.GetText();
		if (number is null)
		{
			return Empty;
		}

		var profile = context.IDENTIFIER()?.GetText();
		return Single(new GLSLVersion(ShaderVariant, number, profile));
	}

	public override IEnumerable<GLSLDefinition> VisitExtension_statement(GLSLParser.Extension_statementContext context)
	{
		var name = context.extension_name.Text;
		var status = context.extension_status.Text;
		return Single(new GLSLExtension(ShaderVariant, name, status));
	}

	public override IEnumerable<GLSLDefinition> VisitMember_declaration(GLSLParser.Member_declarationContext context)
	{
		var members = new List<GLSLDefinition>();
		for (var list = context.struct_declarator_list(); list != null; list = list.struct_declarator_list())
		{
			var declarator = list.struct_declarator();
			var identifier = declarator.IDENTIFIER().GetText();
			var glslBrackets = declarator.array_specifier()?.GetText() ?? "";
			var brackets = GLSLUtils.GLSLToCSArrayBrackets(glslBrackets);
			var glslConstants = GLSLUtils.GLSLArraySizeConstants(glslBrackets);
			var member =
				CreateMemberFromFullySpecifiedType(context.fully_specified_type(), identifier, brackets, glslConstants);
			members.Add(member);
		}

		return members;
	}

	protected override IEnumerable<GLSLDefinition> AggregateResult(IEnumerable<GLSLDefinition> aggregate,
		IEnumerable<GLSLDefinition> nextResult) => aggregate.Concat(nextResult);

	private static IEnumerable<GLSLDefinition> Single(GLSLDefinition definition) => new[] { definition };

	private GLSLMember CreateMemberFromFullySpecifiedType(GLSLParser.Fully_specified_typeContext fullySpecifiedType,
		string? identifier, string? identifierBrackets, IEnumerable<string>? identifierBracketsConstants)
	{
		var storageType =
			GLSLUtils.StringToGLSLStorageType(fullySpecifiedType.type_qualifier()?.storage_qualifier()?.GetText());

		var typeSpecifier = fullySpecifiedType.type_specifier();
		var typeSpecifierNonArray = typeSpecifier.type_specifier_nonarray();

		var bracketsText = typeSpecifier.array_specifier()?.GetText() ?? "";
		var typeBrackets = GLSLUtils.GLSLToCSArrayBrackets(bracketsText);
		var bracketsConstants = GLSLUtils.GLSLArraySizeConstants(bracketsText).ToList();
		var glslConstants = bracketsConstants.Where(GLSLUtils.IsGLSLConstant).ToList();

		var arrayRank = (typeBrackets.Length + identifierBrackets?.Length ?? 0) / NUM_BRACKETS_PER_ARRAY_RANK;

		var identifierBracketsConstantsList = identifierBracketsConstants?.ToList();
		if (identifierBracketsConstantsList != null)
		{
			glslConstants.AddRange(identifierBracketsConstantsList
				.Where(identifierBracketsConstant => identifierBracketsConstant != null &&
				                                     GLSLUtils.IsGLSLConstant(identifierBracketsConstant)));
		}

		var sizeGuesses = (identifierBracketsConstantsList ?? Enumerable.Empty<string>())
			.Concat(bracketsConstants)
			.ToArray();
		var structSpecifier = typeSpecifierNonArray.struct_specifier();
		if (structSpecifier is not null)
		{
			var structName = structSpecifier.IDENTIFIER()?.GetText();
			var members = structSpecifier.member_list().Accept(this).OfType<GLSLMember>();
			return new GLSLStruct(ShaderVariant, storageType, structName, identifier, members, arrayRank,
				sizeGuesses, glslConstants);
		}

		var type = typeSpecifierNonArray.GetText();
		return new GLSLMember(ShaderVariant, storageType, type, identifier, arrayRank, sizeGuesses,
			glslConstants);
	}
}