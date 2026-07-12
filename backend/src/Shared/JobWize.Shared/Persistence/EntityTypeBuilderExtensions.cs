using JobWize.Shared.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace JobWize.Shared.Persistence
{
    public static class EntityTypeBuilderExtensions
    {
        public static EntityTypeBuilder<TEntity> ConfigureEntityBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : Entity
        {
            builder.HasKey(entity => entity.Id);

            return builder;
        }
        
        public static EntityTypeBuilder<TEntity> ConfigureDomainModelBase<TEntity>(this EntityTypeBuilder<TEntity> builder)
            where TEntity : DomainModel
        {
            builder.ConfigureEntityBase();

            builder.Property(entity => entity.CreatedAt)
                .IsRequired();

            builder.Property(entity => entity.UpdatedAt);

            builder.Property(entity => entity.DeletedAt);

            return builder;
        }
    }
}
