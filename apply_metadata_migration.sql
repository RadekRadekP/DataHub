-- Apply AddMetadataCatalogTables Migration Manually
-- This script creates the MetaEntities, MetaFields, and MetaRelations tables

-- Check if migration has already been applied
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260110090000_AddMetadataCatalogTables')
BEGIN
    -- Create MetaEntities table
    CREATE TABLE [MetaEntities] (
        [Id] int NOT NULL IDENTITY,
        [EntityName] nvarchar(200) NOT NULL,
        [TableName] nvarchar(200) NOT NULL,
        [SchemaName] nvarchar(100) NOT NULL DEFAULT 'dbo',
        [DbContextName] nvarchar(200) NOT NULL,
        [ClrTypeName] nvarchar(500) NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NULL,
        [IsView] bit NOT NULL,
        [IsDiscovered] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MetaEntities] PRIMARY KEY ([Id])
    );

    -- Create MetaFields table
    CREATE TABLE [MetaFields] (
        [Id] int NOT NULL IDENTITY,
        [MetaEntityId] int NOT NULL,
        [FieldName] nvarchar(200) NOT NULL,
        [ColumnName] nvarchar(200) NOT NULL,
        [DisplayName] nvarchar(200) NOT NULL,
        [SqlType] nvarchar(100) NOT NULL,
        [ClrType] nvarchar(200) NOT NULL,
        [IsPrimaryKey] bit NOT NULL,
        [IsForeignKey] bit NOT NULL,
        [IsNullable] bit NOT NULL,
        [IsComputed] bit NOT NULL,
        [MaxLength] int NULL,
        [Precision] int NULL,
        [Scale] int NULL,
        [DefaultValue] nvarchar(max) NULL,
        [DisplayOrder] int NOT NULL,
        [IsVisibleInGrid] bit NOT NULL,
        [IsEditable] bit NOT NULL,
        [UiHint] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MetaFields] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MetaFields_MetaEntities_MetaEntityId] FOREIGN KEY ([MetaEntityId]) REFERENCES [MetaEntities] ([Id]) ON DELETE CASCADE
    );

    -- Create MetaRelations table
    CREATE TABLE [MetaRelations] (
        [Id] int NOT NULL IDENTITY,
        [FromEntityId] int NOT NULL,
        [FromFieldId] int NOT NULL,
        [ToEntityId] int NOT NULL,
        [ToFieldId] int NOT NULL,
        [DisplayFieldId] int NULL,
        [RelationType] nvarchar(50) NOT NULL DEFAULT 'ManyToOne',
        [DisplayName] nvarchar(200) NOT NULL,
        [DeleteBehavior] nvarchar(50) NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_MetaRelations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MetaRelations_MetaEntities_FromEntityId] FOREIGN KEY ([FromEntityId]) REFERENCES [MetaEntities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MetaRelations_MetaEntities_ToEntityId] FOREIGN KEY ([ToEntityId]) REFERENCES [MetaEntities] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MetaRelations_MetaFields_FromFieldId] FOREIGN KEY ([FromFieldId]) REFERENCES [MetaFields] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MetaRelations_MetaFields_ToFieldId] FOREIGN KEY ([ToFieldId]) REFERENCES [MetaFields] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_MetaRelations_MetaFields_DisplayFieldId] FOREIGN KEY ([DisplayFieldId]) REFERENCES [MetaFields] ([Id]) ON DELETE NO ACTION
    );

    -- Create indexes
    CREATE INDEX [IX_MetaFields_MetaEntityId] ON [MetaFields] ([MetaEntityId]);
    CREATE INDEX [IX_MetaRelations_FromEntityId] ON [MetaRelations] ([FromEntityId]);
    CREATE INDEX [IX_MetaRelations_ToEntityId] ON [MetaRelations] ([ToEntityId]);
    CREATE INDEX [IX_MetaRelations_FromFieldId] ON [MetaRelations] ([FromFieldId]);
    CREATE INDEX [IX_MetaRelations_ToFieldId] ON [MetaRelations] ([ToFieldId]);
    CREATE INDEX [IX_MetaRelations_DisplayFieldId] ON [MetaRelations] ([DisplayFieldId]);

    -- Record migration in history
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260110090000_AddMetadataCatalogTables', N'9.0.6');

    PRINT 'Migration 20260110090000_AddMetadataCatalogTables applied successfully.';
END
ELSE
BEGIN
    PRINT 'Migration 20260110090000_AddMetadataCatalogTables has already been applied.';
END
GO
