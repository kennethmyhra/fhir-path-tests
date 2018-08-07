using Hl7.Fhir.ElementModel;
using Hl7.FhirPath;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FhirPathTests
{
    public static class IValueProviderFPExtensions2
    {
        private static Dictionary<string, CompiledExpression> _cache = new Dictionary<string, CompiledExpression>();
        private static List<string> _mruList = new List<string>();      // sigh, no sortedlist in NETSTANDARD 1.0
        private static Object _cacheLock = new Object();
        public static int MAX_FP_EXPRESSION_CACHE_SIZE = 500;

        private static CompiledExpression getCompiledExpression(string expression, FhirPathCompiler compiler = null)
        {
            lock (_cacheLock)
            {
                bool success = _cache.TryGetValue(expression, out CompiledExpression ce);

                if (!success)
                {
                    var internalCompiler = compiler ?? new FhirPathCompiler();
                    ce = internalCompiler.Compile(expression);

                    if (_cache.Count >= MAX_FP_EXPRESSION_CACHE_SIZE)
                    {
                        var lruExpression = _mruList.First();
                        _cache.Remove(lruExpression);
                        _mruList.Remove(lruExpression);
                    }

                    _cache.Add(expression, ce);
                }

                _mruList.Remove(expression);
                _mruList.Add(expression);

                return ce;
            }
        }

        public static IEnumerable<IElementNavigator> Select(this IElementNavigator input, string expression, EvaluationContext context = null, FhirPathCompiler compiler = null)
        {
            var evaluator = getCompiledExpression(expression, compiler);
            return evaluator(input, context ?? EvaluationContext.Default);
        }

        public static object Scalar(this IElementNavigator input, string expression, EvaluationContext context = null, FhirPathCompiler compiler = null)
        {
            var evaluator = getCompiledExpression(expression, compiler);
            return evaluator.Scalar(input, context ?? EvaluationContext.Default);
        }
    }
}
