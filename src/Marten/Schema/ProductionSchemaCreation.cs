using System;
using Baseline;

namespace Marten.Schema
{
    public class ProductionSchemaCreation : IDocumentSchemaCreation
    {
        public void CreateSchema(IDocumentSchema schema, DocumentMapping mapping)
        {
            throw new InvalidOperationException("No document storage exists for type {0} and cannot be created dynamically in production mode".ToFormat(mapping.DocumentType.FullName));
        }

        public void RunScript(string script)
        {
            throw new InvalidOperationException("Running DDL scripts are prohibited in production mode");
        }
    }
}