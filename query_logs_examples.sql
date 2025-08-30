-- Các ví dụ query logs từ bảng trung tâm datarunefLogs
USE DataJonah;
GO

-- 1. Xem tất cả logs gần đây
SELECT TOP 100 
    log_id,
    log_timestamp,
    log_source,
    log_level,
    username,
    computer_code,
    computer_name,
    activity_category,
    activity_type,
    activity_action,
    result_status
FROM datarunefLogs
ORDER BY log_timestamp DESC;
GO

-- 2. Xem logs theo nguồn (API Server vs Client)
SELECT 
    log_source,
    COUNT(*) as total_logs,
    COUNT(CASE WHEN log_level = 'ERROR' THEN 1 END) as error_count,
    COUNT(CASE WHEN log_level = 'WARNING' THEN 1 END) as warning_count
FROM datarunefLogs
GROUP BY log_source;
GO

-- 3. Xem logs theo computer code cụ thể
SELECT 
    log_timestamp,
    log_source,
    log_level,
    activity_category,
    activity_type,
    activity_action,
    result_status,
    activity_details
FROM datarunefLogs
WHERE computer_code = 'YOUR_COMPUTER_CODE'
ORDER BY log_timestamp DESC;
GO

-- 4. Xem tất cả lỗi
SELECT 
    log_timestamp,
    log_source,
    username,
    computer_code,
    computer_name,
    activity_category,
    activity_type,
    activity_action,
    error_message,
    activity_details
FROM datarunefLogs
WHERE log_level = 'ERROR'
ORDER BY log_timestamp DESC;
GO

-- 5. Xem user activities
SELECT 
    log_timestamp,
    username,
    computer_code,
    activity_category,
    activity_type,
    activity_action,
    target_path,
    target_name,
    result_status
FROM datarunefLogs
WHERE activity_category = 'USER_ACTION'
ORDER BY log_timestamp DESC;
GO

-- 6. Xem logs theo thời gian (hôm nay)
SELECT 
    log_timestamp,
    log_source,
    log_level,
    username,
    computer_code,
    activity_category,
    activity_type,
    activity_action,
    result_status
FROM datarunefLogs
WHERE CAST(log_timestamp AS DATE) = CAST(GETDATE() AS DATE)
ORDER BY log_timestamp DESC;
GO

-- 7. Xem logs theo thời gian (tuần này)
SELECT 
    log_timestamp,
    log_source,
    log_level,
    username,
    computer_code,
    activity_category,
    activity_type,
    activity_action,
    result_status
FROM datarunefLogs
WHERE log_timestamp >= DATEADD(DAY, -7, GETDATE())
ORDER BY log_timestamp DESC;
GO

-- 8. Xem logs theo process name
SELECT 
    log_timestamp,
    log_source,
    username,
    computer_code,
    process_name,
    activity_type,
    activity_action,
    result_status
FROM datarunefLogs
WHERE process_name IS NOT NULL
ORDER BY log_timestamp DESC;
GO

-- 9. Xem logs theo file operations
SELECT 
    log_timestamp,
    log_source,
    username,
    computer_code,
    target_path,
    target_name,
    activity_action,
    result_status
FROM datarunefLogs
WHERE target_type = 'FILE'
ORDER BY log_timestamp DESC;
GO

-- 10. Sử dụng function để query theo thời gian
SELECT * FROM datarunef_fn_GetLogsByDateRange(
    DATEADD(DAY, -1, GETDATE()),  -- 1 ngày trước
    GETDATE(),                     -- Hiện tại
    NULL,                          -- Tất cả computer codes
    'ERROR',                       -- Chỉ lỗi
    NULL                           -- Tất cả categories
);
GO

-- 11. Thống kê theo ngày
SELECT 
    CAST(log_timestamp AS DATE) as log_date,
    log_source,
    COUNT(*) as total_logs,
    COUNT(CASE WHEN log_level = 'ERROR' THEN 1 END) as errors,
    COUNT(CASE WHEN log_level = 'WARNING' THEN 1 END) as warnings,
    COUNT(CASE WHEN log_level = 'SUCCESS' THEN 1 END) as successes
FROM datarunefLogs
WHERE log_timestamp >= DATEADD(DAY, -30, GETDATE())
GROUP BY CAST(log_timestamp AS DATE), log_source
ORDER BY log_date DESC, log_source;
GO

-- 12. Xem logs theo session
SELECT 
    session_id,
    username,
    computer_code,
    MIN(log_timestamp) as session_start,
    MAX(log_timestamp) as session_end,
    COUNT(*) as total_activities
FROM datarunefLogs
WHERE session_id IS NOT NULL
GROUP BY session_id, username, computer_code
ORDER BY session_start DESC;
GO

PRINT 'Các ví dụ query logs đã được tạo!';
PRINT 'Thay đổi YOUR_COMPUTER_CODE thành computer code thực tế để test.';
