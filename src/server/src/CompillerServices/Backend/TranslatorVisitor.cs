﻿using System;
using System.Collections.Generic;
using Antlr4.Runtime.Tree;
using CompillerServices.Backend.Writers;
using SlangGrammar;

namespace CompillerServices.Backend
{
    internal class TranslatorVisitor : SlangBaseVisitor<object>, IDisposable
    {
        private readonly ISourceWriter _sourceWriter;

        public TranslatorVisitor(ISourceWriter sourceWriter)
        {
            _sourceWriter = sourceWriter;
        }

        public void Dispose()
        {
            _sourceWriter?.Dispose();
        }

        public override object VisitModuleImports(SlangParser.ModuleImportsContext context)
        {
            _sourceWriter.WriteImportBegin();

            foreach (ITerminalNode module in context.Id())
            {
                _sourceWriter.WriteImport(module.GetText());
            }

            _sourceWriter.WriteImportEnd();

            return base.VisitModuleImports(context);
        }

        public override object VisitModule(SlangParser.ModuleContext context)
        {
            _sourceWriter.WriteModuleBegin(context.Id().GetText());

            object result = base.VisitModule(context);

            _sourceWriter.WriteModuleEnd();

            return result;
        }

        public override object VisitFunc(SlangParser.FuncContext context)
        {
            ITerminalNode modifier = context.AccessModifier();
            ITerminalNode type = context.Type();
            ITerminalNode name = context.Id();

            ITerminalNode[] argTypes = context.argList().Type();
            ITerminalNode[] argNames = context.argList().Id();

            IList<FunctionArgument> arguments = new List<FunctionArgument>(argNames.Length);

            for (int i = 0; i < argNames.Length; i++)
            {
                arguments.Add(new FunctionArgument(argTypes[i].GetText(), argNames[i].GetText()));
            }

            _sourceWriter.WriteFunction(modifier.GetText(), type.GetText(), name.GetText(), arguments);

            return Visit(context.statementBlock());
        }

        public override object VisitProc(SlangParser.ProcContext context)
        {
            ITerminalNode modifier = context.AccessModifier();
            ITerminalNode name = context.Id();

            ITerminalNode[] argTypes = context.argList().Type();
            ITerminalNode[] argNames = context.argList().Id();

            IList<FunctionArgument> arguments = new List<FunctionArgument>(argNames.Length);

            for (int i = 0; i < argNames.Length; i++)
            {
                arguments.Add(new FunctionArgument(argTypes[i].GetText(), argNames[i].GetText()));
            }

            _sourceWriter.WriteProcedure(modifier.GetText(), name.GetText(), arguments);

            return Visit(context.statementBlock());
        }

        public override object VisitStatementBlock(SlangParser.StatementBlockContext context)
        {
            _sourceWriter.WriteBlockBegin();
            object result = base.VisitStatementBlock(context);
            _sourceWriter.WriteBlockEnd();

            return result;
        }

        public override object VisitDeclare(SlangParser.DeclareContext context)
        {
            ITerminalNode type = context.Type();
            ITerminalNode id = context.Id();

            _sourceWriter.WriteType(type.GetText());
            _sourceWriter.WriteIdentifier(id.GetText());

            if (context.mathExp() != null || context.boolOr() != null)
            {
                _sourceWriter.WriteAssign();
            }

            object result = base.VisitDeclare(context);
            _sourceWriter.WriteStatementEnd();

            return result;
        }

        public override object VisitLet(SlangParser.LetContext context)
        {
            ITerminalNode id = context.Id();
            _sourceWriter.WriteIdentifier(id.GetText());
            _sourceWriter.WriteAssign();

            object result = base.VisitLet(context);

            if (context.let() == null)
            {
                _sourceWriter.WriteStatementEnd();
            }

            return result;
        }

        public override object VisitInput(SlangParser.InputContext context)
        {
            ITerminalNode id = context.Id();
            _sourceWriter.WriteInput(id.GetText());

            return base.VisitInput(context);
        }

        public override object VisitOutput(SlangParser.OutputContext context)
        {
            _sourceWriter.WriteOutputBegin();
            object result = base.VisitOutput(context);
            _sourceWriter.WriteOutputEnd();

            return result;
        }

        public override object VisitReturn(SlangParser.ReturnContext context)
        {
            _sourceWriter.WriteReturnBegin();
            object result = base.VisitReturn(context);
            _sourceWriter.WriteReturnEnd();

            return result;
        }

        public override object VisitMathExpSum(SlangParser.MathExpSumContext context)
        {
            Visit(context.mathTerm());
            _sourceWriter.WriteSum();
            Visit(context.mathExp());

            return null;
        }

        public override object VisitMathExpSub(SlangParser.MathExpSubContext context)
        {
            Visit(context.mathTerm());
            _sourceWriter.WriteSubstraction();
            Visit(context.mathExp());

            return null;
        }

        public override object VisitMathTermMul(SlangParser.MathTermMulContext context)
        {
            Visit(context.mathFactor());
            _sourceWriter.WriteMultiply();
            Visit(context.mathTerm());

            return null;
        }

        public override object VisitMathTermDiv(SlangParser.MathTermDivContext context)
        {
            Visit(context.mathFactor());
            _sourceWriter.WriteDivision();
            Visit(context.mathTerm());

            return null;
        }

        public override object VisitMathTermMod(SlangParser.MathTermModContext context)
        {
            Visit(context.mathFactor());
            _sourceWriter.WriteMod();
            Visit(context.mathTerm());

            return null;
        }

        public override object VisitMathFactorBrackets(SlangParser.MathFactorBracketsContext context)
        {
            _sourceWriter.WriteBracketBegin();
            object result = Visit(context.mathExp());
            _sourceWriter.WriteBracketEnd();

            return result;
        }

        public override object VisitMathFactorUnaryPlus(SlangParser.MathFactorUnaryPlusContext context)
        {
            _sourceWriter.WritePlus();
            return base.VisitMathFactorUnaryPlus(context);
        }

        public override object VisitMathFactorUnaryMinus(SlangParser.MathFactorUnaryMinusContext context)
        {
            _sourceWriter.WriteMinus();
            return base.VisitMathFactorUnaryMinus(context);
        }

        public override object VisitMathAtom(SlangParser.MathAtomContext context)
        {
            _sourceWriter.WriteRaw(context.GetChild(0).GetText());
            return base.VisitMathAtom(context);
        }

        public override object VisitLogicOr(SlangParser.LogicOrContext context)
        {
            Visit(context.boolAnd());
            _sourceWriter.WriteLogicOr();
            Visit(context.boolOr());

            return null;
        }

        public override object VisitLogicAnd(SlangParser.LogicAndContext context)
        {
            Visit(context.boolEquality());
            _sourceWriter.WriteLogicAnd();
            Visit(context.boolAnd());

            return null;
        }

        public override object VisitBoolEqual(SlangParser.BoolEqualContext context)
        {
            Visit(context.boolInequality());
            _sourceWriter.WriteEquality();
            Visit(context.boolEquality());

            return null;
        }

        public override object VisitBoolNotEqual(SlangParser.BoolNotEqualContext context)
        {
            Visit(context.boolInequality());
            _sourceWriter.WriteInequality();
            Visit(context.boolEquality());

            return null;
        }

        public override object VisitMathEqual(SlangParser.MathEqualContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteEquality();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitMathNotEqual(SlangParser.MathNotEqualContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteEquality();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitBigger(SlangParser.BiggerContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteBigger();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitLesser(SlangParser.LesserContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteLesser();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitBiggerOrEqual(SlangParser.BiggerOrEqualContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteBiggerOrEqual();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitLesserOrEqual(SlangParser.LesserOrEqualContext context)
        {
            Visit(context.mathExp(0));
            _sourceWriter.WriteLesserOrEqual();
            Visit(context.mathExp(1));

            return null;
        }

        public override object VisitNot(SlangParser.NotContext context)
        {
            _sourceWriter.WriteNot();
            return base.VisitNot(context);
        }

        public override object VisitBoolAtomBrackets(SlangParser.BoolAtomBracketsContext context)
        {
            _sourceWriter.WriteBracketBegin();
            object result = base.VisitBoolAtomBrackets(context);
            _sourceWriter.WriteBracketEnd();

            return result;
        }

        public override object VisitBoolAtomBracketsNot(SlangParser.BoolAtomBracketsNotContext context)
        {
            _sourceWriter.WriteNot();
            _sourceWriter.WriteBracketBegin();
            object result = base.VisitBoolAtomBracketsNot(context);
            _sourceWriter.WriteBracketEnd();

            return result;
        }

        public override object VisitBoolAtom(SlangParser.BoolAtomContext context)
        {
            _sourceWriter.WriteRaw(context.GetChild(0).GetText());
            return base.VisitBoolAtom(context);
        }
    }
}