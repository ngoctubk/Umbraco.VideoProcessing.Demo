CREATE DATABASE MediaProcessMetadata;
GO

USE MediaProcessMetadata;
GO

SET QUOTED_IDENTIFIER ON;
GO

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE TABLE [VideoEvents] (
        [Id] uniqueidentifier NOT NULL,
        [VideoId] uniqueidentifier NOT NULL,
        [VideoPath] nvarchar(250) NOT NULL,
        [Event] nvarchar(100) NOT NULL,
        [EventDate] datetime2 NOT NULL,
        CONSTRAINT [PK_VideoEvents] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE TABLE [Videos] (
        [Id] uniqueidentifier NOT NULL,
        [VideoPath] nvarchar(250) NOT NULL,
        [IsExtracted] bit NOT NULL,
        [IsCut] bit NOT NULL,
        [RawMetadata] nvarchar(max) NULL,
        [SuccessEventId] uniqueidentifier NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NULL,
        CONSTRAINT [PK_Videos] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_VideoEvents_Event] ON [VideoEvents] ([Event]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_VideoEvents_EventDate] ON [VideoEvents] ([EventDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_VideoEvents_VideoId] ON [VideoEvents] ([VideoId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_VideoEvents_VideoPath] ON [VideoEvents] ([VideoPath]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_Videos_CreatedDate] ON [Videos] ([CreatedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_Videos_IsCut] ON [Videos] ([IsCut]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_Videos_IsExtracted] ON [Videos] ([IsExtracted]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_Videos_ModifiedDate] ON [Videos] ([ModifiedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    CREATE INDEX [IX_Videos_VideoPath] ON [Videos] ([VideoPath]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701021206_InitDb'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240701021206_InitDb', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701031808_SetModifiedDateIsRequired'
)
BEGIN
    DROP INDEX [IX_Videos_ModifiedDate] ON [Videos];
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Videos]') AND [c].[name] = N'ModifiedDate');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Videos] DROP CONSTRAINT [' + @var0 + '];');
    EXEC(N'UPDATE [Videos] SET [ModifiedDate] = ''0001-01-01T00:00:00.0000000'' WHERE [ModifiedDate] IS NULL');
    ALTER TABLE [Videos] ALTER COLUMN [ModifiedDate] datetime2 NOT NULL;
    ALTER TABLE [Videos] ADD DEFAULT '0001-01-01T00:00:00.0000000' FOR [ModifiedDate];
    CREATE INDEX [IX_Videos_ModifiedDate] ON [Videos] ([ModifiedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240701031808_SetModifiedDateIsRequired'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240701031808_SetModifiedDateIsRequired', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    DROP INDEX [IX_VideoEvents_VideoId] ON [VideoEvents];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Videos]') AND [c].[name] = N'SuccessEventId');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Videos] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [Videos] DROP COLUMN [SuccessEventId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[VideoEvents]') AND [c].[name] = N'VideoId');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [VideoEvents] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [VideoEvents] DROP COLUMN [VideoId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    CREATE TABLE [ProcessingTasks] (
        [Id] uniqueidentifier NOT NULL,
        [VideoPath] nvarchar(250) NOT NULL,
        [TaskName] nvarchar(100) NOT NULL,
        [IsDone] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ModifiedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_ProcessingTasks] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    CREATE INDEX [IX_ProcessingTasks_CreatedDate] ON [ProcessingTasks] ([CreatedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    CREATE INDEX [IX_ProcessingTasks_ModifiedDate] ON [ProcessingTasks] ([ModifiedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    CREATE INDEX [IX_ProcessingTasks_TaskName] ON [ProcessingTasks] ([TaskName]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    CREATE INDEX [IX_ProcessingTasks_VideoPath] ON [ProcessingTasks] ([VideoPath]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704030957_AddProcessingTask'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240704030957_AddProcessingTask', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704080904_AddEventMessage'
)
BEGIN
    ALTER TABLE [VideoEvents] ADD [EventMessage] nvarchar(400) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704080904_AddEventMessage'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240704080904_AddEventMessage', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704084032_AddTaskContent'
)
BEGIN
    ALTER TABLE [ProcessingTasks] ADD [TaskContent] nvarchar(400) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704084032_AddTaskContent'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240704084032_AddTaskContent', N'8.0.6');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704091833_AddMediaPartPath'
)
BEGIN
    ALTER TABLE [ProcessingTasks] ADD [MediaPartPath] nvarchar(250) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704091833_AddMediaPartPath'
)
BEGIN
    ALTER TABLE [ProcessingTasks] ADD [Resolution] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704091833_AddMediaPartPath'
)
BEGIN
    CREATE INDEX [IX_ProcessingTasks_MediaPartPath] ON [ProcessingTasks] ([MediaPartPath]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240704091833_AddMediaPartPath'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240704091833_AddMediaPartPath', N'8.0.6');
END;
GO

COMMIT;
GO