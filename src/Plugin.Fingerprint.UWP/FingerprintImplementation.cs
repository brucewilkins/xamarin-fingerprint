using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;
using Plugin.Fingerprint.Abstractions;
using System.Collections.Generic;

namespace Plugin.Fingerprint
{
    internal class FingerprintImplementation : FingerprintImplementationBase
    {
        protected override async Task<FingerprintAuthenticationResult> NativeAuthenticateAsync(AuthenticationRequestConfiguration authRequestConfig, CancellationToken cancellationToken = new CancellationToken())
        {
            var result = new FingerprintAuthenticationResult();

            try
            {
                var verificationResult = await UserConsentVerifier.RequestVerificationAsync(authRequestConfig.Reason);

                switch (verificationResult)
                {
                    case UserConsentVerificationResult.Verified:
                        result.Status = FingerprintAuthenticationResultStatus.Succeeded;
                        break;

                    case UserConsentVerificationResult.DeviceBusy:
                    case UserConsentVerificationResult.DeviceNotPresent:
                    case UserConsentVerificationResult.DisabledByPolicy:
                    case UserConsentVerificationResult.NotConfiguredForUser:
                        result.Status = FingerprintAuthenticationResultStatus.NotAvailable;
                        break;
                    
                    case UserConsentVerificationResult.RetriesExhausted:
                        result.Status = FingerprintAuthenticationResultStatus.TooManyAttempts;
                        break;
                    case UserConsentVerificationResult.Canceled:
                        result.Status = FingerprintAuthenticationResultStatus.Cancelled;
                        break;
                    default:
                        result.Status = FingerprintAuthenticationResultStatus.Failed;
                        break;
                }
            }
            catch (Exception ex)
            {
                result.Status = FingerprintAuthenticationResultStatus.UnknownError;
                result.ErrorMessage = ex.Message;
            }

            return result;
        }

        public override async Task<FingerprintAvailability> GetAvailabilityAsync()
        {
            var uwpAvailability = await UserConsentVerifier.CheckAvailabilityAsync();

            switch (uwpAvailability)
            {
                case UserConsentVerifierAvailability.Available:
                    return FingerprintAvailability.Available;
                case UserConsentVerifierAvailability.DeviceNotPresent:
                    return FingerprintAvailability.NoSensor;
                case UserConsentVerifierAvailability.NotConfiguredForUser:
                    return FingerprintAvailability.NoFingerprint;
                case UserConsentVerifierAvailability.DisabledByPolicy:
                    return FingerprintAvailability.NoPermission;
                case UserConsentVerifierAvailability.DeviceBusy:
                default:
                    return FingerprintAvailability.Unknown;
            }
        }

        protected override Task<SecureValueResult> NativeSetSecureValue(SetSecureValueRequestConfiguration setSecureValueRequestConfig, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SecureValueResult
            {
                Status = FingerprintAuthenticationResultStatus.NotAvailable,
                ErrorMessage = "Not implemented for the current platform."
            });
        }

        protected override Task<SecureValueResult> NativeRemoveSecureValue(SecureValueRequestConfiguration secureValueRequestConfig, CancellationToken cancellationToken)
        {
            return Task.FromResult(new SecureValueResult
            {
                Status = FingerprintAuthenticationResultStatus.NotAvailable,
                ErrorMessage = "Not implemented for the current platform."
            });
        }

        protected override Task<GetSecureValueResult> NativeGetSecureValue(SecureValueRequestConfiguration secureValueRequestConfig, CancellationToken cancellationToken)
        {
            return Task.FromResult(new GetSecureValueResult
            {
                Status = FingerprintAuthenticationResultStatus.NotAvailable,
                ErrorMessage = "Not implemented for the current platform."
            });
        }
    }
}