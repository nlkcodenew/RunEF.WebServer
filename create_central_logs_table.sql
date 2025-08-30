-- Tạo bảng trung tâm datarunefLogs để thay thế cả datarunefApplicationLogs và datarunefclientlogs
-- Bảng này sẽ chứa tất cả logs từ cả API server và client với cấu trúc tối ưu

USE DataJonah;
GO

-- Tạo bảng trung tâm
CREATE TABLE datarunefLogs (
    log_id BIGINT IDENTITY(1,1) PRIMARY KEY,
    
    -- Thông tin cơ bản
    log_timestamp DATETIME2(3) NOT NULL DEFAULT GETDATE(),
    created_date DATETIME2(3) NOT NULL DEFAULT GETDATE(),
    
    -- Thông tin nguồn log
    log_source NVARCHAR(20) NOT NULL,           -- 'API_SERVER', 'CLIENT', 'SYSTEM'
    log_level NVARCHAR(20) NOT NULL,            -- 'INFO', 'WARNING', 'ERROR', 'SUCCESS', 'DEBUG'
    
    -- Thông tin người dùng
    username NVARCHAR(100) NULL,                -- Tên người dùng thực hiện hành động
    computer_code NVARCHAR(50) NOT NULL,        -- Mã PC client
    computer_name NVARCHAR(100) NULL,           -- Tên máy tính
    remote_ip NVARCHAR(45) NULL,                -- IP của client
    
    -- Thông tin session
    session_id NVARCHAR(100) NULL,              -- ID phiên làm việc
    session_start_time DATETIME2(3) NULL,       -- Thời gian bắt đầu session
    session_end_time DATETIME2(3) NULL,         -- Thời gian kết thúc session
    
    -- Phân loại hoạt động
    activity_category NVARCHAR(50) NOT NULL,    -- 'SYSTEM', 'USER_ACTION', 'APPLICATION', 'FILE', 'DATABASE', 'NETWORK'
    activity_type NVARCHAR(50) NOT NULL,        -- Loại hoạt động cụ thể
    activity_action NVARCHAR(100) NULL,         -- Hành động cụ thể
    
    -- Thông tin đối tượng
    target_type NVARCHAR(50) NULL,              -- 'FILE', 'FOLDER', 'PROCESS', 'DATABASE', 'API'
    target_path NVARCHAR(500) NULL,             -- Đường dẫn file/folder
    target_name NVARCHAR(255) NULL,             -- Tên file/folder/process
    process_name NVARCHAR(100) NULL,            -- Tên process liên quan
    
    -- Chi tiết hoạt động
    activity_details NVARCHAR(1000) NULL,       -- Chi tiết hoạt động
    result_status NVARCHAR(20) NULL,            -- 'SUCCESS', 'FAILED', 'WARNING', 'PENDING'
    error_message NVARCHAR(500) NULL,           -- Thông báo lỗi nếu có
    
    -- Metadata
    client_version NVARCHAR(20) NULL,           -- Phiên bản ứng dụng client
    api_version NVARCHAR(20) NULL,              -- Phiên bản API
    additional_data NVARCHAR(MAX) NULL,         -- Dữ liệu bổ sung dạng JSON
    
    -- Indexes để tối ưu query
    INDEX IX_datarunefLogs_timestamp (log_timestamp),
    INDEX IX_datarunefLogs_source (log_source),
    INDEX IX_datarunefLogs_level (log_level),
    INDEX IX_datarunefLogs_category (activity_category),
    INDEX IX_datarunefLogs_computer_code (computer_code),
    INDEX IX_datarunefLogs_username (username),
    INDEX IX_datarunefLogs_session_id (session_id),
    INDEX IX_datarunefLogs_status (result_status)
);
GO

-- Tạo stored procedure để thêm log
CREATE PROCEDURE datarunef_sp_AddLog
    @log_source NVARCHAR(20),
    @log_level NVARCHAR(20),
    @username NVARCHAR(100) = NULL,
    @computer_code NVARCHAR(50),
    @computer_name NVARCHAR(100) = NULL,
    @remote_ip NVARCHAR(45) = NULL,
    @session_id NVARCHAR(100) = NULL,
    @session_start_time DATETIME2(3) = NULL,
    @session_end_time DATETIME2(3) = NULL,
    @activity_category NVARCHAR(50),
    @activity_type NVARCHAR(50),
    @activity_action NVARCHAR(100) = NULL,
    @target_type NVARCHAR(50) = NULL,
    @target_path NVARCHAR(500) = NULL,
    @target_name NVARCHAR(255) = NULL,
    @process_name NVARCHAR(100) = NULL,
    @activity_details NVARCHAR(1000) = NULL,
    @result_status NVARCHAR(20) = NULL,
    @error_message NVARCHAR(500) = NULL,
    @client_version NVARCHAR(20) = NULL,
    @api_version NVARCHAR(20) = NULL,
    @additional_data NVARCHAR(MAX) = NULL
AS
BEGIN
    INSERT INTO datarunefLogs (
        log_source, log_level, username, computer_code, computer_name, remote_ip,
        session_id, session_start_time, session_end_time,
        activity_category, activity_type, activity_action,
        target_type, target_path, target_name, process_name,
        activity_details, result_status, error_message,
        client_version, api_version, additional_data
    ) VALUES (
        @log_source, @log_level, @username, @computer_code, @computer_name, @remote_ip,
        @session_id, @session_start_time, @session_end_time,
        @activity_category, @activity_type, @activity_action,
        @target_type, @target_path, @target_name, @process_name,
        @activity_details, @result_status, @error_message,
        @client_version, @api_version, @additional_data
    );
END;
GO

-- Tạo view để dễ dàng query logs theo nguồn
CREATE VIEW vw_ApiServerLogs AS
SELECT * FROM datarunefLogs WHERE log_source = 'API_SERVER';

CREATE VIEW vw_ClientLogs AS
SELECT * FROM datarunefLogs WHERE log_source = 'CLIENT';

CREATE VIEW vw_SystemLogs AS
SELECT * FROM datarunefLogs WHERE log_source = 'SYSTEM';

CREATE VIEW vw_ErrorLogs AS
SELECT * FROM datarunefLogs WHERE log_level = 'ERROR';

CREATE VIEW vw_UserActivityLogs AS
SELECT * FROM datarunefLogs WHERE activity_category = 'USER_ACTION';
GO

-- Tạo function để tìm logs theo thời gian
CREATE FUNCTION datarunef_fn_GetLogsByDateRange
(
    @start_date DATETIME2,
    @end_date DATETIME2,
    @computer_code NVARCHAR(50) = NULL,
    @log_level NVARCHAR(20) = NULL,
    @activity_category NVARCHAR(50) = NULL
)
RETURNS TABLE
AS
RETURN
(
    SELECT *
    FROM datarunefLogs
    WHERE log_timestamp BETWEEN @start_date AND @end_date
    AND (@computer_code IS NULL OR computer_code = @computer_code)
    AND (@log_level IS NULL OR log_level = @log_level)
    AND (@activity_category IS NULL OR activity_category = @activity_category)
    ORDER BY log_timestamp DESC
);
GO

-- Tạo index cho performance
CREATE INDEX IX_datarunefLogs_composite ON datarunefLogs (computer_code, log_timestamp, log_level);
CREATE INDEX IX_datarunefLogs_search ON datarunefLogs (activity_category, activity_type, result_status);
GO

PRINT 'Bảng trung tâm datarunefLogs đã được tạo thành công!';
PRINT 'Các view và function đã được tạo để dễ dàng query logs.';
