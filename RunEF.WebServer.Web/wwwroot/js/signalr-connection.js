// Global SignalR Connection Manager
class SignalRConnectionManager {
    constructor() {
        this.connection = null;
        this.isConnected = false;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.reconnectDelay = 5000; // 5 seconds
        this.callbacks = {
            'ClientStatusUpdate': [],
            'FactoryStatusUpdate': [],
            'CommandResponse': [],
            'LogEntry': [],
            'DashboardUpdate': [],
            'SystemLog': [],
            'ClientUpdate': [],
            'ClientStatusChange': []
        };
        this.init();
    }

    init() {
        this.connection = new signalR.HubConnectionBuilder()
            .withUrl("/monitoringHub")
            .withAutomaticReconnect({
                nextRetryDelayInMilliseconds: retryContext => {
                    if (retryContext.previousRetryCount < 3) {
                        return Math.random() * 10000;
                    } else {
                        return null; // Stop retrying
                    }
                }
            })
            .build();

        this.setupEventHandlers();
        this.connect();
    }

    setupEventHandlers() {
        // Connection events
        this.connection.onclose(async () => {
            this.isConnected = false;
            console.log('SignalR connection closed');
            this.showConnectionStatus('Disconnected', 'danger');
            await this.reconnect();
        });

        this.connection.onreconnecting(() => {
            console.log('SignalR reconnecting...');
            this.showConnectionStatus('Reconnecting...', 'warning');
        });

        this.connection.onreconnected(() => {
            this.isConnected = true;
            this.reconnectAttempts = 0;
            console.log('SignalR reconnected');
            this.showConnectionStatus('Connected', 'success');
        });

        // Register all callback handlers
        Object.keys(this.callbacks).forEach(method => {
            this.connection.on(method, (...args) => {
                this.callbacks[method].forEach(callback => {
                    try {
                        callback(...args);
                    } catch (error) {
                        console.error(`Error in ${method} callback:`, error);
                    }
                });
            });
        });
    }

    async connect() {
        try {
            await this.connection.start();
            this.isConnected = true;
            this.reconnectAttempts = 0;
            console.log('SignalR connected successfully');
            this.showConnectionStatus('Connected', 'success');
            
            // Join monitoring group
            await this.connection.invoke('JoinGroup', 'Monitoring');
        } catch (error) {
            console.error('SignalR connection failed:', error);
            this.showConnectionStatus('Connection Failed', 'danger');
            await this.reconnect();
        }
    }

    async reconnect() {
        if (this.reconnectAttempts >= this.maxReconnectAttempts) {
            console.log('Max reconnection attempts reached');
            this.showConnectionStatus('Connection Lost', 'danger');
            return;
        }

        this.reconnectAttempts++;
        console.log(`Reconnection attempt ${this.reconnectAttempts}/${this.maxReconnectAttempts}`);
        
        setTimeout(async () => {
            if (!this.isConnected) {
                await this.connect();
            }
        }, this.reconnectDelay);
    }

    // Subscribe to events
    on(method, callback) {
        if (this.callbacks[method]) {
            this.callbacks[method].push(callback);
        } else {
            console.warn(`Unknown SignalR method: ${method}`);
        }
    }

    // Unsubscribe from events
    off(method, callback) {
        if (this.callbacks[method]) {
            const index = this.callbacks[method].indexOf(callback);
            if (index > -1) {
                this.callbacks[method].splice(index, 1);
            }
        }
    }

    // Send message to server
    async invoke(method, ...args) {
        if (this.isConnected && this.connection) {
            try {
                return await this.connection.invoke(method, ...args);
            } catch (error) {
                console.error(`Error invoking ${method}:`, error);
                throw error;
            }
        } else {
            console.warn('SignalR not connected. Cannot invoke method:', method);
            throw new Error('SignalR connection not available');
        }
    }

    // Show connection status
    showConnectionStatus(message, type) {
        // Remove existing status indicators
        const existingStatus = document.querySelector('.signalr-status');
        if (existingStatus) {
            existingStatus.remove();
        }

        // Create new status indicator
        const statusDiv = document.createElement('div');
        statusDiv.className = `alert alert-${type} alert-dismissible fade show signalr-status`;
        statusDiv.style.cssText = 'position: fixed; top: 70px; right: 20px; z-index: 9999; min-width: 200px;';
        statusDiv.innerHTML = `
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'warning' ? 'exclamation-triangle' : 'times-circle'}"></i>
            SignalR: ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(statusDiv);

        // Auto-hide success messages
        if (type === 'success') {
            setTimeout(() => {
                if (statusDiv.parentNode) {
                    statusDiv.remove();
                }
            }, 3000);
        }
    }

    // Get connection state
    getConnectionState() {
        return {
            isConnected: this.isConnected,
            state: this.connection ? this.connection.state : 'Disconnected',
            reconnectAttempts: this.reconnectAttempts
        };
    }
}

// Global instance
window.signalRManager = new SignalRConnectionManager();

// Utility functions for backward compatibility
window.initializeSignalR = function() {
    console.log('SignalR already initialized globally');
    return window.signalRManager;
};

// Export for modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SignalRConnectionManager;
}