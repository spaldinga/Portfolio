/***************************************************************
  cleanup_entity_versions.sql (anonymized)
  - Purpose: preview and remove duplicate EntityVersions rows (keeps oldest by CreatedAt)
    for Title IN ('Template A','Template B').
  - Deletes related child records in this order:
      1) EntityAssignment
      2) Entity
      3) EntityVersions
  - Drops a self-referencing FK (if present) during the delete, then re-creates it.
  - Archives deleted rows into corresponding *_Deleted tables.
  - Uses transaction, validation checks, and safe isolation.
  - NOTE: This is an anonymized example for portfolio use. Test on a copy before applying to production.
***************************************************************/

/* ----------  PREVIEW: run this first to inspect affected parent rows  ---------- */
PRINT '--- PREVIEW: Candidate EntityVersions (duplicates to be removed) ---';

;WITH cte AS (
    SELECT
        Id,
        [Title],
        [GroupKey],
        [CreatedAt],
        [Version],
        ROW_NUMBER() OVER (
            PARTITION BY [Title], [GroupKey]
            ORDER BY [CreatedAt] ASC, [Version] ASC, [Id] ASC
        ) AS rn,
        COUNT(*) OVER (PARTITION BY [Title], [GroupKey]) AS grpCount
    FROM dbo.EntityVersions
    WHERE [Title] IN ('Template A','Template B')
)
SELECT *
FROM cte
WHERE grpCount > 1 AND rn > 1
ORDER BY [Title], [GroupKey], [CreatedAt];

PRINT '--- END PREVIEW ---';
GO

/* ----------  TRANSACTIONAL DELETE (archive -> delete -> recreate FK)  ---------- */
SET XACT_ABORT ON;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

DECLARE @fkName sysname = N'FK_EntityVersions_ParentVersionId';
DECLARE @fkExists BIT = 0;

-- Determine if the self-referencing FK exists
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys
    WHERE name = @fkName
      AND referenced_object_id = OBJECT_ID('dbo.EntityVersions')
)
    SET @fkExists = 1;
ELSE
    SET @fkExists = 0;

BEGIN TRY
    -- Build list of parents -> entities -> assignments to delete (preview inside script)
    IF OBJECT_ID('tempdb..#ParentToDelete') IS NOT NULL DROP TABLE #ParentToDelete;
    IF OBJECT_ID('tempdb..#EntitiesToDelete') IS NOT NULL DROP TABLE #EntitiesToDelete;
    IF OBJECT_ID('tempdb..#AssignToDelete') IS NOT NULL DROP TABLE #AssignToDelete;

    ;WITH cte AS (
        SELECT
            Id,
            [Title],
            [GroupKey],
            [CreatedAt],
            [Version],
            ROW_NUMBER() OVER (
                PARTITION BY [Title], [GroupKey]
                ORDER BY [CreatedAt] ASC, [Version] ASC, [Id] ASC
            ) AS rn,
            COUNT(*) OVER (PARTITION BY [Title], [GroupKey]) AS grpCount
        FROM dbo.EntityVersions
        WHERE [Title] IN ('Template A','Template B')
    )
    SELECT Id, [Title], [GroupKey]
    INTO #ParentToDelete
    FROM cte
    WHERE grpCount > 1 AND rn > 1;

    DECLARE @ExpectedParent INT = (SELECT COUNT(*) FROM #ParentToDelete);

    IF @ExpectedParent = 0
    BEGIN
        PRINT 'No duplicate EntityVersions found for the specified titles. Nothing to delete.';
        -- cleanup
        IF OBJECT_ID('tempdb..#ParentToDelete') IS NOT NULL DROP TABLE #ParentToDelete;
        RETURN;
    END

    -- Entity rows referencing those parent versions
    SELECT DISTINCT e.Id, e.EntityVersionId
    INTO #EntitiesToDelete
    FROM dbo.[Entity] e
    JOIN #ParentToDelete p ON e.EntityVersionId = p.Id;

    DECLARE @ExpectedEntities INT = (SELECT COUNT(*) FROM #EntitiesToDelete);

    -- EntityAssignment rows referencing those Entity rows
    SELECT a.Id, a.EntityId
    INTO #AssignToDelete
    FROM dbo.EntityAssignment a
    JOIN #EntitiesToDelete s ON a.EntityId = s.Id;

    DECLARE @ExpectedAssign INT = (SELECT COUNT(*) FROM #AssignToDelete);

    PRINT 'Preview counts (will be deleted if you run the transactional block):';
    PRINT '  Parent EntityVersions to delete: ' + CAST(@ExpectedParent AS VARCHAR(20));
    PRINT '  Entity rows to delete: ' + CAST(@ExpectedEntities AS VARCHAR(20));
    PRINT '  EntityAssignment rows to delete: ' + CAST(@ExpectedAssign AS VARCHAR(20));

    -- Drop the self-referencing FK if it exists (so deletes can proceed). We'll re-create later.
    IF @fkExists = 1
    BEGIN
        PRINT 'Dropping self-referencing FK: ' + @fkName;
        IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = @fkName)
        BEGIN
            ALTER TABLE dbo.EntityVersions
            DROP CONSTRAINT [FK_EntityVersions_ParentVersionId];
        END
    END

    -- Ensure archive tables exist (create if missing)
    IF OBJECT_ID('dbo.EntityAssignment_Deleted', 'U') IS NULL
    BEGIN
        SELECT TOP (0) * INTO dbo.EntityAssignment_Deleted FROM dbo.EntityAssignment;
        ALTER TABLE dbo.EntityAssignment_Deleted
        ADD DeletedOn datetime2(7) NULL, DeletedBy sysname NULL;
    END

    IF OBJECT_ID('dbo.Entity_Deleted', 'U') IS NULL
    BEGIN
        SELECT TOP (0) * INTO dbo.Entity_Deleted FROM dbo.[Entity];
        ALTER TABLE dbo.Entity_Deleted
        ADD DeletedOn datetime2(7) NULL, DeletedBy sysname NULL;
    END

    IF OBJECT_ID('dbo.EntityVersions_Deleted', 'U') IS NULL
    BEGIN
        SELECT TOP (0) * INTO dbo.EntityVersions_Deleted FROM dbo.EntityVersions;
        ALTER TABLE dbo.EntityVersions_Deleted
        ADD DeletedOn datetime2(7) NULL, DeletedBy sysname NULL;
    END

    -- Begin transaction for deletes and validations
    BEGIN TRANSACTION;

    DECLARE @DeletedAssign INT = 0, @DeletedEntities INT = 0, @DeletedParent INT = 0;

    -- 1) Archive & delete EntityAssignment rows first
    IF @ExpectedAssign > 0
    BEGIN
        INSERT INTO dbo.EntityAssignment_Deleted
        SELECT a.*, SYSUTCDATETIME() AS DeletedOn, SUSER_SNAME() AS DeletedBy
        FROM dbo.EntityAssignment a
        JOIN #AssignToDelete atd ON a.Id = atd.Id;

        DELETE a
        FROM dbo.EntityAssignment a
        JOIN #AssignToDelete atd ON a.Id = atd.Id;

        SET @DeletedAssign = @@ROWCOUNT;

        IF @DeletedAssign <> @ExpectedAssign
        BEGIN
            THROW 65001, 'Deleted EntityAssignment count does not match expected count. Rolling back.', 1;
        END
    END

    -- 2) Archive & delete Entity rows next
    IF @ExpectedEntities > 0
    BEGIN
        INSERT INTO dbo.Entity_Deleted
        SELECT e.*, SYSUTCDATETIME() AS DeletedOn, SUSER_SNAME() AS DeletedBy
        FROM dbo.[Entity] e
        JOIN #EntitiesToDelete s ON e.Id = s.Id;

        DELETE e
        FROM dbo.[Entity] e
        JOIN #EntitiesToDelete s ON e.Id = s.Id;

        SET @DeletedEntities = @@ROWCOUNT;

        IF @DeletedEntities <> @ExpectedEntities
        BEGIN
            THROW 65002, 'Deleted Entity count does not match expected count. Rolling back.', 1;
        END
    END

    -- 3) Archive & delete EntityVersions (parents) last
    IF @ExpectedParent > 0
    BEGIN
        INSERT INTO dbo.EntityVersions_Deleted
        SELECT pv.*, SYSUTCDATETIME() AS DeletedOn, SUSER_SNAME() AS DeletedBy
        FROM dbo.EntityVersions pv
        JOIN #ParentToDelete p ON pv.Id = p.Id;

        DELETE pv
        FROM dbo.EntityVersions pv
        JOIN #ParentToDelete p ON pv.Id = p.Id;

        SET @DeletedParent = @@ROWCOUNT;

        IF @DeletedParent <> @ExpectedParent
        BEGIN
            THROW 65003, 'Deleted EntityVersions count does not match expected count. Rolling back.', 1;
        END
    END

    -- 4) Null out any ParentVersionId values that now reference deleted parents
    --    (so the FK can be re-created cleanly)
    IF @fkExists = 1
    BEGIN
        UPDATE dbo.EntityVersions
        SET ParentVersionId = NULL
        WHERE ParentVersionId IN (SELECT Id FROM #ParentToDelete);
    END

    -- 5) Re-create the self-referencing FK if it existed previously (use original definition)
    IF @fkExists = 1
    BEGIN
        PRINT 'Re-creating self-referencing FK: ' + @fkName;
        ALTER TABLE [dbo].[EntityVersions]  WITH CHECK ADD  CONSTRAINT [FK_EntityVersions_ParentVersionId] FOREIGN KEY([ParentVersionId])
        REFERENCES [dbo].[EntityVersions] ([Id]);
        ALTER TABLE [dbo].[EntityVersions] CHECK CONSTRAINT [FK_EntityVersions_ParentVersionId];
    END

    -- 6) Final verification: ensure no duplicate groups remain for the two titles
    IF EXISTS (
        SELECT 1
        FROM (
            SELECT [Title], [GroupKey], COUNT(*) AS cnt
            FROM dbo.EntityVersions
            WHERE [Title] IN ('Template A','Template B')
            GROUP BY [Title], [GroupKey]
            HAVING COUNT(*) > 1
        ) t
    )
    BEGIN
        THROW 65004, 'Duplicate groups remain after delete. Rolling back.', 1;
    END

    COMMIT TRANSACTION;

    PRINT 'Success: Deleted and archived rows:';
    PRINT '  EntityAssignment deleted: ' + CAST(@DeletedAssign AS VARCHAR(20));
    PRINT '  Entity deleted: ' + CAST(@DeletedEntities AS VARCHAR(20));
    PRINT '  EntityVersions deleted: ' + CAST(@DeletedParent AS VARCHAR(20));

    -- Cleanup temp tables
    IF OBJECT_ID('tempdb..#AssignToDelete') IS NOT NULL DROP TABLE #AssignToDelete;
    IF OBJECT_ID('tempdb..#EntitiesToDelete') IS NOT NULL DROP TABLE #EntitiesToDelete;
    IF OBJECT_ID('tempdb..#ParentToDelete') IS NOT NULL DROP TABLE #ParentToDelete;

END TRY
BEGIN CATCH
    -- Attempt rollback and report error
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    DECLARE @ErrNum INT = ERROR_NUMBER();
    DECLARE @ErrState INT = ERROR_STATE();
    DECLARE @ErrLine INT = ERROR_LINE();

    -- Attempt to re-create FK if it originally existed but we rolled back after dropping it earlier.
    IF @fkExists = 1
    BEGIN
        BEGIN TRY
            IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = @fkName)
            BEGIN
                ALTER TABLE [dbo].[EntityVersions]  WITH CHECK ADD  CONSTRAINT [FK_EntityVersions_ParentVersionId] FOREIGN KEY([ParentVersionId])
                REFERENCES [dbo].[EntityVersions] ([Id]);
                ALTER TABLE [dbo].[EntityVersions] CHECK CONSTRAINT [FK_EntityVersions_ParentVersionId];
                PRINT 'FK re-created after rollback.';
            END
        END TRY
        BEGIN CATCH
            PRINT 'Warning: failed to re-create FK after rollback. Manual intervention may be required.';
        END CATCH
    END

    -- Cleanup temp tables (best effort)
    IF OBJECT_ID('tempdb..#AssignToDelete') IS NOT NULL DROP TABLE #AssignToDelete;
    IF OBJECT_ID('tempdb..#EntitiesToDelete') IS NOT NULL DROP TABLE #EntitiesToDelete;
    IF OBJECT_ID('tempdb..#ParentToDelete') IS NOT NULL DROP TABLE #ParentToDelete;

    RAISERROR('Error %d (state %d, line %d): %s', 16, 1, @ErrNum, @ErrState, @ErrLine, @ErrMsg);
END CATCH;
GO