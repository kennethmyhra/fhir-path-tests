using Hl7.Fhir.ElementModel;
using Hl7.FhirPath.Expressions;
using System.Collections.Generic;

namespace FhirPathTests
{
    public class CustomFhirPathFunctions
    {
        public static SymbolTable GetSymbolTable()
        {
            SymbolTable symbolTable = new SymbolTable().AddStandardFP();

            symbolTable.Add("join", (IEnumerable<IElementNavigator> f, string separator) =>
            {
                string output = null;
                foreach (IElementNavigator navigator in f)
                {
                    if (output != null) output += separator;
                    output += navigator.Value;
                }

                return output;
            });

            return symbolTable;
        }
    }
}
