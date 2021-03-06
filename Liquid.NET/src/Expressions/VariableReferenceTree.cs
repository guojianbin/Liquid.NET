﻿using System.Collections.Generic;
using System.Linq;
using Liquid.NET.Constants;
using Liquid.NET.Symbols;
using Liquid.NET.Utils;

namespace Liquid.NET.Expressions
{
    public class VariableReferenceTree : IExpressionDescription
    {

        public VariableReferenceTree Parent { get; set; }

        public IExpressionDescription Value { get; set; }

        public IExpressionDescription IndexExpression { get; set; }


//        public void Accept(IExpressionDescriptionVisitor expressionDescriptionVisitor)
//        {
//            throw new NotImplementedException("");
//        }

        public LiquidExpressionResult Eval(ITemplateContext templateContext, IEnumerable<Option<ILiquidValue>> childresults)
        {
            return EvalExpression(templateContext, this, childresults);
        }

        private LiquidExpressionResult EvalExpression(
            ITemplateContext templateContext, 
            VariableReferenceTree o,
            IEnumerable<Option<ILiquidValue>> childresults)
        {
            var childResultsList = childresults as IList<Option<ILiquidValue>> ?? childresults.ToList();
            var valueResult = o.Value.Eval(templateContext, childResultsList);
            if (valueResult.IsError)
            {
                return valueResult;
            }
            LiquidExpressionResult indexResult;
            if (o.IndexExpression != null)
            {
                indexResult = o.IndexExpression.Eval(templateContext, childResultsList);
                if (indexResult.IsError)
                {
                    return indexResult;
                }               
            }
            else
            {
                return valueResult;
            }
            if (!valueResult.SuccessResult.HasValue)
            {
                return LiquidExpressionResult.Success(new None<ILiquidValue>());
                //return LiquidExpressionResult.Error(SymbolTable.NotFoundError(valueResult));
            }
            if (!indexResult.SuccessResult.HasValue)
            {
                return LiquidExpressionResult.Success(new None<ILiquidValue>());
                //return LiquidExpressionResult.Error("ERROR: the index for "+valueResult.SuccessResult.Value+" has no value");
            }
            return new IndexDereferencer().Lookup(templateContext, valueResult.SuccessResult.Value, indexResult.SuccessResult.Value);
        }



    }
}
