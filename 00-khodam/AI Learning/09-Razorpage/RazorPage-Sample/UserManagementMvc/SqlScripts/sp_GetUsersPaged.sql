/*
    این Stored Procedure برای لیست کاربران استفاده می‌شود.
    قابلیت‌های آن:
    1) جستجو روی نام، نام خانوادگی، ایمیل و تلفن
    2) فیلتر شهر
    3) مرتب‌سازی
    4) صفحه‌بندی

    نکته آموزشی:
    در این پروژه، Repository این Stored Procedure را صدا می‌زند
    تا یاد بگیری چطور می‌توان ADO.NET و EF Core را کنار هم استفاده کرد.
*/

CREATE OR ALTER PROCEDURE dbo.sp_GetUsersPaged
    @SearchTerm NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @SortField NVARCHAR(50) = N'Id',
    @SortDirection NVARCHAR(4) = N'asc',
    @Page INT = 1,
    @PageSize INT = 5
AS
BEGIN
    SET NOCOUNT ON;

    IF (@Page < 1) SET @Page = 1;
    IF (@PageSize < 1) SET @PageSize = 5;

    ;WITH FilteredUsers AS
    (
        SELECT
            Id,
            Name,
            Family,
            BirthDate,
            City,
            Address,
            Email,
            Tel
        FROM Users
        WHERE
            (ISNULL(@SearchTerm, N'') = N''
             OR Name LIKE N'%' + @SearchTerm + N'%'
             OR Family LIKE N'%' + @SearchTerm + N'%'
             OR Email LIKE N'%' + @SearchTerm + N'%'
             OR Tel LIKE N'%' + @SearchTerm + N'%')
            AND
            (ISNULL(@City, N'') = N'' OR City = @City)
    )
    SELECT
        Id,
        Name,
        Family,
        BirthDate,
        City,
        Email,
        Tel,
        COUNT(1) OVER() AS TotalCount
    FROM FilteredUsers
    ORDER BY
        CASE WHEN @SortField = N'Id' AND @SortDirection = N'asc' THEN Id END ASC,
        CASE WHEN @SortField = N'Id' AND @SortDirection = N'desc' THEN Id END DESC,
        CASE WHEN @SortField = N'Name' AND @SortDirection = N'asc' THEN Name END ASC,
        CASE WHEN @SortField = N'Name' AND @SortDirection = N'desc' THEN Name END DESC,
        CASE WHEN @SortField = N'Family' AND @SortDirection = N'asc' THEN Family END ASC,
        CASE WHEN @SortField = N'Family' AND @SortDirection = N'desc' THEN Family END DESC,
        CASE WHEN @SortField = N'City' AND @SortDirection = N'asc' THEN City END ASC,
        CASE WHEN @SortField = N'City' AND @SortDirection = N'desc' THEN City END DESC,
        CASE WHEN @SortField = N'Email' AND @SortDirection = N'asc' THEN Email END ASC,
        CASE WHEN @SortField = N'Email' AND @SortDirection = N'desc' THEN Email END DESC,
        CASE WHEN @SortField = N'BirthDate' AND @SortDirection = N'asc' THEN BirthDate END ASC,
        CASE WHEN @SortField = N'BirthDate' AND @SortDirection = N'desc' THEN BirthDate END DESC,
        Id ASC
    OFFSET (@Page - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
