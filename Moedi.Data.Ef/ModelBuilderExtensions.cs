using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Moedi.Data.Ef
{
    public static class ModelBuilderExtensions
    {
        public static void RegisterEntities(this ModelBuilder builder, Type[] contextTypes)
        {
            var entries = new List<IEntityType>();
            foreach (var ctxType in contextTypes)
            {
                using (var context = (MoediDbContext)Activator.CreateInstance(ctxType))
                {
                    entries.AddRange(context.Model.GetEntityTypes());
                }
            }

            var entityTypes = entries
                .Select(x => (x.ClrType.Name, x.ClrType, x.GetSchema(), x.GetTableName()))
                .ToList();

            var copies = entityTypes
                .GroupBy(x => x.Name)
                .Select(x => new { x.Key, Count = x.Count() })
                .Where(x => x.Count > 1)
                .Select(x => x.Key)
                .ToList();

            if (copies.Count > 0)
                throw new DuplicateNameException("Duplicate entities found: " + string.Join(',', copies));

            foreach (var (_, type, schema, tableName) in entityTypes)
            {
                var etBuilder = builder.Entity(type);
                etBuilder.Metadata.SetOrRemoveAnnotation(RelationalAnnotationNames.Schema, schema);
                etBuilder.Metadata.SetOrRemoveAnnotation(RelationalAnnotationNames.TableName, tableName);
            }
        }
    }
}