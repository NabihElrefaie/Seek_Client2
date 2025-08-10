# Seek Application - Secure Database Encryption

This document describes the secure key management system implemented for the Seek application's database encryption.

## Overview

The system provides a secure way to handle database encryption keys without storing them in plain text in configuration files. It uses a multi-layered approach to derive and securely store encryption keys.

## Features

- **Device Binding**: Keys are tied to the specific hardware, preventing database access from unauthorized devices
- **User Password Protection** (optional): Add an extra layer of security with user-provided passwords
- **Offline Operation**: Works completely offline with no external dependencies
- **Multi-storage Strategy**: Keys are stored redundantly to prevent data loss
- **Protected Storage**: Uses Windows DPAPI when available or secure alternatives on other platforms

## Setup Instructions

### 1. Basic Configuration

The system works out of the box with no configuration needed. When first run, it will:
- Generate a secure random encryption key
- Store it securely on the device
- Bind it to the specific hardware
- Use it for all future database access

### 2. Adding User Password Protection

To require a password for database access:

```json
// In appsettings.json - ONLY FOR INITIAL SETUP
{
  "UserPassword": "initial-secure-password-here"
}
```

After setup, remove the password from the configuration and implement UI for password entry:

```csharp
// Example code to set new password
var keyManager = serviceProvider.GetRequiredService<Security.SecureKeyManager>();
keyManager.SetUserPassword("new-secure-password");

// Example code to validate and use password
if (keyManager.ValidatePassword(userInputPassword))
{
    // Password is correct, proceed
}
```

## Security Considerations

1. **Key Recovery**: If both the registry and file storage of keys are lost, database access will be impossible without a backup. Consider implementing a secure key backup system for production.

2. **Password Management**: If using password protection, implement a secure password recovery mechanism.

3. **Device Migration**: When moving the database to a new device, you'll need to migrate keys as well. Consider implementing a secure key export/import feature.

## Implementation Details

The system uses the following security measures:

1. **Key Storage**:
   - Windows Registry (on Windows)
   - Protected files
   - DPAPI protection (on Windows)
   - Custom encryption on other platforms

2. **Hardware Binding**:
   - CPU identifiers
   - Volume serial numbers
   - Machine and user names

3. **Key Derivation**:
   - PBKDF2 with SHA-256
   - High iteration count (50,000)
   - Hardware-derived salt

## Default Configurations

- Key size: 256-bit (32 bytes)
- PBKDF2 iterations: 50,000
- Hash algorithm: SHA-256 for keys, SHA-512 for verification

## Logging and Diagnostics

The system logs all key operations with appropriate log levels:
- INFO: Successful key operations
- WARNING: Fallbacks or non-critical failures
- ERROR: Critical failures that affect database access

## Extension Points

The system is designed to be extensible. Consider adding:

1. **TPM Integration**: For devices with TPM chips, bind keys to the TPM
2. **Smart Card Support**: Allow keys to be stored on physical smart cards
3. **Key Rotation**: Implement periodic key rotation for enhanced security