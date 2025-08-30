-- Script migrate dữ liệu từ các bảng cũ sang bảng trung tâm datarunefLogs
USE DataJonah;
GO

-- Migrate dữ liệu từ datarunefApplicationLogs
INSERT INTO datarunefLogs (
    log_source, log_level, username, computer_code, computer_name, remote_ip,
    session_id, session_start_time, session_end_time,
    activity_category, activity_type, activity_action,
    target_type, target_path, target_name, process_name,
    activity_details, result_status, error_message,
    client_version, api_version, additional_data,
    log_timestamp, created_date
)
SELECT 
    'API_SERVER' as log_source,
    CASE 
        WHEN LogType = 'ERROR' THEN 'ERROR'
        WHEN LogType = 'WARNING' THEN 'WARNING'
        WHEN LogType = 'SUCCESS' THEN 'SUCCESS'
        ELSE 'INFO'
    END as log_level,
    Username,
    ComputerCode,
    NULL as computer_name,
    IpAddress as remote_ip,
    NULL as session_id,
    NULL as session_start_time,
    NULL as session_end_time,
    CASE 
        WHEN LogType = 'FolderData' THEN 'FILE'
        WHEN LogType = 'ActivityLog' THEN 'APPLICATION'
        WHEN LogType = 'FolderRestart' THEN 'SYSTEM'
        ELSE 'APPLICATION'
    END as activity_category,
    LogType as activity_type,
    Action as activity_action,
    NULL as target_type,
    NULL as target_path,
    NULL as target_name,
    NULL as process_name,
    Details as activity_details,
    Result as result_status,
    NULL as error_message,
    NULL as client_version,
    '1.0' as api_version,
    Message as additional_data,
    LogTime as log_timestamp,
    CreatedAt as created_date
FROM datarunefApplicationLogs
WHERE CreatedAt IS NOT NULL;
GO

-- Migrate dữ liệu từ datarunefclientlogs
INSERT INTO datarunefLogs (
    log_source, log_level, username, computer_code, computer_name, remote_ip,
    session_id, session_start_time, session_end_time,
    activity_category, activity_type, activity_action,
    target_type, target_path, target_name, process_name,
    activity_details, result_status, error_message,
    client_version, api_version, additional_data,
    log_timestamp, created_date
)
SELECT 
    'CLIENT' as log_source,
    CASE 
        WHEN result_status = 'FAILED' THEN 'ERROR'
        WHEN result_status = 'WARNING' THEN 'WARNING'
        WHEN result_status = 'SUCCESS' THEN 'SUCCESS'
        ELSE 'INFO'
    END as log_level,
    username,
    computer_code,
    computer_name,
    remote_ip,
    session_id,
    session_start_time,
    session_end_time,
    activity_category,
    activity_type,
    activity_action,
    CASE 
        WHEN target_path IS NOT NULL AND (target_path LIKE '%.%' OR target_path LIKE '%\\%' OR target_path LIKE '%/%') THEN 'FILE'
        WHEN target_path IS NOT NULL THEN 'FOLDER'
        WHEN process_name IS NOT NULL THEN 'PROCESS'
        ELSE NULL
    END as target_type,
    target_path,
    target_name,
    process_name,
    activity_details,
    result_status,
    error_message,
    client_version,
    '1.0' as api_version,
    NULL as additional_data,
    log_timestamp,
    created_date
FROM datarunefclientlogs
WHERE created_date IS NOT NULL;
GO

-- Tạo view để xem dữ liệu đã migrate
CREATE VIEW vw_MigratedLogs AS
SELECT 
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
    target_path,
    target_name,
    process_name,
    activity_details,
    result_status,
    error_message,
    client_version
FROM datarunefLogs
ORDER BY log_timestamp DESC;
GO

-- Hiển thị thống kê migration
SELECT 
    log_source,
    COUNT(*) as total_logs,
    MIN(log_timestamp) as earliest_log,
    MAX(log_timestamp) as latest_log
FROM datarunefLogs
GROUP BY log_source;
GO

PRINT 'Migration completed successfully!';
PRINT 'Use vw_MigratedLogs to view all migrated data.';
