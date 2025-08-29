// Account Management JavaScript Functions

function showChangePasswordModal(accountId, username) {
    document.getElementById('accountId').value = accountId;
    document.getElementById('accountUsername').value = username;
    document.getElementById('newPassword').value = '';
    document.getElementById('confirmPassword').value = '';
    
    var modal = new bootstrap.Modal(document.getElementById('changePasswordModal'));
    modal.show();
}

function changePassword() {
    const accountId = document.getElementById('accountId').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const username = document.getElementById('accountUsername').value;
    
    // Validation
    if (!newPassword || newPassword.length < 6) {
        showAlert('Mật khẩu phải có ít nhất 6 ký tự!', 'warning');
        return;
    }
    
    if (newPassword !== confirmPassword) {
        showAlert('Mật khẩu xác nhận không khớp!', 'warning');
        return;
    }
    
    // Show loading
    const submitBtn = event.target;
    const originalText = submitBtn.innerHTML;
    submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
    submitBtn.disabled = true;
    
    // Send AJAX request
    fetch('/AccountManagement/ChangePassword', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: `id=${accountId}&newPassword=${encodeURIComponent(newPassword)}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert(`Đổi mật khẩu cho tài khoản "${username}" thành công!`, 'success');
            bootstrap.Modal.getInstance(document.getElementById('changePasswordModal')).hide();
        } else {
            showAlert(data.message || 'Có lỗi xảy ra khi đổi mật khẩu!', 'danger');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showAlert('Có lỗi xảy ra khi đổi mật khẩu!', 'danger');
    })
    .finally(() => {
        submitBtn.innerHTML = originalText;
        submitBtn.disabled = false;
    });
}

function deleteAccount(accountId, username) {
    // Store account info for confirmation modal
    window.pendingDeleteAccount = { accountId, username };
    
    // Show confirmation modal
    showDeleteConfirmationModal(username);
}

function showDeleteConfirmationModal(username) {
    // Remove existing modal if any
    const existingModal = document.getElementById('deleteConfirmationModal');
    if (existingModal) {
        existingModal.remove();
    }
    
    // Create confirmation modal
    const modalHtml = `
        <div class="modal fade" id="deleteConfirmationModal" tabindex="-1">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header bg-danger text-white">
                        <h5 class="modal-title">
                            <i class="fas fa-exclamation-triangle me-2"></i>Xác nhận xóa tài khoản
                        </h5>
                        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="modal"></button>
                    </div>
                    <div class="modal-body">
                        <div class="text-center">
                            <i class="fas fa-user-times text-danger" style="font-size: 3rem; margin-bottom: 1rem;"></i>
                            <h6>Bạn có chắc chắn muốn xóa tài khoản:</h6>
                            <h5 class="text-danger fw-bold">${username}</h5>
                            <p class="text-muted mt-3">
                                <i class="fas fa-exclamation-circle me-1"></i>
                                Hành động này không thể hoàn tác!
                            </p>
                        </div>
                    </div>
                    <div class="modal-footer">
                        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
                            <i class="fas fa-times me-1"></i>Hủy
                        </button>
                        <button type="button" class="btn btn-danger" onclick="confirmDeleteAccount()">
                            <i class="fas fa-trash me-1"></i>Xóa tài khoản
                        </button>
                    </div>
                </div>
            </div>
        </div>
    `;
    
    document.body.insertAdjacentHTML('beforeend', modalHtml);
    
    // Show modal
    const modal = new bootstrap.Modal(document.getElementById('deleteConfirmationModal'));
    modal.show();
}

function confirmDeleteAccount() {
    if (!window.pendingDeleteAccount) {
        return;
    }
    
    const { accountId, username } = window.pendingDeleteAccount;
    
    // Hide confirmation modal
    const modal = bootstrap.Modal.getInstance(document.getElementById('deleteConfirmationModal'));
    modal.hide();
    
    // Show loading on the delete button
    const deleteBtn = document.querySelector(`tr[data-account-id="${accountId}"] .btn-danger`);
    const originalText = deleteBtn.innerHTML;
    deleteBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xóa...';
    deleteBtn.disabled = true;
    
    // Send AJAX request
    fetch('/AccountManagement/DeleteAccount', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: `id=${accountId}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert(`Xóa tài khoản "${username}" thành công!`, 'success');
            // Remove the row from table
            const row = document.querySelector(`tr[data-account-id="${accountId}"]`);
            if (row) {
                row.remove();
            }
        } else {
            showAlert(data.message || 'Có lỗi xảy ra khi xóa tài khoản!', 'danger');
            deleteBtn.innerHTML = originalText;
            deleteBtn.disabled = false;
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showAlert('Có lỗi xảy ra khi xóa tài khoản!', 'danger');
        deleteBtn.innerHTML = originalText;
        deleteBtn.disabled = false;
    })
    .finally(() => {
        // Clean up
        window.pendingDeleteAccount = null;
    });
}

function showAlert(message, type) {
    // Remove existing alerts
    const existingAlerts = document.querySelectorAll('.alert-custom');
    existingAlerts.forEach(alert => alert.remove());
    
    // Create new alert
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show alert-custom`;
    alertDiv.style.position = 'fixed';
    alertDiv.style.top = '20px';
    alertDiv.style.right = '20px';
    alertDiv.style.zIndex = '9999';
    alertDiv.style.minWidth = '300px';
    
    alertDiv.innerHTML = `
        <i class="fas fa-${getAlertIcon(type)} me-2"></i>${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    document.body.appendChild(alertDiv);
    
    // Auto remove after 5 seconds
    setTimeout(() => {
        if (alertDiv.parentNode) {
            alertDiv.remove();
        }
    }, 5000);
}

function getAlertIcon(type) {
    switch (type) {
        case 'success': return 'check-circle';
        case 'danger': return 'exclamation-triangle';
        case 'warning': return 'exclamation-circle';
        case 'info': return 'info-circle';
        default: return 'info-circle';
    }
}

function getAntiForgeryToken() {
    // Try to get from meta tag first
    const token = document.querySelector('meta[name="__RequestVerificationToken"]');
    if (token) {
        return token.getAttribute('content');
    }
    
    // Try to get from hidden input
    const hiddenInput = document.querySelector('input[name="__RequestVerificationToken"]');
    if (hiddenInput) {
        return hiddenInput.value;
    }
    
    return '';
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', function() {
    console.log('Account Management JavaScript loaded successfully');
});