using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Seek.EF.Email_templates
{
    public class Email_Template
    {
        /// Builds the email body for a get verification Key
        public static string VerificationCodeEmailBody(string verificationCode)
        {
            return $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 5px; }}
                        .header {{ background-color: #f8f8f8; padding: 20px; border-bottom: 1px solid #ddd; }}
                        .content {{ padding: 20px; }}
                        .code-box {{ background-color: #f2f2f2; padding: 15px; border: 1px solid #ddd; border-radius: 5px; text-align: center; font-size: 24px; font-weight: bold; letter-spacing: 5px; margin: 20px 0; }}
                        .footer {{ background-color: #f8f8f8; padding: 20px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>Seek Application Verification</h2>
                        </div>
                        <div class='content'>
                            <p>Thank you for installing the Seek application. To verify and activate your installation, please use the verification code below:</p>
            
                            <div class='code-box'>
                                {verificationCode}
                            </div>
            
                            <p>Enter this code in the verification prompt in your Seek application to complete the activation process.</p>
            
                            <p><strong>Important:</strong> This verification code is required to activate your application. If you did not request this code, please ignore this email.</p>
                        </div>
                        <div class='footer'>
                            This is an automated message from your Seek application. Please do not reply to this email.
                            <br>
                            Verification code expires in 24 hours.
                        </div>
                    </div>
                </body>
                </html>";
        }
        /// Builds the email body for a new device registration notification
        public static string NewDeviceRegistrationEmailBody(string deviceId, string ipAddress, string encryptionKey, bool passwordProtected)
        {
            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; margin: 0; padding: 20px; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; border: 1px solid #ddd; border-radius: 5px; }}
                    .header {{ background-color: #f8f8f8; padding: 20px; border-bottom: 1px solid #ddd; }}
                    .content {{ padding: 20px; }}
                    .alert {{ color: #721c24; background-color: #f8d7da; padding: 10px; border-radius: 5px; margin-bottom: 20px; }}
                    .footer {{ background-color: #f8f8f8; padding: 20px; border-top: 1px solid #ddd; font-size: 12px; text-align: center; }}
                    table {{ width: 100%; border-collapse: collapse; margin-bottom: 20px; }}
                    th, td {{ padding: 10px; text-align: left; border-bottom: 1px solid #ddd; }}
                    th {{ background-color: #f2f2f2; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h2>Security Alert: New Device Registration</h2>
                    </div>
                    <div class='content'>
                        <div class='alert'>
                            A new device has been registered with your Seek application.
                        </div>
            
                        <p>A new device has been registered to access your encrypted data. If this was you, no action is needed. If you did not register a new device, please contact your system administrator immediately.</p>
            
                        <table>
                            <tr>
                                <th>Information</th>
                                <th>Details</th>
                            </tr>
                            <tr>
                                <td>Device ID:</td>
                                <td>{deviceId}</td>
                            </tr>
                            <tr>
                                <td>IP Address:</td>
                                <td>{ipAddress}</td>
                            </tr>
                            <tr>
                                <td>Time (UTC):</td>
                                <td>{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}</td>
                            </tr>
                            <tr>
                                <td>Password Protected:</td>
                                <td>{(passwordProtected ? "Yes" : "No")}</td>
                            </tr>
                           <tr>
                            <td>Encryption Key:</td>
                            <td><code style=""background-color: #f8f8f8; padding: 5px; border: 1px solid #ddd; border-radius: 3px;"">{encryptionKey}</code></td>
                        </tr>
                        </table>


                        <div style=""background-color: #fff3cd; border: 1px solid #ffeeba; color: #856404; padding: 15px; margin: 20px 0; border-radius: 5px;"">
                            <strong>Important:</strong> The encryption key above is required for database decryption. Please store it securely for technical support purposes.
                        </div>
                        <p><strong> What should I do?</strong></p>
                        <p> If you authorized this device, no action is required.If you did not authorize this device, please:</p>
                        <ol>
                            <li> Change your password immediately </li>
                            <li> Contact your system administrator </li>
                            <li> Review your security settings </li>
                        </ol>
                    </div>
                    <div class='footer'>
                        This is an automated message from your Seek application.Please do not reply to this email.
                    </div>
                </div>
            </body>
            </html>";
        }

    }
}
